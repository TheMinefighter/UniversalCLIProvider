using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UniversalCLIProvider.Attributes;
using UniversalCLIProvider.Internals;

namespace UniversalCLIProvider.Interpreters {
public class ManagedConfigurationInterpreter : BaseInterpreter {
	private string _configurationRootName;
	private string[] _contextTrace;
	private Dictionary<CmdConfigurationNamespaceAttribute, MemberInfo> _namespaces;
	private CmdConfigurationNamespaceAttribute _root;
	private Dictionary<CmdConfigurationValueAttribute, MemberInfo> _values;

	protected ManagedConfigurationInterpreter(CommandlineOptionInterpreter top, CmdConfigurationNamespaceAttribute root, int offset = 0) : base(top,
		offset) {
		_root = root; }

	internal override bool Interpret(bool printErrors = true) {
		if (Offset == TopInterpreter.Args.Length || IsParameterEqual("help", TopInterpreter.Args[Offset], "?")) {
			HelpGenerators.PrintConfigurationContextHelp(_root,this);
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

		//_root.Interpret(printErrors);
		throw new NotImplementedException();
		return true;
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
Decision has fallen, old namespace based interpretation wil be dropped-> done
*/