using System.Collections.Generic;
using UniversalCLIOptionProvider.Attributes;

namespace UniversalCLIOptionProvider.Interpreters {
	public class ConfigurationNamespaceInterpreter {
		private CmdConfigurationNamespaceAttribute _attribute;
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


		public void Interpret() { }
	}
}