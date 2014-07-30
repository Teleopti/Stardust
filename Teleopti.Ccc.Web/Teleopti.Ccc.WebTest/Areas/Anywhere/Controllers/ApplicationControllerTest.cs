using System;
using System.IO;
using System.Web.Mvc;
using System.Web.Routing;
using MvcContrib.TestHelper.Fakes;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Anywhere.Controllers;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Controllers
{
	public class ApplicationControllerTest
	{
		private ApplicationController target;
		private IPrincipalAuthorization authorization;
		private ICurrentTeleoptiPrincipal currentTeleoptiPrincipal;
		private IPersonRepository _personRepository;

		[SetUp]
		public void Setup()
		{
			authorization = MockRepository.GenerateMock<IPrincipalAuthorization>();
			currentTeleoptiPrincipal = MockRepository.GenerateMock<ICurrentTeleoptiPrincipal>();
			_personRepository = MockRepository.GenerateMock<IPersonRepository>();
			target = new ApplicationController(authorization, currentTeleoptiPrincipal, _personRepository);
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
			var principal = MockRepository.GenerateMock<ITeleoptiPrincipal>();
			var identity = MockRepository.GenerateMock<ITeleoptiIdentity>();

			authorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyTimeWeb)).Return(true);
			authorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview)).Return(false);
			currentTeleoptiPrincipal.Stub(x => x.Current()).Return(principal);
			principal.Stub(x => x.Identity).Return(identity);
			identity.Stub(x => x.Name).Return("fake");
			var person = PersonFactory.CreatePersonWithId();
			principal.Stub(x => x.GetPerson(_personRepository)).Return(person);

			var result = target.NavigationContent();
			dynamic content = result.Data;

			((object) content.UserName).Should().Be.EqualTo("fake");
			((object)content.IsMyTimeAvailable).Should().Be.EqualTo(true);
			((object)content.IsRealTimeAdherenceAvailable).Should().Be.EqualTo(false);
			((object)content.PersonId).Should().Be.EqualTo(person.Id);
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
		}

		[Test]
		public void ShouldReturnTranslation()
		{
			var request = MockRepository.GenerateStub<FakeHttpRequest>("/", new Uri("http://localhost/"), new Uri("http://localhost/"));
			var context = new FakeHttpContext("/");
			context.SetRequest(request);
			target.ControllerContext = new ControllerContext(context, new RouteData(), target);

			request.Stub(x => x.MapPath("")).IgnoreArguments().Return(
                Path.GetFullPath(@"..\..\..\Teleopti.Ccc.Web\Areas\Anywhere\Content\Translation\TranslationTemplate.txt"));

			var result = target.Resources() as ContentResult;
			result.ContentType.Should().Be("text/javascript");
			result.Content.Should().Not.Be.Null();
		}

	}
}