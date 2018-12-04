using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UniversalCLIProvider.Attributes;

namespace UniversalCLIProvider {
public static class CommandlineMethods {
	public static string ToHexArgumentString(string[] originalArguments, Encoding encoding = null) {
		encoding = encoding ?? Encoding.UTF8;
		int typicalEncodingLength = encoding.GetByteCount("s");
		StringBuilder stringBuilder =
			new StringBuilder(typicalEncodingLength * originalArguments.Sum(x => x.Length) + originalArguments.Length * 8);
		foreach (string argument in originalArguments) {
			stringBuilder.Append(argument.Length.ToString("x8"));
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
	private static Type[] _nullableOverridenTypes = {typeof(DateTime?), typeof(DateTimeOffset?), typeof(TimeSpan?), typeof(Guid?)};

	private static Type[] _overridenTypes =
		{typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan), typeof(string), typeof(Guid), typeof(Uri)};

	/// <summary>
	/// 
	/// </summary>
	/// <param name="source"></param>
	/// <param name="expectedType"></param>
	/// <param name="value"></param>
	/// <remarks>For supported primary types see https://github.com/JamesNK/Newtonsoft.Json/blob/master/Src/Newtonsoft.Json/Utilities/ConvertUtils.cs (@ internal enum PrimitiveTypeCode) </remarks>
	/// <remarks>Types with modified support:</remarks>
	/// <remarks><see cref="Enum"/>: Allowing Enum.Parse (e.g. <c>NumberStyles.Any</c>, <c>Any</c>) when enum is the explicit target type</remarks>
	/// <remarks><see cref="DateTime"/>, <see cref="TimeSpan"/>, <see cref="DateTimeOffset"/>, <see cref="Uri"/>, <see cref="Guid"/>,<see cref="string"/>Allowed without quotation marks when explicit</remarks>
	/// <remarks><see cref="Nullable{T}"/> types also supported</remarks>
	/// <returns>Whether parsing was successful</returns>
	public static bool GetValueFromString(string source, Type expectedType, out object value) {
		value = null;
		if (expectedType.IsEnum ||
		    (expectedType.IsGenericType&& expectedType.GetGenericTypeDefinition() == typeof(Nullable<>) && expectedType.GenericTypeArguments.Length == 1 &&
		     expectedType.GenericTypeArguments[0].IsEnum) && Enum.IsDefined(expectedType, source)) {
			value = Enum.Parse(expectedType, source);
			return true;
		}

		if (_overridenTypes.Contains(expectedType)) {
			if (!source.StartsWith("\"")) {
				source = "\"" + source + "\"";
			}
		}

		if (_nullableOverridenTypes.Contains(expectedType)) {
			if (source == "null") {
				return true;
			}

			if (!source.StartsWith("\"")) {
				source = "\"" + source + "\"";
			}
		}
		try {
			value = JsonConvert.DeserializeObject(source, expectedType, new IsoDateTimeConverter() {DateTimeFormat = "dd/MM/yyyy"});
		}
		catch (Exception) {
			return false;
		}

		return true;
	}

	public static TypeInfo GetTypeInfo(MemberInfo member) {
		switch (member) {
			case PropertyInfo propertyInfo:
				propertyInfo.PropertyType.GetTypeInfo();
				break;
			case FieldInfo fieldInfo:
				fieldInfo.FieldType.GetTypeInfo();
				break;
		}

		throw new ArgumentOutOfRangeException(nameof(member), member, "Must be  or FieldInfo");
	}

	public static bool WithDeclarationAllowed(this CmdParameterUsage src) {
		switch (src) {
			case CmdParameterUsage.RawValueWithDeclaration:
				return true;
			case CmdParameterUsage.NoRawsButDeclaration:
				return true;
			case CmdParameterUsage.DirectOrDeclaredAlias:
				return true;
			case CmdParameterUsage.OnlyDirectAlias:
				return false;
			case CmdParameterUsage.Default:
				return false;
			default:
				throw new ArgumentOutOfRangeException(nameof(src), src, null);
		}
	}

	public static bool WithoutDeclarationAllowed(this CmdParameterUsage src) {
		switch (src) {
			case CmdParameterUsage.RawValueWithDeclaration:
				return false;
			case CmdParameterUsage.NoRawsButDeclaration:
				return false;
			case CmdParameterUsage.DirectOrDeclaredAlias:
				return true;
			case CmdParameterUsage.OnlyDirectAlias:
				return true;
			case CmdParameterUsage.Default:
				return false;
			default:
				throw new ArgumentOutOfRangeException(nameof(src), src, null);
		}
	}


	public static bool RawAllowed(this CmdParameterUsage src) {
		switch (src) {
			case CmdParameterUsage.RawValueWithDeclaration:
				return true;
			case CmdParameterUsage.NoRawsButDeclaration:
				return false;
			case CmdParameterUsage.DirectOrDeclaredAlias:
				return false;
			case CmdParameterUsage.OnlyDirectAlias:
				return false;
			case CmdParameterUsage.Default:
				return false;
			default:
				throw new ArgumentOutOfRangeException(nameof(src), src, null);
		}
	}
}
}