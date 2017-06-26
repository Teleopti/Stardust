using System;
using System.IO;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Ccc.Web.Areas.Anywhere.Controllers;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere
{
	public class ApplicationControllerTest
	{
		private ApplicationController target;
		private IAuthorization authorization;
		private ICurrentTeleoptiPrincipal currentTeleoptiPrincipal;
		private IIanaTimeZoneProvider ianaTimeZoneProvider;
		private ICurrentDataSource currentDataSource;

		[SetUp]
		public void Setup()
		{
			authorization = MockRepository.GenerateMock<IAuthorization>();
			currentTeleoptiPrincipal = MockRepository.GenerateMock<ICurrentTeleoptiPrincipal>();
			ianaTimeZoneProvider = MockRepository.GenerateMock<IIanaTimeZoneProvider>();
			currentDataSource = new FakeCurrentDatasource("ds1");
			target = new ApplicationController(authorization, currentTeleoptiPrincipal, ianaTimeZoneProvider, new FakeToggleManager(), new UtcTimeZone(), new Now(), currentDataSource);
		}

		[TearDown]
		public void Teardown()
		{
			target.Dispose();
		}

		[Test]
		public void ShouldReturnDefaultViewInDefaultAction()
		{
			var result = target.Index();
			result.ViewName.Should().Be.Empty();
		}

		[Test]
		public void ShouldReturnBasicNavigation()
		{
			var principal = (ITeleoptiPrincipal)MockRepository.GenerateStrictMock(typeof (ITeleoptiPrincipal), new[] {typeof (IUnsafePerson)});
			var identity = MockRepository.GenerateMock<ITeleoptiIdentity>();
			var regional = MockRepository.GenerateMock<IRegional>();

			authorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyTimeWeb)).Return(true);
			authorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview)).Return(false);
			currentTeleoptiPrincipal.Stub(x => x.Current()).Return(principal);
			principal.Stub(x => x.Identity).Return(identity);
			identity.Stub(x => x.Name).Return("fake");
			principal.Stub(x => x.Regional).Return(regional);
			var person = PersonFactory.CreatePersonWithId();
			((IUnsafePerson)principal).Stub(x => x.Person).Return(person);

			regional.Stub(x => x.TimeZone).Return(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			ianaTimeZoneProvider.Stub(x => x.WindowsToIana("W. Europe Standard Time")).Return("Europe/Berlin");

			var result = target.NavigationContent();
			dynamic content = result.Data;

			((object) content.UserName).Should().Be.EqualTo("fake");
			((object)content.IsMyTimeAvailable).Should().Be.EqualTo(true);
			((object)content.IsRealTimeAdherenceAvailable).Should().Be.EqualTo(false);
			((object)content.PersonId).Should().Be.EqualTo(person.Id);
			((object)content.IanaTimeZone).Should().Be.EqualTo("Europe/Berlin");
		}

		[Test]
		public void ShouldReturnPermissionsIHave()
		{
			var principal = MockRepository.GenerateMock<ITeleoptiPrincipal>();

			authorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.AddFullDayAbsence)).Return(true);
			authorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.AddIntradayAbsence)).Return(true);
			authorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.RemoveAbsence)).Return(true);
			authorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.AddActivity)).Return(true);
			authorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.MoveActivity)).Return(true);

			currentTeleoptiPrincipal.Stub(x => x.Current()).Return(principal);

			var result = target.Permissions();
			dynamic content = result.Data;

			((object)content.IsAddFullDayAbsenceAvailable).Should().Be.EqualTo(true);
			((object)content.IsAddIntradayAbsenceAvailable).Should().Be.EqualTo(true);
			((object)content.IsRemoveAbsenceAvailable).Should().Be.EqualTo(true);
			((object)content.IsAddActivityAvailable).Should().Be.EqualTo(true);
			((object)content.IsMoveActivityAvailable).Should().Be.EqualTo(true);
			((object)content.IsSmsLinkAvailable).Should().Be.EqualTo(false);
		}

		[Test]
		public void ShouldReturnTranslation()
		{
			var request = MockRepository.GenerateStub<FakeHttpRequest>("/", new Uri("http://localhost/"), new Uri("http://localhost/"));
			var context = new FakeHttpContext("/");
			context.SetRequest(request);
			target.ControllerContext = new ControllerContext(context, new RouteData(), target);

			request.Stub(x => x.MapPath("")).IgnoreArguments().Return(
                Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, @"..\..\..\Teleopti.Ccc.Web\Areas\Anywhere\Content\Translation\TranslationTemplate.txt")));

			var result = target.Resources() as ContentResult;
			result.ContentType.Should().Be("text/javascript");
			result.Content.Should().Not.Be.Null();
		}
	}
}