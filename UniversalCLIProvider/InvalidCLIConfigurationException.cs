using System;

namespace UniversalCLIProvider {
/// <inheritdoc/>
/// <summary>
/// Wraps all exceptions caused by a library user configuring the CLI wrong, cannot be caused by weird user input alone
/// </summary>
public class InvalidCLIConfigurationException : Exception {
	/// <inheritdoc />
	public InvalidCLIConfigurationException() { }

	/// <inheritdoc />
	public InvalidCLIConfigurationException(string message) : base(message) { }

	/// <inheritdoc />
	public InvalidCLIConfigurationException(string message, Exception innerException) : base(message, innerException) { }
}
}