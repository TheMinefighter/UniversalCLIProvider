using System;
using JetBrains.Annotations;
using PropertyOrFieldInfoPackage;

namespace UniversalCLIProvider.Attributes {
/// <summary>
/// Marks that the given Property/Field provides a reference to a given configuration Object
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class CmdConfigurationProviderAttribute : Attribute {
	[NotNull] public string Name;
	[CanBeNull] public string ShortForm;
	public PropertyOrFieldInfo UnderlyingPropertyOrField;
	public CmdConfigurationNamespaceAttribute Root;
	public CmdConfigurationProviderAttribute([NotNull] string name, [CanBeNull] string shortForm=null) {
		this.ShortForm = shortForm;
		this.Name = name;
	}
}
}