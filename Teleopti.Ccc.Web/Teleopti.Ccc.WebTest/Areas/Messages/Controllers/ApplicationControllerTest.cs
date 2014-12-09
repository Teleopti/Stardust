using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Ccc.Web.Areas.Messages.Controllers;
using Teleopti.Ccc.Web.Areas.Messages.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Messages.Controllers
{
	public class ApplicationControllerTest
	{
		private ApplicationController target;
		private IPersonRepository _personRepository;
		private ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;
		private IPrincipalAuthorization _principalAuthorization;
		private INotifier _notifier;

		[SetUp]
		public void Setup()
		{
			_personRepository = MockRepository.GenerateMock<IPersonRepository>();
			_currentTeleoptiPrincipal = MockRepository.GenerateMock<ICurrentTeleoptiPrincipal>();
			_principalAuthorization = MockRepository.GenerateMock<IPrincipalAuthorization>();
			_notifier = MockRepository.GenerateMock<INotifier>();
			target = new ApplicationController(_personRepository, _currentTeleoptiPrincipal,_principalAuthorization, _notifier);
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
		public void ShouldGetPersons()
		{
			var person1 = PersonFactory.CreatePersonWithGuid("a", "a");
			var person2 = PersonFactory.CreatePersonWithGuid("b", "b");
			_personRepository.Stub(x => x.FindPeople(new[] {person1.Id.Value, person2.Id.Value})).IgnoreArguments()
				.Return(new[] {person1, person2});

			var result = target.GetPersons(person1.Id.Value + "," + person2.Id.Value);

			(result.Data as SendMessageViewModel).People.Count().Should().Be.EqualTo(2);
			(result.Data as SendMessageViewModel).People.First().Name.Should().Be.EqualTo(person1.Name.ToString());
		}

		[Test]
		public void ShouldSendMessage()
		{
			var person1 = PersonFactory.CreatePersonWithGuid("a", "a");
			var person2 = PersonFactory.CreatePersonWithGuid("b", "b");
			var persons = new[] { person1, person2 };
			_personRepository.Stub(x => x.FindPeople(new[] { person1.Id.Value, person2.Id.Value })).IgnoreArguments()
				.Return(persons);

			const string subject = "test";
			const string testBody = "test body";
			target.SendMessage(new[] {person1.Id.Value, person2.Id.Value}, subject, testBody);

			_notifier.AssertWasCalled(
				x => x.Notify(Arg<INotificationMessage>.Matches(s => s.Subject == subject && s.Messages.First() == testBody), Arg<IPerson[]>.Is.Equal(persons)));
		}

		[Test]
		public void ShouldReturnBasicNavigation()
		{
			var principal = (ITeleoptiPrincipal)MockRepository.GenerateStrictMock(typeof(ITeleoptiPrincipal), new[] { typeof(IUnsafePerson) });
			var identity = MockRepository.GenerateMock<ITeleoptiIdentity>();

			_principalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyTimeWeb)).Return(true);
			_principalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.Anywhere)).Return(false);
			_currentTeleoptiPrincipal.Stub(x => x.Current()).Return(principal);
			principal.Stub(x => x.Identity).Return(identity);
			identity.Stub(x => x.Name).Return("fake");
			var person = PersonFactory.CreatePersonWithId();
			((IUnsafePerson)principal).Stub(x => x.Person).Return(person);

			var result = target.NavigationContent();
			dynamic content = result.Data;

			((object)content.UserName).Should().Be.EqualTo("fake");
			((object)content.IsMyTimeAvailable).Should().Be.EqualTo(true);
			((object)content.IsAnywhereAvailable).Should().Be.EqualTo(false);
			((object)content.PersonId).Should().Be.EqualTo(person.Id);
		}


		[Test]
		public void ShouldReturnTranslation()
		{
			var request = MockRepository.GenerateStub<FakeHttpRequest>("/", new Uri("http://localhost/"), new Uri("http://localhost/"));
			var context = new FakeHttpContext("/");
			context.SetRequest(request);
			target.ControllerContext = new ControllerContext(context, new RouteData(), target);

			request.Stub(x => x.MapPath("")).IgnoreArguments().Return(
				Path.GetFullPath(@"..\..\..\Teleopti.Ccc.Web\Areas\Messages\Content\Translation\TranslationTemplate.txt"));

			var result = target.Resources() as ContentResult;
			result.ContentType.Should().Be("text/javascript");
			result.Content.Should().Not.Be.Null();
		}

	}
}