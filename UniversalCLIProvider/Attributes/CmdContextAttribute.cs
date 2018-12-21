using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UniversalCLIProvider.Internals;

namespace UniversalCLIProvider.Attributes {
[AttributeUsage(AttributeTargets.Class), UsedImplicitly]
public class CmdContextAttribute : Attribute {
	private bool _loaded;
	public IList<CmdActionAttribute> ctxActions = new List<CmdActionAttribute>();
	public IList<CmdParameterAttribute> ctxParameters = new List<CmdParameterAttribute>();
	public ContextDefaultAction DefaultAction;
	public string Description;

	public string[] LongDescription;

	[CanBeNull] public string Name;

	[CanBeNull] public string ShortForm;

	public IList<CmdContextAttribute> subCtx;
	public TypeInfo UnderlyingType;

	private static readonly char[] IllegalCharactersInName = {':', '=', ' ', '\'', '\"'};

	public CmdContextAttribute([NotNull] string name, string description=null, string[] longDescription=null, string[] usageExamples=null, string shortForm=null) {
#if DEBUG
		
		if (string.IsNullOrWhiteSpace(name)) {
			throw new InvalidCLIConfigurationException("A name is required for any context",new ArgumentException("Value cannot be null or whitespace.", nameof(name))); 
		}

		if (name.IndexOfAny(IllegalCharactersInName)!=-1) {
			throw new InvalidCLIConfigurationException("The name contains at least one illegal character ",new ArgumentException("Illegal name",nameof(name))); 
		}
#endif


		Name = name;
		if (longDescription is null&&!(description is null)) {
			longDescription=new string[]{description};
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
		subCtx= new List<CmdContextAttribute>();
		foreach (TypeInfo nestedSubcontext in UnderlyingType.DeclaredNestedTypes) {
			CmdContextAttribute contextAttribute = nestedSubcontext.GetCustomAttribute<CmdContextAttribute>();
			if (contextAttribute != null) {
				contextAttribute.UnderlyingType = nestedSubcontext;
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
		}
	}
}
}