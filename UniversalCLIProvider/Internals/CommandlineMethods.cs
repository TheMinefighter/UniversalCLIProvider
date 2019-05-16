using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UniversalCLIProvider.Internals {
/// <summary>
///  Provides Methods needed for basic interpretation tasks
/// </summary>
public static class CommandlineMethods {
	/// <summary>
	///  The nullable types for which <see cref="GetValueFromString" /> overrides quote and null behaviour
	/// </summary>
	private static readonly Type[] NullableOverridenTypes =
		{typeof(DateTime?), typeof(DateTimeOffset?), typeof(TimeSpan?), typeof(string), typeof(Guid?)};

	/// <summary>
	///  The non nullable types for which <see cref="GetValueFromString" /> overrides quote behaviour
	/// </summary>
	private static readonly Type[] OverridenTypes =
		{typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan), typeof(Guid), typeof(Uri)};

	public static bool GetValueFromString<T>([NotNull] string source, out T value,
		[CanBeNull] JsonSerializerSettings serializerSettings = null, bool enableCustomCompatSupport = true) {
		bool success = GetValueFromString(source, typeof(T), out object tmp, serializerSettings, enableCustomCompatSupport);
		value = tmp is T t ? t : default(T);
		return success;
	}

	/// <summary>
	///  Parses a string while taking the special requirements for CLIs into account
	/// </summary>
	/// <param name="source">The string to parse</param>
	/// <param name="expectedType">The type expected, should be as narrow as possible</param>
	/// <param name="value">The value the parsing operation returned</param>
	/// <param name="serializerSettings">Custom JSON SerializerSettings</param>
	/// <param name="enableCustomCompatSupport">Whether to enable the CLI optimizing changes before the JSON Deserializer</param>
	/// <remarks>
	///  For supported primary types see
	///  https://github.com/JamesNK/Newtonsoft.Json/blob/master/Src/Newtonsoft.Json/Utilities/ConvertUtils.cs (@
	///  internal enum PrimitiveTypeCode)
	/// </remarks>
	/// <remarks>Types with modified support:</remarks>
	/// <remarks>
	///  <see cref="Enum" />: Allowing Enum.Parse (e.g. <c>NumberStyles.Any</c>, <c>Any</c>) when enum is the explicit target
	///  type
	/// </remarks>
	/// <remarks>
	///  <see cref="DateTime" />, <see cref="TimeSpan" />, <see cref="DateTimeOffset" />, <see cref="Uri" />, <see cref="Guid" />,
	///  <see cref="string" />Allowed without quotation marks when explicit
	/// </remarks>
	/// <remarks><see cref="Nullable{T}" /> types also supported</remarks>
	/// <returns>Whether parsing was successful</returns>
	public static bool GetValueFromString([NotNull] string source, [NotNull] Type expectedType, out object value,
		[CanBeNull] JsonSerializerSettings serializerSettings = null, bool enableCustomCompatSupport = true) {
		serializerSettings = serializerSettings ?? new JsonSerializerSettings();
		serializerSettings.Converters.Add(new StringEnumConverter());
		value = null;
		if (enableCustomCompatSupport) {
			if (expectedType.IsEnum ||
				expectedType.IsGenericType && expectedType.GetGenericTypeDefinition() == typeof(Nullable<>) &&
				expectedType.GenericTypeArguments.Length == 1 &&
				expectedType.GenericTypeArguments[0].IsEnum && Enum.IsDefined(expectedType, source)) {
				value = Enum.Parse(expectedType, source);
				return true;
			}

			if (OverridenTypes.Contains(expectedType)) {
				if (!source.StartsWith("\"")) {
					source = "\"" + source + "\"";
				}
			}

			if (NullableOverridenTypes.Contains(expectedType)) {
				if (source == "null") {
					return true;
				}

				if (!source.StartsWith("\"")) {
					if (expectedType == typeof(string)) {
						value = source;
						return true;
					}

					source = "\"" + source + "\"";
				}
			}
		}

		try {
			value = JsonConvert.DeserializeObject(source, expectedType, serializerSettings);
		}
		catch (Exception) {
			return false;
		}

		return true;
	}

	/// <summary>
	///  Breaks up a given string in multiple lines of a given width, while making each end with a word end when possible
	/// </summary>
	/// <param name="text"> The text to format</param>
	/// <param name="width"> The width of the textoutput to format for</param>
	/// <param name="indent"> The indent to use for each new line</param>
	/// <param name="tw">The builder to append the formatted text to</param>
	public static void PrintWithPotentialIndent([NotNull] string text, int width, int indent, TextWriter tw = null) {
		tw = tw ?? Console.Out;
		if (text.Length < width) {
			tw.WriteLine(text);
			return;
		}

//TODO split splitting and printing
		bool firstLine = true;
		int lastFallback = -1;
		int lineStart = 0;
		for (int i = 0; i < text.Length; i++) {
			if (i - lineStart == width) {
				int breakIndex = lastFallback != -1 ? lastFallback : i;
				if (!firstLine) {
					tw.WriteLine(text.Substring(lineStart,
						breakIndex - lineStart));
				}
				else {
					tw.Write(new string(' ', indent));
					tw.WriteLine(text.Substring(lineStart, breakIndex - lineStart));
				}

				lineStart = breakIndex + 1;
				if (!firstLine) {
					width -= indent;
				}

				firstLine = false;
			}

			if (text[i] == ' ') {
				lastFallback = i;
			}
		}

		tw.Write(new string(' ', indent));
		tw.WriteLine(text.Substring(lineStart));
	}

	/// <summary>
	///  Pads a given string to centered by a given padding char in a given width
	/// </summary>
	/// <param name="src">The original string</param>
	/// <param name="width">The targeted width</param>
	/// <param name="pad">The padding character to use, defaults to =</param>
	/// <remarks>
	///  When the difference between the length of <paramref name="src" /> and <paramref name="width" /> is odd, there will be one less
	///  padding
	///  character on the left
	/// </remarks>
	/// <returns>The padded string</returns>
	[NotNull]
	public static string PadCentered([NotNull] string src, int width, char pad = '=') {
		if (src.Length > width) {
			return src;
		}
		else {
			return new string(pad, (width - src.Length) / 2) + src + new string(pad, (width - src.Length + 1) / 2);
		}
	}

	/// <summary>
	///  Writes a line in a given color
	/// </summary>
	/// <param name="toWrite">The test to write</param>
	/// <param name="foreground">the new foreground color, null keeps it unchanged, defaults to null</param>
	/// <param name="background">the new background color, null keeps it unchanged, defaults to null</param>
	public static void WriteColorfulLine(string toWrite, ConsoleColor? foreground = null, ConsoleColor? background = null) {
		(ConsoleColor, ConsoleColor) backup = (Console.BackgroundColor, Console.ForegroundColor);
		Console.BackgroundColor = background ?? Console.BackgroundColor;
		Console.ForegroundColor = foreground ?? Console.ForegroundColor;
		Console.WriteLine(toWrite);
		(Console.BackgroundColor, Console.ForegroundColor) = backup;
	}

	/// <summary>
	/// Checks if a given parameter matches a specified one
	/// </summary>
	/// <param name="expected">The expected parameter</param>
	/// <param name="given">The given parameter to compare with</param>
	/// <param name="ignoreCase">Whether to ignore the case of the parameter provided</param>
	/// <param name="expectedShortForm">The ShortForm of <paramref name="expected"/> null for none</param>
	/// <param name="allowPrefixFree">Whether it can be used without prefix, defaults to no</param>
	/// <returns>Whether the given form matched the expected form or its ShortForm</returns>
	internal static bool IsParameterEqual([CanBeNull] string expected, [NotNull] string given, bool ignoreCase,
		string expectedShortForm = null,
		bool allowPrefixFree = false) {
		if (expected is null) {
			return false;
		}

		if (ignoreCase) {
			given = given.ToLower();
			expected = expected.ToLower();
		}

		if (!(expectedShortForm is null) && ('/' + expectedShortForm == given || '-' + expectedShortForm == given)) {
			return true;
		}

		return '/' + expected == given || "--" + expected == given || allowPrefixFree && expected == given;
	}

	public static IEnumerable<string> ArgumentsFromString(string readLine) {
		var lastStringBuilder = new StringBuilder();
		bool quoting = false;
		bool backslashActive = false;
		foreach (char c in readLine) { //TODO Might want to add support for backslashed quotes
			switch (c) {
				case '\"' when backslashActive:
					lastStringBuilder.Append("\"");
					break;
				case '"' when !backslashActive:
					quoting ^= true;
					break;
				case ' ' when !quoting: {
					string tmpString = lastStringBuilder.ToString();
					if (tmpString != string.Empty) {
						yield return tmpString;
						lastStringBuilder = new StringBuilder();
					}

					break;
				}

				case '\\' when !backslashActive:
					backslashActive = true;
					break;
				case '\\' when backslashActive:
					lastStringBuilder.Append('\\');
					backslashActive = false;
					break;
				default:
					if (backslashActive) {
						lastStringBuilder.Append('\\');
					}

					backslashActive = false;
					lastStringBuilder.Append(c);
					break;
			}
		}

		string lastString = lastStringBuilder.ToString();
		if (lastString != string.Empty) {
			yield return lastString;
		}
	}
}
}