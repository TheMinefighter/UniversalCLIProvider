using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UniversalCLIProvider.Attributes;

namespace UniversalCLIProvider.Interpreters {
public class ManagedConfigurationInterpreter : BaseInterpreter {
	private string _configurationRootName;
	private string[] _contextTrace;
	private Dictionary<CmdConfigurationNamespaceAttribute, MemberInfo> _namespaces;
	private CmdConfigurationNamespaceAttribute _root;
	private RootRequirements _rootRequired;
	private Dictionary<CmdConfigurationValueAttribute, MemberInfo> _values;

	protected ManagedConfigurationInterpreter(CommandlineOptionInterpreter top, int offset = 0) : base(top, offset) { }

	protected ManagedConfigurationInterpreter(BaseInterpreter parent, string name, int offset = 0) :
		base(parent, name, offset) { }

	internal void PrintHelp() {
		int maxlength =
			new int[] {_namespaces.Keys.Select(x => x.Description.Length).Max(), _values.Keys.Select(x => x.Help.Length).Max()}.Max() +
			1;
		var ConsoleStack = new StringBuilder(); //TODO replace with textwriter
		Console.WriteLine($"Syntax: {Path} ");
		foreach (CmdConfigurationNamespaceAttribute cmdConfigurationNamespaceAttribute in _namespaces.Keys) {
			//  TopInterpreter.ConsoleIO.WriteLineToConsole
			ConsoleStack.Append(cmdConfigurationNamespaceAttribute.Name.PadRight(maxlength) +
				cmdConfigurationNamespaceAttribute.Description);
			ConsoleStack.Append(Environment.NewLine);
		}

		Console.Write(ConsoleStack.ToString());
		throw new NotImplementedException();
	}

	internal override bool Interpret(bool printErrors = true) {
		if (Offset == TopInterpreter.Args.Length || IsParameterEqual("?", TopInterpreter.Args[Offset], "?")) {
			if (printErrors) {
				PrintHelp();
			}
			else {
				return false;
			}
		}

		_contextTrace = TopInterpreter.Args[Offset].Split('.').Select(x => x.ToLower()).ToArray();
		if (_rootRequired.HasFlag(RootRequirements.RootAllowed)) {
			if (_contextTrace[0].Equals(_configurationRootName, StringComparison.OrdinalIgnoreCase)) {
				Offset++;
			}
			else {
				if (!_rootRequired.HasFlag(RootRequirements.RootFreeAllowed)) {
					Console.WriteLine($"Expected token (\"{_configurationRootName}\") not found");
					return false;
				}
			}
		}

		_root.Interpret(printErrors);
		throw new NotImplementedException();
		return true;
	}

	[Flags]
	private enum RootRequirements : byte {
		RootAllowed = 1 << 0,
		RootFreeAllowed = 1 << 1,
		BothAllowed = RootAllowed | RootFreeAllowed
	}
}
}
/* Rethought the way managed configurations will work here my current Proposal:
Program --config --path PathOfValue --Get
Wil output the value
Program --config --path PathOfValueOrCtx --Help
Wil output the value
Program --config --path PathOfValue --Set NewValue
Sets the value
Program --config --path PathOfValue --Add AdditionalValue
Adds a value to an ICollection
Program --config --path PathOfValue --RemoveAt Index
Removes a value from an ICollection at the given Index
Program --config --path PathOfValue --Remove element
Removes all elements from the ICollection where the element equals the described one 
this might be subject to change

Further Ideas: Remove Root Requirements completely
Quoteless strings will be directly returned
Support single quote string globally
Decision has fallen, old namespace based interpretation wil be dropped
*/