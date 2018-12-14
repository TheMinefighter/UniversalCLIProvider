using System;

namespace UniversalCLIProvider {
public class InvalidCLIConfigurationException: Exception {
	public InvalidCLIConfigurationException() { }
	public InvalidCLIConfigurationException(string message) : base(message) { }
	public InvalidCLIConfigurationException(string message, Exception innerException) : base(message, innerException) { }
}
}