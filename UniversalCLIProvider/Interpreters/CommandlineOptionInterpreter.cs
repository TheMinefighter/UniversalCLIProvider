using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using UniversalCLIProvider.Attributes;
using UniversalCLIProvider.Internals;

namespace UniversalCLIProvider.Interpreters {
/// <summary>
/// Manages the initial steps of every interpretation workloads
/// and contains the public interfaces for starting interpretations
/// </summary>
public class CommandlineOptionInterpreter {
	/// <summary>
	/// The options to be used for interpretation tasks run by this Interpreter
	/// </summary>
	public readonly InterpretingOptions Options;

	/// <summary>
	/// The <see cref="CmdContextAttribute"/> of the class that is used as initial context
	/// </summary>
	private CmdContextAttribute _topContextAttribute;

	/// <summary>
	/// The arguments to be interpreted
	/// </summary>
	public string[] Args;

	public CommandlineOptionInterpreter(string[] args, InterpretingOptions options = null) {
		Args = args;
		Options = options ?? new InterpretingOptions();
	}

	/// <summary>
	/// Shortcut of <see cref="Interpret"/> for one time interpretation use
	/// </summary>
	/// <param name="args">The arguments to interpret</param>
	/// <param name="options">optional options for interpretation</param>
	/// <typeparam name="T">The type to use as initial context</typeparam>
	/// <remarks>
	/// <code>CommandlineOptionInterpreter.Interpret&lt;T&gt;(args,options);</code>
	/// is a shortcut of
	/// <code>new CommandlineOptionInterpreter(args,options).Interpret(typeof(T));</code>
	/// </remarks>
	[UsedImplicitly]
	public static void Interpret<T>(string[] args, InterpretingOptions options = null) =>
		new CommandlineOptionInterpreter(args, options).Interpret(typeof(T));


	/// <summary>
	/// Starts the Interpretation of commandline arguments
	/// </summary>
	/// <param name="baseContext">The type which shall be used as initial context, which shall have a CmdContextAttribute</param>
	/// <exception cref="InvalidCLIConfigurationException">Thrown if the CLI is not defined according to the specification</exception>
	[UsedImplicitly]
	public void Interpret(Type baseContext) {
		_topContextAttribute = baseContext.GetCustomAttribute(typeof(CmdContextAttribute)) as CmdContextAttribute;
		if (_topContextAttribute is null) {
			throw new InvalidCLIConfigurationException("The Type provided has no CmdContextAttribute");
		}

		var contextInterpreter =
			new ContextInterpreter(this, _topContextAttribute)
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