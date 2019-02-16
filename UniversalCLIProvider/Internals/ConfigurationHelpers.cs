using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UniversalCLIProvider.Attributes;

namespace UniversalCLIProvider.Internals {
public static class ConfigurationHelpers {
	/// <summary>
	/// Resolves a configuration path recursively
	/// </summary>
	/// <param name="path"> The path to resolve</param>
	/// <param name="typeInfoOfItem"> The TypeInfo of the Item to process </param>
	/// <param name="currentItem"> The currentItem, for which the path shall be resolved</param>
	/// <param name="prop">The property found the match the path</param>
	/// <param name="requiredIndexers">The indexers needed to call the property</param>
	/// <param name="ro">Whether the property is ReadOnly</param>
	/// <param name="lastNonIndexer">The last property not being an indexer</param>
	/// <returns></returns>
	public static bool ResolvePathRecursive([NotNull] string path, [NotNull] TypeInfo typeInfoOfItem, ref object currentItem, out PropertyInfo prop,
		out object[] requiredIndexers, ref bool ro,
		out PropertyInfo lastNonIndexer) {
		prop = null;
		requiredIndexers = null;
		lastNonIndexer = null;
		path = path.Trim();
		string remainingPath = null;
		if (path.StartsWith("[")) {
			if (!ResolveIndexerInPath(path, typeInfoOfItem, ref prop, ref requiredIndexers, out remainingPath)) {
				return false;
			}
		}
		else {
			int dotIndex = path.IndexOf('.');
			int bracketIndex = path.IndexOf('[');
			int endOfCurrentBlock = path.Length;
			if (bracketIndex != dotIndex) { //Implies that one of them exists and that the recursion has to go deeper
				if (bracketIndex == -1 || dotIndex < bracketIndex && dotIndex != -1) {
					endOfCurrentBlock = dotIndex;
					remainingPath = path.Substring(dotIndex + 1);
				}
				else {
					endOfCurrentBlock = bracketIndex;
					remainingPath = path.Substring(bracketIndex );
				}
			}

			string currentPath = endOfCurrentBlock != -1 ? path.Substring(0, endOfCurrentBlock) : path;
			prop = null;
			foreach (PropertyInfo property in typeInfoOfItem.GetUnderlyingTypes().Distinct().SelectMany(x => x.DeclaredProperties)) {
				if (property.Name.Equals(currentPath, StringComparison.OrdinalIgnoreCase)) {
					var configurationFieldAttribute = property.GetCustomAttribute<CmdConfigurationFieldAttribute>();
					if (configurationFieldAttribute is null) {
						continue;
					}

					if (configurationFieldAttribute.IsReadonly) {
						ro = true;
					}

					typeInfoOfItem = property.PropertyType.GetTypeInfo();
					prop = property;
					lastNonIndexer = prop;
					break;
				}
			}

			if (prop is null) {
				return false;
			}
		}

		if (!string.IsNullOrEmpty(remainingPath)) { //Initiating the next recursive step
			try {
				currentItem = requiredIndexers is null
					? prop.GetValue(currentItem)
					: prop.GetValue(currentItem, requiredIndexers); //Evaluating props value when another recursive step shall be performed
			}
			catch (Exception) {
				return false;
			}

			if (!ResolvePathRecursive(remainingPath,currentItem.GetType().GetTypeInfo(), ref currentItem, out prop, out requiredIndexers, ref ro,
				out PropertyInfo possibleLastNonIndexer)) {
				return false;
			}

			if (!(possibleLastNonIndexer is null)) {
				lastNonIndexer = possibleLastNonIndexer;
			}
		}

		return true;
	}

	/// <summary>
	///  Resolves a given indexing operator in a path
	/// </summary>
	/// <param name="path">The path starting with a [</param>
	/// <param name="typeInfoOfItem">The <see cref="TypeInfo" /> of the item to which indexer shall be resolved</param>
	/// <param name="prop">The resolved indexing property</param>
	/// <param name="requiredIndexers">The indexing operators to be used</param>
	/// <param name="remainingPath">The path remaining to be resolved later on</param>
	/// <returns>Whether the operation were successful</returns>
	private static bool ResolveIndexerInPath([NotNull] string path, [NotNull] TypeInfo typeInfoOfItem, ref PropertyInfo prop,
		ref object[] requiredIndexers, out string remainingPath) {
		remainingPath = null;
		int endOfIndexer = path.IndexOf(']');
		if (endOfIndexer == -1) {
			return false;
		}

		if (!SplitIndexerArguments(path.Substring(1, endOfIndexer - 1), out string[] indexerParameters)) {
			return false;
		}

		if (!ResolveIndexerParameters(indexerParameters, typeInfoOfItem, out requiredIndexers, out prop)) {
			return false;
		}

		if (path.Length - 1 > endOfIndexer) {
			remainingPath = path.Substring(endOfIndexer + 1);
			if (remainingPath.StartsWith(".")) {
				remainingPath = remainingPath.Substring(1);
			}
		}

		return true;
	}

	public static bool ResolveIndexerParameters([NotNull] string[] parameters, [NotNull] TypeInfo type, out object[] indexParameters,
		out PropertyInfo indexer) {
		indexParameters = new object[parameters.Length];
		indexer = null;
		foreach (PropertyInfo possibleIndexer in type.GetUnderlyingTypes().Distinct().SelectMany(x => x.DeclaredProperties)
			.Where(x => x.GetIndexParameters().Length == parameters.Length&&x.PropertyType!=typeof(object))) {
			bool success = true;
			ParameterInfo[] paras = possibleIndexer.GetIndexParameters();
			for (int i = 0; i < parameters.Length; i++) {
				try {
					indexParameters[i] = JsonConvert.DeserializeObject(parameters[i], paras[i].ParameterType);
				}
				catch (JsonException) {
					success = false;
					break;
				}
			}

			if (success) {
				indexer = possibleIndexer;
				return true;
			}
		}

		return false;
	}

	public static bool SplitIndexerArguments([NotNull] string src, out string[] result) {
		result = new[] {src};
		return true;
		if (!src.Contains(',')) {
			result = new[] {src};
			return true;
		} //TODO Implement

		throw new NotImplementedException("Multiple Indexer parameters haven't been implemented yet");
	}

	[NotNull]
	public static IEnumerable<TypeInfo> GetUnderlyingTypes([NotNull] this TypeInfo src) {
		IEnumerable<TypeInfo> baseEnumerator = src.ImplementedInterfaces.SelectMany(x => x.GetTypeInfo().GetUnderlyingTypes()).Append(src);
		if (!(src.BaseType is null)) {
			baseEnumerator = baseEnumerator.Concat(src.BaseType.GetTypeInfo().GetUnderlyingTypes());
		}

		return baseEnumerator;
	}
}
}