using JetBrains.Annotations;

namespace UniversalCLIProvider {
public class InterpretingOptions {
	/// <summary>
	/// The indent to use after making a linebreak within a paragraph
	/// </summary>
	public int DefaultIndent = 3;

	/// <summary>
	/// The option to indicate that hexadecimal encoded arguments follow
	/// </summary>
	/// <remarks>For further information see the <see cref="UniversalCLIProvider.Internals.HexArgumentEncoding"/></remarks>
	[NotNull] public string HexOption = "Master:Hex";

	/// <summary>
	/// Whether the case of parameters shall be ignored
	/// </summary>
	public bool IgnoreParameterCase = true;

	/// <summary>
	/// The option for entering the interactive shell
	/// </summary>
	[NotNull] public string InteractiveOption = "Master:Interactive";
}
}