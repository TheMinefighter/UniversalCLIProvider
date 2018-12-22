using JetBrains.Annotations;
using UniversalCLIProvider.Internals;

namespace UniversalCLIProvider {
public class InterpretingOptions {
	public int DefaultIndent = 3;

	public string HexOption = "Master:Hex";

	public bool IgnoreParameterCase = true;
	public string InteractiveOption = "Master:Interactive";

	public string PreferredArgumentPrefix = "/";

	[NotNull] public ContextDefaultAction StandardDefaultAction = ContextDefaultAction.PrintHelp();
}
}