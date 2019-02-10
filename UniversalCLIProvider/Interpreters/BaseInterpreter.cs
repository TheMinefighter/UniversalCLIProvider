using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
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
			} while (current != null);
		}
	}

	/// <summary>
	///  A List off all Parents to the Top
	/// </summary>
	public IEnumerable<BaseInterpreter> Path => PathBottomUp.Reverse();


	protected BaseInterpreter([NotNull] CommandlineOptionInterpreter top, [NotNull] string name, int offset = 0) {
		Offset = offset;
		TopInterpreter = top ?? throw new ArgumentNullException(nameof(top));
		Parent = null;
		Name = name ?? throw new ArgumentNullException(nameof(name));
	}

	protected BaseInterpreter([NotNull] BaseInterpreter parent, [NotNull] string name, int offset = 0) {
		if (parent == null) throw new ArgumentNullException(nameof(parent));
		TopInterpreter = parent.TopInterpreter;
		Parent = parent;
		Offset = offset;
		Name = name ?? throw new ArgumentNullException(nameof(name));
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