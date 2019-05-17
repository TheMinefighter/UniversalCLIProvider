using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UniversalCLIProvider.Attributes;
using UniversalCLIProvider.Internals;

namespace UniversalCLIProvider.Interpreters {
/// <summary>
/// Implementation of <see cref="BaseInterpreter"/> for interpreting commandline actions
/// </summary>
public class ActionInterpreter : BaseInterpreter {
	/// <summary>
	/// All collection types that a parameter can have for which a basic lists will work a value
	/// </summary>
	private static readonly Type[] CollectionsCoveredByLists = {
		typeof(List<>), typeof(IList<>), typeof(ICollection<>), typeof(IEnumerable<>), typeof(IReadOnlyList<>),
		typeof(IReadOnlyCollection<>), typeof(ReadOnlyCollection<>)
	};

	/// <summary>
	/// The action that is interpreted by this Interpreter
	/// </summary>
	private readonly CmdActionAttribute _underlyingAction;

	public ActionInterpreter(CmdActionAttribute myActionAttribute, BaseInterpreter parent, int offset = 0) : base(parent,
		myActionAttribute.Name, offset) => _underlyingAction = myActionAttribute;


	/// <inheritdoc />
	internal override void Interpret() {
		_underlyingAction.LoadParametersAndAlias();
		//TODO Investigate mult layer help
		if (TopInterpreter.Args.Skip(Offset - 1).Any(x => IsParameterEqual("help", x, "?"))) {
			if (TopInterpreter.Args.Length - 1 == Offset) {
				HelpGenerators.PrintActionHelp(_underlyingAction, this);
				return;
			}
			else {
				IncreaseOffset();
				if (IsAnyParameterDeclaration(out CmdParameterAttribute found, allowPrefixFree: true)) {
					HelpGenerators.PrintParameterHelp(found, this);
					return;
				}

				CmdParameterAliasAttribute aliasAttribute =
					_underlyingAction.Parameters.SelectMany(x => x.ParameterAliases)
						.FirstOrDefault(x => IsParameterEqual(x.Name, TopInterpreter.Args[Offset], x.ShortForm));
				if (aliasAttribute is null) {
					//TODO special error
					HelpGenerators.PrintActionHelp(_underlyingAction, this);
					return;
				}

				HelpGenerators.PrintAliasHelp(aliasAttribute, this);
			}
		}

		//Dictionary<CmdParameterAttribute, object> invocationArguments = new Dictionary<CmdParameterAttribute, object>();
		Dictionary<CmdParameterAttribute, object> invocationArguments =
			Offset == TopInterpreter.Args.Length ? new Dictionary<CmdParameterAttribute, object>() : GetValues();

		ParameterInfo[] allParameterInfos = _underlyingAction.UnderlyingMethod.GetParameters();
		var invokers = new object[allParameterInfos.Length];
		var invokersDeclared = new bool[allParameterInfos.Length];
		foreach (KeyValuePair<CmdParameterAttribute, object> invocationArgument in invocationArguments) {
			int position = invocationArgument.Key.UnderlyingParameter.Position;
			invokers[position] = invocationArgument.Value;
			invokersDeclared[position] = true;
		}

		for (int i = 0; i < allParameterInfos.Length; i++) {
			if (!invokersDeclared[i]) {
				if (!allParameterInfos[i].HasDefaultValue) {
					throw new CLIUsageException(
						$"The parameter {allParameterInfos[i].Name} of type {allParameterInfos[i].ParameterType} is not defined.");
				}

				invokers[i] = allParameterInfos[i].DefaultValue;
				invokersDeclared[i] = true;
			}
		}

		object returned = _underlyingAction.UnderlyingMethod.Invoke(null, invokers);
	}

	/// <summary>
	/// Reads all following commandline parameters matches them up to action parameters parses them
	/// </summary>
	/// <returns> A <see cref="Dictionary{TKey,TValue}"/> of <see cref="CmdParameterAttribute"/> and their values</returns>
	private Dictionary<CmdParameterAttribute, object> GetValues() {
		var invocationArguments = new Dictionary<CmdParameterAttribute, object>();
		// value = null;
		while (true) {
			if (!IsAnyParameterDeclaration(out CmdParameterAttribute found)) {
				if (IsAnyAlias(out found, out object aliasValue) && found.Usage.HasFlag(CmdParameterUsage.SupportDirectAlias)) {
					invocationArguments.Add(found, aliasValue);
				}
				else {
					throw new CLIUsageException($"Couldn't resolve what {TopInterpreter.Args[Offset]} should be");
				}
			}
			else {
				Type iEnumerableCache = null; //Used to cache the IEnumerable base when  found
				if (IncreaseOffset()) {
					//What if Empty Array - Resolved: no empty arrays allowed this way, shall be done by json if anyone thinks this was funny
					throw new CLIUsageException(
						"After a parameter declaration there shall follow data of the parameter, but in this case the command ended after the parameter declaration");
				}

				Type parameterType = found.UnderlyingParameter.ParameterType;
				if (IsAlias(found, out object aliasValue)) {
					invocationArguments.Add(found, aliasValue);
				}
				else if (found.Usage.HasFlag(CmdParameterUsage.SupportDeclaredRaw) &&
					CommandlineMethods.GetValueFromString(TopInterpreter.Args[Offset], parameterType, out object given)) {
					invocationArguments.Add(found, given);
				}
				else if (found.Usage.HasFlag(CmdParameterUsage.SupportDeclaredRaw) && parameterType.GetInterfaces().Any(
						x => (iEnumerableCache = x).IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>)
					)
				) {
					invocationArguments.Add(found,HandleEnumerableInput(iEnumerableCache, found));
				}

				else {
					throw new CLIUsageException($"The data for the parameter {found.Name} was not formatted properly");
				}
			}

			if (IncreaseOffset()) {
				return invocationArguments;
			}
		}
	}

	/// <summary>
	/// Handles the input for IEnumerables that could not be processed in another way
	/// </summary>
	/// <param name="enumerableTargetType"></param>
	/// <param name="foundParameter"></param>
	/// <returns></returns>
	/// <exception cref="CLIUsageException">If the user provided parameters are invalid for the given function parameter</exception>
	private IEnumerable HandleEnumerableInput(Type enumerableTargetType, CmdParameterAttribute foundParameter) {
		Type parameterType = foundParameter.UnderlyingParameter.ParameterType;

		#region Based upon https://stackoverflow.com/a/2493258/6730162 last access 04.03.2018

		Type realType = enumerableTargetType.GetGenericArguments()[0];
		Type specificList = typeof(List<>).MakeGenericType(realType);
		ConstructorInfo ci = specificList.GetConstructor(new Type[] { });
		var listOfRealType = (IList) ci.Invoke(new object[] { });

		#endregion

		MethodInfo addMethodInfo = specificList.GetMethod("Add");
		Offset--;
		while (true) {
			if (IncreaseOffset()) {
				break;
			}

			if (IsAnyAlias(out CmdParameterAttribute tmpParameterAttribute, out object _) &&
				tmpParameterAttribute.Usage.HasFlag(CmdParameterUsage.SupportDirectAlias) ||
				IsAnyParameterDeclaration(out CmdParameterAttribute _)) {
				break;
			}

			CommandlineMethods.GetValueFromString(TopInterpreter.Args[Offset], realType, out object toAppend);
			Debug.Assert(addMethodInfo != null,
				nameof(addMethodInfo) +
				" != null"); //safe because of the fact that any generic type based on List<> will have an add method
			addMethodInfo.Invoke(listOfRealType, new object[] {toAppend});
		}

		if (CollectionsCoveredByLists.Select(x => x.MakeGenericType(realType)).Contains(parameterType)) {
			return listOfRealType;
		}
		else if (parameterType == realType.MakeArrayType()) {
			MethodInfo baseMethodToArray = typeof(Enumerable).GetMethod("ToArray");
			Debug.Assert(baseMethodToArray != null,
				nameof(baseMethodToArray) + " != null"); // safe because The Type has such method
			var arrayOfRealType = (Array) baseMethodToArray.MakeGenericMethod(realType)
				.Invoke(null, new object[] {listOfRealType});
			return arrayOfRealType;
		}
		else {
			//Just to provide an open interface for custom types

			ConstructorInfo constructorInfo =
				parameterType.GetConstructor(new Type[] {typeof(IEnumerable<>).MakeGenericType(realType)});

			if (constructorInfo is null) {
				throw new CLIUsageException(
					$"The data for the parameter {foundParameter.Name} was not formatted properly, or the collection type could not be initialized");
			}

			return (IEnumerable) constructorInfo.Invoke(new object[] {listOfRealType});
		}
	}

	private bool IsAnyParameterDeclaration(out CmdParameterAttribute found, string search = null, bool allowPrefixFree = false) {
		search = search ?? TopInterpreter.Args[Offset];
		foreach (CmdParameterAttribute cmdParameterAttribute in (IEnumerable<CmdParameterAttribute>) _underlyingAction.Parameters) {
			if (IsParameterEqual(cmdParameterAttribute.Name, search, cmdParameterAttribute.ShortForm, allowPrefixFree)) {
				found = cmdParameterAttribute;
				return true;
			}
		}

		found = null;
		return false;
	}


	private bool IsAnyAlias(out CmdParameterAttribute aliasType, out object value, string source = null) {
		foreach (CmdParameterAttribute cmdParameterAttribute in _underlyingAction.Parameters) {
			if (IsAlias(cmdParameterAttribute, out value, source ?? TopInterpreter.Args[Offset])) {
				aliasType = cmdParameterAttribute;
				return true;
			}
		}

		aliasType = null;
		value = null;
		return false;
	}

	//TODO inline this method
	private bool IsAlias(CmdParameterAttribute expectedAliasType, out object value, string search = null) {
		foreach (CmdParameterAliasAttribute cmdParameterAlias in expectedAliasType.ParameterAliases) {
			if (IsParameterEqual(cmdParameterAlias.Name, search ?? TopInterpreter.Args[Offset], cmdParameterAlias.ShortForm)) {
				value = cmdParameterAlias.Value;
				return true;
			}
		}

		value = null;
		return false;
	}
}
}