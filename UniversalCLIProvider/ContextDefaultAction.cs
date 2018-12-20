using System;
using UniversalCLIProvider.Interpreters;

namespace UniversalCLIProvider {
public class ContextDefaultAction {
	private bool direct;
	public Action<ContextInterpreter> Interpret { get; internal set; }

	internal ContextDefaultAction() { }

	public static implicit operator ContextDefaultAction(ContextAction action) {
		ContextDefaultAction ret = new ContextDefaultAction();
		switch (action) {
			case ContextAction.PrintHelp:
				ret.Interpret = x => x.PrintHelp();
				break;
			case ContextAction.Exit:
				ret.Interpret = x => Environment.Exit(-1);
				break;
			case ContextAction.Interactive:
				ret.Interpret = x => x.InteractiveInterpreter(true);
				break;
			case ContextAction.PrintError:
				ret.Interpret = x => x.PrintError();
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(action), action, null);
		}

		return ret;
	}

	public static implicit operator ContextDefaultAction(Action todo) {
		return new ContextDefaultAction {Interpret = x => todo()};
	}
}
}