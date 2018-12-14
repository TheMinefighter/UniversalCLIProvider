using System.Collections.Generic;

namespace UniversalCLIProvider {
	public class CommandlineParameterAlias {
		public IEnumerable<string> extendedHelp;
		public string Help;

		public string Name;

		public object Value;

		public CommandlineParameterAlias(string name, object value, string help = "", IEnumerable<string> extendedHelp = null) {
			Name = name;
			Value = value;
			Help = help;
			this.extendedHelp = extendedHelp ?? new string[0];
		}
	}
}