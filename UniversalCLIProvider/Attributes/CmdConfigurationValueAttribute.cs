using System;
using JetBrains.Annotations;
using PropertyOrFieldInfoPackage;

namespace UniversalCLIProvider.Attributes {
[AttributeUsage(AttributeTargets.Parameter), UsedImplicitly]
public class CmdConfigurationValueAttribute : Attribute {
	public string[] LongDescription;
	public string Description;
	public bool IsReadonly;
	public string Name;
	internal PropertyOrFieldInfo UnderlyingPropertyOrFieldInfo;

	public CmdConfigurationValueAttribute(string description = null, string[] longDescription = null, bool isReadonly = false, string name=null) {
		Name = name;
		IsReadonly = isReadonly;
		Description = description;
		LongDescription = longDescription;
	}
}
}