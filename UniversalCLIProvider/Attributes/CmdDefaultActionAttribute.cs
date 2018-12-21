using System;
using JetBrains.Annotations;
using UniversalCLIProvider.Internals;

namespace UniversalCLIProvider.Attributes {
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class), UsedImplicitly]
public class CmdDefaultActionAttribute : Attribute {
	private bool IsDirect;
	private ContextDefaultAction toRun;
}
}