using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using UniversalCLIProvider.Attributes;

namespace UniversalCLIProvider.Interpreters {
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

		public bool BinaryReprocessor(Encoding encoding = null) {
			encoding = encoding ?? Encoding.UTF8;
			int typicalEncodingLength = encoding.GetByteCount("s");
			string arg = Args[1];
			int currentOffset = 0;
			int count = 0;
			List<string> newArgs = new List<string>(16);
			while (true) {
				if (arg.Length == currentOffset) {
					break;
				}

				if (arg.Length < currentOffset + 8) {
					Console.WriteLine($"The binary data is not long enough for evaluating the proposed length of argument {count}");
					return false;
				}

				int proposedLength;
				try {
					proposedLength = int.Parse(arg.Substring(currentOffset,8), NumberStyles.HexNumber);
				}
				catch (Exception) {
					Console.WriteLine($"Error while parsing string length of argument {count}");
					return false;
				}

				currentOffset += 8;
				if (arg.Length < currentOffset + proposedLength * 2) {
					Console.WriteLine($"The binary data is not long enough for content of argument {count}");
					return false;
				}

				byte[] rawArgument = new byte[proposedLength];
				for (int i = 0; i < proposedLength; i++) {
					//TODO Can be optimized later (dual loop)
					rawArgument[i] = byte.Parse(arg.Substring(currentOffset, 2), NumberStyles.HexNumber);
					currentOffset += 2;
				}

				count++;
				newArgs.Add(encoding.GetString(rawArgument));
			}

			Args = newArgs.ToArray();
			return true;
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
				if (Args.Length > 0) {
					if (contextInterpreter.IsParameterEqual(Options.HexOption, Args[0])) {
						if (BinaryReprocessor()) {
							Interpret(baseContext, defaultAction);
						}

						return;
					}

					if (contextInterpreter.IsParameterEqual(Options.InteractiveOption, Args[0])) {
						contextInterpreter.IncreaseOffset();
						contextInterpreter.InteractiveInterpreter(contextInterpreter.Offset < Args.Length);
					}
				}

				contextInterpreter.Interpret();
			}
		}
	}
}