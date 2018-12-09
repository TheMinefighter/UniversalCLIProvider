using System;
using System.Collections.Generic;
using System.IO;
using PropertyOrFieldInfoPackage;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UniversalCLIProvider.OtherInternals;

namespace UniversalCLIProvider.OtherInternals {
public static class ManagedConfigurationHelpers {
	public static bool ResolvePath(string path, object item, out PropertyInfo prop, out object[] requiredIndexers,
		out object newObject) {
		prop = null;
		newObject = null;
		requiredIndexers = null;
		path = path.Trim();
		TypeInfo typeInfoOfItem = item.GetType().GetTypeInfo();
		if (path.StartsWith("[")) {
			int endOfIndexer = path.IndexOf(']');
			if (endOfIndexer == -1) {
				return false;
			}

			if (!SplitIndexerArguments(path.Substring(1, endOfIndexer - 2), out string[] indexerParameters)) {
				return false;
			}

			if (!ResolveIndexerParameters(indexerParameters, typeInfoOfItem, out requiredIndexers, out prop)) {
				return false;
			}

			if (path.Length < endOfIndexer + 1) {
				object newItem;
				try {
					newItem = prop.GetValue(item, requiredIndexers);
				}
				catch (Exception) {
					return false;
				}

				if (!ResolvePath(path.Substring(endOfIndexer + 1), newItem, out prop, out requiredIndexers, out newObject)) {
					return false;
				}
			}

			return true;
		}

		string currentPath=path;
		int dotIndex = path.IndexOf('.');
		if (dotIndex!=-1) {
			currentPath = path.Substring(0, dotIndex);
		}

		prop = typeInfoOfItem.GetUnderlyingTypes().SelectMany(x => x.DeclaredProperties)
			.FirstOrDefault(x => x.Name.Equals(currentPath, StringComparison.OrdinalIgnoreCase));
		if (prop is null) {
			return false;
		}
		if (dotIndex!=-1) {
			object newItem;
			try {
				newItem = prop.GetValue(item);
			}
			catch (Exception) {
				return false;
			}

			if (!ResolvePath(path.Substring(dotIndex + 1), newItem, out prop, out requiredIndexers, out newObject)) {
				return false;
			}
		}
		return true;
	}

	public static bool ResolveIndexerParameters(string[] parameters, TypeInfo type, out object[] indexParameters,
		out PropertyInfo indexer) {
		indexParameters = new object[parameters.Length];
		indexer = null;
		foreach (PropertyInfo possibleIndexer in type.GetUnderlyingTypes().SelectMany(x => x.DeclaredProperties)
			.Where(x => x.GetIndexParameters().Length == parameters.Length)) {
			bool success = true;
			ParameterInfo[] paras = possibleIndexer.GetIndexParameters();
			for (int i = 0; i < parameters.Length; i++) {
				try {
					indexParameters[i] = JsonConvert.DeserializeObject(parameters[i], paras[i].ParameterType);
				}
				catch (JsonException) {
					success = false;
				}
			}

			if (success) {
				indexer = possibleIndexer;
				return true;
			}
		}

		return false;
	}

	public static bool SplitIndexerArguments(string src, out string[] result) {
		if (!src.Contains(',')) {
			result = new[] {src};
			return true;
		}

		//TODO Implement
		throw new NotImplementedException("Multiple Indexer parameters haven't been implemented yet");
	}

	public static IEnumerable<TypeInfo> GetUnderlyingTypes(this TypeInfo src) {
		IEnumerable<TypeInfo> baseEnumerator = src.ImplementedInterfaces.SelectMany(x => x.GetTypeInfo().GetUnderlyingTypes()).Append(src);
		if (!(src.BaseType is null)) {
			baseEnumerator = baseEnumerator.Concat(src.BaseType.GetTypeInfo().GetUnderlyingTypes());
		}

		return baseEnumerator;
	}
}
}