using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Core.Startup;
using Teleopti.Ccc.WebTest.Filters;

namespace Teleopti.Ccc.WebTest.Core
{
	[TestFixture]
	public class BlockOtherOriginHttpFilterTests
	{
		[Test]
		public void ShouldBlockOtherOrigin()
		{
			var context = CreateExecutedContext();
			context.Request.RequestUri = new Uri("http://wfmserver1/teleoptiwfm");
			context.Request.Headers.Add("Origin","http://burp/test/2");
			var filter = new CsrfFilterHttp();
			filter.OnActionExecuting(context);

			context.Response.IsSuccessStatusCode.Should().Be.False();
		}

		[Test]
		public void ShouldNotBlockOtherOriginWhenIPv4Host()
		{
			var context = CreateExecutedContext();
			context.Request.RequestUri = new Uri("http://wfmserver1/teleoptiwfm");
			context.Request.Headers.Add("Origin", "http://192.168.0.1/test/2");
			var filter = new CsrfFilterHttp();
			filter.OnActionExecuting(context);

			context.Response.IsSuccessStatusCode.Should().Be.True();
		}

		[Test]
		public void ShouldNotBlockOtherOriginWhenHostIPv4()
		{
			var context = CreateExecutedContext();
			context.Request.RequestUri = new Uri("http://192.168.0.1/teleoptiwfm");
			context.Request.Headers.Add("Origin", "http://wfmserver1/test/2");
			var filter = new CsrfFilterHttp();
			filter.OnActionExecuting(context);

			context.Response.IsSuccessStatusCode.Should().Be.True();
		}

		[Test]
		public void ShouldBlockOtherReferrer()
		{
			var context = CreateExecutedContext();
			context.Request.RequestUri = new Uri("http://wfmserver1/teleoptiwfm");
			context.Request.Headers.Referrer = new Uri("http://burp/test/2");
			var filter = new CsrfFilterHttp();
			filter.OnActionExecuting(context);

			context.Response.IsSuccessStatusCode.Should().Be.False();
		}

		[Test]
		public void ShouldNotBlockOtherReferrerWhenIPv4Host()
		{
			var context = CreateExecutedContext();
			context.Request.RequestUri = new Uri("http://wfmserver1/teleoptiwfm");
			context.Request.Headers.Referrer = new Uri("http://192.168.0.1/test/2");
			var filter = new CsrfFilterHttp();
			filter.OnActionExecuting(context);

			context.Response.IsSuccessStatusCode.Should().Be.True();
		}

		[Test]
		public void ShouldNotBlockOtherReferrerWhenHostIPv4()
		{
			var context = CreateExecutedContext();
			context.Request.RequestUri = new Uri("http://192.168.0.1/teleoptiwfm");
			context.Request.Headers.Referrer = new Uri("http://wfmserver1/test/2");
			var filter = new CsrfFilterHttp();
			filter.OnActionExecuting(context);

			context.Response.IsSuccessStatusCode.Should().Be.True();
		}

		[Test]
		public void ShouldAllowEmptyReferrerAndOrigin()
		{
			var context = CreateExecutedContext();
			context.Request.RequestUri = new Uri("http://wfmserver1/teleoptiwfm");
			var filter = new CsrfFilterHttp();
			filter.OnActionExecuting(context);

			context.Response.IsSuccessStatusCode.Should().Be.True();
		}

		[Test]
		public void ShouldPrioritizeOrigin()
		{
			var context = CreateExecutedContext();
			context.Request.Headers.Referrer = new Uri("http://wfmserver1/test/2");
			context.Request.Headers.Add("Origin", "http://burp/test/2");
			context.Request.RequestUri = new Uri("http://wfmserver1/teleoptiwfm");
			var filter = new CsrfFilterHttp();
			filter.OnActionExecuting(context);

			context.Response.IsSuccessStatusCode.Should().Be.False();
		}

		[Test]
		public void ShouldAllowOkOrigin()
		{
			var context = CreateExecutedContext();
			context.Request.Headers.Add("Origin", "http://wfmserver1/test/2");
			context.Request.RequestUri = new Uri("http://wfmserver1/teleoptiwfm");
			var filter = new CsrfFilterHttp();
			filter.OnActionExecuting(context);

			context.Response.IsSuccessStatusCode.Should().Be.True();
		}

		[Test]
		public void ShouldAllowRelativeOrigin()
		{
			var context = CreateExecutedContext();
			context.Request.Headers.Add("Origin", "/test/2");
			context.Request.RequestUri = new Uri("http://wfmserver1/teleoptiwfm");
			var filter = new CsrfFilterHttp();
			filter.OnActionExecuting(context);

			context.Response.IsSuccessStatusCode.Should().Be.True();
		}

		[Test]
		public void ShouldIgnoreCheckWhenRequestIsNull()
		{
			var context = new HttpActionContext();
			var filter = new CsrfFilterHttp();
			filter.OnActionExecuting(context);

			context.Response.Should().Be.Null();
		}

		[Test]
		public void ShouldIgnoreCheckWhenRequestUriIsNull()
		{
			var context = CreateExecutedContext();
			context.Request.Headers.Add("Origin", "http://wfmserver1/test/2");
			context.Request.RequestUri = null;
			var filter = new CsrfFilterHttp();
			filter.OnActionExecuting(context);

			context.Response.IsSuccessStatusCode.Should().Be.True();
		}

		[Test]
		public void ShouldCreateResponseWhenNotSet()
		{
			var context = CreateExecutedContext();
			context.Request.RequestUri = new Uri("http://wfmserver1/teleoptiwfm");
			context.Request.Headers.Add("Origin", "http://wfmserver2/test/2");
			context.Response = null;
			var filter = new CsrfFilterHttp();
			filter.OnActionExecuting(context);

			context.Response.IsSuccessStatusCode.Should().Be.False();
		}

		private HttpActionContext CreateExecutedContext()
		{
			var httpActionContext = new HttpActionContext();
			httpActionContext.ControllerContext = new HttpControllerContext(new HttpRequestContext(), new HttpRequestMessage(), new HttpControllerDescriptor(), new FakeController());
			httpActionContext.Response = new HttpResponseMessage();
			return httpActionContext;
		}
	}

	[TestFixture]
	public class BlockOtherOriginFilterTests
	{
		[Test]
		public void ShouldBlockOtherOrigin()
		{
			var filter = new CsrfFilter();
			var filterTester = new FilterTester();
			filterTester.AddHeader("Origin", "http://burp/test/2");
			filterTester.UsePost();
			filterTester.InvokeFilter(filter);

			filterTester.ControllerContext.HttpContext.Response.StatusCode.Should().Be.EqualTo(HttpStatusCode.Forbidden);
		}

		[Test]
		public void ShouldNotBlockOtherOriginWhenIPv4OriginHost()
		{
			var filter = new CsrfFilter();
			var filterTester = new FilterTester();
			filterTester.AddHeader("Origin", "http://192.168.0.1/test/2");
			filterTester.UsePost();
			filterTester.InvokeFilter(filter);

			filterTester.ControllerContext.HttpContext.Response.StatusCode.Should().Not.Be.EqualTo(HttpStatusCode.Forbidden);
		}

		[Test]
		public void ShouldNotBlockOtherOriginWhenIPv4RequestHost()
		{
			var filter = new CsrfFilter();
			var filterTester = new FilterTester();
			filterTester.UseUri(new Uri("http://192.168.0.1/test/2"));
			filterTester.AddHeader("Origin", "http://tempuri.org/test/2");
			filterTester.UsePost();
			filterTester.InvokeFilter(filter);

			filterTester.ControllerContext.HttpContext.Response.StatusCode.Should().Not.Be.EqualTo(HttpStatusCode.Forbidden);
		}

		[Test]
		public void ShouldBlockOtherReferrer()
		{
			var filter = new CsrfFilter();
			var filterTester = new FilterTester();
			filterTester.AddHeader("Referer", "http://burp/test/2");
			filterTester.UsePost();
			filterTester.InvokeFilter(filter);

			filterTester.ControllerContext.HttpContext.Response.StatusCode.Should().Be.EqualTo(HttpStatusCode.Forbidden);
		}

		[Test]
		public void ShouldNotBlockOtherReferrerWhenIPv4Host()
		{
			var filter = new CsrfFilter();
			var filterTester = new FilterTester();
			filterTester.AddHeader("Referer", "http://192.168.0.1/test/2");
			filterTester.UsePost();
			filterTester.InvokeFilter(filter);

			filterTester.ControllerContext.HttpContext.Response.StatusCode.Should().Not.Be.EqualTo(HttpStatusCode.Forbidden);
		}

		[Test]
		public void ShouldAllowEmptyReferrerAndOrigin()
		{
			var filter = new CsrfFilter();
			var filterTester = new FilterTester();
			filterTester.UsePost();
			filterTester.InvokeFilter(filter);

			filterTester.ControllerContext.HttpContext.Response.StatusCode.Should().Not.Be.EqualTo(HttpStatusCode.Forbidden);
		}

		[Test]
		public void ShouldPrioritizeOrigin()
		{
			var filter = new CsrfFilter();
			var filterTester = new FilterTester();
			filterTester.AddHeader("Origin", "http://burp/test/2");
			filterTester.AddHeader("Referer", "http://tempuri.org/foo/test/2");
			filterTester.UsePost();
			filterTester.InvokeFilter(filter);

			filterTester.ControllerContext.HttpContext.Response.StatusCode.Should().Be.EqualTo(HttpStatusCode.Forbidden);
		}

		[Test]
		public void ShouldHandleRelativeOrigin()
		{
			var filter = new CsrfFilter();
			var filterTester = new FilterTester();
			filterTester.AddHeader("Origin", "/test/2");
			filterTester.UsePost();
			filterTester.InvokeFilter(filter);

			filterTester.ControllerContext.HttpContext.Response.StatusCode.Should().Not.Be.EqualTo(HttpStatusCode.Forbidden);
		}

		[Test]
		public void ShouldHandleRelativeReferer()
		{
			var filter = new CsrfFilter();
			var filterTester = new FilterTester();
			filterTester.AddHeader("Referer", "/test/2");
			filterTester.UsePost();
			filterTester.InvokeFilter(filter);

			filterTester.ControllerContext.HttpContext.Response.StatusCode.Should().Not.Be.EqualTo(HttpStatusCode.Forbidden);
		}

		[Test]
		public void ShouldAllowOkOrigin()
		{
			var filter = new CsrfFilter();
			var filterTester = new FilterTester();
			filterTester.AddHeader("Origin", "http://tempuri.org/foo/test/2");
			filterTester.UsePost();
			filterTester.InvokeFilter(filter);
			
			filterTester.ControllerContext.HttpContext.Response.StatusCode.Should().Not.Be.EqualTo(HttpStatusCode.Forbidden);
		}
	}
}