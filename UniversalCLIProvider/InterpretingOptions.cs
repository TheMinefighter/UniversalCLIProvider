using JetBrains.Annotations;

namespace UniversalCLIProvider {
/// <summary>
/// Some really basic options for CL interpretation
/// </summary>
public class InterpretingOptions {
	/// <summary>
	/// Whether the case of parameters shall be ignored
	/// </summary>
	public bool IgnoreParameterCase = true;

	/// <summary>
	/// The indent to use after making a linebreak within a paragraph
	/// </summary>
	public int DefaultIndent = 3;

	/// <summary>
	/// The option to indicate that hexadecimal encoded arguments follow, use null to disable this
	/// </summary>
	/// <remarks>For further information see the <see cref="UniversalCLIProvider.Internals.HexArgumentEncoding"/></remarks>
	[CanBeNull] public string HexOption = "Master:Hex";

	/// <summary>
	/// The option for entering the interactive shell, use null to disable this
	/// </summary>
	[CanBeNull] public string InteractiveOption = "Master:Interactive";
}
}