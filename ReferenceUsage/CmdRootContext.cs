using System;
using UniversalCLIProvider.Attributes;

namespace ReferenceUsage {
[CmdContext("ReferenceUsage")]
public abstract class CmdRootContext {
	public enum ATestEnum {
		State1,
		State2,
		State3
	}

	[CmdAction("TestA")]
	public static void A([CmdParameter("TestArg")] params string[] args) {
		Console.WriteLine($"You have successfully entered action a with the parameters {string.Join(",", args)}");
	}

	[CmdAction("TestB", "This is test B", new[] {"Roses are red", "violets are blue", "this is a test and the rest of this class too"},
		new[] {"ReferenceUsage /TestB LOL", "Writes LOL to the console"}, "B")]
	public static void B([CmdParameter("Argument-One")] string test) {
		Console.WriteLine(test);
	}

	[CmdAction("AliasTest")]
	public static void AliasTest(
		[CmdParameterAlias("StateX", ATestEnum.State1, "Results in State1", "X"),
		 CmdParameterAlias("StateY", ATestEnum.State2, "Results in State2", "Y"),
		 CmdParameterAlias("StateZ", ATestEnum.State3, "Results in State3", "Z"), CmdParameter("Type")]
		ATestEnum alias = ATestEnum.State3) {
		Console.WriteLine($"The alias you have entered resulted in {alias}");
	}

	[CmdAction("AParameterFreeAction")]
	public static void Back() {
		Console.WriteLine("You have successfully entered a action that has no parameters");
	}

	[CmdContext("SendTo")]
	public abstract class SendTo {
		[CmdAction("Set")]
		public static void SetSendTo(
			[CmdParameter("Enabled"), CmdParameterAlias("Enable", true), CmdParameterAlias("Disable", false)] bool enable = true) { }

		[CmdAction("Get")]
		public static void GetSendTo() { }
	}
}
}