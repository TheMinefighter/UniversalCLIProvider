using System;
using System.IO;
using UniversalCLIProvider.Attributes;
using UniversalCLIProvider.Interpreters;

namespace UniversalCLIProvider.Internals {
public static partial class HelpGenerators {
	/// <summary>
	///  Prints help for an fieldAttribute
	/// </summary>
	/// <param name="namespaceAttribute">The fieldAttribute to print help for</param>
	/// <param name="interpreter">The interpreter to use</param>
	/// <param name="showGenericConfigurationInfo"></param>
	public static void PrintConfigurationContextHelp(CmdConfigurationNamespaceAttribute namespaceAttribute, BaseInterpreter interpreter,
		bool showGenericConfigurationInfo = false) =>
		ConfigurationNamespaceHelp(namespaceAttribute, Console.WindowWidth, interpreter.TopInterpreter.Options.DefaultIndent);

	public static void ConfigurationNamespaceHelp(CmdConfigurationNamespaceAttribute alias, int width, int indent = 3, TextWriter tw = null) {
		tw = tw ?? Console.Out;
		tw.Write(CommandlineMethods.PadCentered($"{alias.Name}{(alias.IsReadonly ? " (RW)" : "")}", width));
		if (!(alias.LongDescription is null)) {
			foreach (string s in alias.LongDescription) {
				CommandlineMethods.PrintWithPotentialIndent(s, width, 0, tw);
			}
		}

		tw.Write(CommandlineMethods.PadCentered("Values", width));
		foreach (CmdConfigurationFieldAttribute valueAttribute in alias.ConfigurationFields) {
			if (valueAttribute.Description is null) {
				tw.WriteLine(valueAttribute.Name);
			}
			else {
				CommandlineMethods.PrintWithPotentialIndent(
					$"{valueAttribute.Name}: {valueAttribute.Description}", width, indent, tw);
			}
		}
	}

	/// <summary>
	///  Prints help for an fieldAttribute
	/// </summary>
	/// <param name="fieldAttribute">The fieldAttribute to print help for</param>
	/// <param name="interpreter">The interpreter to use</param>
	/// <param name="showGenericConfigurationInfo"></param>
	public static void PrintConfigurationFieldHelp(CmdConfigurationFieldAttribute fieldAttribute, BaseInterpreter interpreter,
		bool showGenericConfigurationInfo = false) =>
		ConfigurationFieldHelp(fieldAttribute, Console.WindowWidth, interpreter.TopInterpreter.Options.DefaultIndent);

	public static void ConfigurationFieldHelp(CmdConfigurationFieldAttribute fieldAttribute, int width, int indent = 3, TextWriter tw = null) {
		tw = tw ?? Console.Out;
		tw.Write(CommandlineMethods.PadCentered(
			$"{fieldAttribute.Name} (R{(fieldAttribute.IsReadonly ? "W" : "O")},{fieldAttribute.UnderlyingPropertyOrFieldInfo.ValueType})", width));
		if (fieldAttribute.LongDescription is null) {
			if (fieldAttribute.Description is null) {
				tw.WriteLine("This value has no further description");
			}
			else {
				CommandlineMethods.PrintWithPotentialIndent(fieldAttribute.Description, width, indent, tw);
			}
		}
		else {
			foreach (string s in fieldAttribute.LongDescription) {
				CommandlineMethods.PrintWithPotentialIndent(s, width, 0, tw);
			}
		}
	}

	public static void ConfigurationGenericHelp(int width, BaseInterpreter interpreter, int indent = 3, TextWriter tw = null) {
		tw = tw ?? Console.Out;
		tw.Write(CommandlineMethods.PadCentered($"Usage of {interpreter.Name}", width));
		tw.WriteLine($"{interpreter.Path} NameOfValue help");
		string indentString = new string(' ',indent);
		tw.WriteLine(indentString+ "Prints help for the field");
		tw.WriteLine($"{interpreter.Path} NameOfValue get");
		tw.WriteLine(indentString+ "Reads the field");
		tw.WriteLine($"{interpreter.Path} NameOfValue set NewValue");
		tw.WriteLine(indentString+ "Sets the field to the value provided");
	}
}
}