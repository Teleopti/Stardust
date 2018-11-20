using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.ViewModelFactory
{
	class AbsenceRequestDetailViewModelFactoryTests
	{
		[Test]
		public void ShouldRetrieveMyAbsenceWaitlistPosition()
		{
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var personRequestProvider = MockRepository.GenerateMock<IPersonRequestProvider>();
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();

			var absence = AbsenceFactory.CreateAbsenceWithId();
			var absenceRequestWaitlistProvider = new AbsenceRequestWaitlistProvider(personRequestRepository, personRepository);

			var personRequestList = new List<IPersonRequest>();
			personRequestRepository.Stub(x => x.FindAbsenceAndTextRequests(null, out _, true)).Return(personRequestList).IgnoreArguments();

			var workflowControlSet = createWorkFlowControlSet(new DateTime(2015, 01, 01, 00, 00, 00, DateTimeKind.Utc), new DateTime(2019, 01, 01, 00, 00, 00, DateTimeKind.Utc), absence);

			var personRequest1 = createWaitlistedPersonRequest(absence, workflowControlSet);
			var personRequest2 = createWaitlistedPersonRequest(absence, workflowControlSet);
			var personRequest3 = createWaitlistedPersonRequest(absence, workflowControlSet);

			personRequestList.AddRange(new[] { personRequest1, personRequest2, personRequest3 });

			var personBudgetGroupNameList = new List<PersonBudgetGroupName>
			{
				new PersonBudgetGroupName {PersonId = personRequest1.Person.Id.Value, BudgetGroupName = ""},
				new PersonBudgetGroupName {PersonId = personRequest2.Person.Id.Value, BudgetGroupName = ""},
				new PersonBudgetGroupName {PersonId = personRequest3.Person.Id.Value, BudgetGroupName = ""}
			};
			personRepository.Stub(x => x.FindBudgetGroupNameForPeople(null, DateTime.Now)).Return(personBudgetGroupNameList).IgnoreArguments();

			var property = typeof(PersonRequest).GetProperty("CreatedOn");
			property.SetValue(personRequest1, new DateTime(2016, 01, 01, 10, 00, 00));
			property.SetValue(personRequest3, new DateTime(2016, 01, 01, 8, 00, 00));
			property.SetValue(personRequest2, new DateTime(2016, 01, 01, 11, 00, 00));

			personRequestProvider.Stub(p => p.RetrieveRequest(personRequest1.Id.GetValueOrDefault())).Return(personRequest1);

			var absenceRequestDetailViewModelFactory = new AbsenceRequestDetailViewModelFactory(personRequestProvider, absenceRequestWaitlistProvider);
			var absenceRequestDetailViewModel = absenceRequestDetailViewModelFactory.CreateAbsenceRequestDetailViewModel(personRequest1.Id.GetValueOrDefault());

			Assert.AreEqual(2, absenceRequestDetailViewModel.WaitlistPosition);
		}

		private static PersonRequest createWaitlistedPersonRequest(IAbsence absence, IWorkflowControlSet workflowControlSet)
		{
			var person = PersonFactory.CreatePersonWithId();
			person.WorkflowControlSet = workflowControlSet;

			var absenceRequest = new AbsenceRequest(absence,
				new DateTimePeriod(new DateTime(2015, 10, 10, 00, 00, 00, DateTimeKind.Utc),
					new DateTime(2015, 10, 10, 23, 59, 00, DateTimeKind.Utc)));

			var personRequest = new PersonRequest(person)
			{
				Request = absenceRequest
			};

			personRequest.Deny("work harder", new PersonRequestAuthorizationCheckerForTest());

			absenceRequest.SetId(Guid.NewGuid());
			absenceRequest.Parent.SetId(Guid.NewGuid());

			return personRequest;
		}


		private static WorkflowControlSet createWorkFlowControlSet(DateTime startDate, DateTime endDate, IAbsence absence)
		{
			var workflowControlSet = new WorkflowControlSet { AbsenceRequestWaitlistEnabled = true };
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));

			var absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				Period = dateOnlyPeriod,
				AbsenceRequestProcess = new GrantAbsenceRequest(),
				OpenForRequestsPeriod = dateOnlyPeriod
			};

			workflowControlSet.InsertPeriod(absenceRequestOpenPeriod, 0);

			return workflowControlSet;

		}

	}
}
