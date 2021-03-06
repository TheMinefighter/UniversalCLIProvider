﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace UniversalCLIProvider.Attributes {
[AttributeUsage(AttributeTargets.GenericParameter | AttributeTargets.Parameter | AttributeTargets.Field |
	 AttributeTargets.Property), UsedImplicitly]
//TODO Add Defaults
public class CmdParameterAttribute : Attribute {
	private bool _loaded;
	public string Description;
	[NotNull] public string Name;
	public IEnumerable<CmdParameterAliasAttribute> ParameterAliases;
	public string ShortForm;
	public ParameterInfo UnderlyingParameter;
	public CmdParameterUsage Usage;


	public CmdParameterAttribute(string name, string description = null, CmdParameterUsage usage = CmdParameterUsage.Default) {
		Name = name;
		Description = description;
		Usage = usage;
	}

	public void LoadAlias() {
		if (!_loaded) {
			ParameterAliases = UnderlyingParameter.GetCustomAttributes(typeof(CmdParameterAliasAttribute), false)
				.Cast<CmdParameterAliasAttribute>().ToArray();
			if (Usage == CmdParameterUsage.Default) {
				Usage = ParameterAliases.Any() ? CmdParameterUsage.SupportDirectAlias : CmdParameterUsage.SupportDeclaredRaw;
			}

			_loaded = true;
		}
	}
}
}