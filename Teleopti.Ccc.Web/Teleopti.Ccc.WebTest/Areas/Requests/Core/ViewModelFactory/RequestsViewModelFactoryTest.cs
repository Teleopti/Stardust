using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
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
		public IPermissionProvider PermissionProvider;
		private static void setPersonRequestValue(string prop, PersonRequest personRequest, DateTime dateTime)
		{
			typeof(PersonRequest).GetProperty(prop).SetValue(personRequest, dateTime);
		}

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
			setPersonRequestValue("CreatedOn", personRequest1, new DateTime(2015, 01, 03, 18, 00, 00));

			var personRequest2 = new PersonRequest(PersonFactory.CreatePerson("test2"),
				new AbsenceRequest(AbsenceFactory.CreateAbsence("absence1"), new DateTimePeriod()));
			personRequest2.SetId(Guid.NewGuid());
			setPersonRequestValue("CreatedOn", personRequest2, new DateTime(2015, 01, 01, 18, 00, 00));

			var textRequest3 = new TextRequest(new DateTimePeriod());
			var personRequest3 = new PersonRequest(PersonFactory.CreatePerson("test3"), textRequest3);
			personRequest3.SetId(Guid.NewGuid());
			setPersonRequestValue("CreatedOn", personRequest3, new DateTime(2015, 01, 02, 18, 00, 00));

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



		[Test]
		public void ShouldNotSeeRequestWithoutPermission()
		{
			var textRequest = new TextRequest(new DateTimePeriod(2015, 12, 1, 2015, 12, 15));
			var personRequest1 = new PersonRequest(PersonFactory.CreatePerson("test1"), textRequest);
			personRequest1.SetId(Guid.NewGuid());

			var personRequest2 = new PersonRequest(PersonFactory.CreatePerson("test2"),
				new AbsenceRequest(AbsenceFactory.CreateAbsence("absence1"), new DateTimePeriod(2015, 12, 10, 2015, 12, 15)));
			personRequest2.SetId(Guid.NewGuid());

			var personRequestRepository = PersonRequestRepository as FakePersonRequestRepository;
			personRequestRepository.Add(personRequest1);
			personRequestRepository.Add(personRequest2);


			var permissionProvider = PermissionProvider as Global.FakePermissionProvider;
			permissionProvider.Enable();

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 12, 1),
				EndDate = new DateOnly(2015, 12, 31)
			};

			var result = Target.Create(input);
			result.Count().Should().Be.EqualTo(0);
		}


		[Test]
		public void ShouldNotSeeRequestBeforePermissionDate()
		{
			var textRequest = new TextRequest(new DateTimePeriod(2015, 12, 1, 2015, 12, 15));
			var personRequest1 = new PersonRequest(PersonFactory.CreatePerson("test1"), textRequest);
			personRequest1.SetId(Guid.NewGuid());

			var personRequest2 = new PersonRequest(PersonFactory.CreatePerson("test2"),
				new AbsenceRequest(AbsenceFactory.CreateAbsence("absence1"), new DateTimePeriod(2015, 12, 10, 2015, 12, 15)));
			personRequest2.SetId(Guid.NewGuid());

			var textRequest3 = new TextRequest(new DateTimePeriod(2015, 12, 20, 2015, 12, 25));
			var personRequest3 = new PersonRequest(PersonFactory.CreatePerson("test3"), textRequest3);
			personRequest3.SetId(Guid.NewGuid());


			var personRequestRepository = PersonRequestRepository as FakePersonRequestRepository;
			personRequestRepository.Add(personRequest1);
			personRequestRepository.Add(personRequest2);
			personRequestRepository.Add(personRequest3);

			var permissionProvider = PermissionProvider as Global.FakePermissionProvider;
			permissionProvider.Enable();

			permissionProvider.Permit(DefinedRaptorApplicationFunctionPaths.WebRequests, new DateOnly(2015, 12, 18));

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 12, 1),
				EndDate = new DateOnly(2015, 12, 31)
			};

			var result = Target.Create(input);
			result.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldOrderByNameAndUpdatedTimeAndCreatedTimeCorrectly()
		{
			var dateOnlyPeriod = new DateOnlyPeriod(2015, 11, 01, 2015, 11, 02);

			var personRequest1 = new PersonRequest(PersonFactory.CreatePerson("test1"), new TextRequest(new DateTimePeriod()));
			personRequest1.SetId(Guid.NewGuid());
			setPersonRequestValue("CreatedOn", personRequest1, new DateTime(2015, 01, 01, 18, 00, 00));
			personRequest1.UpdatedOn = new DateTime(2015, 11, 11, 00, 00, 00);

			var personRequest2 = new PersonRequest(PersonFactory.CreatePerson("test2"), new AbsenceRequest(AbsenceFactory.CreateAbsence("absence2"), new DateTimePeriod()));
			personRequest2.SetId(Guid.NewGuid());
			setPersonRequestValue("CreatedOn", personRequest2, new DateTime(2015, 02, 02, 18, 00, 00));
			personRequest2.UpdatedOn = new DateTime(2015, 11, 30, 00, 00, 00);

			var personRequest3 = new PersonRequest(PersonFactory.CreatePerson("test2"), new TextRequest(new DateTimePeriod()));
			personRequest3.SetId(Guid.NewGuid());
			setPersonRequestValue("CreatedOn", personRequest3, new DateTime(2015, 02, 03, 18, 00, 00));
			personRequest3.UpdatedOn = new DateTime(2015, 11, 06, 00, 00, 00);

			var personRequest4 = new PersonRequest(PersonFactory.CreatePerson("test2"), new AbsenceRequest(AbsenceFactory.CreateAbsence("absence4"), new DateTimePeriod()));
			personRequest4.SetId(Guid.NewGuid());
			setPersonRequestValue("CreatedOn", personRequest4, new DateTime(2015, 02, 02, 18, 00, 00));
			personRequest4.UpdatedOn = new DateTime(2015, 11, 20, 00, 00, 00);

			var personRequestRepository = PersonRequestRepository as FakePersonRequestRepository;

			personRequestRepository.Add(personRequest1);
			personRequestRepository.Add(personRequest2);
			personRequestRepository.Add(personRequest3);
			personRequestRepository.Add(personRequest4);

			var input = new AllRequestsFormData
			{
				StartDate = dateOnlyPeriod.StartDate,
				EndDate = dateOnlyPeriod.EndDate,
				SortingOrders = new[] { RequestsSortingOrder.AgentNameDesc, RequestsSortingOrder.CreatedOnDesc, RequestsSortingOrder.UpdatedOnAsc }
			};

			var result = Target.Create(input);
			result.First().UpdatedTime.Should().Be.EqualTo(new DateTime(2015, 11, 06, 00, 00, 00));
			result.Second().CreatedTime.Should().Be.EqualTo(new DateTime(2015, 02, 02, 18, 00, 00));
			result.Last().AgentName.Should().Be.EqualTo("test1 test1");
		}
	}
}