using UniversalCLIProvider.Interpreters;

namespace ReferenceUsage {
internal static class Program {
	public static void Main(string[] args) => CommandlineOptionInterpreter.Interpret<CmdRootContext>(args);
}
}