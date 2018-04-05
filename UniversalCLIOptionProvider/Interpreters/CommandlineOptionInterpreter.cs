using System;
using System.Reflection;
using UniversalCommandlineInterface.Attributes;

namespace UniversalCommandlineInterface.Interpreters {
   public class CommandlineOptionInterpreter {
      public string[] Args;
      public ConsoleIO ConsoleIO;

      //    internal int ArgsLengthMinus1; 
      public InterpretingOptions Options;

      public CommandlineOptionInterpreter(string[] args, InterpretingOptions options = null, ConsoleIO consoleIO = null) {
         Args = args;
         ConsoleIO = consoleIO ?? ConsoleIO.DefaultIO;
         Options = options ?? InterpretingOptions.DefaultOptions;
      }

      public void Interpret<T>(Action defaultAction = null) {
         Interpret(typeof(T), defaultAction);
      }

      public void Interpret(Type baseContext, Action defaultAction = null) {
         if (Args.Length == 0 && defaultAction != null) {
            defaultAction();
         }

         else {
            ContextInterpreter contextInterpreter = new ContextInterpreter(this) {
               MyContextAttribute =
                  baseContext.GetCustomAttribute(typeof(CmdContextAttribute)) as CmdContextAttribute,
               Offset = 0
            };

            contextInterpreter.MyContextAttribute.MyInfo = baseContext.GetTypeInfo();
            contextInterpreter.MyContextAttribute.LoadIfNot();
            if (Args.Length > 0 && contextInterpreter.IsParameterEqual(Options.InteractiveOption, Args[0])) {
               contextInterpreter.IncreaseOffset();
               contextInterpreter.InteractiveInterpreter(contextInterpreter.Offset < Args.Length);
            }

            contextInterpreter.Interpret();
         }
      }
   }
}