using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UniversalCLIProvider.Attributes;

namespace UniversalCLIProvider {
public static class CommandlineMethods {
	/// <summary>
	/// Converts the given Arguments of the given encoding to a hex string fixing commandline related quote issues, to be used by 3rd programs for inter-program communication
	/// </summary>
	/// <param name="originalArguments">The original arguments</param>
	/// <param name="encoding">The encoding to use</param>
	/// <returns>A long string of hex data which can be supplied to programs </returns>
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

//      public static bool GetAliasValue(out object value, CmdParameterAttribute cmdParameterAttribute, string search) {
//         value = null;
//         bool success = false;
//         foreach (CmdParameterAliasAttribute commandlineParameterAlias in cmdParameterAttribute.ParameterAliases) {
//            if (BaseInterpreter.IsParameterEqual(commandlineParameterAlias.Name, search)) {
//               success = true;
//               value = commandlineParameterAlias.Value;
//               break;
//            }
//         }
//
//         return success;
//      }
/// <summary>
/// The nullable types for which <see cref="GetValueFromString"/> overrides quote and null behaviour
/// </summary>
	private static readonly Type[] NullableOverridenTypes = {typeof(DateTime?), typeof(DateTimeOffset?), typeof(TimeSpan?), typeof(Guid?)};
/// <summary>
/// The non nullable types for which <see cref="GetValueFromString"/> overrides quote behaviour
/// </summary>
	private static readonly Type[] OverridenTypes =
		{typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan), typeof(string), typeof(Guid), typeof(Uri)};

	/// <summary>
	/// Parses a string while taking the special requirements for CLIs into account
	/// </summary>
	/// <param name="source">The string to parse</param>
	/// <param name="expectedType">The type expected, should be as narrow as possible</param>
	/// <param name="value">The value the parsing operation returned</param>
	/// <param name="serializerSettings">Custom JSON SerializerSettings</param>
	/// <param name="enableCustomCompatSupport">Whether to enable the CLI optimizing changes before the JSON Deserializer</param>
	/// <remarks>For supported primary types see https://github.com/JamesNK/Newtonsoft.Json/blob/master/Src/Newtonsoft.Json/Utilities/ConvertUtils.cs (@ internal enum PrimitiveTypeCode) </remarks>
	/// <remarks>Types with modified support:</remarks>
	/// <remarks><see cref="Enum"/>: Allowing Enum.Parse (e.g. <c>NumberStyles.Any</c>, <c>Any</c>) when enum is the explicit target type</remarks>
	/// <remarks><see cref="DateTime"/>, <see cref="TimeSpan"/>, <see cref="DateTimeOffset"/>, <see cref="Uri"/>, <see cref="Guid"/>,<see cref="string"/>Allowed without quotation marks when explicit</remarks>
	/// <remarks><see cref="Nullable{T}"/> types also supported</remarks>
	/// <returns>Whether parsing was successful</returns>
	public static bool GetValueFromString(string source, Type expectedType, out object value, JsonSerializerSettings serializerSettings=null, bool enableCustomCompatSupport=true) {
		value = null;
		if (enableCustomCompatSupport) {
			if (expectedType.IsEnum ||
			    (expectedType.IsGenericType&& expectedType.GetGenericTypeDefinition() == typeof(Nullable<>) && expectedType.GenericTypeArguments.Length == 1 &&
			     expectedType.GenericTypeArguments[0].IsEnum) && Enum.IsDefined(expectedType, source)) {
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
					source = "\"" + source + "\"";
				}
			}
		}
		try {
			value = JsonConvert.DeserializeObject(source, expectedType,serializerSettings);
		}
		catch (Exception) {
			return false;
		}

		return true;
	}

	public static bool ArgumentsFromHex(string arg, out List<string> newArgs) {
		newArgs = null;
		int currentOffset = 0;
		if (arg.Length < 8 + currentOffset) {
			Console.WriteLine("The hexadecimal data is not long enough for evaluating the encoding");
			return false;
		}

		Encoding encoding = Encoding.GetEncoding(int.Parse(arg.Substring(currentOffset, 8), NumberStyles.HexNumber));
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
				proposedLength = int.Parse(arg.Substring(currentOffset, 8), NumberStyles.HexNumber);
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
				rawArgument[i] = byte.Parse(arg.Substring(currentOffset, 2), NumberStyles.HexNumber);
				currentOffset += 2;
			}

			count++;
			newArgs.Add(encoding.GetString(rawArgument));
		}

		return true;
	}
}
}