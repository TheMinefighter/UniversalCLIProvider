using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PropertyOrFieldInfoPackage;
using UniversalCLIProvider.Attributes;

namespace ReferenceUsage {
[CmdConfigurationNamespace("InvisibleName","Description")]
public class ReferenceConfig : IConfigurationRoot {
	[CmdConfigurationValue("IntA")]
	public int IntA { get; set; }=0;
	public string StringB = "A String";
	public void Save(IEnumerable<PropertyOrFieldInfo> c) {
		Console.WriteLine($"The configuration has been saved to be {JsonConvert.SerializeObject(this)}");
	}
}
}