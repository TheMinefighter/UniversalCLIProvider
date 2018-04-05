using System;

namespace UniversalCommandlineInterface.Attributes {
   [AttributeUsage(AttributeTargets.Class)]
   public class CmdConfigurationNamespaceAttribute : Attribute {
      public string ExtendedHelp;
      public string Help;
      public bool IsReadonly;
      public string Name;

      public CmdConfigurationNamespaceAttribute(bool isReadonly, string name, string help, string extendedHelp) {
         IsReadonly = isReadonly;
         Name = name;
         Help = help;
         ExtendedHelp = extendedHelp;
      }
   }
}