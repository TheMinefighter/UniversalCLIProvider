using System;
using JetBrains.Annotations;
using UniversalCLIProvider.Internals;

namespace UniversalCLIProvider.Attributes {
/// <inheritdoc />
/// <summary>
/// Indicates that the method should be treated as context default action 
/// </summary>
[AttributeUsage(AttributeTargets.Method), UsedImplicitly]
public class CmdDefaultActionAttribute : Attribute {
	internal object[] Parameters;
	/// <summary>
	/// Marks a method as context default action
	/// </summary>
	/// <param name="parameters">The parameters to use to call the method</param>
	/// <remarks>When the number of parameters supplied to the <see cref="T:UniversalCLIProvider.Attributes.CmdDefaultActionAttribute" />
	/// is on less than the number of parameters the marked method takes,
	/// the **last** parameter supplied will be the current <see cref="T:UniversalCLIProvider.Attributes.CmdContextAttribute" /></remarks>
	public CmdDefaultActionAttribute(params object[] parameters) => Parameters = parameters;
}
}