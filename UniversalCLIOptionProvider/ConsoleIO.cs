using System;

namespace UniversalCLIOptionProvider {
	/// <summary>
	///  Class storing the Actions for Console Operations
	/// </summary>
	public class ConsoleIO {
//      private static ConsoleIO _primary = DefaultIO;

		/// <summary>
		///  Reads a line from Console
		/// </summary>
		public Func<string> ReadLine;

		/// <summary>
		///  Writes a message to Console
		/// </summary>
		public Action<string> Write;


//      /// <summary>
//      ///    Sets the visibility of the Console Window
//      /// </summary>
//      public Action<bool> SetVisibiltyToConsole;

		/// <summary>
		///  Writes a message to Console and a linebreak afterwards
		/// </summary>
		public Action<string> WriteLine;

		public static ConsoleIO DefaultIO => new ConsoleIO {
			ReadLine = Console.ReadLine,
//         SetVisibiltyToConsole = x => ShowWindow(GetConsoleWindow(), x ? SW_SHOW : SW_HIDE),
			WriteLine = Console.WriteLine,
			Write = Console.Write
		};

//      public ConsoleIO(bool isPrimary = true) {
//         if (isPrimary) {
//            _primary = this;
//         }
//      }
//
//      /// <summary>
//      ///    Writes a message to Console and a linebreak afterwards
//      /// </summary>
//      /// <param name="message">The message to write to console</param>
//      public static void WriteLine(string message) {
//         _primary.WriteLineToConsole(message);
//      }
//
//      /// <summary>
//      ///    Reads a line from Console
//      /// </summary>
//      /// <returns>The line the user entered</returns>
//      public static string ReadLine() => _primary.ReadFromConsole();
//
//      /// <summary>
//      ///    Writes a message to Console
//      /// </summary>
//      /// <param name="message">The message to write to Console</param>
//      public static void Write(string message) {
//         _primary.WriteToConsole(message);
//      }
//
//      /// <summary>
//      ///    Sets the visibility of the Console Window
//      /// </summary>
//      /// <param name="visible">Whether the console window should be visible</param>
//      public static void SetVisibility(bool visible) {
//         _primary.SetVisibiltyToConsole(visible);
//      }
//
//      #region From https://stackoverflow.com/a/3571628/6730162 access on 08.01.2018
//
//      [DllImport("kernel32.dll")]
//      public static extern IntPtr GetConsoleWindow();
//
//      [DllImport("user32.dll")]
//      public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
//
//      public const int SW_HIDE = 0;
//      public const int SW_SHOW = 5;
//
//      #endregion
	}
}