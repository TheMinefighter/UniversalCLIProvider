using System;

namespace UniversalCLIOptionProvider.Attributes {
	[AttributeUsage(AttributeTargets.Parameter)]
	public class CmdConfigurationValueAttribute : Attribute {
		public string ExtendedHelp;
		public string Help;
		public bool IsReadonly;

		public CmdConfigurationValueAttribute(string help = null, string extendedHelp = null, bool isReadonly = false) {
			IsReadonly = isReadonly;
			Help = help;
			ExtendedHelp = extendedHelp;
		}
	}
}