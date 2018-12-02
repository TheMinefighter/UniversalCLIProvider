using System;
using System.Data.SqlClient;
using System.Globalization;
using UniversalCLIProvider;
using Xunit;

namespace UnitTests {
public class Tests {
	public static TheoryData<string,Type,bool, object> GetValueFromStringData => new TheoryData<string,Type,bool, object>() {
		{"1",typeof(int),true,1},
		{"Sat, 01 Nov 2008 19:35:00",typeof(DateTime),true,new DateTime(2008,11,1,19,35,0)},
		{"1.1",typeof(double),true,1.1}
	};
	[Theory]
	[MemberData(nameof(GetValueFromStringData))]
	public void GetValueFromString(string src,Type expectedType,bool expectedSuccess, object expectedResult) {
		bool success = CommandlineMethods.GetValueFromString(src,expectedType,out object result);
		Assert.Equal(expectedSuccess,success);
		if (success) {
			Assert.Equal(expectedResult,result);
		}
	}
}
}