using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using PropertyOrFieldInfoPackage;
using UniversalCLIProvider.Internals;

namespace UniversalCLIProvider.Attributes {
[AttributeUsage(AttributeTargets.Class), UsedImplicitly]
public class CmdContextAttribute : Attribute {
	/// <summary>
	///  The characters forbidden
	/// </summary>
	private static readonly char[] IllegalCharactersInName = {':', '=', ' ', '\'', '\"'};

	/// <summary>
	///  The short description of the context, only shown in the help of the parent context
	/// </summary>
	/// <remarks>Doing nothing for the root context</remarks>
	[CanBeNull] public readonly string Description;

	/// <summary>
	///  The Long description of the context, each element in the array represents one paragraph, linebreaks within the paragraph will be
	///  created
	///  automatically
	/// </summary>
	[CanBeNull] public readonly string[] LongDescription;

	/// <summary>
	///  The name of the context
	/// </summary>
	[NotNull] public readonly string Name;

	/// <summary>
	///  The shortform of the context, making it callable with <c>-shortForm</c>
	/// </summary>
	[CanBeNull] public readonly string ShortForm;

	/// <summary>
	///  If the context has been loaded with the <see cref="Load" /> function
	/// </summary>
	private bool _loaded;

	/// <summary>
	///  All configuration providers of this context, initialized by the <see cref="Load" /> function
	/// </summary>
	internal IList<CmdConfigurationProviderAttribute> CfgProviders;

	/// <summary>
	///  All context parameters of this context, initialized by the <see cref="Load" /> function
	/// </summary>
	internal IList<CmdActionAttribute> CtxActions;

	/// <summary>
	///  The action to be run when no further Arguments are supplied, can easily be set with the defaultActionPreset constructor
	///  parameter
	///  or the <see cref="CmdDefaultActionAttribute" />
	/// </summary>
	[NotNull] public ContextDefaultAction DefaultAction;

	/// <summary>
	///  All subcontexts of this context, initialized by the <see cref="Load" /> function
	/// </summary>
	public IList<CmdContextAttribute> SubCtx;

	/// <summary>
	///  The owner type of this context
	/// </summary>
	public TypeInfo UnderlyingType;

	/// <summary>
	///  Initializes a new <see cref="CmdContextAttribute" />
	/// </summary>
	/// <param name="name">the name of the newly created context</param>
	/// <param name="description">The description of the context, defaults to null</param>
	/// <param name="longDescription">
	///  The extended description of the context, each item of the array representing a paragraph, defaults
	///  to null
	/// </param>
	/// <param name="shortForm">A shortform for this context</param>
	/// <param name="defaultActionPreset">
	///  The preset for the <see cref="ContextDefaultAction" />, aka the action to run when no further arguments are
	///  supplied
	/// </param>
	/// <exception cref="InvalidCLIConfigurationException">When the CLI is not configured correctly</exception>
	public CmdContextAttribute([NotNull] string name, string description = null, string[] longDescription = null,
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

	/// <summary>
	///  Loads the context if not loaded yet
	/// </summary>
	public void Load() {
		if (!_loaded) {
			SubCtx = new List<CmdContextAttribute>();
			CtxActions = new List<CmdActionAttribute>();
			CfgProviders = new List<CmdConfigurationProviderAttribute>();
			//CtxParameters = new List<CmdParameterAttribute>();
			foreach (TypeInfo nestedSubcontext in UnderlyingType.DeclaredNestedTypes) {
				var contextAttribute = nestedSubcontext.GetCustomAttribute<CmdContextAttribute>();
				if (contextAttribute != null) {
					contextAttribute.UnderlyingType = nestedSubcontext;
					contextAttribute.DefaultAction = DefaultAction;
					SubCtx.Add(contextAttribute);
				}
			}

			foreach (PropertyOrFieldInfo propertyOrField in UnderlyingType.DeclaredPropertiesAndFields()) {
				/*var parameterAttribute = propertyOrField.MemberInfo.GetCustomAttribute<CmdParameterAttribute>();
			if (parameterAttribute != null) {
				parameterAttribute.UnderlyingParameter = propertyOrField;
				CtxParameters.Add(parameterAttribute);
			}*/

				var configurationProviderAttribute =
					propertyOrField.MemberInfo.GetCustomAttribute<CmdConfigurationProviderAttribute>();
				if (!(configurationProviderAttribute is null)) {
					configurationProviderAttribute.UnderlyingPropertyOrField = propertyOrField;
					configurationProviderAttribute.Root =
						configurationProviderAttribute.UnderlyingPropertyOrField.ValueType
							.GetCustomAttribute<CmdConfigurationNamespaceAttribute>();
#if DEBUG
					if (configurationProviderAttribute.Root is null) {
						throw new InvalidCLIConfigurationException(
							$"The type of the property/field \"{propertyOrField.Name}\", which is marked with a CmdConfigurationProviderAttribute, ({configurationProviderAttribute.UnderlyingPropertyOrField.ValueType}) has no CmdConfigurationNamespaceAttribute as required.");
					}
#endif
					CfgProviders.Add(configurationProviderAttribute);
				}
			}

			foreach (MethodInfo methodInfo in UnderlyingType.DeclaredMethods) {
				var actionAttribute = methodInfo.GetCustomAttribute<CmdActionAttribute>();
				if (actionAttribute != null) {
					actionAttribute.UnderlyingMethod = methodInfo;
					CtxActions.Add(actionAttribute);
				}

				var defaultActionAttribute = methodInfo.GetCustomAttribute<CmdDefaultActionAttribute>();
				if (defaultActionAttribute != null) {
					if (methodInfo.GetParameters().Length == defaultActionAttribute.Parameters.Length) {
						DefaultAction = new ContextDefaultAction(() => methodInfo.Invoke(null, defaultActionAttribute.Parameters));
					}
					else if (methodInfo.GetParameters().Length == defaultActionAttribute.Parameters.Length + 1) {
#if DEBUG
						if (!methodInfo.GetParameters().Last().ParameterType.IsAssignableFrom(typeof(ContextInterpreter))) {
							throw new InvalidCLIConfigurationException($"In the type {UnderlyingType.FullName} you have set the method {methodInfo.Name}" +
								" to be the context's default action, but if the method takes one more parameter than provided by the attribute," +
								" the type of the last method parameter must be Assignable from typeof(ContextInterpreter)");
						}

#endif
						DefaultAction =
							new ContextDefaultAction(interpreter: x =>
								methodInfo.Invoke(null, defaultActionAttribute.Parameters.Append(x).ToArray()));
					}
					else {
						throw new InvalidCLIConfigurationException(
							$"In the type {UnderlyingType.FullName} you have set the method {methodInfo.Name}" +
							$" to be the context's default action, in the defining attribute you have supplied {defaultActionAttribute.Parameters.Length} parameters" +
							" but that count must be the count of parameters accepted by the method or on less. Neither was the case.");
					}
				}
			}

			_loaded = true;
		}
	}
}
}