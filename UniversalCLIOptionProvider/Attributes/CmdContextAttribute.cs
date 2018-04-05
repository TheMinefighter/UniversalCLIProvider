using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UniversalCommandlineInterface.Attributes {
   [AttributeUsage(AttributeTargets.Class)]
   public class CmdContextAttribute : Attribute {
      private bool _loaded;
      public IList<CmdActionAttribute> ctxActions = new List<CmdActionAttribute>();
      public IList<CmdParameterAttribute> ctxParameters = new List<CmdParameterAttribute>();
      public TypeInfo MyInfo;
      public ContextDefaultAction DefaultAction;
      public string Name;
      public IList<CmdContextAttribute> subCtx = new List<CmdContextAttribute>();

      public CmdContextAttribute(string name,ContextDefaultAction defaultAction = null ) {
         defaultAction = defaultAction ?? ContextAction.PrintHelp;
         DefaultAction = defaultAction;
         Name = name;
      }

      public CmdContextAttribute() {
      }

      
      public void LoadIfNot() {
         if (!_loaded) {
            Load();

            _loaded = true;
         }
      }

      internal void Load() {
         foreach (TypeInfo myInfoDeclaredNestedType in MyInfo.DeclaredNestedTypes) {
            CmdContextAttribute contextAttribute = myInfoDeclaredNestedType.GetCustomAttribute<CmdContextAttribute>();
            if (contextAttribute != null) {
               contextAttribute.MyInfo = myInfoDeclaredNestedType;
               subCtx.Add(contextAttribute);
            }
         }

         IEnumerable<MemberInfo> members = MyInfo.DeclaredFields.Cast<MemberInfo>().Concat(MyInfo.DeclaredProperties);
         foreach (MemberInfo memberInfo in members) {
            CmdParameterAttribute parameterAttribute = memberInfo.GetCustomAttribute<CmdParameterAttribute>();
            if (parameterAttribute != null) {
               parameterAttribute.MyInfo = memberInfo;
               ctxParameters.Add(parameterAttribute);
            }
         }

         foreach (MethodInfo methodInfo in MyInfo.DeclaredMethods) {
            CmdActionAttribute actionAttribute = methodInfo.GetCustomAttribute<CmdActionAttribute>();
            if (actionAttribute != null) {
               actionAttribute.MyInfo = methodInfo;
               ctxActions.Add(actionAttribute);
            }
         }
      }
   }
}