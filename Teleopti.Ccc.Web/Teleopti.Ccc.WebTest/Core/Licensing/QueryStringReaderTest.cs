using System.Collections.Specialized;
using MvcContrib.TestHelper.Fakes;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Core.Licensing;

namespace Teleopti.Ccc.WebTest.Core.Licensing
{
	public class QueryStringReaderTest
	{
		[Test]
		public void ShouldGetValue()
		{
			var context = new FakeCurrentHttpContext(new FakeHttpContext("test", null, null, new NameValueCollection { { "datasource", "test1" } }, null, null));
			var target = new QueryStringReader(context);

			target.GetValue("datasource").Should().Be.EqualTo("test1");
		}

		[Test]
		public void ShouldReturnNullIfNotFound()
		{
			var context = new FakeCurrentHttpContext(new FakeHttpContext("test", null, null, new NameValueCollection { { "datasource1", "test1" } }, null, null));
			var target = new QueryStringReader(context);

			target.GetValue("datasource").Should().Be.Null();
		}
	}
}