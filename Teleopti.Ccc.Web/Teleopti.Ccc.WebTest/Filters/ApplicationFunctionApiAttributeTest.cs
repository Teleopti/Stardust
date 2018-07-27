using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.WebTest.Filters
{
	[TestFixture]
	public class ApplicationFunctionApiAttributeTest
	{
		[Test]
		public void ShouldDenyWhenNoValidPrincipal()
		{
			var authorization = new FakePermissions();
			authorization.HasPermission("Test");
			var target = new ApplicationFunctionApiAttribute("Test");
			using (CurrentAuthorization.ThreadlyUse(authorization))
			{
				var httpActionContext = getHttpActionContext(target);
				target.OnAuthorization(httpActionContext);
				httpActionContext.Response.StatusCode.Should().Be.EqualTo(HttpStatusCode.Forbidden);
			}
		}
		

		[Test]
		public void ShouldDenyWhenLogonUserDoesNotHavePermission()
		{
			var authorization = new FakePermissions();
			authorization.HasPermission("Test");

			var target = new ApplicationFunctionApiAttribute(new[] { typeof(anonController) }, "Admin");
			using (CurrentAuthorization.ThreadlyUse(authorization))
			{
				var httpActionContext = getHttpActionContext(target);
				target.OnAuthorization(httpActionContext);
				httpActionContext.Response.StatusCode.Should().Be.EqualTo(HttpStatusCode.Forbidden);
			}
		}

		[Test]
		public void ShouldAuthorizeRequestIfLogonUserHasPermission()
		{
			var authorization = new FakePermissions();
			authorization.HasPermission("Test");
			var target = new ApplicationFunctionApiAttribute(new[] { typeof(anonController) }, "Test");

			using (CurrentAuthorization.ThreadlyUse(authorization))
			{
				var httpActionContext =getHttpActionContext(target);
				target.OnAuthorization(httpActionContext);
				httpActionContext.Response.Should().Be.Null();
			}
		}

		private HttpActionContext getHttpActionContext(ApplicationFunctionApiAttribute target)
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
