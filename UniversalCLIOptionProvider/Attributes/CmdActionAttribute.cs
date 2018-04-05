using System;
 using System.Linq;
 using System.Reflection;
 
 namespace UniversalCommandlineInterface.Attributes {
    [AttributeUsage(AttributeTargets.Method)]
    public class CmdActionAttribute : Attribute {
       private bool _cached;
       public MethodInfo MyInfo;
       public string Name;
 
       public CmdActionAttribute(string name) => Name = name;
 
       
       public void LoadParametersAndAlias() {
          foreach (ParameterInfo parameterInfo in MyInfo.GetParameters()) {
             foreach (CmdParameterAttribute parameterAttribute in parameterInfo.GetCustomAttributes(typeof(CmdParameterAttribute), false).Cast<CmdParameterAttribute>()) {
                parameterAttribute.MyInfo = parameterInfo;
                parameterAttribute.LoadAlias();
             }
          }
       }
    }
 }