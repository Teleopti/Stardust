using System;
using System.IO;
using System.Web.Mvc;
using System.Web.Routing;
using MvcContrib.TestHelper.Fakes;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Areas.Team.Controllers;

namespace Teleopti.Ccc.WebTest.Areas.Team.Controllers
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture]
	public class ApplicationControllerTest
	{
		private ApplicationController target;
		private IPrincipalAuthorization authorization;
		private ICurrentTeleoptiPrincipal currentTeleoptiPrincipal;

		[SetUp]
		public void Setup()
		{
			authorization = MockRepository.GenerateMock<IPrincipalAuthorization>();
			currentTeleoptiPrincipal = MockRepository.GenerateMock<ICurrentTeleoptiPrincipal>();
			target = new ApplicationController(authorization, currentTeleoptiPrincipal);
		}

		[Test]
		public void ShouldReturnDefaultViewInDefaultAction()
		{
			var result = target.Index();
			result.FileName.Should().Be("~/Areas/Team/Content/Templates/index.html");
			result.ContentType.Should().Be("text/html");
		}

		[Test]
		public void ShouldReturnBasicNavigation()
		{
			ITeleoptiPrincipal principal = MockRepository.GenerateMock<ITeleoptiPrincipal>();
			ITeleoptiIdentity identity = MockRepository.GenerateMock<ITeleoptiIdentity>();

			authorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyTimeWeb)).Return(true);
			authorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.MobileReports)).Return(false);
			currentTeleoptiPrincipal.Stub(x => x.Current()).Return(principal);
			principal.Stub(x => x.Identity).Return(identity);
			identity.Stub(x => x.Name).Return("fake");

			var result = target.NavigationContent();
			dynamic content = result.Data;

			((object) content.UserName).Should().Be.EqualTo("fake");
			((object)content.IsMyTimeAvailable).Should().Be.EqualTo(true);
			((object)content.IsMobileReportsAvailable).Should().Be.EqualTo(false);
		}

		[Test]
		public void ShouldReturnTranslation()
		{
			var request = MockRepository.GenerateStub<FakeHttpRequest>("/", new Uri("http://localhost/"), new Uri("http://localhost/"));
			var context = new FakeHttpContext("/");
			context.SetRequest(request);
			target.ControllerContext = new ControllerContext(context, new RouteData(), target);

			request.Stub(x => x.MapPath("")).IgnoreArguments().Return(
				Path.GetFullPath(@"..\..\..\Teleopti.Ccc.Web\Areas\Team\Content\Translation\TranslationTemplate.txt"));

			target.Resources().Should().Not.Be.Null();
		}

		[TearDown]
		public void Teardown()
		{
			target.Dispose();
		}
	}
}