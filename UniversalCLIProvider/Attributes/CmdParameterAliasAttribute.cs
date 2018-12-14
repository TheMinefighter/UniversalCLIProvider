using System;
using JetBrains.Annotations;

namespace UniversalCLIProvider.Attributes {
	[AttributeUsage(AttributeTargets.GenericParameter | AttributeTargets.Parameter | AttributeTargets.Property |
	                AttributeTargets.Field,
		AllowMultiple = true),UsedImplicitly]
	public class CmdParameterAliasAttribute : Attribute {
		public readonly string Name;

		public readonly object Value;
		public string[] ExtendedHelp;
		public string Help;

		public CmdParameterAliasAttribute(string name, object value, string help = "", string[] extendedHelp = null) {
			Name = name;
			Value = value;
			Help = help;
			ExtendedHelp = extendedHelp ?? new string[] { };
		}
	}
}