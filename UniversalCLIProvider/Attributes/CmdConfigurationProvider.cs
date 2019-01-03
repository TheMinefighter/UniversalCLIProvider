using System;

namespace UniversalCLIProvider.Attributes {
/// <summary>
/// Marks that the given Property/Field provides a reference to a given configuration Object
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class CmdConfigurationProvider : Attribute { }
}