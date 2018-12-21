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
	private ConfigurationNamespaceInterpreter _root;
	private RootRequirements _rootRequired;
	private Dictionary<CmdConfigurationValueAttribute, MemberInfo> _values;

	protected ManagedConfigurationInterpreter(CommandlineOptionInterpreter top, int offset = 0) : base(top, offset) { }

	protected ManagedConfigurationInterpreter(BaseInterpreter parent, string name, int offset = 0) :
		base(parent, name, offset) { }

	internal override void PrintHelp() {
		int maxlength =
			new int[] {_namespaces.Keys.Select(x => x.Help.Length).Max(), _values.Keys.Select(x => x.Help.Length).Max()}.Max() +
			1;
		StringBuilder ConsoleStack = new StringBuilder(); //TODO replace with textwriter
		Console.WriteLine($"Syntax: {Path} ");
		foreach (CmdConfigurationNamespaceAttribute cmdConfigurationNamespaceAttribute in _namespaces.Keys) {
			//  TopInterpreter.ConsoleIO.WriteLineToConsole
			ConsoleStack.Append(cmdConfigurationNamespaceAttribute.Name.PadRight(maxlength) +
			                    cmdConfigurationNamespaceAttribute.Help);
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

		_root.Interpret();
		throw new NotImplementedException();
		return true;
	}

	[Flags]
	private enum RootRequirements : byte {
		RootAllowed = 1 << 0,
		RootFreeAllowed = 1 << 1,
		AllAllowed = RootAllowed | RootFreeAllowed
	}
}
}