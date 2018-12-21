using System;
using System.Collections.Generic;
using System.Reflection;
using UniversalCLIProvider.Attributes;
using UniversalCLIProvider.Internals;

namespace UniversalCLIProvider.Interpreters {
public class CommandlineOptionInterpreter {
	public string[] Args;

	//    internal int ArgsLengthMinus1; 
	public InterpretingOptions Options;

	public CmdContextAttribute TopContext;

	public CommandlineOptionInterpreter(string[] args, InterpretingOptions options = null) {
		Args = args;
		Options = options ?? new InterpretingOptions();
	}

	public void Interpret<T>(Action defaultAction = null) {
		Interpret(typeof(T), defaultAction);
	}

	public bool HexadecimalPreprocessor() {
		string arg = Args[1];
		if (!HexArgumentEncoding.ArgumentsFromHex(arg, out List<string> newArgs)) {
			return false;
		}

		Args = newArgs.ToArray();
		return true;
	}

	public void Interpret(Type baseContext, Action defaultAction = null) {
		if (Args.Length == 0 && defaultAction != null) {
			defaultAction();
		}

		else {
			TopContext = baseContext.GetCustomAttribute(typeof(CmdContextAttribute)) as CmdContextAttribute;
			if (TopContext is null) {
				throw new InvalidCLIConfigurationException("The Type provided has no CmdContextAttribute");
			}

			ContextInterpreter contextInterpreter = new ContextInterpreter(this) {
				UnderlyingContextAttribute =
					TopContext,
				Offset = 0
			};

			contextInterpreter.UnderlyingContextAttribute.UnderlyingType = baseContext.GetTypeInfo();
			contextInterpreter.UnderlyingContextAttribute.LoadIfNot();
			if (Args.Length > 0) {
				if (contextInterpreter.IsParameterEqual(Options.HexOption, Args[0])) {
					if (HexadecimalPreprocessor()) {
						Interpret(baseContext, defaultAction);
					}

					return;
				}

				if (contextInterpreter.IsParameterEqual(Options.InteractiveOption, Args[0])) {
					contextInterpreter.IncreaseOffset();
					contextInterpreter.InteractiveInterpreter(contextInterpreter.Offset < Args.Length);
				}
			}

			contextInterpreter.Interpret();
		}
	}
}
}