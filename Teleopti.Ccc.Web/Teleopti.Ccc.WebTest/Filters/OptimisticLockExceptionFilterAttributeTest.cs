using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.WebTest.Filters
{
	public class OptimisticLockExceptionFilterAttributeTest
	{
		[Test]
		public void ShouldHandleOptimisticLockException()
		{
			var optimisticLockExceptionFilterAttribute = new OptimisticLockExceptionFilterAttribute();
			var actionExecutedContext = new HttpActionExecutedContext
			{
				ActionContext = new HttpActionContext
				{
					ControllerContext = new HttpControllerContext
					{
						Request = new HttpRequestMessage()
					}
				},
				Exception = new OptimisticLockException("test error"),
				Response = new HttpResponseMessage(HttpStatusCode.OK)
			};
			optimisticLockExceptionFilterAttribute.OnException(actionExecutedContext);
			var responseMessage = actionExecutedContext.Response;
			responseMessage.StatusCode.Should().Be(HttpStatusCode.Conflict);
		}

		[Test]
		public void ShouldNotHandleOtherException()
		{
			var optimisticLockExceptionFilterAttribute = new OptimisticLockExceptionFilterAttribute();
			var actionExecutedContext = new HttpActionExecutedContext
			{
				ActionContext = new HttpActionContext
				{
					ControllerContext = new HttpControllerContext
					{
						Request = new HttpRequestMessage()
					}
				},
				Exception = new NotImplementedException(),
				Response = new HttpResponseMessage(HttpStatusCode.OK)
			};
			optimisticLockExceptionFilterAttribute.OnException(actionExecutedContext);


			var responseMessage = actionExecutedContext.Response;
			responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
		}
	}
}