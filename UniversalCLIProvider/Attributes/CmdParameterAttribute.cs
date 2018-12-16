using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace UniversalCLIProvider.Attributes {
[AttributeUsage(AttributeTargets.GenericParameter | AttributeTargets.Parameter | AttributeTargets.Field |
	                AttributeTargets.Property),UsedImplicitly]
	//TODO Add Defaults
	public class CmdParameterAttribute : Attribute {
		private bool _loaded;
		public bool IsParameter;
		public ICustomAttributeProvider UnderlyingParameter;
		[NotNull] public string Name;
		public string ShortForm;
		public IEnumerable<CmdParameterAliasAttribute> ParameterAliases;
		public CmdParameterUsage Usage;
		public string Description;


		public CmdParameterAttribute(string name, CmdParameterUsage usage = CmdParameterUsage.Default) {
			Name = name;
			Usage = usage;
		}

		public void LoadAlias() {
			if (!_loaded) {
				ParameterAliases = UnderlyingParameter.GetCustomAttributes(typeof(CmdParameterAliasAttribute), false)
					.Cast<CmdParameterAliasAttribute>();
				if (Usage == CmdParameterUsage.Default) {
					Usage = ParameterAliases.Any() ? CmdParameterUsage.OnlyDirectAlias : CmdParameterUsage.RawValueWithDeclaration;
				}

				_loaded = true;
			}
		}
	}
}