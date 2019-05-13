using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UniversalCLIProvider.Attributes;
using UniversalCLIProvider.Internals;

namespace UniversalCLIProvider.Interpreters {
public class ContextInterpreter : BaseInterpreter {
	[NotNull] internal CmdContextAttribute UnderlyingContextAttribute;

	internal ContextInterpreter([NotNull] CommandlineOptionInterpreter top,
		[NotNull] CmdContextAttribute underlyingContextAttribute, int offset = 0) :
		base(top, underlyingContextAttribute.Name, offset) =>
		UnderlyingContextAttribute = underlyingContextAttribute;

	internal ContextInterpreter([NotNull] BaseInterpreter parent,
		[NotNull] CmdContextAttribute attribute, int offset = 0) :
		base(parent, attribute.Name, offset) =>
		UnderlyingContextAttribute = attribute;

	/// <summary>
	///  Starts the interactive interpretation of this context
	/// </summary>
	/// <param name="interpretOn">whether there are further arguments that need to be interpreted as a command in the interactive shell</param>
	internal void InteractiveInterpreter(bool interpretOn = false) {
		ContextInterpreter currentContextInterpreter = this;
		while (true) {
			if (interpretOn) {
				interpretOn = false;
				TopInterpreter.Args = TopInterpreter.Args.Skip(Offset).ToArray();
			}
			else {
				Console.Write(string.Join(TopInterpreter.Options.InteractiveSubPathSeparator,
					currentContextInterpreter.Path.Select(x => x.Name)) + ">");
				string readLine = Console.ReadLine();
				if (readLine is null) {
					continue;
				}

				TopInterpreter.Args = CommandlineMethods.ArgumentsFromString(readLine).ToArray();
			}

			currentContextInterpreter.Reset();
			ContextInterpreter tmpContextInterpreter = null;
			try {
				currentContextInterpreter.Interpret(out tmpContextInterpreter, true);
			}
			catch (CLIUsageException e) {
				Console.WriteLine(e.Message);
				if (e.InnerException != null) {
					Console.WriteLine(e.InnerException);
				}
			}

			currentContextInterpreter = tmpContextInterpreter;
		}
	}

	/// <summary>
	///  Makes this Interpreter do it's work
	/// </summary>
	/// <param name="newCtx">The new context for interactive interpretation</param>
	/// <param name="interactive">Whether the interpretation comes from an interactive interpreter</param>
	/// <returns>Whether the interpretation was successful</returns>
	internal void Interpret(out ContextInterpreter newCtx, bool interactive = false) {
		newCtx = null;
		if (Offset >= TopInterpreter.Args.Length) {
			if (interactive) {
				newCtx = this;
				Reset();
				return;
			}

			UnderlyingContextAttribute.Load();
			UnderlyingContextAttribute.DefaultAction.Interpret(this);
			return;
		}

		if (interactive && TopInterpreter.Args[Offset] == "..") {
			if (Parent == null) {
				Environment.Exit(0);
			}
			else {
				var parentInterpreter = (ContextInterpreter) Parent;
				parentInterpreter.Reset();
				newCtx = parentInterpreter;
				return;
			}
		}

		string search = TopInterpreter.Args[Offset];
		if (IsParameterEqual("help", search, "?")) {
			HelpGenerators.PrintContextHelp(UnderlyingContextAttribute, this);
			return;
		}

		UnderlyingContextAttribute.Load();
		foreach (CmdContextAttribute cmdContextAttribute in UnderlyingContextAttribute.SubCtx) {
			if (IsParameterEqual(cmdContextAttribute.Name, search, cmdContextAttribute.ShortForm, interactive)) {
				if (IncreaseOffset() && interactive) {
					newCtx = new ContextInterpreter(this, cmdContextAttribute);
					return;
				}

				var subInterpreter = new ContextInterpreter(this, cmdContextAttribute, Offset);
				if (interactive) {
					subInterpreter.Interpret(out newCtx, true);
				}
				else {
					subInterpreter.Interpret();
				}

				return;
			}
		}

		foreach (CmdActionAttribute cmdActionAttribute in UnderlyingContextAttribute.CtxActions) {
			if (IsParameterEqual(cmdActionAttribute.Name, search, cmdActionAttribute.ShortForm, true)) {
				IncreaseOffset();
				var actionInterpreter = new ActionInterpreter(cmdActionAttribute, this, Offset);
				actionInterpreter.Interpret();

				newCtx = this;
				return;
			}
		}

		foreach (CmdConfigurationProviderAttribute provider in UnderlyingContextAttribute.CfgProviders) {
			if (IsParameterEqual(provider.Name, search, provider.ShortForm, true)) {
				var cfgInterpreter = new ConfigurationInterpreter(TopInterpreter, provider.Root,
					provider.UnderlyingPropertyOrField.GetValue(null),
					provider.UnderlyingPropertyOrField.ValueType.GetTypeInfo());
				cfgInterpreter.Interpret();
				return;
			}
		}

		throw new CLIUsageException($"Failed to Parse Argument Nr {Offset + 1}: \"{search}\"");
	}

	internal override void Interpret() => Interpret(out ContextInterpreter _);
}
}