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
	[NotNull]
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
			tw.WriteLine(CommandlineMethods.PadCentered("Examples",width));
			foreach (string s in action.UsageExamples) {
				CommandlineMethods.PrintWithPotentialIndent(s, width, indent, tw);
			}
		}

	}

	[NotNull]
	private static List<string> ContextHelp([NotNull] CmdContextAttribute context, int width, int indent = 3) {
		context.LoadIfNot();
		List<string> ret = new List<string> {CommandlineMethods.PadCentered(context.Name, width)};
		ret.AddRange(context.LongDescription.SelectMany(x => CommandlineMethods.PrintWithPotentialIndent(x, width, 0)));
		if (context.subCtx.Count != 0) {
			ret.Add(CommandlineMethods.PadCentered("Contexts", indent));
		}

		foreach (CmdContextAttribute subCtx in context.subCtx) {
			if (subCtx.Description is null) {
				ret.Add(subCtx.Name);
			}
			else {
				ret.AddRange(CommandlineMethods.PrintWithPotentialIndent($"{subCtx.Name}: {subCtx.Description}", width, indent));
			}
		}

		if (context.ctxActions.Count != 0) {
			ret.Add(CommandlineMethods.PadCentered("Actions", indent));
		}

		foreach (CmdActionAttribute action in context.ctxActions) {
			if (action.LongDescription is null) {
				ret.Add(action.Name);
			}
			else {
				ret.AddRange(CommandlineMethods.PrintWithPotentialIndent($"{action.Name}: {action.Description}", width, indent));
			}
		}

		return ret;
	}

	public static void PrintActionHelp(CmdActionAttribute action, BaseInterpreter interpreter) =>
		ActionHelp(action, Console.WindowWidth, interpreter.TopInterpreter.Options.DefaultIndent).ForEach(ConsoleIO.WriteToMain);
}
}