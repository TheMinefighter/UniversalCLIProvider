using UniversalCLIProvider.Interpreters;

namespace ReferenceUsage {
internal static class Program {
	public static void Main(string[] args) {
		new CommandlineOptionInterpreter(args).Interpret<CmdRootContext>();
	}
}
}