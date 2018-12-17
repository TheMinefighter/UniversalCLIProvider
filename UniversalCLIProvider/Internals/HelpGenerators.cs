using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UniversalCLIProvider.Attributes;

namespace UniversalCLIProvider.Internals {
/// <summary>
///  Methods generating --description texts
/// </summary>
public class HelpGenerators {
	[NotNull]
	private List<string> ActionHelp([NotNull] CmdActionAttribute action, int width, int indent = 3) {
		action.LoadParametersAndAlias();
		List<string> ret = new List<string> {CommandlineMethods.PadCentered(action.Name, width)};
		ret.AddRange(action.LongDescription.SelectMany(x => CommandlineMethods.PrintWithPotentialIndent(x, width, 0)));
		if (action.Parameters.Count != 0) {
			ret.Add(CommandlineMethods.PadCentered("Parameters", indent));
		}

		foreach (CmdParameterAttribute parameter in action.Parameters) {
			if (parameter.Usage.HasFlag(CmdParameterUsage.SupportRaw)) {
				if (parameter.Description is null) {
					ret.Add((parameter.ShortForm is null ? "--" : "-" + parameter.ShortForm + " | --") + parameter.Name);
				}
				else {
					ret.AddRange(CommandlineMethods.PrintWithPotentialIndent(
						$"{(parameter.ShortForm is null ? "" : "-" + parameter.ShortForm + " | ")}--{parameter.Name}: {parameter.Description}",
						width, indent));
				}
			}

			if (parameter.Usage.HasFlag(CmdParameterUsage.SupportDirectAlias) || parameter.Usage.HasFlag(CmdParameterUsage.SupportDeclaredAlias)) {
				foreach (CmdParameterAliasAttribute alias in parameter.ParameterAliases) {
					if (alias.Description is null) {
						ret.Add((alias.ShortForm is null ? "--" : "-" + alias.ShortForm + " | --") + alias.Name);
					}
					else {
						ret.AddRange(CommandlineMethods.PrintWithPotentialIndent(
							$"{(alias.ShortForm is null ? "" : "-" + alias.ShortForm + " | ")}--{alias.Name}: {alias.Description}",
							width, indent));
					}
				}
			}
		}

		if (!(action.UsageExamples is null)) {
			ret.AddRange(action.UsageExamples.SelectMany(x => CommandlineMethods.PrintWithPotentialIndent(x, width, indent)));
		}

		return ret;
	}

	[NotNull]
	private List<string> ContextHelp([NotNull] CmdContextAttribute context, int width, int indent = 3) {
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
				ret.AddRange(CommandlineMethods.PrintWithPotentialIndent($"{action.Name}: {action.LongDescription}", width, indent));
			}
		}

		return ret;
	}
}
}