using System;

namespace UniversalCLIProvider {
/// <inheritdoc />
public class CLIUsageException : Exception {
	/// <inheritdoc />
	public CLIUsageException(string message) : base(message) { }

	/// <inheritdoc />
	public CLIUsageException(string message, Exception innerException) : base(message, innerException) { }
}
}