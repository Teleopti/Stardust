using NUnit.Framework;
using SharpTestsEx;
using System.Runtime.Serialization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Util;


namespace Teleopti.Ccc.WebTest.Core
{
	[TestFixture]
	public class JsonSerializationExtensionsTest
	{
		[DataContract]
		internal class TestObject
		{
			[DataMember(Name = "hiredate", Order =2)]
			public DateOnly Date { get; set; }

			[DataMember(Order = 1)]
			public string Name { get; set; }

			[DataMember(Name = "array", Order = 3)]
			public string[] Months { get; set; }

			[DataMember(Name = "object", Order = 4)]
			public TestObject Other { get; set; }
		}


		[TestAttribute]
		public void ShouldSerializeToJsonString()
		{
			const string expectedResult = "{\"Name\":\"P\",\"hiredate\":\"2011-06-10\",\"array\":[\"Jan\",\"Feb\"],\"object\":{\"Name\":\"W\",\"hiredate\":\"0001-01-01\",\"array\":null,\"object\":null}}";

			var emp = new TestObject
			          	{Date = new  DateOnly(2011, 6,10), Name = "P", Months = new[] {"Jan", "Feb"}, Other = new TestObject {Name = "W"}};
			var result = emp.ToJson();

			result.Should().Be.EqualTo(expectedResult);
		}
	}
}