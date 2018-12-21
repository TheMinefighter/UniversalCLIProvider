using System;
using JetBrains.Annotations;
using UniversalCLIProvider.Attributes;
using UniversalCLIProvider.Interpreters;

namespace UniversalCLIProvider.Internals {
public class ContextDefaultAction {
	[NotNull]
	public Action<ContextInterpreter> Interpret { get; internal set; }
	public static ContextDefaultAction Exit(int exitCode=-1)=> new ContextDefaultAction(()=> Environment.Exit(exitCode));
	public static ContextDefaultAction PrintHelp()=> new ContextDefaultAction(x=> HelpGenerators.PrintContextHelp(x.UnderlyingContextAttribute,x));
	public static ContextDefaultAction InteractiveInterpreter(int exitCode=-1)=> new ContextDefaultAction(x=> x.InteractiveInterpreter(true));

	public ContextDefaultAction(Action toRun) => Interpret = x => toRun();
	
	public ContextDefaultAction(Action<CmdContextAttribute> toRun) => Interpret = x => toRun(x.UnderlyingContextAttribute);
	
	public ContextDefaultAction(Action<ContextInterpreter> toRun) => Interpret = toRun;
}
}