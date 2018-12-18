using System;

namespace UniversalCLIProvider.Attributes {
/// <summary>
///  Defines  in which ways a certain parameter can be supplied
/// </summary>
[Flags]
public enum CmdParameterUsage {
	/// <summary>
	///  The default usage will be replaced with one of the other ones once loaded
	/// </summary>
	Default = 0,
	SupportDeclaredAlias = 1<<0,//1
	SupportDirectAlias = 1<<1,//2
	SupportRaw = 1<<2,//4
	RawValueWithDeclaration = SupportRaw | SupportDeclaredAlias,
	OnlyDirectAlias = SupportDirectAlias,
	NoRawsButDeclaration = SupportDeclaredAlias,
	DirectOrDeclaredAlias = SupportDirectAlias | SupportDeclaredAlias
}
}