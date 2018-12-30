using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniversalCLIProvider.Attributes;
using UniversalCLIProvider.Internals;

namespace UniversalCLIProvider.Interpreters {
public class ContextInterpreter : BaseInterpreter {
	public CmdContextAttribute UnderlyingContextAttribute;

	internal ContextInterpreter(CommandlineOptionInterpreter top, int offset = 0) : base(top, offset) { }

	internal ContextInterpreter(BaseInterpreter parent, CmdContextAttribute attribute, int offset = 0) : base(parent, attribute.Name, offset) =>
		UnderlyingContextAttribute = attribute;

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
				var lastStringBuilder = new StringBuilder();
				bool quoting = false;
				foreach (char c in Console.ReadLine()) { //TODO Might want to add support for backslashed quotes
					switch (c) {
						case '"':
							quoting ^= true;
							break;
						case ' ' when !quoting: {
							string tmpString = lastStringBuilder.ToString();
							if (tmpString != string.Empty) {
								arguments.Add(tmpString);
								lastStringBuilder = new StringBuilder();
							}

							break;
						}
						default:
							lastStringBuilder.Append(c);
							break;
					}
				}

				string lastString = lastStringBuilder.ToString();
				if (lastString != string.Empty) {
					arguments.Add(lastString);
				}

				TopInterpreter.Args = arguments.ToArray();
			}

			currentContextInterpreter.Reset();
			currentContextInterpreter.UnderlyingContextAttribute.LoadIfNot();
			if (currentContextInterpreter.Interpret(out ContextInterpreter tmpContextInterpreter, true)) {
				currentContextInterpreter = tmpContextInterpreter;
			}
		}
	}

	internal bool Interpret(out ContextInterpreter newCtx, bool interactive = false) {
		if (interactive) {
			if (TopInterpreter.Args[Offset] == "..") {
				if (DirectParent == null) {
					Environment.Exit(0);
				}
				else {
					var parentInterpreter = DirectParent as ContextInterpreter;
					parentInterpreter.Reset();
					newCtx = parentInterpreter;
					return true;
				}
			}
		}

		newCtx = null;
		if (TopInterpreter.Args.Length == 0) {
			UnderlyingContextAttribute.DefaultAction.Interpret(this);
		}

		string search = TopInterpreter.Args[Offset];
		foreach (CmdContextAttribute cmdContextAttribute in UnderlyingContextAttribute.SubCtx) {
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

				var subInterpreter = new ContextInterpreter(this, cmdContextAttribute, Offset);
				cmdContextAttribute.LoadIfNot();
				if (interactive) {
					subInterpreter.Interpret(out newCtx, interactive);
					return newCtx != null;
				}
				else {
					subInterpreter.Interpret();
				}

				return true;
			}
		}

		foreach (CmdActionAttribute cmdActionAttribute in UnderlyingContextAttribute.CtxActions) {
			if (IsParameterEqual(cmdActionAttribute.Name, search, allowPrefixFree: true)) {
				IncreaseOffset();
				var actionInterpreter = new ActionInterpreter(cmdActionAttribute, this, Offset);
				if (!actionInterpreter.Interpret()) {
					HelpGenerators.PrintActionHelp(cmdActionAttribute, this);
				}

				newCtx = this;
				return true;
			}
		}

		foreach (CmdParameterAttribute cmdParameterAttribute in UnderlyingContextAttribute.CtxParameters) {
			//TODO Implement this
		}

//TODO if null
		UnderlyingContextAttribute.DefaultAction.Interpret(this);
		return false;
	}

	internal override bool Interpret(bool printErrors = true) => Interpret(out ContextInterpreter _);
}
}