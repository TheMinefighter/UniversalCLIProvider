namespace UniversalCommandlineInterface {
   public class InterpretingOptions {
      public static InterpretingOptions DefaultOptions = new InterpretingOptions {
         IgnoreParameterCase = true,
         PreferredArgumentPrefix = '/'
      };

      public ContextDefaultAction StandardDefaultAction;
      public bool IgnoreParameterCase = true;
      public string InteractiveOption = "Interactive";
      public char PreferredArgumentPrefix = '/';
      public string RootName = ".";
   }
}