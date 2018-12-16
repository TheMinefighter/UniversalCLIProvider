using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace UniversalCLIProvider.Internals {
/// <summary>
/// Provides Methods needed for basic interpretation tasks
/// </summary>
public static class CommandlineMethods {
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

	/// <summary>
	/// Breaks up a given string in multiple lines of a given width, while making each end with a word end when possible
	/// </summary>
	/// <param name="text"> The text to format</param>
	/// <param name="width"> The width of the textoutput to format for</param>
	/// <param name="indent"> The indent to use for each new line</param>
	/// <returns>The formatted text</returns>
	public static List<string> PrintWithPotentialIndent(string text, int width, int indent) {
		List<string> lines= new List<string>(text.Length/width*2);
		int? lastFallback = null;
		int lineStart=0;
		for (int i = 0; i < text.Length; i++) {
			if (i-lineStart==width) {
				int breakIndex = lastFallback ?? i;
				string line;
				if (lines.Count==0) {
					line = text.Substring(lineStart,breakIndex-lineStart-(lastFallback is null?0:1));
				}
				else {
					line=new string(' ',indent)+ text.Substring(lineStart,breakIndex-lineStart);
				}
				lines.Add(line);
				lineStart = i;
				if (lines.Count==1) {
					width -= indent;
				}
			}
			if (text[i]==' ') {
				lastFallback = i;
			}
		}

		return lines;
	}
/// <summary>
/// Pads a given string to centered by a given padding char in a given width
/// </summary>
/// <param name="src">The original string</param>
/// <param name="width">The targeted width</param>
/// <param name="pad">The padding character to use, defaults to =</param>
/// <remarks>When the difference between the length of <paramref name="src"/> and <paramref name="width"/> is odd, there will be one less padding character on the left</remarks>
/// <returns>The padded string</returns>
public static string PadCentered(string src, int width, char pad = '=') {
	if (src.Length > width) {
		return src;
	}
	else {
		return new string(pad, (width - src.Length) / 2) + src + new string(pad, (width - src.Length + 1) / 2);
	}
}
}
}