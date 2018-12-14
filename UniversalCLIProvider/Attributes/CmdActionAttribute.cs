using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace UniversalCLIProvider.Attributes {
	[AttributeUsage(AttributeTargets.Method), UsedImplicitly]
	public class CmdActionAttribute : Attribute {
		private bool _cached;
		public MethodInfo UnderlyingMethod;
		public string Name;
		public string[] Description;
		public string[] UsageExamples;

		public CmdActionAttribute(string name ) => Name = name;

		public void LoadParametersAndAlias() {
			foreach (ParameterInfo parameterInfo in UnderlyingMethod.GetParameters()) {
				foreach (CmdParameterAttribute parameterAttribute in parameterInfo
					.GetCustomAttributes(typeof(CmdParameterAttribute), false)
					.Cast<CmdParameterAttribute>()) {
					parameterAttribute.MyInfo = parameterInfo;
					parameterAttribute.LoadAlias();
				}
			}
		}
	}
}