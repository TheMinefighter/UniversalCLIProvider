using JetBrains.Annotations;

namespace UniversalCLIProvider {
public class InterpretingOptions {
	public int DefaultIndent = 3;

	[NotNull] public string HexOption = "Master:Hex";

	public bool IgnoreParameterCase = true;
	[NotNull] public string InteractiveOption = "Master:Interactive";

	[NotNull] public string PreferredArgumentPrefix = "/";
}
}