using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using UniversalCLIProvider.Attributes;
using UniversalCLIProvider.Internals;

namespace UniversalCLIProvider.Interpreters {
public class ContextInterpreter : BaseInterpreter {
	[NotNull] internal CmdContextAttribute UnderlyingContextAttribute;

	internal ContextInterpreter([NotNull] CommandlineOptionInterpreter top,
		[NotNull] CmdContextAttribute underlyingContextAttribute, int offset = 0) :
		base(top, offset) =>
		UnderlyingContextAttribute = underlyingContextAttribute;

	internal ContextInterpreter([NotNull] BaseInterpreter parent,
		[NotNull] CmdContextAttribute attribute, int offset = 0) :
		base(parent, attribute.Name, offset) =>
		UnderlyingContextAttribute = attribute;

	/// <summary>
	/// Starts the interactive interpretation of this context
	/// </summary>
	/// <param name="interpretOn">whether there are further arguments that need to be interpreted as a command in the interactive shell</param>
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
				Console.Write(string.Join(TopInterpreter.Options.InteractiveSubPathSeparator, currentContextInterpreter.Path.Select(x => x.Name)) + ">");
				string readLine = Console.ReadLine();
				if (readLine is null) {
					continue;
				}
				List<string> arguments = new List<string>();
				var lastStringBuilder = new StringBuilder();
				bool quoting = false;
				foreach (char c in readLine) { //TODO Might want to add support for backslashed quotes
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
			if (currentContextInterpreter.Interpret(out ContextInterpreter tmpContextInterpreter, true)) {
				currentContextInterpreter = tmpContextInterpreter;
			}
		}
	}

	internal bool Interpret(out ContextInterpreter newCtx, bool interactive = false) {
		if (interactive) {
			if (TopInterpreter.Args[Offset] == "..") {
				if (Parent == null) {
					Environment.Exit(0);
				}
				else {
					var parentInterpreter = Parent as ContextInterpreter;
					parentInterpreter.Reset();
					newCtx = parentInterpreter;
					return true;
				}
			}
		}

		UnderlyingContextAttribute.LoadIfNot();
		newCtx = null;
		if (TopInterpreter.Args.Length == 0) {
			UnderlyingContextAttribute.DefaultAction.Interpret(this);
			return true;
		}

		string search = TopInterpreter.Args[Offset];
		foreach (CmdContextAttribute cmdContextAttribute in UnderlyingContextAttribute.SubCtx) {
			if (CommandlineMethods.IsParameterEqual(cmdContextAttribute.Name, search, interactive)) {
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

		foreach (CmdConfigurationProviderAttribute provider in UnderlyingContextAttribute.CfgProviders) {
			if (IsParameterEqual(provider.Name, search, provider.ShortForm, true)) {
				var cfgInterpreter = new ConfigurationInterpreter(TopInterpreter, provider.Root, provider.UnderlyingPropertyOrField.GetValue(null),
					provider.UnderlyingPropertyOrField.ValueType.GetTypeInfo());
				return cfgInterpreter.Interpret();
			}
		}
		/*foreach (CmdParameterAttribute cmdParameterAttribute in UnderlyingContextAttribute.CtxParameters) {
			//TODO Implement this
		}*/

		UnderlyingContextAttribute.DefaultAction.Interpret(this);
		return false;
	}

	internal override bool Interpret() => Interpret(out ContextInterpreter _);
}
}