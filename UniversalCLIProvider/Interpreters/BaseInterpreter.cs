using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UniversalCLIProvider.Attributes;

namespace UniversalCLIProvider.Interpreters {
public abstract class BaseInterpreter {
	public string Name { get; }

	/// <summary>
	///  The index of the argument currently interpreted
	/// </summary>
	public int Offset { get; internal set; }

	public CommandlineOptionInterpreter TopInterpreter { get; }
	public BaseInterpreter DirectParent { get; }
	public List<BaseInterpreter> ParentInterpreters { get; }

	public List<string> Path {
		get {
			if (Name == null) {
				return new List<string> {TopInterpreter.TopContext.Name};
			}

			List<string> tmpList = DirectParent.Path;
			tmpList.Add(Name);
			return tmpList;
		}
	}

	private BaseInterpreter() { }

	protected BaseInterpreter(CommandlineOptionInterpreter top, int offset = 0) {
		Offset = offset;
		ParentInterpreters = new List<BaseInterpreter> {this};
		TopInterpreter = top;
		DirectParent = null;
	}

	protected BaseInterpreter(BaseInterpreter parent, string name, int offset = 0) {
		TopInterpreter = parent.TopInterpreter;
		DirectParent = parent;
		List<BaseInterpreter> parentInterpreters = parent.ParentInterpreters.ToList();
		parentInterpreters.Add(parent);
		ParentInterpreters = parentInterpreters;
		Offset = offset;
		Name = name;
	}

	public override string ToString() => string.Join(" ", Path);

	/// <summary>
	/// </summary>
	/// <returns>Whether the end of the args has been reached</returns>
	public bool IncreaseOffset() {
		Offset++;
		return Offset >= TopInterpreter.Args.Length;
	}

	internal void Reset() {
		Offset = 0;
	}

	internal abstract void PrintHelp();
	internal abstract bool Interpret(bool printErrors = true);


	public void PrintError(string argName = null) {
		Console.Write(
			$"An error occurred while parsing argument {argName ?? Name} use {TopInterpreter.Options.PreferredArgumentPrefix}? for description");
	}

	internal bool IsParameterDeclaration(out CmdParameterAttribute found,
		IEnumerable<CmdParameterAttribute> possibleParameters, string search, bool allowPrefixFree = false) {
		foreach (CmdParameterAttribute cmdParameterAttribute in possibleParameters) {
			if (IsParameterEqual(cmdParameterAttribute.Name, search, cmdParameterAttribute.ShortForm, allowPrefixFree)) {
				found = cmdParameterAttribute;
				return true;
			}
		}

		found = null;
		return false;
	}

	internal bool IsAlias(CmdParameterAttribute expectedAliasType, out object value, string source = null) {
		foreach (CmdParameterAliasAttribute cmdParameterAlias in expectedAliasType.ParameterAliases) {
			if (IsParameterEqual(cmdParameterAlias.Name, source ?? TopInterpreter.Args[Offset], cmdParameterAlias.ShortForm)) {
				value = cmdParameterAlias.Value;
				return true;
			}
		}

		value = null;
		return false;
	}

	internal bool IsParameterEqual(string expected, string given, string expectedShortForm = null, bool allowPrefixFree = false) =>
		IsParameterEqual(expected, given, TopInterpreter.Options.IgnoreParameterCase, expectedShortForm, allowPrefixFree);

	internal static bool IsParameterEqual([NotNull] string expected, [NotNull] string given, bool ignoreCase, string expectedShortForm = null,
		bool allowPrefixFree = false) {
		if (ignoreCase) {
			given = given.ToLower();
			expected = expected.ToLower();
		}

		if (!(expectedShortForm is null) && ('/' + expectedShortForm == given || '-' + expectedShortForm == given)) {
			return false;
		}

		return '/' + expected == given || "--" + expected == given || allowPrefixFree && expected == given;
	}
}
}