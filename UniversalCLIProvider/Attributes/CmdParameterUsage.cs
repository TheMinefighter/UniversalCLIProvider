using System;

namespace UniversalCLIProvider.Attributes {
/// <summary>
///  Defines  in which ways a certain parameter can be supplied
/// </summary>
[Flags]
public enum CmdParameterUsage : byte {
	/// <summary>
	///  The default usage will be replaced with one of the other ones once loaded
	/// </summary>
	/// <remarks>When there is no alias it will result in <see cref="SupportDeclaredRaw" /> otherwise in <see cref="SupportDirectAlias" /></remarks>
	Default = 255,

	/// <summary>
	///  Indicates that an alias with a declaration of the parameter supported
	/// </summary>
	/// <remarks>e.g. <c>--parameterName --aliasName</c></remarks>
	SupportDeclaredAlias = 1 << 0, //1

	/// <summary>
	///  Indicates that an alias is supported without prior declaration
	/// </summary>
	/// <remarks>e.g. <c>--aliasName</c></remarks>
	SupportDirectAlias = 1 << 1, //2

	/// <summary>
	///  Indicates that normally supplied data is supported
	/// </summary>
	/// <remarks>e.g. <c>--intParameterName 6</c></remarks>
	SupportDeclaredRaw = 1 << 2 //4
}
}