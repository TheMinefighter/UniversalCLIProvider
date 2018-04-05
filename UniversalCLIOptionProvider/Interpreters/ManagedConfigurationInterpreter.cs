using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UniversalCommandlineInterface.Attributes;

namespace UniversalCommandlineInterface.Interpreters {
   public class ManagedConfigurationInterpreter : BaseInterpreter {
      private Dictionary<CmdConfigurationNamespaceAttribute, MemberInfo> _namespaces;
      private Dictionary<CmdConfigurationValueAttribute, MemberInfo> _values;
      private string _configurationRootName;
      private bool _rootRequired;
      private string[] _contextTrace;
      protected ManagedConfigurationInterpreter(CommandlineOptionInterpreter top, int offset = 0) : base(top, offset) {
      }

      protected ManagedConfigurationInterpreter(BaseInterpreter parent, string name, int offset = 0) : base(parent, name, offset) {
      }

      internal override void PrintHelp() {
         int maxlength =
            new int[] {_namespaces.Keys.Select(x => x.Help.Length).Max(), _values.Keys.Select(x => x.Help.Length).Max()}.Max() + 1;
         StringBuilder ConsoleStack = new StringBuilder();
         TopInterpreter.ConsoleIO.WriteLineToConsole($"Syntax: {Path} ");
         foreach (CmdConfigurationNamespaceAttribute cmdConfigurationNamespaceAttribute in _namespaces.Keys) {
            //  TopInterpreter.ConsoleIO.WriteLineToConsole
            ConsoleStack.Append(cmdConfigurationNamespaceAttribute.Name.PadRight(maxlength) +
                                cmdConfigurationNamespaceAttribute.Help);
            ConsoleStack.Append(Environment.NewLine);
         }

         TopInterpreter.ConsoleIO.WriteToConsole(ConsoleStack.ToString());
         throw new NotImplementedException();
      }


      internal override bool Interpret(bool printErrors = true) {
         if (Offset==TopInterpreter.Args.Length||IsParameterEqual("?", TopInterpreter.Args[Offset])) {
            if (printErrors) {
               PrintHelp();
            }
            else {
               return false;
            }
         }

         _contextTrace = TopInterpreter.Args[Offset].Split('.').Select(x=>x.ToLower()).ToArray();
         if (_rootRequired) {
            if (_contextTrace[0].Equals(_configurationRootName,StringComparison.OrdinalIgnoreCase)) {
            
            }
         }

         return true;
      }
   }
}