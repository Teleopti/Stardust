using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.WebTest.Filters
{
	[TestFixture]
	public class AuthorizeTeleoptiAttributeTest
	{
		[Test]
		public void ShouldBeAbleAccessControllersExcludedByBaseType()
		{
			var target = new AuthorizeTeleoptiAttribute(new[] { typeof(anonController) });

			var configuration = new HttpConfiguration();
			configuration.Filters.Add(target);

			var httpControllerDescriptor = new HttpControllerDescriptor{ControllerType = typeof(anonController)};
			var httpRequestMessage = new HttpRequestMessage();
			var httpControllerContext = new HttpControllerContext{Controller = new anonController(),Configuration = configuration, ControllerDescriptor = httpControllerDescriptor, Request = httpRequestMessage};
			var httpActionContext = new HttpActionContext(httpControllerContext, new CustomHttpActionDescriptorForTest(httpControllerDescriptor));
			target.OnAuthorization(httpActionContext);

			httpActionContext.Response.Should().Be.Null();
		}

		[Test]
		public void ShouldNotBeAbleAccessControllersByDefault()
		{
			var target = new AuthorizeTeleoptiAttribute(new Type[] {  });

			var configuration = new HttpConfiguration();
			configuration.Filters.Add(target);

			var httpControllerDescriptor = new HttpControllerDescriptor { ControllerType = typeof(anonController) };
			var httpRequestMessage = new HttpRequestMessage();
			var httpControllerContext = new HttpControllerContext { Controller = new anonController(), Configuration = configuration, ControllerDescriptor = httpControllerDescriptor, Request = httpRequestMessage };
			var httpActionContext = new HttpActionContext(httpControllerContext, new CustomHttpActionDescriptorForTest(httpControllerDescriptor));
			target.OnAuthorization(httpActionContext);

			httpActionContext.Response.StatusCode.Should().Be.EqualTo(HttpStatusCode.Unauthorized);
		}

		private class anonController : ApiController
		{
		}
	}
}