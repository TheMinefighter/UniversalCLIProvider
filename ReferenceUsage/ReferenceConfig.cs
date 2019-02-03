using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PropertyOrFieldInfoPackage;
using UniversalCLIProvider.Attributes;

namespace ReferenceUsage {
[CmdConfigurationNamespace("InvisibleName", "Description")]
public class ReferenceConfig : IConfigurationRoot {
	public string StringB = "A String";

	[CmdConfigurationField("IntA")]
	public int IntA { get; set; } = 0;

	public void Save(IEnumerable<PropertyOrFieldInfo> c) {
		Console.WriteLine($"The configuration has been saved to be {JsonConvert.SerializeObject(this)}");
	}
}
}