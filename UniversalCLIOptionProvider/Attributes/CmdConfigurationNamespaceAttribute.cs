using System;
using System.Reflection;

namespace UniversalCLIOptionProvider.Attributes {
	[AttributeUsage(AttributeTargets.Class)]
	public class CmdConfigurationNamespaceAttribute : Attribute {
		private CmdConfigurationValueAttribute[] _configurationValues;
		private bool _loaded;
		private CmdConfigurationNamespaceAttribute[] _namespaceAttributes;
		public string ExtendedHelp;
		public string Help;
		public bool IsReadonly;
		public string Name;
		private TypeInfo tp;

		public CmdConfigurationNamespaceAttribute(bool isReadonly, string name, string help, string extendedHelp) {
			IsReadonly = isReadonly;
			Name = name;
			Help = help;
			ExtendedHelp = extendedHelp;
		}

		private void Load() {
			ParameterInfo[] parameterInfo = tp.;
		}
	}
}