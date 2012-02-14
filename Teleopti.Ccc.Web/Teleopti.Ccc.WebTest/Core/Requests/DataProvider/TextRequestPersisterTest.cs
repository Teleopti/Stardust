using System;
using System.Web;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.DataProvider
{
	[TestFixture]
	public class TextRequestPersisterTest
	{
		[Test]
		public void ShouldAddTextRequest()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var personRequest = MockRepository.GenerateMock<IPersonRequest>();
			var target = new TextRequestPersister(personRequestRepository, mapper);
			var form = new TextRequestForm();

			mapper.Stub(x => x.Map<TextRequestForm, IPersonRequest>(form)).Return(personRequest);

			target.Persist(form);

			personRequestRepository.AssertWasCalled(x => x.Add(personRequest));
		}

		[Test]
		public void ShouldDelete()
		{
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var target = new TextRequestPersister(personRequestRepository, null);
			var personRequest = new PersonRequest(new Person());
			personRequest.SetId(Guid.NewGuid());

			personRequestRepository.Stub(x => x.Find(personRequest.Id.Value)).Return(personRequest);

			target.Delete(personRequest.Id.Value);

			personRequestRepository.AssertWasCalled(x => x.Remove(personRequest));
		}

		[Test]
		public void ShouldThrowHttp404OIfTextRequestDoesNotExists()
		{
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var target = new TextRequestPersister(personRequestRepository, null);
			var id = Guid.NewGuid();

			personRequestRepository.Stub(x => x.Find(id)).Return(null);

			var exception = Assert.Throws<HttpException>(() => target.Delete(id));
			exception.GetHttpCode().Should().Be(404);
		}

		[Test]
		public void ShouldUpdateExistingTextRequest()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var personRequest = MockRepository.GenerateMock<IPersonRequest>();
			var target = new TextRequestPersister(personRequestRepository, mapper);
			var form = new TextRequestForm();
			var id = Guid.NewGuid();
			form.EntityId = id;

			personRequestRepository.Stub(x => x.Find(id)).Return(personRequest);
			mapper.Stub(x => x.Map<TextRequestForm, IPersonRequest>(form)).Return(personRequest);

			target.Persist(form);

			mapper.AssertWasCalled(x => x.Map(form, personRequest));
		}
	}
}