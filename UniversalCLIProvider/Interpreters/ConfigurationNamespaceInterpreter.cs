using System.Collections.Generic;
using UniversalCLIProvider.Attributes;

namespace UniversalCLIProvider.Interpreters {
public class ConfigurationNamespaceInterpreter : BaseInterpreter {
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

	public ConfigurationNamespaceInterpreter(CommandlineOptionInterpreter top, int offset = 0) : base(top, offset) { }
	public ConfigurationNamespaceInterpreter(BaseInterpreter parent, string name, int offset = 0) : base(parent, name, offset) { }
	internal override bool Interpret(bool printErrors = true) => throw new System.NotImplementedException();
}
}