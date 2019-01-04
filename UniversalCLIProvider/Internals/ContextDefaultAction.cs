using System;
using JetBrains.Annotations;
using UniversalCLIProvider.Attributes;
using UniversalCLIProvider.Interpreters;

namespace UniversalCLIProvider.Internals {
public class ContextDefaultAction {
	[NotNull]
	public Action<ContextInterpreter> Interpret { get; }

	public ContextDefaultAction(Action toRun) => Interpret = x => toRun();

	public ContextDefaultAction(Action<CmdContextAttribute> context) => Interpret = x => context(x.UnderlyingContextAttribute);

	public ContextDefaultAction(Action<ContextInterpreter> interpreter) => Interpret = interpreter;
	public static ContextDefaultAction Exit(int exitCode = -1) => new ContextDefaultAction(() => Environment.Exit(exitCode));
	public static ContextDefaultAction PrintHelp() => new ContextDefaultAction(x => HelpGenerators.PrintContextHelp(x.UnderlyingContextAttribute, x));
	public static ContextDefaultAction InteractiveInterpreter() => new ContextDefaultAction(x => x.InteractiveInterpreter());
}
}