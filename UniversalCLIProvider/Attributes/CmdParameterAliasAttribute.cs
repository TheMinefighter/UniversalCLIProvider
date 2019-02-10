using System;
using JetBrains.Annotations;

namespace UniversalCLIProvider.Attributes {
[AttributeUsage(AttributeTargets.GenericParameter | AttributeTargets.Parameter | AttributeTargets.Property |
	 AttributeTargets.Field,
	 AllowMultiple = true), UsedImplicitly]
public class CmdParameterAliasAttribute : Attribute {
	[CanBeNull] public readonly string Description;
	[NotNull] public readonly string Name;
	[CanBeNull] public readonly string ShortForm;
	[CanBeNull] public readonly object Value;

	public CmdParameterAliasAttribute([NotNull] string name, [CanBeNull] object value, [CanBeNull] string description = null,
		[CanBeNull] string shortForm = null) {
		Name = name;
		Value = value;
		ShortForm = shortForm;
		Description = description;
	}
}
}