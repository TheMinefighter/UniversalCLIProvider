using System;
using System.Reflection;
using Newtonsoft.Json;
using UniversalCLIOptionProvider.Attributes;

namespace UniversalCLIOptionProvider {
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

      public static bool GetValueFromString(string source, Type expectedType, out object value) {
         value = null;
         switch (Type.GetTypeCode(expectedType)) {
            case TypeCode.SByte: {
               bool parsed = sbyte.TryParse(source, out sbyte tmp);
               value = tmp;
               return parsed;
            }
            case TypeCode.Byte: {
               bool parsed = byte.TryParse(source, out byte tmp);
               value = tmp;
               return parsed;
            }
            case TypeCode.Int16: {
               bool parsed = short.TryParse(source, out short tmp);
               value = tmp;
               return parsed;
            }
            case TypeCode.UInt16: {
               bool parsed = ushort.TryParse(source, out ushort tmp);
               value = tmp;
               return parsed;
            }
            case TypeCode.Int32: {
               bool parsed = int.TryParse(source, out int tmp);
               value = tmp;
               return parsed;
            }
            case TypeCode.UInt32: {
               bool parsed = uint.TryParse(source, out uint tmp);
               value = tmp;
               return parsed;
            }
            case TypeCode.Int64: {
               bool parsed = long.TryParse(source, out long tmp);
               value = tmp;
               return parsed;
            }
            case TypeCode.UInt64: {
               bool parsed = ulong.TryParse(source, out ulong tmp);
               value = tmp;
               return parsed;
            }
            case TypeCode.Boolean: {
               bool parsed = bool.TryParse(source, out bool tmp);
               value = tmp;
               return parsed;
            }
            case TypeCode.Single: {
               bool parsed = float.TryParse(source, out float tmp);
               value = tmp;
               return parsed;
            }
            case TypeCode.Double: {
               bool parsed = double.TryParse(source, out double tmp);
               value = tmp;
               return parsed;
            }
            case TypeCode.Decimal: {
               bool parsed = decimal.TryParse(source, out decimal tmp);
               value = tmp;
               return parsed;
            }
            case TypeCode.DateTime: {
               bool parsed = DateTime.TryParse(source, out DateTime tmp);
               value = tmp;
               return parsed;
            }
            case TypeCode.String: {
               value = source;
               return true;
            }
            case TypeCode.Char:
               value = source[0];
               return true;

            case TypeCode.Object: {
               if (expectedType.IsEnum) {
                  bool parseable = Enum.IsDefined(expectedType, source);
                  if (parseable) {
                     value = Enum.Parse(expectedType, source);
                  }

                  return parseable;
               }
               else if (source.StartsWith("{") && source.EndsWith("}")) {
                  try {
                     JsonConvert.DeserializeObject(source, expectedType);
                  }
                  catch (Exception) {
                     return false;
                  }

                  return true;
               }

               return false;
            }
            case TypeCode.Empty:
            case TypeCode.DBNull:
            default: return false;
         }
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

      public static bool WithDeclerationAllowed(this CmdParameterAttribute.CmdParameterUsage src) {
         switch (src) {
            case CmdParameterAttribute.CmdParameterUsage.RawValueWithDecleration:
               return true;
            case CmdParameterAttribute.CmdParameterUsage.NoRawsButDecleration:
               return true;
            case CmdParameterAttribute.CmdParameterUsage.DirectAliasOrDeclared:
               return true;
            case CmdParameterAttribute.CmdParameterUsage.OnlyDirectAlias:
               return false;
            case CmdParameterAttribute.CmdParameterUsage.Default:
               return false;
            default:
               throw new ArgumentOutOfRangeException(nameof(src), src, null);
         }
      }

      public static bool WithoutDeclerationAllowed(this CmdParameterAttribute.CmdParameterUsage src) {
         switch (src) {
            case CmdParameterAttribute.CmdParameterUsage.RawValueWithDecleration:
               return false;
            case CmdParameterAttribute.CmdParameterUsage.NoRawsButDecleration:
               return false;
            case CmdParameterAttribute.CmdParameterUsage.DirectAliasOrDeclared:
               return true;
            case CmdParameterAttribute.CmdParameterUsage.OnlyDirectAlias:
               return true;
            case CmdParameterAttribute.CmdParameterUsage.Default:
               return false;
            default:
               throw new ArgumentOutOfRangeException(nameof(src), src, null);
         }
      }

      
      public static bool RawAllowed(this CmdParameterAttribute.CmdParameterUsage src) {
         switch (src) {
            case CmdParameterAttribute.CmdParameterUsage.RawValueWithDecleration:
               return true;
            case CmdParameterAttribute.CmdParameterUsage.NoRawsButDecleration:
               return false;
            case CmdParameterAttribute.CmdParameterUsage.DirectAliasOrDeclared:
               return false;
            case CmdParameterAttribute.CmdParameterUsage.OnlyDirectAlias:
               return false;
            case CmdParameterAttribute.CmdParameterUsage.Default:
               return false;
            default:
               throw new ArgumentOutOfRangeException(nameof(src), src, null);
         }
      }
   }
}