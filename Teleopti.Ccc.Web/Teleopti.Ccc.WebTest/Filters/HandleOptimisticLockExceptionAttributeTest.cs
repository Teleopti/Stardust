using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.WebTest.Filters
{
	public class HandleOptimisticLockExceptionAttributeTest
	{
		[Test]
		public void ShouldHandleOptimisticLockException()
		{
			var handleOptimisticLockExceptionAttribute = new HandleOptimisticLockExceptionAttribute();
			var actionExecutedContext = new ActionExecutedContext
			{
				Exception = new OptimisticLockException("test error"),
				ExceptionHandled = false,
				Canceled = false,
				HttpContext = createHttpContext()
			};
			handleOptimisticLockExceptionAttribute.OnActionExecuted(actionExecutedContext);
			var result = actionExecutedContext.Result as JsonResult;
			result.Should().Not.Be(null);
			var data = result.Data as ModelStateResult;
			data.Should().Not.Be(null);
			data.Errors.Count().Should().Be(1);
			data.Errors.ElementAt(0).Should().Be(Resources.OptimisticLockText);
		}

		[Test]
		public void ShouldNotHandleOtherException()
		{
			var handleOptimisticLockExceptionAttribute = new HandleOptimisticLockExceptionAttribute();
			var actionExecutedContext = new ActionExecutedContext
			{
				Exception = new Exception("test"),
				ExceptionHandled = false,
				Canceled = false,
				HttpContext = createHttpContext()
			};
			handleOptimisticLockExceptionAttribute.OnActionExecuted(actionExecutedContext);
			actionExecutedContext.Result.Should().Be.InstanceOf<EmptyResult>();
		}

		[Test]
		public void ShouldNotHandleNonAjaxRequestException()
		{
			var handleOptimisticLockExceptionAttribute = new HandleOptimisticLockExceptionAttribute();
			var actionExecutedContext = new ActionExecutedContext
			{
				Exception = new OptimisticLockException("test error"),
				ExceptionHandled = false,
				Canceled = false,
				HttpContext = createHttpContext(false)
			};
			handleOptimisticLockExceptionAttribute.OnActionExecuted(actionExecutedContext);
			actionExecutedContext.Result.Should().Be.InstanceOf<EmptyResult>();
		}

		[Test]
		public void ShouldReturn400HttpStatusCode()
		{
			var handleOptimisticLockExceptionAttribute = new HandleOptimisticLockExceptionAttribute();
			var actionExecutedContext = new ActionExecutedContext
			{
				Exception = new OptimisticLockException("test error"),
				ExceptionHandled = false,
				Canceled = false,
				HttpContext = createHttpContext()
			};
			handleOptimisticLockExceptionAttribute.OnActionExecuted(actionExecutedContext);
			var result = actionExecutedContext.Result as JsonResult;
			result.Should().Not.Be(null);
			var data = result.Data as ModelStateResult;
			data.Should().Not.Be(null);
			data.Errors.Count().Should().Be(1);
			actionExecutedContext.HttpContext.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
		}

		private HttpContextBase createHttpContext(bool createAjaxRequest = true)
		{
			var httpContext = new FakeHttpContext();
			if (!createAjaxRequest) return httpContext;
			httpContext.Request.Headers.Add("X-Requested-With", "XMLHttpRequest");
			httpContext.Items.Add("X-Requested-With", "XMLHttpRequest");
			return httpContext;
		}
	}
}
