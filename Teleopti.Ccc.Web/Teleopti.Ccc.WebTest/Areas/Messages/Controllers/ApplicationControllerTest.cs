using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Ccc.Web.Areas.Messages.Controllers;
using Teleopti.Ccc.Web.Areas.Messages.Models;

namespace Teleopti.Ccc.WebTest.Areas.Messages.Controllers
{
	public class ApplicationControllerTest
	{
		private ApplicationController target;
		private IPersonRepository _personRepository;
		private ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;
		private IAuthorization _authorization;
		private INotifier _notifier;
		private ILicenseCustomerNameProvider _licenseCustomerNameProvider;

		[SetUp]
		public void Setup()
		{
			_personRepository = MockRepository.GenerateMock<IPersonRepository>();
			_currentTeleoptiPrincipal = MockRepository.GenerateMock<ICurrentTeleoptiPrincipal>();
			_authorization = MockRepository.GenerateMock<IAuthorization>();
			_notifier = MockRepository.GenerateMock<INotifier>();
			_licenseCustomerNameProvider = MockRepository.GenerateMock<ILicenseCustomerNameProvider>();
			target = new ApplicationController(_personRepository, _currentTeleoptiPrincipal,_authorization, _notifier, _licenseCustomerNameProvider);
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
		public async Task ShouldSendMessage()
		{
			var person1 = PersonFactory.CreatePersonWithGuid("a", "a");
			var person2 = PersonFactory.CreatePersonWithGuid("b", "b");
			var persons = new[] { person1, person2 };
			_personRepository.Stub(x => x.FindPeople(new[] { person1.Id.Value, person2.Id.Value })).IgnoreArguments()
				.Return(persons);
			_notifier.Stub(x => x.Notify(null, persons)).IgnoreArguments().Return(Task.FromResult(true));

			const string subject = "test";
			const string testBody = "test body";
			await target.SendMessage(new[] {person1.Id.Value, person2.Id.Value}, subject, testBody);

			_notifier.AssertWasCalled(
				x => x.Notify(Arg<INotificationMessage>.Matches(s => s.Subject == subject && s.Messages.First() == testBody), Arg<IPerson[]>.Is.Equal(persons)));
		}

		[Test]
		public async Task ShouldSendMessageAndIncludeCustomerName()
		{
			string customerName = "SomeTestLicenseCustomerName";
			var person1 = PersonFactory.CreatePersonWithGuid("a", "a");
			var person2 = PersonFactory.CreatePersonWithGuid("b", "b");
			var persons = new[] { person1, person2 };
			_personRepository.Stub(x => x.FindPeople(new[] { person1.Id.Value, person2.Id.Value })).IgnoreArguments()
				.Return(persons);
			_notifier.Stub(x => x.Notify(null, persons)).IgnoreArguments().Return(Task.FromResult(true));

			_licenseCustomerNameProvider.Stub(x => x.GetLicenseCustomerName()).Return(customerName);

			const string subject = "test";
			const string testBody = "test body";
			await target.SendMessage(new[] { person1.Id.Value, person2.Id.Value }, subject, testBody);

			_notifier.AssertWasCalled(
				x => x.Notify(Arg<INotificationMessage>.Matches(s => s.Subject == subject && s.Messages.First() == testBody && s.CustomerName == customerName), Arg<IPerson[]>.Is.Equal(persons)));
		}

		[Test]
		public void ShouldReturnBasicNavigation()
		{
			var principal = (ITeleoptiPrincipal)MockRepository.GenerateStrictMock(typeof(ITeleoptiPrincipal), new Type[] { });
			var identity = MockRepository.GenerateMock<ITeleoptiIdentity>();
			var person = PersonFactory.CreatePersonWithId();

			_authorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyTimeWeb)).Return(true);
			_authorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.Anywhere)).Return(false);
			_currentTeleoptiPrincipal.Stub(x => x.Current()).Return(principal);
			principal.Stub(x => x.Identity).Return(identity);
			principal.Stub(x => x.PersonId).Return(person.Id.GetValueOrDefault());
			identity.Stub(x => x.Name).Return("fake");

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
				Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, @"..\..\..\Teleopti.Ccc.Web\Areas\Messages\Content\Translation\TranslationTemplate.txt")));

			var result = target.Resources() as ContentResult;
			result.ContentType.Should().Be("text/javascript");
			result.Content.Should().Not.Be.Null();
		}
	}
}