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
		bool showGenericConfigurationInfo = false) {
		if (showGenericConfigurationInfo) {
			ConfigurationGenericHelp(interpreter, Console.WindowWidth, interpreter.TopInterpreter.Options.DefaultIndent);
		}
		ConfigurationNamespaceHelp(namespaceAttribute, Console.WindowWidth, interpreter.TopInterpreter.Options.DefaultIndent);
	}
/// <summary>
/// Prints help for a configurationNamespace
/// </summary>
/// <param name="namespaceAttribute">The configurationNamespace to print help for</param>
/// <param name="width">The width of the console</param>
/// <param name="indent">The indent to use</param>
/// <param name="tw">The Textwriter to write the help to</param>
	public static void ConfigurationNamespaceHelp(CmdConfigurationNamespaceAttribute namespaceAttribute, int width, int indent = 3, TextWriter tw = null) {
		tw = tw ?? Console.Out;
		tw.Write(CommandlineMethods.PadCentered($"{namespaceAttribute.Name}{(namespaceAttribute.IsReadonly ? " (RW)" : "")}", width));
		if (!(namespaceAttribute.LongDescription is null)) {
			foreach (string s in namespaceAttribute.LongDescription) {
				CommandlineMethods.PrintWithPotentialIndent(s, width, 0, tw);
			}
		}

		tw.Write(CommandlineMethods.PadCentered("Values", width));
		foreach (CmdConfigurationFieldAttribute valueAttribute in namespaceAttribute.ConfigurationFields) {
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
	///  Prints help for a configurationField
	/// </summary>
	/// <param name="fieldAttribute">The configurationField to print help for</param>
	/// <param name="interpreter">The interpreter to use</param>
	/// <param name="showGenericConfigurationInfo"></param>
	public static void PrintConfigurationFieldHelp(CmdConfigurationFieldAttribute fieldAttribute, BaseInterpreter interpreter,
		bool showGenericConfigurationInfo = false) {
		if (showGenericConfigurationInfo) {
			ConfigurationGenericHelp(interpreter, Console.WindowWidth, interpreter.TopInterpreter.Options.DefaultIndent);
		}
		ConfigurationFieldHelp(fieldAttribute, Console.WindowWidth, interpreter.TopInterpreter.Options.DefaultIndent);
	}
/// <summary>
/// Prints help about a certain configuration field
/// </summary>
/// <param name="fieldAttribute">The field attribute to print help for</param>
/// <param name="width">The width of the console</param>
/// <param name="indent">The indent to use</param>
/// <param name="tw">The Textwriter to write the help to</param>
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
/// <summary>
/// Prints generic help about the configuration
/// </summary>
/// <param name="interpreter">The current interpreter</param>
/// <param name="width">The width of the console</param>
/// <param name="indent">The indent to use</param>
/// <param name="tw">The Textwriter to write the help to</param>
	public static void ConfigurationGenericHelp(BaseInterpreter interpreter, int width, int indent = 3, TextWriter tw = null) {
		tw = tw ?? Console.Out;
		string indentString = new string(' ',indent);
		tw.Write(CommandlineMethods.PadCentered($"Usage of {interpreter.Name}", width));
		tw.WriteLine($"{interpreter.Path} NameOfValue help");
		tw.WriteLine(indentString+ "Prints help for the field");
		tw.WriteLine($"{interpreter.Path} NameOfValue get");
		tw.WriteLine(indentString+ "Reads the field");
		tw.WriteLine($"{interpreter.Path} NameOfValue set NewValue");
		tw.WriteLine(indentString+ "Sets the field to the value provided");
		//TODO Add add&remove when implemented
	}
}
}