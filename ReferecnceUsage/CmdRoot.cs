using System;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Xml;
using UniversalCLIProvider;
using UniversalCLIProvider.Attributes;

namespace ReferecnceUsage {
	[CmdContext("ReferenceUsage")]
	public abstract class CmdRootContext {
		public enum ATestEnum {
			State1,
			State2,
			State3
		}

		[CmdAction("TestA")]
		public static void A([CmdParameter("TestArg")] params string[] args) {
			Console.WriteLine($"You have successfully entered action a with the parameters {string.Join(",",args)}");
		}

		[CmdAction("TestB","This is test B",new []{"Roses are red","violets are blue","this is a test and the rest of this class too"},new []{"ReferenceUsage /TestB LOL","Writes LOL to the console"},"B")]
		public static void B([CmdParameter("Argument-One")] string test) {
			Console.WriteLine(test);
		}

		[CmdAction("Move")]
		public static void Move(
			[CmdParameterAlias("X", ATestEnum.State1), CmdParameterAlias("Y", ATestEnum.State2),
			 CmdParameterAlias("Z", ATestEnum.State3), CmdParameter("Type")]
			ATestEnum moveFileOrFolder = ATestEnum.State3, [CmdParameter("newpath")] string newPath = null
		) {
			
		}

		[CmdAction("AParameterFreeAction")]
		public static void Back() {
	Console.WriteLine("You have successfully entered a action that has no parameters");
		}

		[CmdContext("SendTo")]
		public abstract class SendTo {
			[CmdAction("Set")]
			public static void SetSendTo(
				[CmdParameter("Enabled"), CmdParameterAlias("Enable", true),
				 CmdParameterAlias("Disable", false)]
				bool enable = true) {
				
			}

			[CmdAction("Get")]
			public static void GetSendTo() {
				
			}
		}
	}
}