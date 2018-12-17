using System;

namespace UniversalCLIProvider {
/// <summary>
///  Class storing the Actions for Console Operations
/// </summary>
public class ConsoleIO {
	private static ConsoleIO _primary = DefaultIO;

	/// <summary>
	///  Reads a line from Console
	/// </summary>
	public Func<string> ReadLine;

	/// <summary>
	///  Writes a message to Console
	/// </summary>
	public Action<string> Write;

	/// <summary>
	///  Writes a message to Console and a linebreak afterwards
	/// </summary>
	public Action<string> WriteLine;

	public static ConsoleIO DefaultIO => new ConsoleIO {
		ReadLine = Console.ReadLine,
		WriteLine = Console.WriteLine,
		Write = Console.Write
	};

	public ConsoleIO(bool isPrimary = true) {
		if (isPrimary) {
			_primary = this;
		}
	}

	/// <summary>
	///  Writes a message to Console and a linebreak afterwards
	/// </summary>
	/// <param name="message">The message to write to console</param>
	public static void WriteLineToMain(string message) {
		_primary.WriteLine(message);
	}

	/// <summary>
	///  Reads a line from Console
	/// </summary>
	/// <returns>The line the user entered</returns>
	public static string ReadLineFrommain() => _primary.ReadLine();

	/// <summary>
	///  Writes a message to Console
	/// </summary>
	/// <param name="message">The message to write to Console</param>
	public static void WriteToMain(string message) {
		_primary.Write(message);
	}
}
}