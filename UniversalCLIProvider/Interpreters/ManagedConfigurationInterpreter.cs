using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using PropertyOrFieldInfoPackage;
using UniversalCLIProvider.Attributes;
using UniversalCLIProvider.Internals;

namespace UniversalCLIProvider.Interpreters {
public class ManagedConfigurationInterpreter : BaseInterpreter {
	private readonly CmdConfigurationNamespaceAttribute _root;
	private readonly object _referenceToObject;
	private readonly TypeInfo _typeInfoOfConfiguration;

	public ManagedConfigurationInterpreter(CommandlineOptionInterpreter top, CmdConfigurationNamespaceAttribute root, object referenceToObject,
		TypeInfo typeInfoOfConfiguration, int offset = 0) : base(top,
		offset) {
		_root = root;
		_referenceToObject = referenceToObject;
		_typeInfoOfConfiguration = typeInfoOfConfiguration;
	}

	internal override bool Interpret(bool printErrors = true) {
		if (Offset + 1 >= TopInterpreter.Args.Length || IsParameterEqual("help", TopInterpreter.Args[Offset], "?")) {
			HelpGenerators.PrintConfigurationContextHelp(_root, this, true);
		}

		bool ro = true;
		IncreaseOffset();
		object requiredObject= _referenceToObject;
		if (!ConfigurationHelpers.ResolvePathRecursive(TopInterpreter.Args[Offset], _typeInfoOfConfiguration, ref requiredObject, out PropertyInfo prop,
			out object[] indexers, ref ro, out PropertyInfo lastNonIndexer)) {
			return false;
		}

		IncreaseOffset();
		string Operator = TopInterpreter.Args[Offset];
		if (IsParameterEqual("Help", Operator, allowPrefixFree: true)) {
			var contextAttribute = lastNonIndexer.PropertyType.GetCustomAttribute<CmdConfigurationNamespaceAttribute>();
			if (contextAttribute is null) {
				var valueAttribute = lastNonIndexer.GetCustomAttribute<CmdConfigurationFieldAttribute>();
				HelpGenerators.PrintConfigurationFieldHelp(valueAttribute, this);
				return true;
			}
			else {
				HelpGenerators.PrintConfigurationContextHelp(contextAttribute, this);
			}
		}

		if (IsParameterEqual("Get", Operator, allowPrefixFree: true)) {
			object currentValue;
			try {
				currentValue = indexers is null ? prop.GetValue(requiredObject) : prop.GetValue(requiredObject, indexers);
			}
			catch (Exception e) {
				Console.WriteLine("An error occurred while obtaining the value requested:"); //Err
				Console.WriteLine(e);
				return false;
			}

			Console.WriteLine(JsonConvert.SerializeObject(currentValue));
			return true;
		}
		if (IsParameterEqual("Set", Operator, allowPrefixFree: true)) {
			var valueAttribute = prop.GetCustomAttribute<CmdConfigurationFieldAttribute>();
			if (ro||!prop.CanWrite) {
				Console.WriteLine("The given value is not writable");//Err
			}

			if (IncreaseOffset()) {
				Console.WriteLine("Please supply a value to set the given value to!");//Err
			}

			if (!CommandlineMethods.GetValueFromString(TopInterpreter.Args[Offset],prop.PropertyType, out object newValue)) {
				Console.WriteLine($"The given string couldn't be parsed to {prop.PropertyType}!");//Err
				return false;
			}
			try {
				if (indexers is null) {
					prop.SetValue(requiredObject,newValue);
				}
				else {
					prop.SetValue(requiredObject, newValue, indexers);
				}
			}
			catch (Exception e) {
				Console.WriteLine("An error occurred while writing the value:"); //Err
				Console.WriteLine(e);
				return false;
			}

			if (_referenceToObject is IConfigurationRoot iCfgRoot) {
				iCfgRoot.Save(Enumerable.Repeat(new PropertyOrFieldInfo(lastNonIndexer), 1));
			}
			return true;
			
		}
		Console.WriteLine("Could not resolve the operator provided");
		//TODO Remove and Add missing
		//_root.Interpret(printErrors);
		throw new NotImplementedException();
		return true;
	}
}
}
/* Rethought the way managed configurations will work here my current Proposal:
Program --config --path PathOfValue --Get
Wil output the value of the field
Program --config --path PathOfValueOrCtx --Help
Wil output help fo the the field
Program --config --path PathOfValue --Set NewValue
Sets the field to the given value
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