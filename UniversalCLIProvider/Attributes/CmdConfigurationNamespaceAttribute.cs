using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using PropertyOrFieldInfoPackage;

namespace UniversalCLIProvider.Attributes {
[AttributeUsage(AttributeTargets.Class), UsedImplicitly]
public class CmdConfigurationNamespaceAttribute : Attribute {
	private bool _loaded;
	private TypeInfo _underlyingType;
	internal CmdConfigurationFieldAttribute[] ConfigurationFields;
	public string Description;
	public bool IsReadonly;
	public string[] LongDescription;
	public string Name;

	public CmdConfigurationNamespaceAttribute(string name, string description, string[] longDescription = null,
		bool isReadonly = false) {
		IsReadonly = isReadonly;
		Name = name;
		Description = description;
		LongDescription = longDescription;
	}

	public void Load(TypeInfo underlyingType) {
		if (!_loaded) {
			_underlyingType = underlyingType;
			var newValues = new List<CmdConfigurationFieldAttribute>();
			foreach (PropertyOrFieldInfo propertyOrFieldInfo in _underlyingType.DeclaredPropertiesAndFields()) {
				var attribute = propertyOrFieldInfo.MemberInfo.GetCustomAttribute<CmdConfigurationFieldAttribute>();
				if (attribute is null) continue;
				attribute.UnderlyingPropertyOrFieldInfo = propertyOrFieldInfo;
				newValues.Add(attribute);
			}

			ConfigurationFields = newValues.ToArray();
			_loaded = true;
		}
	}
}
}