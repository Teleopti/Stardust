using System.IO;
using System.Web;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Web;

namespace Teleopti.Ccc.InfrastructureTest.Web
{
	[TestFixture]
	public class CurrentHttpContextTest
	{
		[Test]
		public void ShouldReturnCurrentHttpContext()
		{
			var context = new HttpContext(new HttpRequest("file", "http://my.url.com/", "querystring"), new HttpResponse(new StringWriter()));
			HttpContext.Current = context;
			new CurrentHttpContext().Current().Request.Url.ToString().Should().Be("http://my.url.com/");
			HttpContext.Current = null;
		}

		[Test]
		public void ShouldReturnNullIfNoContext()
		{
			HttpContext.Current = null;
			new CurrentHttpContext().Current().Should().Be(null);
		}
	}
}
