using System;
using System.Collections.Generic;
using PropertyOrFieldInfoPackage;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UniversalCLIProvider.OtherInternals;

namespace UniversalCLIProvider.OtherInternals {
public static class ManagedConfigurationHelpers {
	public static bool ResolvePath(string path, object item, PropertyOrFieldInfo prop) {
		path = path.Trim();
		if (path.StartsWith("[")) {
				
		}

		return false;
	}

	public static bool ResolveIndexerParameters(string[] parameters, TypeInfo type,out object[] indexParameters,out PropertyInfo indexer) {
		indexParameters = new object[parameters.Length];
		indexer = null;
		foreach (PropertyInfo possibleIndexer in type.GetUnderlyingTypes().SelectMany(x=>x.DeclaredProperties).Where(x=>x.GetIndexParameters().Length==parameters.Length)) {
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
	
	public static bool SplitIndexerArguments(string src,out string[] result) {
		if (!src.Contains(',')) {
			result = new[] {src};
			return true;
		}
		//TODO Implement
		throw new NotImplementedException("Multiple Indexer parameters haven't been implemented yet");
	}

	public static IEnumerable<TypeInfo> GetUnderlyingTypes(this TypeInfo src) {
		foreach (Type implementedInterface in src.ImplementedInterfaces) {
			TypeInfo underlyingTypes = implementedInterface.GetTypeInfo();
			yield return underlyingTypes;
			foreach (TypeInfo t in underlyingTypes.GetUnderlyingTypes()) {
				yield return t;
			}
		}

		if (src.BaseType is Type) {
			foreach (TypeInfo typeInfo in src.BaseType.GetTypeInfo().GetUnderlyingTypes()) {
				yield return typeInfo;
			}
		}
		
	}
}
}