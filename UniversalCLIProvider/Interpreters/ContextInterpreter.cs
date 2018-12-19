using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniversalCLIProvider.Attributes;
using UniversalCLIProvider.Internals;

namespace UniversalCLIProvider.Interpreters {
public class ContextInterpreter : BaseInterpreter {
	public CmdContextAttribute MyContextAttribute;

	internal ContextInterpreter(CommandlineOptionInterpreter top, int offset = 0) : base(top, offset) { }

	internal ContextInterpreter(BaseInterpreter parent, CmdContextAttribute attribute, int offset = 0) : base(parent,
		attribute.Name,
		offset) => MyContextAttribute = attribute;

	internal override void PrintHelp() { }

	internal void InteractiveInterpreter(bool interpretOn = false) {
		ContextInterpreter currentContextInterpreter = this;
		while (true) {
			if (interpretOn) {
				interpretOn = false;
				List<string> args = TopInterpreter.Args.ToList();
				args.RemoveRange(0, Offset);
				TopInterpreter.Args = args.ToArray();
			}
			else {
				Console.Write(string.Join(" ", currentContextInterpreter.Path) + ">");
				List<string> arguments = new List<string>();
				StringBuilder lastStringBuilder = new StringBuilder();
				bool quoting = false;
				foreach (char c in Console.ReadLine()) {
					if (c == '"') {
						quoting ^= true;
					}
					else if (c == ' ' && !quoting) {
						string tmpString = lastStringBuilder.ToString();
						if (tmpString != string.Empty) {
							arguments.Add(tmpString);
							lastStringBuilder = new StringBuilder();
						}

						//arguments.Add("");
					}
					else {
						lastStringBuilder.Append(c);
					}
				}

				string lastString = lastStringBuilder.ToString();
				if (lastString != string.Empty) {
					arguments.Add(lastString);
					lastStringBuilder = new StringBuilder();
				}

				TopInterpreter.Args = arguments.ToArray();
			}

			currentContextInterpreter.Reset();
			currentContextInterpreter.MyContextAttribute.LoadIfNot();
			if (currentContextInterpreter.Interpret(out ContextInterpreter tmpContextInterpreter, true, true)) {
				currentContextInterpreter = tmpContextInterpreter;
			}
		}
	}

	internal bool Interpret(out ContextInterpreter newCtx, bool printErrors = true, bool interactive = false) {
		if (interactive) {
			if (TopInterpreter.Args[Offset] == "..") {
				if (DirectParent == null) {
					Environment.Exit(0);
				}
				else {
					ContextInterpreter parentInterpreter = DirectParent as ContextInterpreter;
					parentInterpreter.Reset();
					newCtx = parentInterpreter;
					return true;
				}
			}
		}

		newCtx = null;
		if (TopInterpreter.Args.Length==0) {
			if (TopInterpreter.Options.StandardDefaultAction is null) {
				InteractiveInterpreter();//TODO Replace with clean code
			}
			else {
				TopInterpreter.Options.StandardDefaultAction.Interpret(this);
			}
		}
		string search = TopInterpreter.Args[Offset];
		foreach (CmdContextAttribute cmdContextAttribute in MyContextAttribute.subCtx) {
			if (IsParameterEqual(cmdContextAttribute.Name, search, interactive)) {
				if (IncreaseOffset()) {
					if (interactive) {
						Reset();
						newCtx = new ContextInterpreter(this, cmdContextAttribute);
						return true;
					}
					else {
						//throw
						return false;
					}
				}

				ContextInterpreter subInterpreter = new ContextInterpreter(this, cmdContextAttribute, Offset);
				cmdContextAttribute.LoadIfNot();
				if (interactive) {
					subInterpreter.Interpret(out newCtx, printErrors, interactive);
					return newCtx != null;
				}
				else {
					subInterpreter.Interpret();
				}

				return true;
			}
		}

		foreach (CmdActionAttribute cmdActionAttribute in MyContextAttribute.ctxActions) {
			if (IsParameterEqual(cmdActionAttribute.Name, search, true)) {
				IncreaseOffset();
				ActionInterpreter actionInterpreter = new ActionInterpreter(cmdActionAttribute, this, Offset);
				if (!actionInterpreter.Interpret()) {
					HelpGenerators.PrintActionHelp(cmdActionAttribute,this);
				}
				newCtx = this;
				return true;
			}
		}

		foreach (CmdParameterAttribute cmdParameterAttribute in MyContextAttribute.ctxParameters) {
			//TODO Implement this
		}
//TODO if null
		MyContextAttribute.DefaultAction.Interpret(this);
		return false;
	}

	internal override bool Interpret(bool printErrors = true) => Interpret(out ContextInterpreter _, printErrors, false);
}
}