namespace UniversalCLIProvider {
public class InterpretingOptions {
	public static InterpretingOptions DefaultOptions = new InterpretingOptions {
		IgnoreParameterCase = true,
		PreferredArgumentPrefix = '/'
	};

	public string HexOption = "Master:Hex";

	public bool IgnoreParameterCase = true;
	public string InteractiveOption = "Master:Interactive";

	public char PreferredArgumentPrefix = '/';
	public string RootName = ".";
	public int DefaultIndent=3;

	public ContextDefaultAction StandardDefaultAction;
}
}