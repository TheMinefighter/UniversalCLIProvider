using System.Collections.Generic;
using System.Linq;
using UniversalCLIProvider.Internals;

namespace UniversalCLIProvider.Interpreters {
public abstract class BaseInterpreter {
	public string Name { get; }

	/// <summary>
	///  The index of the argument currently interpreted
	/// </summary>
	public int Offset { get; internal set; }

	/// <summary>
	///  The TopInterpreter contains much basic information for the interpretation, like the arguments
	/// </summary>
	public CommandlineOptionInterpreter TopInterpreter { get; }

	public BaseInterpreter Parent { get; }

	public IEnumerable<BaseInterpreter> PathBottomUp {
		get {
			BaseInterpreter current = this;
			do {
				yield return current;
				current = current.Parent;
			} while (current.Parent != null);
		}
	}

	/// <summary>
	///  A List off all Parents to the Top
	/// </summary>
	public IEnumerable<BaseInterpreter> Path => PathBottomUp.Reverse();


	protected BaseInterpreter(CommandlineOptionInterpreter top, int offset = 0) {
		Offset = offset;
		TopInterpreter = top;
		Parent = null;
	}

	protected BaseInterpreter(BaseInterpreter parent, string name, int offset = 0) {
		TopInterpreter = parent.TopInterpreter;
		Parent = parent;
		Offset = offset;
		Name = name;
	}

	public override string ToString() => string.Join(" ", Path.Select(x => x.Name));

	/// <summary>
	/// </summary>
	/// <returns>Whether the end of the args has been reached</returns>
	public bool IncreaseOffset() {
		Offset++;
		return Offset >= TopInterpreter.Args.Length;
	}

	/// <summary>
	///  Resets the Interpreter
	/// </summary>
	internal void Reset() {
		Offset = 0;
	}

	/// <summary>
	///  Starts the interpretation
	/// </summary>
	/// <returns>Whether the interpretation was successful</returns>
	internal abstract bool Interpret();


	internal bool IsParameterEqual(string expected, string given = null, string expectedShortForm = null, bool allowPrefixFree = false) =>
		CommandlineMethods.IsParameterEqual(expected, given ?? TopInterpreter.Args[Offset], TopInterpreter.Options.IgnoreParameterCase,
			expectedShortForm, allowPrefixFree);
}
}