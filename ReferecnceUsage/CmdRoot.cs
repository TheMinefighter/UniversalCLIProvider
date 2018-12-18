using System;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Xml;
using UniversalCLIProvider;
using UniversalCLIProvider.Attributes;

namespace ReferecnceUsage {
	[CmdContext]
	public abstract class CmdRootContext {
		public enum FileOrFolder {
			File,
			Folder,
			Automatic
		}

		internal const string StartupProceedFileName = "StorageManagementProceed.lnk";

		[CmdAction("TestA")]
		public static void A([CmdParameter("TestArg")] params string[] args) {
			ConsoleIO.WriteLineToMain($"You have successfully entered action a with the parameters {string.Join(",",args)}");
		}

		[CmdAction("TestB","This is test B",new []{"Roses are red","violets are blue","this is a test and the rest of this class too"},new []{"ReferenceUsage /TestB LOL","Writes LOL to the console"},"B")]
		public static void B([CmdParameter("Argument-One")] string test) {
			ConsoleIO.WriteLineToMain(test);
		}

		[CmdAction("Move")]
		public static void Move(
			[CmdParameter("Srcpath")] string[] oldPaths,
			[CmdParameterAlias("File", FileOrFolder.File), CmdParameterAlias("Folder", FileOrFolder.Folder),
			 CmdParameterAlias("Auto-detect", FileOrFolder.Automatic), CmdParameter("Type")]
			FileOrFolder moveFileOrFolder = FileOrFolder.Automatic, [CmdParameter("newpath")] string newPath = null
		) {
			
		}

		[CmdAction("Background")]
		public static void Back() {

		}

		[CmdAction("ProtectInstallationFolder")]
		public static void ProtectInstallationFolder() {
			
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