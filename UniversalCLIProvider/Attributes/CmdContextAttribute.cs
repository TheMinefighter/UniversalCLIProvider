using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

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

	[CanBeNull] public string ShortName;

	public IList<CmdContextAttribute> subCtx = new List<CmdContextAttribute>();
	public TypeInfo UnderlyingType;

	public CmdContextAttribute(string name) {
		DefaultAction = ContextAction.PrintHelp;
		Name = name;
	}

	public CmdContextAttribute() { }


	public void LoadIfNot() {
		if (!_loaded) {
			Load();

			_loaded = true;
		}
	}

	internal void Load() {
		foreach (TypeInfo myInfoDeclaredNestedType in UnderlyingType.DeclaredNestedTypes) {
			CmdContextAttribute contextAttribute = myInfoDeclaredNestedType.GetCustomAttribute<CmdContextAttribute>();
			if (contextAttribute != null) {
				contextAttribute.UnderlyingType = myInfoDeclaredNestedType;
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