using UniversalCLIProvider.Internals;

namespace UniversalCLIProvider {
public class InterpretingOptions {
	public static InterpretingOptions DefaultOptions = new InterpretingOptions {
		IgnoreParameterCase = true,
		PreferredArgumentPrefix = '/'
	};

	public int DefaultIndent = 3;

	public string HexOption = "Master:Hex";

	public bool IgnoreParameterCase = true;
	public string InteractiveOption = "Master:Interactive";

	public char PreferredArgumentPrefix = '/';

	public ContextDefaultAction StandardDefaultAction;
}
}