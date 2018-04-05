using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using UniversalCLIOptionProvider.Attributes;

namespace UniversalCLIOptionProvider.Interpreters {
   public class ActionInterpreter : BaseInterpreter ,IDisposable {
      private bool _cached;
      public CmdActionAttribute MyActionAttribute;
      private IEnumerable<CmdParameterAttribute> parameters;

      public ActionInterpreter(CommandlineOptionInterpreter top, int i) : base(top) {
         i++;
      }

      public ActionInterpreter(CmdActionAttribute myActionAttribute, BaseInterpreter parent, int offset = 0) : base(parent,
         myActionAttribute.Name, offset) => MyActionAttribute = myActionAttribute;

      internal override void PrintHelp() {
      }

      internal void LoadParameters() {
         if (!_cached) {
            LoadParametersWithoutCache();
            _cached = true;
         }
      }

      private void LoadParametersWithoutCache() {
         parameters = MyActionAttribute.MyInfo.GetParameters().Select(x =>
               new KeyValuePair<ParameterInfo, Attribute>(x, x.GetCustomAttribute(typeof(CmdParameterAttribute))))
            .Where(x => x.Value != null).Select(x => {
               CmdParameterAttribute cmdParameterAttribute = x.Value as CmdParameterAttribute;
               cmdParameterAttribute.MyInfo = x.Key;
               cmdParameterAttribute.ParameterAliases =
                  x.Key.GetCustomAttributes(typeof(CmdParameterAliasAttribute)).Cast<CmdParameterAliasAttribute>();
               cmdParameterAttribute.LoadAlias();
               return cmdParameterAttribute;
            });
      }

      internal override bool Interpret(bool printErrors = true) {
         LoadParameters();
         //Dictionary<CmdParameterAttribute, object> invokationArguments = new Dictionary<CmdParameterAttribute, object>();
         Dictionary<CmdParameterAttribute, object> invokationArguments;
         if (Offset != TopInterpreter.Args.Length) {
            if (!GetValues(out invokationArguments)) {
               return true;
            }
         }
         else {
            invokationArguments = new Dictionary<CmdParameterAttribute, object>();
         }

         ParameterInfo[] allParameterInfos = MyActionAttribute.MyInfo.GetParameters();
         object[] invokers = new object[allParameterInfos.Length];
         bool[] invokersDeclared = new bool[allParameterInfos.Length];
         foreach (KeyValuePair<CmdParameterAttribute, object> invokationArgument in invokationArguments) {
            int position = (invokationArgument.Key.MyInfo as ParameterInfo).Position;
            invokers[position] = invokationArgument.Value;
            invokersDeclared[position] = true;
         }

         for (int i = 0; i < allParameterInfos.Length; i++) {
            if (!invokersDeclared[i]) {
               if (allParameterInfos[i].HasDefaultValue) {
                  invokers[i] = allParameterInfos[i].DefaultValue;
                  invokersDeclared[i] = true;
               }
               else {
                  //throw
                  return false;
               }
            }
         }

         MyActionAttribute.MyInfo.Invoke(null, invokers);
         return true;
         //throw new NotImplementedException();
      }

      /// <summary>
      ///    reads all arguments
      /// </summary>
      /// <param name="invokationArguments"></param>
      /// <returns></returns>
      private bool GetValues(out Dictionary<CmdParameterAttribute, object> invokationArguments) {
         invokationArguments = new Dictionary<CmdParameterAttribute, object>();
         Type iEnumerableCache = null;
         // value = null;
         while (true) {
            if (IsParameterDeclaration(out CmdParameterAttribute found)) {
               if (IncreaseOffset()) {
                  //TODO What if Empty Array
                  //throw

                  return false;
               }

               Type parameterType = (found.MyInfo as ParameterInfo).ParameterType;
               if (IsAlias(found, out object aliasValue)) {
                  invokationArguments.Add(found, aliasValue);
               }
               else if (found.Usage.RawAllowed() &&
                        CommandlineMethods.GetValueFromString(TopInterpreter.Args[Offset], parameterType, out object given)) {
                  invokationArguments.Add(found, given);
               }
               else if (found.Usage.RawAllowed() && parameterType.GetInterfaces().Any(x => {
                              bool isIEnumerable = x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>);
                              iEnumerableCache = x;
                              return isIEnumerable;
                           }
                        )
               ) {
                  #region Based upon https://stackoverflow.com/a/2493258/6730162 last access 04.03.2018

                  Type realType = iEnumerableCache.GetGenericArguments()[0];
                  Type specificList = typeof(List<>).MakeGenericType(realType);
                  ConstructorInfo ci = specificList.GetConstructor(new Type[] { });
                  object listOfRealType = ci.Invoke(new object[] { });

                  #endregion

                  MethodInfo addMethodInfo = specificList.GetMethod("Add");
                  Offset--;
                  while (true) {
                     if (IncreaseOffset()) {
                        break;
                     }

                     if (IsAlias(out CmdParameterAttribute tmpParameterAttribute, out object _) &&
                         tmpParameterAttribute.Usage.WithoutDeclerationAllowed() ||
                         IsParameterDeclaration(out CmdParameterAttribute _)) {
                        break;
                     }

                     if (!CommandlineMethods.GetValueFromString(TopInterpreter.Args[Offset], realType, out object toAppend)) {
                        //throw
                        return false;
                     }
                     else {
                        addMethodInfo.Invoke(listOfRealType, new object[] {toAppend});
                     }
                  }

                  if (new Type[] {
                     typeof(List<>), typeof(IList<>), typeof(ICollection<>), typeof(IEnumerable<>), typeof(IReadOnlyList<>),
                     typeof(IReadOnlyCollection<>), typeof(ReadOnlyCollection<>)
                  }.Select(x => x.MakeGenericType(realType)).Contains(parameterType)) {
                     invokationArguments.Add(found, listOfRealType);
                  }
                  else if (parameterType == realType.MakeArrayType()) {
                     object arrayOfRealType = typeof(Enumerable).GetMethod("ToArray").MakeGenericMethod(realType)
                        .Invoke(null, new object[] {listOfRealType});
                     invokationArguments.Add(found, arrayOfRealType);
                  }
                  else {
                     ConstructorInfo constructorInfo;
                     try {
                        constructorInfo = parameterType.GetConstructor(new Type[] {typeof(IEnumerable<>).MakeGenericType(realType)});
                     }
                     catch (Exception e) {
                        Console.WriteLine(e);
                        throw;
                     }

                     constructorInfo.Invoke(new object[] {listOfRealType});
                  }
               }

               else {
                  //throw
                  return false;
               }
            }
            else if (IsAlias(out found, out object aliasValue) && found.Usage.WithoutDeclerationAllowed()) {
               invokationArguments.Add(found, aliasValue);
            }
            else {
               //throw
               return false;
            }

            if (IncreaseOffset()) {
               return true;
            }
         }

/*
         foreach (CmdParameterAttribute cmdParameterAttribute in parameters) {
            if (cmdParameterAttribute.AvailableWithoutAlias && CommandlineMethods.IsParameterEqual(cmdParameterAttribute.Name, search)) {
               if (!CommandlineMethods.GetAliasValue(out value, cmdParameterAttribute, TopInterpreter.Args.ElementAt(Offset + 1))) {
                  // ReSharper disable once UseMethodIsInstanceOfType
                  // ReSharper disable once UseIsOperator.1
                  Type expectedType = cmdParameterAttribute.GetType();

                  if (typeof(IEnumerable<>).IsAssignableFrom(expectedType)) {
                     int i = 1;
                     Type realtType = expectedType.GetGenericArguments()[0];

                     Type listGenericType = typeof(List<>);

                     Type list = listGenericType.MakeGenericType(realtType);
                     ConstructorInfo ci = list.GetConstructor(new Type[] { });
                     object listInt = ci.Invoke(new object[] { });
                     MethodInfo addMethodInfo = typeof(List<>).GetMethod("Add").MakeGenericMethod(realtType);
                     while (CommandlineMethods.GetValueFromString(TopInterpreter.Args.ElementAt(Offset + i), expectedType,
                        out value)) {
                        i++;
                     }
                  }
                  else {
                     if (!CommandlineMethods.GetValueFromString(TopInterpreter.Args.ElementAt(Offset + 1), expectedType,
                        out value)) {
                        PrintHelp();
                        return true;
                     }

                     invokationArguments.Add(cmdParameterAttribute, value);
                  }

                  break;
               }
               else {
                  invokationArguments.Add(cmdParameterAttribute, value);
                  break;
               }
            }

            if (!cmdParameterAttribute.DeclerationNeeded) {
               if (!CommandlineMethods.GetAliasValue(out value, cmdParameterAttribute, search)) {
                  PrintHelp();
                  return true;
               }
               else {
                  invokationArguments.Add(cmdParameterAttribute, value);
                  break;
               }
            }

            //   invokationArguments.Add();
         }
*/
      }

      internal bool IsParameterDeclaration(out CmdParameterAttribute found, string search = null) =>
         IsParameterDeclaration(out found, parameters, search ?? TopInterpreter.Args[Offset]);

//      internal bool IsAlias(CmdParameterAttribute expectedAliasType, out object value, string source = null) {
//         return base.IsAlias(expectedAliasType, out value, source ?? TopInterpreter.Args[Offset]);
//      }

      internal bool IsAlias(out CmdParameterAttribute aliasType, out object value, string source = null) {
         foreach (CmdParameterAttribute cmdParameterAttribute in parameters) {
            if (IsAlias(cmdParameterAttribute, out value, source ?? TopInterpreter.Args[Offset])) {
               aliasType = cmdParameterAttribute;
               return true;
            }
         }

         aliasType = null;
         value = null;
         return false;
      }

      public void Dispose() {
         _cached = false;
         MyActionAttribute = null;
      }
   }
}