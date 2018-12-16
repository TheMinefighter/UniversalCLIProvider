using System;

namespace UniversalCLIProvider.Attributes {
/// <summary>
/// Defines  in which ways a certain parameter can be supplied
/// </summary>
[Flags]
public enum CmdParameterUsage {
	/// <summary>
	/// The default usage will be replaced with one of the other ones once loaded
	/// </summary>
	Default=0,
	SupportDeclaredAlias=0b0001,
	SupportDirectAlias=0b0010,
	SupportRaw=0b0100,
	RawValueWithDeclaration=SupportRaw|SupportDeclaredAlias,
	OnlyDirectAlias=SupportDirectAlias,
	NoRawsButDeclaration=SupportDeclaredAlias,
	DirectOrDeclaredAlias=SupportDirectAlias|SupportDeclaredAlias
}
}