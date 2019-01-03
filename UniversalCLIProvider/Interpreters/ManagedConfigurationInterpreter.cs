using System;
using System.Reflection;
using Newtonsoft.Json;
using UniversalCLIProvider.Attributes;
using UniversalCLIProvider.Internals;

namespace UniversalCLIProvider.Interpreters {
public class ManagedConfigurationInterpreter : BaseInterpreter {
	private readonly CmdConfigurationNamespaceAttribute _root;
	private readonly object _referenceToObject;
	private readonly TypeInfo _typeInfo;

	protected ManagedConfigurationInterpreter(CommandlineOptionInterpreter top, CmdConfigurationNamespaceAttribute root, object referenceToObject,
		TypeInfo typeInfo, int offset = 0) : base(top,
		offset) {
		_root = root;
		_referenceToObject = referenceToObject;
		_typeInfo = typeInfo;
	}

	internal override bool Interpret(bool printErrors = true) {
		if (Offset + 1 < TopInterpreter.Args.Length || IsParameterEqual("help", TopInterpreter.Args[Offset], "?")) {
			HelpGenerators.PrintConfigurationContextHelp(_root, this, true);
		}

		IncreaseOffset();
		object requiredObject;
		if (!ManagedConfigurationHelpers.ResolvePathRecursive(TopInterpreter.Args[Offset], _typeInfo, ref requiredObject, out PropertyInfo prop,
			out object[] indexers, out PropertyInfo lastNonIndexer)) {
			return false;
		}

		IncreaseOffset();
		string Operator = TopInterpreter.Args[Offset];
		if (IsParameterEqual("Help", Operator, allowPrefixFree: true)) {
			var attribute = lastNonIndexer.GetCustomAttribute<CmdConfigurationValueAttribute>();
			HelpGenerators.PrintConfigurationValueHelp(attribute, this);
			return true;
		}

		if (IsParameterEqual("Get", Operator, allowPrefixFree: true)) {
			object val = null;
			try {
				val = indexers is null ? prop.GetValue(requiredObject) : prop.GetValue(requiredObject, indexers);
			}
			catch (Exception e) {
				Console.WriteLine("An error occurred while obtaining the value requested:"); //Err
				Console.WriteLine(e);
			}

			Console.WriteLine(JsonConvert.SerializeObject(val));
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