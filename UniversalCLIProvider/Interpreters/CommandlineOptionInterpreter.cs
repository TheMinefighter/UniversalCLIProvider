﻿using System;
using System.Collections.Generic;
using System.Reflection;
using UniversalCLIProvider.Attributes;
using UniversalCLIProvider.Internals;

namespace UniversalCLIProvider.Interpreters {
public class CommandlineOptionInterpreter {
	public string[] Args;
	public InterpretingOptions Options;
	public CmdContextAttribute TopContextAttribute;

	public CommandlineOptionInterpreter(string[] args, InterpretingOptions options = null) {
		Args = args;
		Options = options ?? new InterpretingOptions();
	}

	public void Interpret<T>() {
		Interpret(typeof(T));
	}

	public void Interpret(Type baseContext) {
		TopContextAttribute = baseContext.GetCustomAttribute(typeof(CmdContextAttribute)) as CmdContextAttribute;
		if (TopContextAttribute is null) {
			throw new InvalidCLIConfigurationException("The Type provided has no CmdContextAttribute");
		}

		var contextInterpreter =
			new ContextInterpreter(this, TopContextAttribute)
				{UnderlyingContextAttribute = {UnderlyingType = baseContext.GetTypeInfo()}};

		contextInterpreter.UnderlyingContextAttribute.Load();
		if (Args.Length > 0) {
			if (contextInterpreter.IsParameterEqual(Options.HexOption, Args[0])) {
				HexArgumentEncoding.ArgumentsFromHex(Args[1], out List<string> newArgs);
				Args = newArgs.ToArray();
				Interpret(baseContext);

				return;
			}

			if (contextInterpreter.IsParameterEqual(Options.InteractiveOption, Args[0])) {
				contextInterpreter.IncreaseOffset();
				contextInterpreter.InteractiveInterpreter(contextInterpreter.Offset < Args.Length);
			}
		}

		try {
			contextInterpreter.Interpret();
		}
		catch (CLIUsageException e) {
			Console.WriteLine(e.Message);
			if (e.InnerException != null) {
				Console.WriteLine(e.InnerException);
			}

			throw;
		}
	}
}
}