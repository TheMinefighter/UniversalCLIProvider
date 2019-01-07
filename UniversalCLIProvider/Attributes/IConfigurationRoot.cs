using System.Collections.Generic;
using PropertyOrFieldInfoPackage;

namespace UniversalCLIProvider.Attributes {
/// <summary>
/// An interface that can optionally be implemented for managed configuration roots
/// </summary>
public interface IConfigurationRoot {
	/// <summary>
	///  Saves the config file
	/// </summary>
	void Save(IEnumerable<PropertyOrFieldInfo> changedValues);
}
}