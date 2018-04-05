using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UniversalCLIOptionProvider.Attributes {
   [AttributeUsage(AttributeTargets.GenericParameter | AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
   //TODO Add Defaults
   public class CmdParameterAttribute : Attribute {
      public enum CmdParameterUsage {
         RawValueWithDecleration,
         NoRawsButDecleration,
         DirectAliasOrDeclared,
         OnlyDirectAlias,
         Default
      }

      private bool _loaded;

//      public bool AvailableWithoutAlias;
//      private bool _AvailableWithoutAliasExplictitlyDeclared;
//      public bool DeclerationNeeded;
//      private bool _DeclerationNeededExplicitlyDeclared;
      public bool IsParameter;
      public ICustomAttributeProvider MyInfo;
      public string Name;
      public IEnumerable<CmdParameterAliasAttribute> ParameterAliases;
      internal CmdParameterUsage Usage;


      public CmdParameterAttribute(string name, CmdParameterUsage usage = CmdParameterUsage.Default) {
         Name = name;
         Usage = usage;
//         AvailableWithoutAlias = availableWithoutAlias??true;
//         _AvailableWithoutAliasExplictitlyDeclared = availableWithoutAlias != null;
//         DeclerationNeeded = declerationNeeded??true;
//         _DeclerationNeededExplicitlyDeclared = declerationNeeded != null;
      }

      public void LoadAlias() {
         if (!_loaded) {
            ParameterAliases = MyInfo.GetCustomAttributes(typeof(CmdParameterAliasAttribute), false).Cast<CmdParameterAliasAttribute>();
            if (Usage == CmdParameterUsage.Default) {
               Usage = ParameterAliases.Any() ? CmdParameterUsage.OnlyDirectAlias : CmdParameterUsage.RawValueWithDecleration;
            }

            _loaded = true;
         }
      }
   }
}