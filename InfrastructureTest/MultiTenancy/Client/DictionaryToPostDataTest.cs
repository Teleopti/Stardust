using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Client
{
	public class DictionaryToPostDataTest
	{
		[Test]
		public void ShouldHandleSingleItem()
		{
			var data = new Dictionary<string, string>()
			{
				{"key", "value"}
			};
			var target = new DictionaryToPostData();
			target.Convert(data)
				.Should().Be.EqualTo("key=value");
		}

		[Test]
		public void ShouldHandleMultipleItem()
		{
			var data = new Dictionary<string, string>()
			{
				{"key1", "value1"},
				{"key2", "value2"}
			};
			var target = new DictionaryToPostData();
			target.Convert(data)
				.Should().Be.EqualTo("key1=value1&key2=value2");
		}
	}
}