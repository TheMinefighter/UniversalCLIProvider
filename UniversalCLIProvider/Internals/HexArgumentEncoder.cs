using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace UniversalCLIProvider.Internals {
public static class HexArgumentEncoding {
	/// <summary>
	/// Converts the given Arguments of the given encoding to a hex string fixing commandline related quote issues, and also enabling more special character including emojis.
	/// </summary>
	/// <summary>This method has no use in this library itself and is made to be referenced by other programs or to be seen as reference implementation</summary>
	/// <param name="originalArguments">The original arguments</param>
	/// <param name="encoding">The encoding to use, defaults to <see cref="Encoding.UTF8"/></param>
	/// <remarks>The information about the encoding used is self contained in the resulting string using the <see cref="Encoding.CodePage"/> property.</remarks>
	/// <returns>A long string of hex data which can be supplied to programs implementing this interface</returns>
	public static string ToHexArgumentString(string[] originalArguments, Encoding encoding = null) {
		encoding = encoding ?? Encoding.UTF8;
		int typicalEncodingLength = encoding.GetByteCount("s");
		StringBuilder stringBuilder =
			new StringBuilder(typicalEncodingLength * originalArguments.Sum(x => x.Length) + originalArguments.Length * 8+8);
		stringBuilder.Append(encoding.CodePage.ToString("x8"));
		foreach (string argument in originalArguments) {
			stringBuilder.Append( encoding.GetByteCount(argument).ToString("x8"));
			foreach (byte b in encoding.GetBytes(argument)) {
				stringBuilder.Append(b.ToString("x2"));
			}
		}

		return stringBuilder.ToString();
	}

	/// <summary>
	/// Loads the arguments encoded using <see cref="ToHexArgumentString"/>
	/// </summary>
	/// <param name="arg"> The original hexadecimal argument</param>
	/// <param name="newArgs"> the decoded arguments</param>
	/// <returns>Whether the parsing operation were successful</returns>
	public static bool ArgumentsFromHex(string arg, out List<string> newArgs) {
		newArgs = null;
		int currentOffset = 0;
		if (arg.Length < 8 + currentOffset) {
			Console.WriteLine("The hexadecimal data is not long enough for evaluating the encoding");
			return false;
		}

		Encoding encoding = Encoding.GetEncoding(Int32.Parse(arg.Substring(currentOffset, 8), NumberStyles.HexNumber));
		currentOffset += 8;
		int typicalEncodingLength = encoding.GetByteCount("s");
		int count = 0;
		newArgs = new List<string>(16);
		while (true) {
			if (arg.Length == currentOffset) {
				break;
			}

			if (arg.Length < currentOffset + 8) {
				Console.WriteLine($"The hexadecimal data is not long enough for evaluating the proposed length of argument {count}");
				return false;
			}

			int proposedLength;
			try {
				proposedLength = Int32.Parse(arg.Substring(currentOffset, 8), NumberStyles.HexNumber);
			}
			catch (Exception) {
				Console.WriteLine($"Error while parsing string length of argument {count}");
				return false;
			}

			currentOffset += 8;
			if (arg.Length < currentOffset + proposedLength * 2) {
				Console.WriteLine($"The hexadecimal data is not long enough for content of argument {count}");
				return false;
			}

			byte[] rawArgument = new byte[proposedLength];
			for (int i = 0; i < proposedLength; i++) {
				//TODO Can be optimized later (dual counter)
				rawArgument[i] = Byte.Parse(arg.Substring(currentOffset, 2), NumberStyles.HexNumber);
				currentOffset += 2;
			}

			count++;
			newArgs.Add(encoding.GetString(rawArgument));
		}

		return true;
	}
}
}