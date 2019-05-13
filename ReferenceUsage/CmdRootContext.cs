using System;
using System.Linq;
using UniversalCLIProvider.Attributes;
using UniversalCLIProvider.Interpreters;

namespace ReferenceUsage {
[CmdContext("ReferenceUsage", "A description",
	new[] {
		"A long description", "That has multiple lines", "which can manually separated",
		"But when a line is sooo long that it does not fit in your console window, breaks will be added automatically"
	},
	defaultActionPreset: ContextDefaultActionPreset.Interactive)]
public abstract class CmdRootContext {
	[CmdConfigurationProvider("cfg")]
	public static ReferenceConfig Cfg { get; set; } = new ReferenceConfig();

	[CmdAction("BaseTest", "A base Test",
		new[] {"Roses are red", "violets are blue", "this is a test and the rest of this class too"},
		new[] {"ReferenceUsage --BaseTest", "ReferenceUsage --b"}, "b")]
	public static void BaseTest() {
		Console.WriteLine("You have successfully entered the BaseTest action.");
	}

	[CmdAction("TestB", "This is test B",
		new[] {"Roses are red", "violets are blue", "this is a test and the rest of this class too"},
		new[] {"ReferenceUsage /TestB LOL", "Writes LOL to the console"})]
	public static void B([CmdParameter("Argument-One", ShortForm = "o")] string test = "DefaultValue") {
		Console.WriteLine($"You have used \"{test}\" as argument.");
	}

	[CmdAction("TestC", "This is test C",
		new[] {"Roses are red", "violets are blue", "this is a test and the rest of this class too"},
		new[] {"ReferenceUsage /TestB LOL", "Writes LOL to the console"}, "c")]
	public static void C(
		[CmdParameter("TestArg", usage: CmdParameterUsage.All),
		 CmdParameterAlias("HelloW", new[] {"Hello", "World"}, "Uses Hello World as TestArg", "h")]
		string[] args) {
		Console.WriteLine($"TestArg has the following elements: {string.Join(", ", args)}");
	}

	[CmdContext("XZY", "Another context", new[] {"This is a context help", "consisting of multiple lines"}, "x",
		ContextDefaultActionPreset.Exit)]
	public abstract class XZY {
		[CmdAction("TestZ", "A basic Test")]
		public static void TestZ() {
			Console.WriteLine("You have successfully entered the TestZ action in the XYZ context.");
		}

		[CmdContext("789")]
		public abstract class ABC {
			[CmdAction("TestZ", "A basic Test")]
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
}
}