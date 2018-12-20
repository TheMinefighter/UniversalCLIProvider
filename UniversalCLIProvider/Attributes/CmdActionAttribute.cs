using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace UniversalCLIProvider.Attributes {
[AttributeUsage(AttributeTargets.Method), UsedImplicitly]
public class CmdActionAttribute : Attribute {
	private bool _cached;
	public readonly string Description;
	public readonly string[] LongDescription;
	public readonly string Name;
	public List<CmdParameterAttribute> Parameters;
	public MethodInfo UnderlyingMethod;
	public readonly string[] UsageExamples;
	public readonly string ShortForm;
	private static readonly char[] IllegalCharactersInName = {':', '=', ' ', '\'', '\"'};

	public CmdActionAttribute([NotNull] string name, string description=null, string[] longDescription=null, string[] usageExamples=null, string shortForm=null) {
#if DEBUG
				if (string.IsNullOrWhiteSpace(name)) {
      			throw new InvalidCLIConfigurationException("A name is required for any action",new ArgumentException("Value cannot be null or whitespace.", nameof(name))); 
      		}

				if (name.IndexOfAny(IllegalCharactersInName)!=-1) {
					throw new InvalidCLIConfigurationException("The name contains at least one illegal character ",new ArgumentException("Illegal name",nameof(name))); 
				}
#endif


		Name = name;
		UsageExamples = usageExamples;
		if (longDescription is null&&!(description is null)) {
			longDescription=new string[]{description};
		}
		LongDescription = longDescription;
		Description = description;
		ShortForm = shortForm;
	}

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