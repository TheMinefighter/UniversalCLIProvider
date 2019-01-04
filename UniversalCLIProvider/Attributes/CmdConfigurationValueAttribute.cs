using System;
using JetBrains.Annotations;
using PropertyOrFieldInfoPackage;

namespace UniversalCLIProvider.Attributes {
[AttributeUsage(AttributeTargets.Property|AttributeTargets.Field), UsedImplicitly]
public class CmdConfigurationValueAttribute : Attribute {
	public string[] LongDescription;
	public string Description;
	public bool IsReadonly;
	public string Name;
	internal PropertyOrFieldInfo UnderlyingPropertyOrFieldInfo;

	public CmdConfigurationValueAttribute(string name, string description = null, string[] longDescription = null, bool isReadonly = false) {
		Name = name;
		IsReadonly = isReadonly;
		Description = description;
		LongDescription = longDescription;
	}
}
}