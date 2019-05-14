using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UniversalCLIProvider.Attributes;

namespace UniversalCLIProvider.Internals {
public static class ConfigurationHelpers {
	/// <summary>
	///  Resolves a configuration path recursively
	/// </summary>
	/// <param name="path"> The path to resolve</param>
	/// <param name="typeInfoOfItem"> The TypeInfo of the Item to process </param>
	/// <param name="currentItem"> The currentItem, for which the path shall be resolved</param>
	/// <param name="prop">The property found the match the path</param>
	/// <param name="requiredIndexers">The indexers needed to call the property</param>
	/// <param name="ro">Whether the property is ReadOnly</param>
	/// <param name="lastNonIndexer">The last property not being an indexer</param>
	/// <returns></returns>
	public static (PropertyInfo prop, object[] requiredIndexers, PropertyInfo lastNonIndexer) ResolvePathRecursive(
		[NotNull] string path, [NotNull] TypeInfo typeInfoOfItem, ref object currentItem, ref bool ro) {
		PropertyInfo prop = null;
		object[] requiredIndexers = null;
		PropertyInfo lastNonIndexer = null;
		path = path.Trim();
		string remainingPath = null;
		if (path.StartsWith("[")) {
			(prop, requiredIndexers, remainingPath) = ResolveIndexerInPath(path, typeInfoOfItem);
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
					remainingPath = path.Substring(bracketIndex);
				}
			}

			string currentPath = endOfCurrentBlock != -1 ? path.Substring(0, endOfCurrentBlock) : path;
			foreach (PropertyInfo property in typeInfoOfItem.GetUnderlyingTypes().Distinct().SelectMany(x => x.DeclaredProperties)) {
				if (property.Name.Equals(currentPath, StringComparison.OrdinalIgnoreCase)) {
					var configurationFieldAttribute = property.GetCustomAttribute<CmdConfigurationFieldAttribute>();
					if (configurationFieldAttribute is null) {
						continue;
					}

					if (configurationFieldAttribute.IsReadonly) {
						ro = true;
					}

					prop = property;
					lastNonIndexer = prop;
					break;
				}
			}

			if (prop is null) {
				throw new CLIUsageException($"The property {currentItem} could not be resolved for the class {typeInfoOfItem.Name}.");
			}
		}

		if (!string.IsNullOrEmpty(remainingPath)) { //Initiating the next recursive step
			try {
				currentItem = requiredIndexers is null
					? prop.GetValue(currentItem)
					: prop.GetValue(currentItem,
						requiredIndexers); //Evaluating props value when another recursive step shall be performed
			}
			catch (Exception e) {
				throw new CLIUsageException(
					$"Whilst obtaining the value of {currentItem} an error occurred which might be caused by the programs developer:",
					e);
			}

			PropertyInfo possibleLastNonIndexer;
			(prop, requiredIndexers, possibleLastNonIndexer) =
				ResolvePathRecursive(remainingPath, currentItem.GetType().GetTypeInfo(), ref currentItem, ref ro);

			if (!(possibleLastNonIndexer is null)) {
				lastNonIndexer = possibleLastNonIndexer;
			}
		}

		return (prop, requiredIndexers, lastNonIndexer);
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
	private static (PropertyInfo prop, object[] requiredIndexers, string remainingPath) ResolveIndexerInPath([NotNull] string path,
		[NotNull] TypeInfo typeInfoOfItem) {
		(string[] indexerParameters, string remainingPath) = SplitIndexerArguments(path.Substring(1));
		(object[] requiredIndexers, PropertyInfo prop) = ResolveIndexerParameters(indexerParameters, typeInfoOfItem);
		if (remainingPath.Length != 0 && remainingPath.StartsWith(".")) {
			remainingPath = remainingPath.Substring(1);
		}

		return (prop, requiredIndexers, remainingPath);
	}

	public static (object[] indexParameters, PropertyInfo indexer) ResolveIndexerParameters([NotNull] string[] parameters,
		[NotNull] TypeInfo type) {
		var indexParameters = new object[parameters.Length];
		PropertyInfo indexer = null;
		foreach (PropertyInfo possibleIndexer in type.GetUnderlyingTypes().Distinct().SelectMany(x => x.DeclaredProperties)
			.Where(x => x.GetIndexParameters().Length == parameters.Length)) {
			if (indexer is null || indexer.PropertyType.IsAssignableFrom(possibleIndexer.PropertyType)) {
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
				}
			}
		}

		return (indexParameters, indexer);
	}

	public static (string[] splittedArguments, string remainingSrc) SplitIndexerArguments([NotNull] string src) {
		//partially based on https://stackoverflow.com/a/55503527/6730162
		var serializer = new JsonSerializer();
		var resultList = new List<string>();
		string remainingSrc = src;
		while (true) {
			using (var stringReader = new StringReader(remainingSrc))
			using (var reader = new JsonTextReader(stringReader) {SupportMultipleContent = true}) {
				try {
					reader.Read();
					//TODO Cache results
					serializer.Deserialize(reader);
				}
				catch (Exception e) {
					throw new CLIUsageException($"An error occurred whilst trying to resolve the first part of {remainingSrc}:", e);
				}

				resultList.Add(remainingSrc.Substring(0, reader.LinePosition));
				remainingSrc = remainingSrc.Substring(reader.LinePosition);
				bool cont = false;
				while (true) {
					if (remainingSrc.Length == 0) {
						throw new CLIUsageException($"The indexer ({src}) was not terminated");
					}

					char read = remainingSrc[0];
					switch (read) {
						case ' ':
							remainingSrc = remainingSrc.Substring(1);
							continue;
						case ',':
							cont = true;
							break;
						case ']': break;
						default:
							throw new CLIUsageException(
								$"The indexer ({src}) was formatted incorrectly at the beginning of \"{remainingSrc}\"");
					}

					break;
				}

				remainingSrc = remainingSrc.Substring(1);
				if (!cont) {
					break;
				}
			}
		}

		string[] result = resultList.ToArray();
		return (result, remainingSrc);
	}

	[NotNull]
	public static IEnumerable<TypeInfo> GetUnderlyingTypes([NotNull] this TypeInfo src) {
		IEnumerable<TypeInfo> baseEnumerator =
			src.ImplementedInterfaces.SelectMany(x => x.GetTypeInfo().GetUnderlyingTypes()).Append(src);
		if (!(src.BaseType is null)) {
			baseEnumerator = baseEnumerator.Concat(src.BaseType.GetTypeInfo().GetUnderlyingTypes());
		}

		return baseEnumerator;
	}
}
}