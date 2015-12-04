using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Requests.Core.FormData;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModelFactory;
using Teleopti.Ccc.WebTest.Areas.Requests.Core.IOC;
using Teleopti.Ccc.WebTest.Core.WeekSchedule.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Requests.Core.ViewModelFactory
{
	[TestFixture, RequestsTest]
	public class RequestsViewModelFactoryTest
	{
		public IRequestsViewModelFactory Target;
		public IPersonRequestRepository PersonRequestRepository;


		[Test]
		public void ShouldGetCreate()
		{
			var dateOnlyPeriod = new DateOnlyPeriod(2015, 11, 01, 2015, 11, 02);
		
			var textRequest = new TextRequest(new DateTimePeriod());
			var personRequest1 = new PersonRequest(PersonFactory.CreatePerson("test1"), textRequest);
			personRequest1.SetId(Guid.NewGuid());
			var personRequest2 = new PersonRequest(PersonFactory.CreatePerson("test2"),
				new AbsenceRequest(AbsenceFactory.CreateAbsence("absence1"), new DateTimePeriod()));
			personRequest2.SetId(Guid.NewGuid());

			var personRequestRepository = PersonRequestRepository as FakePersonRequestRepository;
			
			personRequestRepository.Add(personRequest1);
			personRequestRepository.Add(personRequest2);

			var input = new AllRequestsFormData
			{
				StartDate = dateOnlyPeriod.StartDate,
				EndDate = dateOnlyPeriod.EndDate
			};

			var result = Target.Create(input);

			var first = result.First();
			first.Subject.Should().Be.EqualTo(personRequest1.GetSubject(new NoFormatting()));
			first.Message.Should().Be.EqualTo(personRequest1.GetMessage(new NoFormatting()));
			first.AgentName.Should().Be.EqualTo("test1 test1");
			first.Id.Should().Be.EqualTo(personRequest1.Id.Value);
			first.UpdatedTime.Should().Be.EqualTo(personRequest1.UpdatedOn);
			first.CreatedTime.Should().Be.EqualTo(personRequest1.CreatedOn);
			first.TypeText.Should().Be.EqualTo(personRequest1.Request.RequestTypeDescription);
			first.Status.Should().Be.EqualTo(RequestStatus.New);
			first.StatusText.Should().Be.EqualTo("New");

			var second = result.Second();
			second.Subject.Should().Be.EqualTo(personRequest2.GetSubject(new NoFormatting()));
			second.Message.Should().Be.EqualTo(personRequest2.GetMessage(new NoFormatting()));
			second.AgentName.Should().Be.EqualTo("test2 test2");
			second.Id.Should().Be.EqualTo(personRequest2.Id.Value);
			second.UpdatedTime.Should().Be.EqualTo(personRequest2.UpdatedOn);
			second.CreatedTime.Should().Be.EqualTo(personRequest2.CreatedOn);
			second.TypeText.Should().Be.EqualTo(personRequest2.Request.RequestTypeDescription);
			second.Status.Should().Be.EqualTo(RequestStatus.New);
			second.StatusText.Should().Be.EqualTo("New");

		}


		[Test]
		public void ShouldOrderByUpdatedTimeByDefault()
		{
			var dateOnlyPeriod = new DateOnlyPeriod(2015, 11, 01, 2015, 11, 02);
		
			var textRequest = new TextRequest(new DateTimePeriod());
			var personRequest1 = new PersonRequest(PersonFactory.CreatePerson("test1"), textRequest);
			personRequest1.SetId(Guid.NewGuid());
			personRequest1.UpdatedOn = new DateTime(2015, 11, 01, 01, 00, 00);
			var personRequest2 = new PersonRequest(PersonFactory.CreatePerson("test2"),
				new AbsenceRequest(AbsenceFactory.CreateAbsence("absence1"), new DateTimePeriod()));
			personRequest2.SetId(Guid.NewGuid());
			personRequest2.UpdatedOn = new DateTime(2015, 11, 01, 03, 00, 00);
			var textRequest3 = new TextRequest(new DateTimePeriod());
			var personRequest3 = new PersonRequest(PersonFactory.CreatePerson("test3"), textRequest3);
			personRequest3.SetId(Guid.NewGuid());
			personRequest3.UpdatedOn = new DateTime(2015, 11, 01, 02, 00, 00);

			var personRequestRepository = PersonRequestRepository as FakePersonRequestRepository;

			personRequestRepository.Add(personRequest1);
			personRequestRepository.Add(personRequest2);
			personRequestRepository.Add(personRequest3);
					

			var input = new AllRequestsFormData
			{
				StartDate = dateOnlyPeriod.StartDate,
				EndDate = dateOnlyPeriod.EndDate
			};

			var result = Target.Create(input);
			result.Last().AgentName.Should().Be.EqualTo("test1 test1");
			result.Second().AgentName.Should().Be.EqualTo("test3 test3");
			result.First().AgentName.Should().Be.EqualTo("test2 test2");
		}

		[Test]
		public void ShouldOrderByNameCorrectly()
		{
			var dateOnlyPeriod = new DateOnlyPeriod(2015, 11, 01, 2015, 11, 02);
			var textRequest = new TextRequest(new DateTimePeriod());
			var personRequest1 = new PersonRequest(PersonFactory.CreatePerson("test1"), textRequest);
			personRequest1.SetId(Guid.NewGuid());
			personRequest1.UpdatedOn = new DateTime(2015, 11, 01, 01, 00, 00);
			var personRequest2 = new PersonRequest(PersonFactory.CreatePerson("test2"),
				new AbsenceRequest(AbsenceFactory.CreateAbsence("absence1"), new DateTimePeriod()));
			personRequest2.SetId(Guid.NewGuid());
			personRequest2.UpdatedOn = new DateTime(2015, 11, 01, 03, 00, 00);
			var textRequest3 = new TextRequest(new DateTimePeriod());
			var personRequest3 = new PersonRequest(PersonFactory.CreatePerson("test3"), textRequest3);
			personRequest3.SetId(Guid.NewGuid());
			personRequest3.UpdatedOn = new DateTime(2015, 11, 01, 02, 00, 00);

			var personRequestRepository = PersonRequestRepository as FakePersonRequestRepository;
			personRequestRepository.Add(personRequest1);
			personRequestRepository.Add(personRequest2);
			personRequestRepository.Add(personRequest3);
			
			var input = new AllRequestsFormData
			{
				StartDate = dateOnlyPeriod.StartDate,
				EndDate = dateOnlyPeriod.EndDate,
				SortingOrders = new List<RequestsSortingOrder> { RequestsSortingOrder.AgentNameAsc }
			};

			var result = Target.Create(input);
			result.First().AgentName.Should().Be.EqualTo("test1 test1");
			result.Second().AgentName.Should().Be.EqualTo("test2 test2");
			result.Last().AgentName.Should().Be.EqualTo("test3 test3");

		}

		[Test]
		public void ShouldOrderByCreatedTimeCorrectly()
		{
			var dateOnlyPeriod = new DateOnlyPeriod(2015, 11, 01, 2015, 11, 02);

			var textRequest = new TextRequest(new DateTimePeriod());
			var personRequest1 = new PersonRequest(PersonFactory.CreatePerson("test1"), textRequest);
			personRequest1.SetId(Guid.NewGuid());
			typeof(PersonRequest).GetProperty("CreatedOn").SetValue(personRequest1, new DateTime(2015, 01, 03, 18, 00, 00), null);

			var personRequest2 = new PersonRequest(PersonFactory.CreatePerson("test2"),
				new AbsenceRequest(AbsenceFactory.CreateAbsence("absence1"), new DateTimePeriod()));
			personRequest2.SetId(Guid.NewGuid());
			typeof(PersonRequest).GetProperty("CreatedOn").SetValue(personRequest2, new DateTime(2015, 01, 01, 18, 00, 00), null);

			var textRequest3 = new TextRequest(new DateTimePeriod());
			var personRequest3 = new PersonRequest(PersonFactory.CreatePerson("test3"), textRequest3);
			personRequest3.SetId(Guid.NewGuid());
			typeof(PersonRequest).GetProperty("CreatedOn").SetValue(personRequest3, new DateTime(2015, 01, 02, 18, 00, 00), null);

			var personRequestRepository = PersonRequestRepository as FakePersonRequestRepository;

			personRequestRepository.Add(personRequest1);
			personRequestRepository.Add(personRequest2);
			personRequestRepository.Add(personRequest3);

			var input = new AllRequestsFormData
			{
				StartDate = dateOnlyPeriod.StartDate,
				EndDate = dateOnlyPeriod.EndDate,
				SortingOrders = new List<RequestsSortingOrder> { RequestsSortingOrder.CreatedOnAsc }
			};

			var result = Target.Create(input);
			result.First().AgentName.Should().Be.EqualTo("test2 test2");
			result.Second().AgentName.Should().Be.EqualTo("test3 test3");
			result.Last().AgentName.Should().Be.EqualTo("test1 test1");
		}
	}
}