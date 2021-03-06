using System;
using System.IO;
using JetBrains.Annotations;
using UniversalCLIProvider.Attributes;
using UniversalCLIProvider.Interpreters;

namespace UniversalCLIProvider.Internals {
/// <summary>
///  Methods generating --help texts
/// </summary>
public static partial class HelpGenerators {
	/// <summary>
	///  Prints help for an action
	/// </summary>
	/// <param name="action">The action to print help for</param>
	/// <param name="interpreter">The interpreter to use</param>
	public static void PrintActionHelp(CmdActionAttribute action, BaseInterpreter interpreter) =>
		ActionHelp(action, Console.WindowWidth, interpreter.TopInterpreter.Options.DefaultIndent);

	/// <summary>
	///  Writes the help for a action to a textwriter
	/// </summary>
	/// <param name="action">The action to provide help</param>
	/// <param name="width">The width of the console</param>
	/// <param name="indent">The indent to use for splitted lines</param>
	/// <param name="tw">The textwriter to output to (defaults to <see cref="Console.Out" /></param>
	private static void ActionHelp([NotNull] CmdActionAttribute action, int width, int indent = 3, TextWriter tw = null) {
		tw = tw ?? Console.Out;
		action.LoadParametersAndAlias();
		tw.Write(CommandlineMethods.PadCentered(action.Name, width));
		if (!(action.LongDescription is null)) {
			foreach (string s in action.LongDescription) {
				CommandlineMethods.PrintWithPotentialIndent(s, width, 0, tw);
			}
		}

		if (action.Parameters.Count != 0) {
			tw.Write(CommandlineMethods.PadCentered("Parameters", width));
		}

		foreach (CmdParameterAttribute parameter in action.Parameters) {
			if (parameter.Usage.HasFlag(CmdParameterUsage.SupportDeclaredRaw)) {
				if (parameter.Description is null) {
					tw.WriteLine((parameter.ShortForm is null ? "--" : "-" + parameter.ShortForm + " | --") + parameter.Name);
				}
				else {
					CommandlineMethods.PrintWithPotentialIndent(
						$"{(parameter.ShortForm is null ? "" : "-" + parameter.ShortForm + " | ")}--{parameter.Name}: {parameter.Description}",
						width, indent, tw);
				}
			}

//TODO Differentiate directs
			if (parameter.Usage.HasFlag(CmdParameterUsage.SupportDirectAlias) ||
				parameter.Usage.HasFlag(CmdParameterUsage.SupportDeclaredAlias)) {
				foreach (CmdParameterAliasAttribute alias in parameter.ParameterAliases) {
					if (alias.Description is null) {
						tw.WriteLine((alias.ShortForm is null ? "--" : "-" + alias.ShortForm + " | --") + alias.Name);
					}
					else {
						CommandlineMethods.PrintWithPotentialIndent(
							$"{(alias.ShortForm is null ? "" : "-" + alias.ShortForm + " | ")}--{alias.Name}: {alias.Description}",
							width, indent, tw);
					}
				}
			}
		}

		if (!(action.UsageExamples is null)) {
			tw.Write(CommandlineMethods.PadCentered("Examples", width));
			foreach (string s in action.UsageExamples) {
				CommandlineMethods.PrintWithPotentialIndent(s, width, indent, tw);
			}
		}
	}

	/// <summary>
	///  Prints help for a context
	/// </summary>
	/// <param name="context">The context to print help</param>
	/// <param name="interpreter">The interpreter to use</param>
	public static void PrintContextHelp(CmdContextAttribute context, BaseInterpreter interpreter) =>
		ContextHelp(context, Console.WindowWidth, interpreter.TopInterpreter.Options.DefaultIndent);

	/// <summary>
	///  Writes the help for a context to a textwriter
	/// </summary>
	/// <param name="context">The context to provide help</param>
	/// <param name="width">The width of the console</param>
	/// <param name="indent">The indent to use for splitted lines</param>
	/// <param name="tw">The textwriter to output to (defaults to <see cref="Console.Out" /></param>
	private static void ContextHelp([NotNull] CmdContextAttribute context, int width, int indent = 3, TextWriter tw = null) {
		tw = tw ?? Console.Out;
		context.Load();
		tw.Write(CommandlineMethods.PadCentered(context.Name, width));
		if (context.LongDescription is null) {
			if (!(context.Description is null)) {
				CommandlineMethods.PrintWithPotentialIndent(context.Description, width, indent, tw);
			}
		}
		else {
			foreach (string s in context.LongDescription) {
				CommandlineMethods.PrintWithPotentialIndent(s, width, 0, tw);
			}
		}

		if (context.SubCtx.Count != 0) {
			tw.Write(CommandlineMethods.PadCentered("Contexts", width));
			foreach (CmdContextAttribute subCtx in context.SubCtx) {
				CommandlineMethods.PrintWithPotentialIndent(
					$"{(subCtx.ShortForm is null ? "" : "-" + subCtx.ShortForm + " | ")}--{subCtx.Name}{(subCtx.Description is null ? "" : $": {subCtx.Description}")}",
					width, indent, tw);
			}
		}

		if (context.CtxActions.Count != 0) {
			tw.Write(CommandlineMethods.PadCentered("Actions", width));
			foreach (CmdActionAttribute action in context.CtxActions) {
				CommandlineMethods.PrintWithPotentialIndent(
					$"{(action.ShortForm is null ? "" : $"-{action.ShortForm} | ")}--{action.Name}{(action.Description is null ? "" : $": {action.Description}")}",
					width, indent, tw);
			}
		}
	}

	public static void ParameterHelp(CmdParameterAttribute parameter, int width, int indent = 3, TextWriter tw = null) {
		tw = tw ?? Console.Out;
		if (parameter.Description is null) {
			tw.WriteLine((parameter.ShortForm is null ? "--" : "-" + parameter.ShortForm + " | --") + parameter.Name);
		}
		else {
			CommandlineMethods.PrintWithPotentialIndent(
				$"{(parameter.ShortForm is null ? "" : "-" + parameter.ShortForm + " | ")}--{parameter.Name}: {parameter.Description}",
				width, indent, tw);
		}
	}


	/// <summary>
	///  Prints help for an parameter
	/// </summary>
	/// <param name="parameter">The parameter to print help for </param>
	/// <param name="interpreter">The interpreter to use</param>
	public static void PrintParameterHelp(CmdParameterAttribute parameter, BaseInterpreter interpreter) =>
		ParameterHelp(parameter, Console.WindowWidth, interpreter.TopInterpreter.Options.DefaultIndent);

	public static void AliasHelp(CmdParameterAliasAttribute alias, int width, int indent = 3, TextWriter tw = null) {
		tw = tw ?? Console.Out;
		if (alias.Description is null) {
			tw.WriteLine((alias.ShortForm is null ? "--" : "-" + alias.ShortForm + " | --") + alias.Name);
		}
		else {
			CommandlineMethods.PrintWithPotentialIndent(
				$"{(alias.ShortForm is null ? "" : "-" + alias.ShortForm + " | ")}--{alias.Name}: {alias.Description}",
				width, indent, tw);
		}
	}


	/// <summary>
	///  Prints help for an fieldAttribute
	/// </summary>
	/// <param name="alias">The fieldAttribute to print help for</param>
	/// <param name="interpreter">The interpreter to use</param>
	public static void PrintAliasHelp(CmdParameterAliasAttribute alias, BaseInterpreter interpreter) =>
		AliasHelp(alias, Console.WindowWidth, interpreter.TopInterpreter.Options.DefaultIndent);
}
}