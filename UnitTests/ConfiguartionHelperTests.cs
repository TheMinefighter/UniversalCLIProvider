using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UniversalCLIProvider.Internals;
using Xunit;

namespace UnitTests {
public class ConfigurationHelperTests {
	//public static TheoryData<string,bool,string[],string>SplitIndexerData=new TheoryData<string, bool, string[], string>(){{"{\"abc\":4},\"xyz\",5]",true,new []{"{\"abc\":4}","\"xyz\"","5"},""}};

	public static TheoryData<string[], Type, bool, object[], PropertyInfo> ResolveTestData =
		new TheoryData<string[], Type, bool, object[], PropertyInfo> {
			{
				new[] {"3"}, typeof(string[]), true, new object[] {3},
				typeof(string[]).GetTypeInfo().GetUnderlyingTypes().SelectMany(x => x.GetRuntimeProperties())
					.First(x => x.GetIndexParameters().Length > 0 && x.PropertyType == typeof(string))
			}, {
				new[] {"\"Test\""}, typeof(Dictionary<string, int>), true, new object[] {"Test"},
				typeof(Dictionary<string, int>).GetTypeInfo().GetUnderlyingTypes().SelectMany(x => x.GetRuntimeProperties())
					.First(x => x.GetIndexParameters().Length > 0 && x.PropertyType == typeof(int))
			}
		};

	[Theory, MemberData(nameof(ResolveTestData))]
	public static void ResolveIndexerParametersTest(string[] parameters, Type indexerOwner, bool expectedSuccess,
		object[] expectedIndexerParameters, PropertyInfo expectedIndexer) {
		bool success = ConfigurationHelpers.ResolveIndexerParameters(parameters, indexerOwner.GetTypeInfo(), out object[] indexParameters,
			out PropertyInfo indexer);
		Assert.Equal(expectedSuccess, success);
		if (success) {
			Assert.Equal(expectedIndexerParameters, indexParameters);
			Assert.Equal(expectedIndexer.ToString(), indexer.ToString()); //Direct equality checks are not supported
		}

		//Array
	}


	[Theory, InlineData("5]", true, new[] {"5"}, ""),
	 InlineData("\"PossiblyErrorCreatingToken:P{},[]\"]", true, new[] {"\"PossiblyErrorCreatingToken:P{},[]\""}, ""),
	 InlineData("{\"MultipleTokens\":4},\"xyz\",5]", true, new[] {"{\"MultipleTokens\":4}", "\"xyz\"", "5"}, ""),
	 InlineData("{\"MultipleTokens\":4} ,\"with whitespaces and\",5]additional Data", true,
		 new[] {"{\"MultipleTokens\":4}", "\"with whitespaces and\"", "5"}, "additional Data"), InlineData("\"InvalidJsonToke", false, null, null),
	 InlineData("\"InvalidEndTerminator\")", false, null, null), InlineData("\"NoEndTerminator\"", false, null, null)]
	public static void SplitIndexersTest(string input, bool expectedSuccess, string[] expectedResult, string expectedRemainings) {
		bool success = ConfigurationHelpers.SplitIndexerArguments(input, out string[] result, out string remainingSrc);
		Assert.Equal(expectedSuccess, success);
		if (success) {
			Assert.Equal(expectedResult, result);
			Assert.Equal(expectedRemainings, remainingSrc);
		}
	}

	/*public class IndexerTests {
		public class A {
			public string this[]
		}
	}*/
}
}