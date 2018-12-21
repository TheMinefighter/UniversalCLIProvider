namespace UniversalCLIProvider.Attributes {
public interface IConfigurationRoot {
	/// <summary>
	///  Demands the config file to be loaded
	/// </summary>
	/// <remarks>Can be left empty when config is loaded before CLI interpreter call</remarks>
	void DemandLoad();

	/// <summary>
	///  Saves the config file
	/// </summary>
	void Safe();
}
}