using System;
using System.Linq;
using UniversalCLIProvider.Attributes;
using UniversalCLIProvider.Interpreters;

namespace ReferenceUsage {
[CmdContext("ReferenceUsage", defaultActionPreset: ContextDefaultActionPreset.Interactive)]
public abstract class CmdRootContext {
	[CmdAction("BaseTest","A base Test",new[] {"Roses are red", "violets are blue", "this is a test and the rest of this class too"},new []{"ReferenceUsage --BaseTest","ReferenceUsage --b"}, "b")]
	public static void BaseTest() {
		Console.WriteLine("You have successfully entered BaseTest action.");
	}

	[CmdAction("TestB", "This is test B", new[] {"Roses are red", "violets are blue", "this is a test and the rest of this class too"},
		new[] {"ReferenceUsage /TestB LOL", "Writes LOL to the console"})]
	public static void B([CmdParameter("Argument-One",ShortForm = "o")] string test="DefaultValue") {
		Console.WriteLine($"You have used \"{test}\" as argument.");
	}

	[CmdAction("TestC", shortForm:"c")]
	public static void C([CmdParameter("TestArg",usage:CmdParameterUsage.All),CmdParameterAlias("HelloW",new []{"Hello","World"},shortForm:"h")]  string[] args) {
		Console.WriteLine($"TestArg has the following elements: {string.Join(", ", args)}");
	}
[CmdContext("XZY", defaultActionPreset: ContextDefaultActionPreset.Exit)]
	public  abstract class XZY {
		[CmdAction("TestZ","A basic Test")]
		public static void TestZ() {
			Console.WriteLine("You have successfully entered the TestZ action in the XYZ context.");
		}
		[CmdContext("789")]
		public  abstract class ABC {
			[CmdAction("TestZ","A basic Test")]
			public static void TestY() {
				Console.WriteLine("You have successfully entered the TestZ action in the XYZ context.");
			}
		}
	}

	[CmdContext("CustomizedDefaultAction")]
	public abstract class Cda {
		[CmdDefaultAction(7)]
		public static void TheDefaultAction(int number, ContextInterpreter interpreter) => Console.WriteLine(
			$"The CmdDefaultActionAttribute supplied the number {number}, the interpreter is currently located in the following path: {string.Join(">", interpreter.Path.Select(x => x.Name))}");
	}

	public enum ATestEnum {
		State1,
		State2,
		State3
	}

	[CmdConfigurationProvider("cfg")]
	public static ReferenceConfig Cfg { get; set; } = new ReferenceConfig();

	[CmdAction("AliasTest")]
	public static void AliasTest(
		[CmdParameterAlias("StateX", ATestEnum.State1, "Results in State1", "X"),
		 CmdParameterAlias("StateY", ATestEnum.State2, "Results in State2", "Y"),
		 CmdParameterAlias("StateZ", ATestEnum.State3, "Results in State3", "Z"), CmdParameter("Type")]
		ATestEnum alias = ATestEnum.State3) {
		Console.WriteLine($"The alias you have entered resulted in {alias}");
	}
}
}