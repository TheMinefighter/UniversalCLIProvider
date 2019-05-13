using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PropertyOrFieldInfoPackage;
using UniversalCLIProvider.Attributes;

namespace ReferenceUsage {
[CmdConfigurationNamespace("AName", "ADescription", new[] {"A", "really", "long", "description"})]
public class ReferenceConfig : IConfigurationRoot {
	[CmdConfigurationField("ManyStrings", "This is a description",
		new[] {"This", "is", "a", "really", "long", "value", "description"})]
	public string[] ManyStrings { get; set; } = {"A String", "Another string"};

	[CmdConfigurationField("IntA", "This is another description")]
	public int IntA { get; set; } = 7;

	[CmdConfigurationField("SubCfg", "This is the third description")]
	public SubCfgClass[] SubCfg { get; set; } = {new SubCfgClass {IntB = 2, SomeBool = false}, new SubCfgClass()};


	public void Save(IEnumerable<PropertyOrFieldInfo> c) {
		Console.WriteLine($"The configuration has been saved to be {JsonConvert.SerializeObject(this)}");
	}

	[CmdConfigurationNamespace("AName", "ADescription")]
	public class SubCfgClass {
		[CmdConfigurationField("IntB")]
		public int IntB { get; set; } = 9;

		[CmdConfigurationField("SomeBool")]
		public bool SomeBool { get; set; } = true;
	}
}
}