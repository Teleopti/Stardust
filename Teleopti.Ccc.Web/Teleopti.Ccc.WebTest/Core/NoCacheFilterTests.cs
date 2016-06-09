using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Core.Startup;

namespace Teleopti.Ccc.WebTest.Core
{
	[TestFixture]
	public class NoCacheFilterTests
	{
		private readonly CacheControlHeaderValue _expectedCacheControlHeader;
		public NoCacheFilterTests()
		{
			_expectedCacheControlHeader = new CacheControlHeaderValue
			{
				NoCache = true,
				NoStore = true,
				MustRevalidate = true,
				Private = true
			};
		}

		[Test]
		public void ResponseShouldIncludeAllNoCacheHeaders()
		{
			var context = CreateExecutedContext();
			var filter = new NoCacheFilter();
			filter.OnActionExecuted(context);
			context.ActionContext.Response.Headers.CacheControl.Should().Be.EqualTo(_expectedCacheControlHeader);
			context.ActionContext.Response.Headers.Pragma.Should().Contain(new NameValueHeaderValue("no-cache"));
			context.ActionContext.Response.Content.Headers.Expires.GetValueOrDefault().DateTime.Should().Be.LessThanOrEqualTo(DateTime.UtcNow);
		}

		private HttpActionExecutedContext CreateExecutedContext()
		{
			return new HttpActionExecutedContext
			{
				ActionContext = new HttpActionContext
				{
					ControllerContext = new HttpControllerContext
					{
						Request = new HttpRequestMessage()
					}
				},
				Response = new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent("content...")
				}
			};
		}
	}
}
