using System.Collections.Generic;

namespace UniversalCommandlineInterface.Interpreters {
   public class ConfigurationNamespaceInterpreter {
      public ManagedConfigurationInterpreter ConfigurationInterpreter;
      public ConfigurationNamespaceInterpreter parent;

      public IList<ConfigurationNamespaceInterpreter> path {
         get {
            if (parent != null) {
               IList<ConfigurationNamespaceInterpreter> parentPath = parent.path;
               parentPath.Add(this);
               return parentPath;
            }

            else {
               return new List<ConfigurationNamespaceInterpreter> {this};
            }
         }
      }

      private bool _loaded;
   }
}