using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
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

		[SetUp]
		public void Setup()
		{
			_personRepository = MockRepository.GenerateMock<IPersonRepository>();
			target = new ApplicationController(_personRepository);
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