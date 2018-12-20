using UniversalCLIProvider.Interpreters;

namespace ReferenceUsage {
internal class Program {
	public static void Main(string[] args) {
		new CommandlineOptionInterpreter(args).Interpret<CmdRootContext>();
	}
}
}