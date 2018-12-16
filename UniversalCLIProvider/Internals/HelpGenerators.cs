using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UniversalCLIProvider.Attributes;

namespace UniversalCLIProvider.Internals {
/// <summary>
/// Methods generating --help texts
/// </summary>
public class HelpGenerators {
	List<string> ActionHelp(CmdActionAttribute action, int width, int indent = 3) {
		List<string> ret = new List<string> {CommandlineMethods.PadCentered(action.Name,width)};
		ret.AddRange(action.Description.SelectMany(x=>CommandlineMethods.PrintWithPotentialIndent(x, width, 0)));
		if (action.Parameters.Count!=0) {
			ret.Add(CommandlineMethods.PadCentered("Parameters", indent));
		}
		foreach (CmdParameterAttribute attribute in action.Parameters) {
			if (attribute.Description is null) {
				ret.Add((attribute.ShortForm is null ? "--" :"-"+ attribute.ShortForm + " | --")+ attribute.Name);
			}
			else {
				foreach (string s in CommandlineMethods.PrintWithPotentialIndent($"{(attribute.ShortForm is null ? "" :"-"+ attribute.ShortForm + " | ")}--{attribute.Name}: {attribute.Description}", width, indent)) {
					ret.Add(s);
				}
			}
		}

		if (!(action.UsageExamples is null)) {
			ret.AddRange(action.UsageExamples.SelectMany(x=>CommandlineMethods.PrintWithPotentialIndent(x, width, indent)));
		}

		return ret;
	}
	
	List<string> ContextHelp(CmdContextAttribute context, int width, int indent = 3) {
		List<string> ret = new List<string> {CommandlineMethods.PadCentered(context.Name,width)};
		ret.AddRange(context.LongDescription.SelectMany(x=>CommandlineMethods.PrintWithPotentialIndent(x, width, 0)));
		if (context.subCtx.Count!=0) {
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

		if (context.ctxActions.Count!=0) {
			ret.Add(CommandlineMethods.PadCentered("Actions", indent));
		}

		return ret;
	}
}
}
