using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using UniversalCLIProvider.Attributes;
using UniversalCLIProvider.Interpreters;

namespace UniversalCLIProvider.Internals {
/// <summary>
///  Methods generating --description texts
/// </summary>
public static class HelpGenerators {
	private static void ActionHelp([NotNull] CmdActionAttribute action, int width, int indent = 3,TextWriter tw=null) {
		tw=tw ?? Console.Out;
		action.LoadParametersAndAlias();
		StringBuilder helpBuilder= new StringBuilder();
		helpBuilder.Append(CommandlineMethods.PadCentered(action.Name, width));
		if (!(action.LongDescription is null)) {
			foreach (string s in action.LongDescription) {
				CommandlineMethods.PrintWithPotentialIndent(s, width, 0, tw);
			}
		}

		if (action.Parameters.Count != 0) {
			tw.Write(CommandlineMethods.PadCentered("Parameters", width));
		}

		foreach (CmdParameterAttribute parameter in action.Parameters) {
			if (parameter.Usage.HasFlag(CmdParameterUsage.SupportRaw)) {
				if (parameter.Description is null) {
					tw.WriteLine((parameter.ShortForm is null ? "--" : "-" + parameter.ShortForm + " | --") + parameter.Name);
				}
				else {
					CommandlineMethods.PrintWithPotentialIndent(
						$"{(parameter.ShortForm is null ? "" : "-" + parameter.ShortForm + " | ")}--{parameter.Name}: {parameter.Description}",
						width, indent,tw);
				}
			}

			if (parameter.Usage.HasFlag(CmdParameterUsage.SupportDirectAlias) || parameter.Usage.HasFlag(CmdParameterUsage.SupportDeclaredAlias)) {
				foreach (CmdParameterAliasAttribute alias in parameter.ParameterAliases) {
					if (alias.Description is null) {
						tw.WriteLine((alias.ShortForm is null ? "--" : "-" + alias.ShortForm + " | --") + alias.Name);
					}
					else {
						CommandlineMethods.PrintWithPotentialIndent(
							$"{(alias.ShortForm is null ? "" : "-" + alias.ShortForm + " | ")}--{alias.Name}: {alias.Description}",
							width, indent,tw);
					}
				}
			}
		}

		if (!(action.UsageExamples is null)) {
			tw.Write(CommandlineMethods.PadCentered("Examples",width));
			foreach (string s in action.UsageExamples) {
				CommandlineMethods.PrintWithPotentialIndent(s, width, indent, tw);
			}
		}

	}

	private static void ContextHelp([NotNull] CmdContextAttribute context, int width, int indent = 3, TextWriter tw = null) {
		tw=tw ?? Console.Out;
		context.LoadIfNot();
		tw.WriteLine(CommandlineMethods.PadCentered(context.Name, width));
		if (context.subCtx.Count != 0) {		if (!(context.LongDescription is null)) {
				foreach (string s in context.LongDescription) {
					CommandlineMethods.PrintWithPotentialIndent(s, width, 0, tw);
				}
			}
			tw.WriteLine(CommandlineMethods.PadCentered("Contexts", indent));
		}

		foreach (CmdContextAttribute subCtx in context.subCtx) {
			if (subCtx.Description is null) {
				tw.WriteLine(subCtx.Name);
			}
			else {
				CommandlineMethods.PrintWithPotentialIndent($"{subCtx.Name}: {subCtx.Description}", width, indent,tw);
			}
		}

		if (context.ctxActions.Count != 0) {
			tw.WriteLine(CommandlineMethods.PadCentered("Actions", indent));
		}

		foreach (CmdActionAttribute action in context.ctxActions) {
			if (action.LongDescription is null) {
				tw.WriteLine(action.Name);
			}
			else {
				CommandlineMethods.PrintWithPotentialIndent($"{action.Name}: {action.Description}", width, indent,tw);
			}
		}
	}

	public static void PrintActionHelp(CmdActionAttribute action, BaseInterpreter interpreter) =>
		ActionHelp(action, Console.WindowWidth, interpreter.TopInterpreter.Options.DefaultIndent);
	
	public static void PrintContextHelp(CmdContextAttribute context, BaseInterpreter interpreter) =>
		ContextHelp(context, Console.WindowWidth, interpreter.TopInterpreter.Options.DefaultIndent);
}
}