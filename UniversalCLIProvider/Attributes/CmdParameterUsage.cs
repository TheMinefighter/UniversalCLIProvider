using System;

namespace UniversalCLIProvider.Attributes {
[Flags]
public enum CmdParameterUsage {
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