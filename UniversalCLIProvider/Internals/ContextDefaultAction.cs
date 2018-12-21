using System;
using UniversalCLIProvider.Attributes;
using UniversalCLIProvider.Interpreters;

namespace UniversalCLIProvider.Internals {
public class ContextDefaultAction {
	private bool direct;
	public Action<ContextInterpreter> Interpret { get; internal set; }
	public static ContextDefaultAction Exit(int exitCode=-1)=> new ContextDefaultAction {Interpret = x=> Environment.Exit(exitCode)};
	public static ContextDefaultAction PrintHelp()=> new ContextDefaultAction {Interpret = x=> HelpGenerators.PrintContextHelp(x.UnderlyingContextAttribute,x)};
	public static ContextDefaultAction InteractiveInterpreter(int exitCode=-1)=> new ContextDefaultAction {Interpret = x=> x.InteractiveInterpreter(true)};
	private ContextDefaultAction() { }

	public ContextDefaultAction(Action toRun) => Interpret = x => toRun();
	
	public ContextDefaultAction(Action<CmdContextAttribute> toRun) => Interpret = x => toRun(x.UnderlyingContextAttribute);
	
	public ContextDefaultAction(Action<ContextInterpreter> toRun) => Interpret = toRun;
	
	public static implicit operator ContextDefaultAction(Action todo) {
		return new ContextDefaultAction {Interpret = x => todo()};
	}
}
}