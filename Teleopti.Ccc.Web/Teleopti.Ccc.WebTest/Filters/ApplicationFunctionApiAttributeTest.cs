using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Web.Http.Controllers;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.WebTest.Filters
{
	[TestFixture]
	public class ApplicationFunctionApiAttributeTest
	{
		[Test]
		public void ShouldSetErrorCodeWhenNoValidPrincipal()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			var target = new ApplicationFunctionApiAttribute("Test") { PermissionProvider = permissionProvider };

			permissionProvider.Stub(x => x.HasApplicationFunctionPermission("Test")).Return(true);

			var httpActionContext =
				new HttpActionContext(
					new HttpControllerContext { ControllerDescriptor = new HttpControllerDescriptor { ControllerType = typeof(object) }, Request = new HttpRequestMessage() },
					new CustomHttpActionDescriptorForTest(new HttpControllerDescriptor { ControllerType = typeof(object) }));

			var before = Thread.CurrentPrincipal;
			Thread.CurrentPrincipal =  null;
			target.OnAuthorization(httpActionContext);

			httpActionContext.Response.IsSuccessStatusCode.Should().Be.False();
			Thread.CurrentPrincipal = before;
		}

		[Test]
		public void ShouldSetErrorCodeWhenNoPermission()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			var target = new ApplicationFunctionApiAttribute("Test") {PermissionProvider = permissionProvider};

			permissionProvider.Stub(x => x.HasApplicationFunctionPermission("Test")).Return(false);

			var httpActionContext =
				new HttpActionContext(
					new HttpControllerContext {ControllerDescriptor = new HttpControllerDescriptor {ControllerType = typeof (object)},Request = new HttpRequestMessage()},
					new CustomHttpActionDescriptorForTest(new HttpControllerDescriptor {ControllerType = typeof (object)}));

			var before = Thread.CurrentPrincipal;
			Thread.CurrentPrincipal = TeleoptiPrincipalCacheable.Make(new TeleoptiIdentity("hej",null,null,null, null), PersonFactory.CreatePerson());
			target.OnAuthorization(httpActionContext);

			httpActionContext.Response.IsSuccessStatusCode.Should().Be.False();
			Thread.CurrentPrincipal = before;
		}

		[Test]
		public void ShouldDenyWhenOnlyGenericPermission()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			var target = new ApplicationFunctionApiAttribute("Test") { PermissionProvider = permissionProvider };

			permissionProvider.Stub(x => x.HasApplicationFunctionPermission("Test")).Return(true);

			var httpActionContext =
				new HttpActionContext(
					new HttpControllerContext { ControllerDescriptor = new HttpControllerDescriptor { ControllerType = typeof(object) }, Request = new HttpRequestMessage() },
					new CustomHttpActionDescriptorForTest(new HttpControllerDescriptor { ControllerType = typeof(object) }));

			var before = Thread.CurrentPrincipal;
			Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("Authenticated"), new []{"Admin"});
			target.OnAuthorization(httpActionContext);

			httpActionContext.Response.StatusCode.Should().Be.EqualTo(HttpStatusCode.Forbidden);
			Thread.CurrentPrincipal = before;
		}
	}
}
