﻿using UniversalCLIProvider;
using UniversalCLIProvider.Interpreters;

namespace ReferecnceUsage {
internal class Program {
	public static void Main(string[] args) {
		new CommandlineOptionInterpreter(args).Interpret<CmdRootContext>();
	}
}
}