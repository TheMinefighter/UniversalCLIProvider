using System;
using JetBrains.Annotations;
using PropertyOrFieldInfoPackage;

namespace UniversalCLIProvider.Attributes {
[AttributeUsage(AttributeTargets.Parameter), UsedImplicitly]
public class CmdConfigurationValueAttribute : Attribute {
	public string ExtendedHelp;
	public string Help;
	public bool IsReadonly;
	internal PropertyOrFieldInfo UnderlyingPropertyOrFieldInfo;
	public CmdConfigurationValueAttribute(string help = null, string extendedHelp = null, bool isReadonly = false) {
		IsReadonly = isReadonly;
		Help = help;
		ExtendedHelp = extendedHelp;
	}
}
}