using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Core.RequestContext;

namespace Teleopti.Ccc.WebTest.Core.RequestContext
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
	}
}
