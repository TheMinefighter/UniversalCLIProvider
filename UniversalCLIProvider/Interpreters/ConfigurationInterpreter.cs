using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using PropertyOrFieldInfoPackage;
using UniversalCLIProvider.Attributes;
using UniversalCLIProvider.Internals;

namespace UniversalCLIProvider.Interpreters {
public class ConfigurationInterpreter : BaseInterpreter {
	private readonly object _referenceToObject;
	private readonly CmdConfigurationNamespaceAttribute _root;
	private readonly TypeInfo _typeInfoOfConfiguration;

	public ConfigurationInterpreter(CommandlineOptionInterpreter top, CmdConfigurationNamespaceAttribute root,
		object referenceToObject,
		TypeInfo typeInfoOfConfiguration, int offset = 0) : base(top, root.Name,
		offset) {
		_root = root;
		_referenceToObject = referenceToObject;
		_typeInfoOfConfiguration = typeInfoOfConfiguration;
	}

	internal override void Interpret() {
		if (Offset + 1 >= TopInterpreter.Args.Length || IsParameterEqual("help", TopInterpreter.Args[Offset + 1], "?")) {
			_root.Load(_typeInfoOfConfiguration);
			HelpGenerators.PrintConfigurationContextHelp(_root, this, true);
			return;
		}

		bool ro = false;
		IncreaseOffset();
		object requiredObject = _referenceToObject;
		(PropertyInfo prop, object[] indexers, PropertyInfo lastNonIndexer) =
			ConfigurationHelpers.ResolvePathRecursive(TopInterpreter.Args[Offset], _typeInfoOfConfiguration, ref requiredObject,
				ref ro);

		IncreaseOffset();
		string Operator = TopInterpreter.Args[Offset];
		if (IsParameterEqual("Help", Operator, allowPrefixFree: true)) {
			var contextAttribute = lastNonIndexer.PropertyType.GetCustomAttribute<CmdConfigurationNamespaceAttribute>();
			if (contextAttribute is null) {
				var valueAttribute = lastNonIndexer.GetCustomAttribute<CmdConfigurationFieldAttribute>();
				valueAttribute.Load(new PropertyOrFieldInfo(lastNonIndexer));
				HelpGenerators.PrintConfigurationFieldHelp(valueAttribute, this);
				return;
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
				throw new CLIUsageException("An error occurred while obtaining the value requested:", e);
			}

			Console.WriteLine(JsonConvert.SerializeObject(currentValue));
			return;
		}

		if (IsParameterEqual("Set", Operator, allowPrefixFree: true)) {
			var valueAttribute = prop.GetCustomAttribute<CmdConfigurationFieldAttribute>();
			if (ro || !prop.CanWrite) {
				throw new CLIUsageException("The given property is read only");
			}

			if (IncreaseOffset()) {
				throw new CLIUsageException("Please supply a value to set the given value to!");
			}

			object newValue = CommandlineMethods.GetValueFromString(TopInterpreter.Args[Offset], prop.PropertyType);

			try {
				if (indexers is null) {
					prop.SetValue(requiredObject, newValue);
				}
				else {
					prop.SetValue(requiredObject, newValue, indexers);
				}
			}
			catch (Exception e) {
				throw new CLIUsageException("An error occurred while writing the value:", e);
			}

			if (_referenceToObject is IConfigurationRoot iCfgRoot) {
				iCfgRoot.Save(Enumerable.Repeat(new PropertyOrFieldInfo(lastNonIndexer), 1));
			}

			return;
		}

		if (IsParameterEqual("RemoveAt", Operator, allowPrefixFree: true)) {
			var valueAttribute = prop.GetCustomAttribute<CmdConfigurationFieldAttribute>();
			if (ro || !prop.CanWrite) {
				throw new CLIUsageException("The given value is not writable");
			}

			if (!typeof(ICollection).IsAssignableFrom(prop.PropertyType)) {
				throw new CLIUsageException("The object that you try to remove an element from is no collection.");
			}

			if (IncreaseOffset()) {
				throw new CLIUsageException("Please supply a value to set the given value to!");
			}

			int removalIndex = CommandlineMethods.GetValueFromString<int>(TopInterpreter.Args[Offset]);	

			try {
				((IList) (indexers is null ? prop.GetValue(requiredObject) : prop.GetValue(requiredObject, indexers)))
					.RemoveAt(removalIndex); //Safe due to previous assignability check
			}
			catch (Exception e) {
				throw new CLIUsageException("An error occurred while removing an object:", e);
			}
		}

		throw new CLIUsageException("Could not resolve the operator provided");
		//TODO Remove and Add missing
		//_root.Interpret(printErrors);
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