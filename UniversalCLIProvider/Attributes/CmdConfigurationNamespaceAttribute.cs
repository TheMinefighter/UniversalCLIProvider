using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using PropertyOrFieldInfoPackage;

namespace UniversalCLIProvider.Attributes {
[AttributeUsage(AttributeTargets.Class), UsedImplicitly]
public class CmdConfigurationNamespaceAttribute : Attribute {
	private CmdConfigurationValueAttribute[] _configurationValues;
	private bool _loaded;
	private TypeInfo _underlyingType;
	public string Description;
	public bool IsReadonly;
	public string[] LongDescription;
	public string Name;

	public CmdConfigurationNamespaceAttribute(string name, bool isReadonly, string description, string[] longDescription) {
		IsReadonly = isReadonly;
		Name = name;
		Description = description;
		LongDescription = longDescription;
	}

	public void Load(TypeInfo underlyingType) {
		if (!_loaded) {
			_underlyingType = underlyingType;
			List<CmdConfigurationValueAttribute> newValues = new List<CmdConfigurationValueAttribute>();
			foreach (PropertyOrFieldInfo propertyOrFieldInfo in _underlyingType.DeclaredPropertiesAndFields()) {
				var attribute = propertyOrFieldInfo.GetCustomAttribute<CmdConfigurationValueAttribute>();
				if (attribute is null) { continue;
				
					
				}
				
			}

			_loaded = true;
		}
	}
}
}