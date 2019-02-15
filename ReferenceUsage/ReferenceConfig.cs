using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PropertyOrFieldInfoPackage;
using UniversalCLIProvider.Attributes;

namespace ReferenceUsage {
[CmdConfigurationNamespace("AName", "ADescription")]
public class ReferenceConfig : IConfigurationRoot {
	[CmdConfigurationField("ManyStrings")]
	public string[] ManyStrings { get; set; }= {"A String","Another string"};

	[CmdConfigurationField("IntA")]
	public int IntA { get; set; } = 7;
	
	[CmdConfigurationField("SubCfg")]
	public SubCfgClass SubCfg { get; set; } = new SubCfgClass();


	public void Save(IEnumerable<PropertyOrFieldInfo> c) {
		Console.WriteLine($"The configuration has been saved to be {JsonConvert.SerializeObject(this)}");
	}
	[CmdConfigurationNamespace("AName", "ADescription")]
	public class SubCfgClass {
		[CmdConfigurationField("IntB")]
		public int IntB { get; set; } = 9;
	}
}
}