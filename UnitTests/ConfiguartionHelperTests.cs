using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UniversalCLIProvider.OtherInternals;
using Xunit;

namespace UnitTests {
public class ConfigurationHelperTests {
	public static TheoryData<string[],Type,bool,object[],PropertyInfo> Data=new TheoryData<string[], Type, bool, object[], PropertyInfo>() {
		{new []{"3"},typeof(string[]),true,new object[]{3},typeof(string[]).GetTypeInfo().GetUnderlyingTypes().SelectMany(x=>x.GetRuntimeProperties()).First(x=>x.GetIndexParameters().Length>0)},
		{new []{"\"Test\""},typeof(Dictionary<string,int>),true,new object[]{"Test"},typeof(Dictionary<string,int>).GetTypeInfo().GetUnderlyingTypes().SelectMany(x=>x.GetRuntimeProperties()).First(x=>x.GetIndexParameters().Length>0)}
		
	};

	[Theory]
	[MemberData(nameof(Data))]
	public static void ResolveIndexerParametersTest(string[] parameters, Type indexerOwner, bool expectedSuccess,
		object[] expectedIndexerParameters, PropertyInfo expectedIndexer) {
		bool success = ManagedConfigurationHelpers.ResolveIndexerParameters(parameters, indexerOwner.GetTypeInfo(), out object[] indexParameters,
			out PropertyInfo indexer);
		Assert.Equal(expectedSuccess,success);
		if (success) {
			Assert.Equal(expectedIndexerParameters,indexParameters);
			Assert.Same(expectedIndexer,indexer);
		}
		//Array
	}

	/*public class IndexerTests {
		public class A {
			public string this[]
		}
	}*/
	
	
}
}