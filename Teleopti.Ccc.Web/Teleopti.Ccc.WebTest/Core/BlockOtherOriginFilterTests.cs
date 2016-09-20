﻿using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Mvc;
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
			filterTester.InvokeFilter(filter);

			filterTester.ControllerContext.HttpContext.Response.StatusCode.Should().Be.EqualTo(HttpStatusCode.Forbidden);
		}

		[Test]
		public void ShouldBlockOtherReferrer()
		{
			var filter = new CsrfFilter();
			var filterTester = new FilterTester();
			filterTester.AddHeader("Referer", "http://burp/test/2");
			filterTester.InvokeFilter(filter);

			filterTester.ControllerContext.HttpContext.Response.StatusCode.Should().Be.EqualTo(HttpStatusCode.Forbidden);
		}

		[Test]
		public void ShouldAllowEmptyReferrerAndOrigin()
		{
			var filter = new CsrfFilter();
			var filterTester = new FilterTester();
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
			filterTester.InvokeFilter(filter);

			filterTester.ControllerContext.HttpContext.Response.StatusCode.Should().Be.EqualTo(HttpStatusCode.Forbidden);
		}

		[Test]
		public void ShouldAllowOkOrigin()
		{
			var filter = new CsrfFilter();
			var filterTester = new FilterTester();
			filterTester.AddHeader("Origin", "http://tempuri.org/foo/test/2");
			filterTester.InvokeFilter(filter);
			
			filterTester.ControllerContext.HttpContext.Response.StatusCode.Should().Not.Be.EqualTo(HttpStatusCode.Forbidden);
		}
	}
}