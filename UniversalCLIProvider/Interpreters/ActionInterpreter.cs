﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using UniversalCLIProvider.Attributes;
using UniversalCLIProvider.Internals;

namespace UniversalCLIProvider.Interpreters {
public class ActionInterpreter : BaseInterpreter {
	private bool _cached;
	public CmdActionAttribute UnderlyingAction;

	public ActionInterpreter(CmdActionAttribute myActionAttribute, BaseInterpreter parent, int offset = 0) : base(parent,
		myActionAttribute.Name, offset) => UnderlyingAction = myActionAttribute;


	internal override bool Interpret() {
		UnderlyingAction.LoadParametersAndAlias();
		if (TopInterpreter.Args.Skip(Offset - 1).Any(x => IsParameterEqual("help", x, "?"))) {
			if (TopInterpreter.Args.Length - 1 == Offset) {
				HelpGenerators.PrintActionHelp(UnderlyingAction, this);
				return true;
			}

			if (TopInterpreter.Args.Length == Offset) {
				IncreaseOffset();
				if (IsParameterDeclaration(out CmdParameterAttribute found, allowPrefixFree: true)) {
					HelpGenerators.PrintParameterHelp(found, this);
					return true;
				}

				CmdParameterAliasAttribute aliasAttribute = UnderlyingAction.Parameters.SelectMany(x => x.ParameterAliases).FirstOrDefault();
				if (aliasAttribute is null) {
					//TODO special error
					HelpGenerators.PrintActionHelp(UnderlyingAction, this);
				}

				HelpGenerators.PrintAliasHelp(aliasAttribute, this);
			}
		}

		//Dictionary<CmdParameterAttribute, object> invocationArguments = new Dictionary<CmdParameterAttribute, object>();
		Dictionary<CmdParameterAttribute, object> invocationArguments;
		if (Offset == TopInterpreter.Args.Length) {
			invocationArguments = new Dictionary<CmdParameterAttribute, object>();
		}
		else {
			if (!GetValues(out invocationArguments)) {
				return false;
				//TODO Error Message
			}
		}

		ParameterInfo[] allParameterInfos = UnderlyingAction.UnderlyingMethod.GetParameters();
		object[] invokers = new object[allParameterInfos.Length];
		bool[] invokersDeclared = new bool[allParameterInfos.Length];
		foreach (KeyValuePair<CmdParameterAttribute, object> invocationArgument in invocationArguments) {
			int position = invocationArgument.Key.UnderlyingParameter.Position;
			invokers[position] = invocationArgument.Value;
			invokersDeclared[position] = true;
		}

		for (int i = 0; i < allParameterInfos.Length; i++) {
			if (!invokersDeclared[i]) {
				if (allParameterInfos[i].HasDefaultValue) {
					invokers[i] = allParameterInfos[i].DefaultValue;
					invokersDeclared[i] = true;
				}
				else {
					//throw
					return false;
				}
			}
		}

		object returned = UnderlyingAction.UnderlyingMethod.Invoke(null, invokers);

		return true;
		//throw new NotImplementedException();
	}

	/// <summary>
	///  reads all arguments
	/// </summary>
	/// <param name="invokationArguments"></param>
	/// <returns></returns>
	private bool GetValues(out Dictionary<CmdParameterAttribute, object> invokationArguments) {
		invokationArguments = new Dictionary<CmdParameterAttribute, object>();
		// value = null;
		while (true) {
			if (IsParameterDeclaration(out CmdParameterAttribute found)) {
				Type iEnumerableCache = null; //Used to cache the IEnumerable base when  found
				if (IncreaseOffset()) {
					//What if Empty Array - Resolved: no empty arrays allowed this way, shall be done by json if anyone thinks this was funny
					return false;
				}

				Type parameterType = found.UnderlyingParameter.ParameterType;
				if (IsAlias(found, out object aliasValue)) {
					invokationArguments.Add(found, aliasValue);
				}
				else if (found.Usage.HasFlag(CmdParameterUsage.SupportDeclaredRaw) &&
					CommandlineMethods.GetValueFromString(TopInterpreter.Args[Offset], parameterType, out object given)) {
					invokationArguments.Add(found, given);
				}
				else if (found.Usage.HasFlag(CmdParameterUsage.SupportDeclaredRaw) && parameterType.GetInterfaces().Any(
						x => (iEnumerableCache = x).IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>)
					)
				) {
					#region Based upon https://stackoverflow.com/a/2493258/6730162 last access 04.03.2018

					Type realType = iEnumerableCache.GetGenericArguments()[0];
					Type specificList = typeof(List<>).MakeGenericType(realType);
					ConstructorInfo ci = specificList.GetConstructor(new Type[] { });
					object listOfRealType = ci.Invoke(new object[] { });

					#endregion

					MethodInfo addMethodInfo = specificList.GetMethod("Add");
					Offset--;
					while (true) {
						if (IncreaseOffset()) {
							break;
						}

						if (IsAlias(out CmdParameterAttribute tmpParameterAttribute, out object _) &&
							tmpParameterAttribute.Usage.HasFlag(CmdParameterUsage.SupportDirectAlias) ||
							IsParameterDeclaration(out CmdParameterAttribute _)) {
							break;
						}

						if (!CommandlineMethods.GetValueFromString(TopInterpreter.Args[Offset], realType, out object toAppend)) {
							//throw
							return false;
						}
						else {
							addMethodInfo.Invoke(listOfRealType, new object[] {toAppend});
						}
					}

					if (new Type[] {
						typeof(List<>), typeof(IList<>), typeof(ICollection<>), typeof(IEnumerable<>), typeof(IReadOnlyList<>),
						typeof(IReadOnlyCollection<>), typeof(ReadOnlyCollection<>)
					}.Select(x => x.MakeGenericType(realType)).Contains(parameterType)) {
						invokationArguments.Add(found, listOfRealType);
					}
					else if (parameterType == realType.MakeArrayType()) {
						object arrayOfRealType = typeof(Enumerable).GetMethod("ToArray").MakeGenericMethod(realType)
							.Invoke(null, new object[] {listOfRealType});
						invokationArguments.Add(found, arrayOfRealType);
					}
					else {
						//Just to provide an open interface for custom types
						ConstructorInfo constructorInfo;
						try {
							constructorInfo =
								parameterType.GetConstructor(new Type[] {typeof(IEnumerable<>).MakeGenericType(realType)});
						}
						catch (Exception e) { //TODO custom
							Console.WriteLine(e);
							throw;
						}

						if (constructorInfo is null) {
							return false;
							//TODO custom error
						}

						constructorInfo.Invoke(new object[] {listOfRealType});
					}
				}

				else {
					//TODO throw
					return false;
				}
			}
			else if (IsAlias(out found, out object aliasValue) && found.Usage.HasFlag(CmdParameterUsage.SupportDirectAlias) ) {
				invokationArguments.Add(found, aliasValue);
			}
			else {
				//TODO throw
				return false;
			}

			if (IncreaseOffset()) {
				return true;
			}
		}
	}

	private bool IsParameterDeclaration(out CmdParameterAttribute found, string search = null, bool allowPrefixFree = false) {
		search = search ?? TopInterpreter.Args[Offset];
		foreach (CmdParameterAttribute cmdParameterAttribute in (IEnumerable<CmdParameterAttribute>) UnderlyingAction.Parameters) {
			if (IsParameterEqual(cmdParameterAttribute.Name, search, cmdParameterAttribute.ShortForm, allowPrefixFree)) {
				found = cmdParameterAttribute;
				return true;
			}
		}

		found = null;
		return false;
	}

//      internal bool IsAlias(CmdParameterAttribute expectedAliasType, out object value, string source = null) {
//         return base.IsAlias(expectedAliasType, out value, source ?? TopInterpreter.Args[Offset]);
//      }

	private bool IsAlias(out CmdParameterAttribute aliasType, out object value, string source = null) {
		foreach (CmdParameterAttribute cmdParameterAttribute in UnderlyingAction.Parameters) {
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