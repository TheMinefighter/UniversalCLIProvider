using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UniversalCLIProvider.Internals;
using UniversalCLIProvider.Attributes;
using UniversalCLIProvider.Interpreters;

namespace UniversalCLIProvider.Attributes {
[AttributeUsage(AttributeTargets.Class), UsedImplicitly]
public class CmdContextAttribute : Attribute {
	private static readonly char[] IllegalCharactersInName = {':', '=', ' ', '\'', '\"'};
	private bool _loaded;
	public IList<CmdActionAttribute> ctxActions = new List<CmdActionAttribute>();
	public IList<CmdParameterAttribute> ctxParameters = new List<CmdParameterAttribute>();
	public ContextDefaultAction DefaultAction;
	[NotNull] public string Description;

	[CanBeNull] public string[] LongDescription;

	[NotNull] public string Name;

	[CanBeNull] public string ShortForm;

	public IList<CmdContextAttribute> subCtx;
	public TypeInfo UnderlyingType;

	public CmdContextAttribute([NotNull] string name, string description = null, string[] longDescription = null, string[] usageExamples = null,
		string shortForm = null, ContextDefaultActionPreset defaultActionPreset = ContextDefaultActionPreset.Help) {
#if DEBUG

		if (string.IsNullOrWhiteSpace(name)) {
			throw new InvalidCLIConfigurationException("A name is required for any context",
				new ArgumentException("Value cannot be null or whitespace.", nameof(name)));
		}

		if (name.IndexOfAny(IllegalCharactersInName) != -1) {
			throw new InvalidCLIConfigurationException("The name contains at least one illegal character ",
				new ArgumentException("Illegal name", nameof(name)));
		}
#endif
		switch (defaultActionPreset) {
			case ContextDefaultActionPreset.Help:
				DefaultAction = ContextDefaultAction.PrintHelp();
				break;
			case ContextDefaultActionPreset.Exit:
				DefaultAction = ContextDefaultAction.Exit();
				break;
			case ContextDefaultActionPreset.Interactive:
				DefaultAction = ContextDefaultAction.InteractiveInterpreter();
				break;
			default:
				throw new InvalidCLIConfigurationException("Invalid default action preset provided",
					new ArgumentOutOfRangeException(nameof(defaultActionPreset), defaultActionPreset, null));
		}

		Name = name;
		if (longDescription is null && !(description is null)) {
			longDescription = new string[] {description};
		}

		LongDescription = longDescription;
		Description = description;
		ShortForm = shortForm;
	}

	public void LoadIfNot() {
		if (!_loaded) {
			Load();

			_loaded = true;
		}
	}

	public void Load() {
		subCtx = new List<CmdContextAttribute>();
		foreach (TypeInfo nestedSubcontext in UnderlyingType.DeclaredNestedTypes) {
			CmdContextAttribute contextAttribute = nestedSubcontext.GetCustomAttribute<CmdContextAttribute>();
			if (contextAttribute != null) {
				contextAttribute.UnderlyingType = nestedSubcontext;
				contextAttribute.DefaultAction = this.DefaultAction;
				subCtx.Add(contextAttribute);
			}
		}

		foreach (MemberInfo memberInfo in UnderlyingType.DeclaredFields.Cast<MemberInfo>().Concat(UnderlyingType.DeclaredProperties)) {
			CmdParameterAttribute parameterAttribute = memberInfo.GetCustomAttribute<CmdParameterAttribute>();
			if (parameterAttribute != null) {
				parameterAttribute.UnderlyingParameter = memberInfo;
				ctxParameters.Add(parameterAttribute);
			}
		}

		foreach (MethodInfo methodInfo in UnderlyingType.DeclaredMethods) {
			CmdActionAttribute actionAttribute = methodInfo.GetCustomAttribute<CmdActionAttribute>();
			if (actionAttribute != null) {
				actionAttribute.UnderlyingMethod = methodInfo;
				ctxActions.Add(actionAttribute);
			}

			CmdDefaultActionAttribute defaultActionAttribute = methodInfo.GetCustomAttribute<CmdDefaultActionAttribute>();
			if (defaultActionAttribute != null) {
				if (methodInfo.GetParameters().Length == defaultActionAttribute.Parameters.Length) {
					this.DefaultAction = new ContextDefaultAction(() => methodInfo.Invoke(null, defaultActionAttribute.Parameters));
				}
				else if (methodInfo.GetParameters().Length == defaultActionAttribute.Parameters.Length + 1) {
#if DEBUG
					if (!methodInfo.GetParameters().Last().ParameterType.IsAssignableFrom(typeof(ContextInterpreter))) {
						throw new InvalidCLIConfigurationException($"In the type {UnderlyingType.FullName} you have set the method {methodInfo.Name}" +
							" to be the context's default action, but if the method takes one more parameter than provided by the attribute," +
							" the type of the last method parameter must be Assignable from typeof(ContextInterpreter)");
					}
					
#endif
					this.DefaultAction =
						new ContextDefaultAction(interpreter: x => methodInfo.Invoke(null, defaultActionAttribute.Parameters.Append(x).ToArray()));
				}
				else {
					throw new InvalidCLIConfigurationException($"In the type {UnderlyingType.FullName} you have set the method {methodInfo.Name}" +
						$" to be the context's default action, in the defining attribute you have supplied {defaultActionAttribute.Parameters.Length} parameters" +
						" but that count must be the count of parameters accepted by the method or on less. Neither was the case.");
				}
			}
		}
	}

	public enum ContextDefaultActionPreset : byte {
		Help,
		Exit,
		Interactive
	}
}
}