using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using UniversalCLIProvider;
using UniversalCLIProvider.OtherInternals;
using Xunit;

namespace UnitTests {
public class StaticTests {
	public static TheoryData<string, Type, bool, object> GetValueFromStringData => new TheoryData<string, Type, bool, object>() {
		{"1", typeof(int), true, 1},
		{"2018-12-04T19:11:44Z", typeof(DateTime), true, new DateTime(2018, 12, 04, 19, 11, 44, DateTimeKind.Utc)},
		{"1.1", typeof(double), true, 1.1},
		{"[1,2]", typeof(int[]), true, new int[] {1, 2}},
		{"Any", typeof(NumberStyles), true, NumberStyles.Any},
		{"393EF354-C45D-47EB-8A7C-32886DA20491", typeof(Guid?), true, new Guid("393EF354-C45D-47EB-8A7C-32886DA20491")},
		{"null", typeof(Guid?), true, null}
		
	};

	[Theory, MemberData(nameof(GetValueFromStringData))]
	public void GetValueFromString(string src, Type expectedType, bool expectedSuccess, object expectedResult) {
		bool success = CommandlineMethods.GetValueFromString(src, expectedType, out object result);
		Assert.Equal(expectedSuccess, success);
		if (success) {
			Assert.Equal(expectedResult, result);
		}
	}

	public static TheoryData<string[], Encoding> HexArgumentProcessingTestsData = new TheoryData<string[], Encoding> {
		{new string[] {"This", "is", "a test", "containing real \"quotes\" and \'single\' quotes"}, Encoding.ASCII},
		{new string[] {"This", "is", "UTF8 test", "🌕", "🏳️‍🌈"}, Encoding.UTF8}
	};

	[Theory, MemberData(nameof(HexArgumentProcessingTestsData))]
	public void HexArgumentProcessingTests(string[] args, Encoding encoding) {
		bool success =
			CommandlineMethods.ArgumentsFromHex(CommandlineMethods.ToHexArgumentString(args, encoding), out List<string> result);
		Assert.True(success);
		Assert.Equal(args, result);
	}
}
}