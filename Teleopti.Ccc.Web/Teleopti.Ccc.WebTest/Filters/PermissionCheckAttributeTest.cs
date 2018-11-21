using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.WebTest.Filters
{
	[TestFixture]
	public class PermissionCheckAttributeTest
	{
		[Test]
		[SetUICulture("en-US")]
		public void ShouldNotBeAbleAccessControllersAndResponseWithResourceMessageIfWithoutPermission()
		{
			var authorization = new FakePermissions();
			authorization.HasPermission("Test");
			var target = new PermissionCheckAttribute(DefinedRaptorApplicationFunctionPaths.ViewSchedules, nameof(Resources.NoPermissionToViewSchedules));
			using (CurrentAuthorization.ThreadlyUse(authorization))
			{
				var httpActionContext = getHttpActionContext(target);
				target.OnAuthorization(httpActionContext);
				httpActionContext.Response.StatusCode.Should().Be.EqualTo(HttpStatusCode.Forbidden);
				httpActionContext.Response.Content.ReadAsStringAsync().GetAwaiter().GetResult().Should().Be.EqualTo("{\"Message\":\"No permission to view schedules\"}");
			}
		}

		private HttpActionContext getHttpActionContext(PermissionCheckAttribute target)
		{
			var configuration = new HttpConfiguration();
			configuration.Filters.Add(target);
			var httpControllerDescriptor = new HttpControllerDescriptor { ControllerType = typeof(anonController) };
			var httpRequestMessage = new HttpRequestMessage();
			var httpControllerContext = new HttpControllerContext
			{
				Controller = new anonController(),
				Configuration = configuration,
				ControllerDescriptor = httpControllerDescriptor,
				Request = httpRequestMessage
			};
			return new HttpActionContext(httpControllerContext, new CustomHttpActionDescriptorForTest(httpControllerDescriptor));

		}

		private class anonController : ApiController
		{
		}
	}
}