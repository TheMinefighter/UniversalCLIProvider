using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace UniversalCLIProvider.Attributes {
[AttributeUsage(AttributeTargets.Method), UsedImplicitly]
public class CmdActionAttribute : Attribute {
	private bool _cached;
	public string Description;
	public string[] LongDescription;
	public string Name;
	public List<CmdParameterAttribute> Parameters;
	public MethodInfo UnderlyingMethod;
	public string[] UsageExamples;

	public CmdActionAttribute(string name) => Name = name;

	public void LoadParametersAndAlias() {
		if (!_cached) {
			Parameters = new List<CmdParameterAttribute>();
			foreach (ParameterInfo parameterInfo in UnderlyingMethod.GetParameters()) {
				foreach (CmdParameterAttribute parameterAttribute in parameterInfo
					.GetCustomAttributes(typeof(CmdParameterAttribute), false)
					.Cast<CmdParameterAttribute>()) {
					parameterAttribute.UnderlyingParameter = parameterInfo;
					parameterAttribute.LoadAlias();
					Parameters.Add(parameterAttribute);
				}
			}

			_cached = true;
		}
	}
}
}