using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.ViewModelFactory
{
	[TestFixture]
	[DomainTest]
	class AbsenceRequestDetailViewModelFactoryTests : IIsolateSystem
	{
		public IPersonRepository PersonRepository;
		public IPersonRequestRepository PersonRequestRepository;
		public IAbsenceRequestDetailViewModelFactory Target;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<AbsenceRequestDetailViewModelFactory>().For<IAbsenceRequestDetailViewModelFactory>();
			isolate.UseTestDouble<PersonRequestProvider>().For<IPersonRequestProvider>();
		}

		[Test]
		public void ShouldRetrieveMyAbsenceWaitlistPosition()
		{
			var baseTime = new DateTime(2016, 1, 1, 0, 0, 0, DateTimeKind.Utc);

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var workflowControlSet = createWorkFlowControlSet(new DateTime(2014, 01, 01),
				new DateTime(2059, 12, 31), absence).WithId();

			var baseCreatedOn = new DateTime(2016, 01, 01);

			var person1 = createAndSetupPerson(workflowControlSet);
			var person2 = createAndSetupPerson(workflowControlSet);
			var person3 = createAndSetupPerson(workflowControlSet);

			var absenceRequest1 = createNewAbsenceRequest(absence,
				new DateTimePeriod(baseTime.AddHours(15), baseTime.AddHours(19)));
			var absenceRequest2 = createNewAbsenceRequest( absence,
				new DateTimePeriod(baseTime.AddHours(08), baseTime.AddHours(16)));
			var absenceRequest3 = createNewAbsenceRequest(absence,
				new DateTimePeriod(baseTime.AddHours(10), baseTime.AddHours(18)));

			var waitListPersonRequestOne = addWaitListPersonRequest(person1, absenceRequest1);

			var waitListPersonRequestTwo = addWaitListPersonRequest(person2, absenceRequest2);

			var waitListPersonRequestThree = addWaitListPersonRequest(person3, absenceRequest3);

			var property = typeof(PersonRequest).GetProperty("CreatedOn");
			property.SetValue(absenceRequest3.Parent, baseCreatedOn.AddHours(08));
			property.SetValue(absenceRequest1.Parent, baseCreatedOn.AddHours(10));
			property.SetValue(absenceRequest2.Parent, baseCreatedOn.AddHours(11));

			var absenceRequestDetailViewModel1 = Target.CreateAbsenceRequestDetailViewModel(waitListPersonRequestOne.Id.GetValueOrDefault());
			Assert.AreEqual(2, absenceRequestDetailViewModel1.WaitlistPosition);

			var absenceRequestDetailViewModel2 = Target.CreateAbsenceRequestDetailViewModel(waitListPersonRequestTwo.Id.GetValueOrDefault());
			Assert.AreEqual(3, absenceRequestDetailViewModel2.WaitlistPosition);

			var absenceRequestDetailViewModel3 = Target.CreateAbsenceRequestDetailViewModel(waitListPersonRequestThree.Id.GetValueOrDefault());
			Assert.AreEqual(1, absenceRequestDetailViewModel3.WaitlistPosition);
		}

		private static WorkflowControlSet createWorkFlowControlSet(DateTime startDate, DateTime endDate, IAbsence absence)
		{
			var workflowControlSet = new WorkflowControlSet
			{
				AbsenceRequestWaitlistEnabled = true,
				AbsenceRequestWaitlistProcessOrder = WaitlistProcessOrder.FirstComeFirstServed
			};
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

		private IAbsenceRequest createNewAbsenceRequest(IAbsence absence, DateTimePeriod requestDateTimePeriod)
		{
			return createAbsenceRequest(absence, requestDateTimePeriod);
		}

		private IPersonRequest addWaitListPersonRequest(IPerson person, IAbsenceRequest absenceRequest)
		{
			var personRequest = new PersonRequest(person, absenceRequest);
			personRequest.SetId(Guid.NewGuid());

			PersonRequestRepository.Add(personRequest);
			personRequest.Pending();
			personRequest.Deny("", new PersonRequestAuthorizationCheckerForTest(), person, PersonRequestDenyOption.AutoDeny);

			return personRequest;
		}

		private IAbsenceRequest createAbsenceRequest(IAbsence absence, DateTimePeriod requestDateTimePeriod)
		{
			var absenceRequest = new AbsenceRequest(absence, requestDateTimePeriod).WithId();

			return absenceRequest;
		}

		private IPerson createAndSetupPerson(IWorkflowControlSet workflowControlSet)
		{
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);

			person.WorkflowControlSet = workflowControlSet;
			return person;
		}
	}
}
