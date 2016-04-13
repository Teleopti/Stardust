using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.AgentInfo.Requests
{
	[TestFixture]
	class AbsenceRequestNoMockTest
	{


		readonly ICurrentScenario _currentScenario = new FakeCurrentScenario();
		private IPersonRepository _personRepository;
		private IPersonRequestRepository _personRequestRepository;
		FakeScheduleDataReadScheduleStorage _scheduleRepository;
		private SwapAndModifyService _swapAndModifyService;

		[SetUp]
		public void Setup()
		{
			_personRepository = new FakePersonRepository();
			_personRequestRepository = new FakePersonRequestRepository();
			_scheduleRepository = new FakeScheduleDataReadScheduleStorage();
			_swapAndModifyService = new SwapAndModifyService(new SwapService(), new DoNothingScheduleDayChangeCallBack());
		}


		[Test]
		public void WhenAbsenceRequestIsAcceptedAbsenceShouldHaveRequestReference()
		{
			var startDateTime = new DateTime(2016, 3, 1, 0, 0, 0, DateTimeKind.Utc);
			var endDateTime = new DateTime(2016, 3, 1, 23, 59, 00, DateTimeKind.Utc);
			var period = new DateTimePeriod (startDateTime, endDateTime);

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var person = createAndSetupPerson(startDateTime, endDateTime);

			var personRequest = createAbsenceRequest(person, absence, new DateTimePeriod(startDateTime, endDateTime));
			var scheduleDictionary = _scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod (person, null, period, _currentScenario.Current());
			
			var requestApprovalServiceScheduler = new RequestApprovalServiceScheduler(
				scheduleDictionary,
				_currentScenario.Current(),
				_swapAndModifyService,
				NewBusinessRuleCollection.Minimum(), new DoNothingScheduleDayChangeCallBack(), new FakeGlobalSettingDataRepository());

			personRequest.Pending();
			personRequest.Approve (requestApprovalServiceScheduler, new PersonRequestAuthorizationCheckerForTest());

			var scheduleDay = scheduleDictionary[person].ScheduledDay(new DateOnly (startDateTime));
			var personAbsences = scheduleDay.PersonAbsenceCollection (true);
			var personAbsence = personAbsences[0];
			
			Assert.AreSame (personRequest.Request, personAbsence.AbsenceRequest);

		}
		
		
		private PersonRequest createAbsenceRequest(IPerson person, IAbsence absence, DateTimePeriod requestDateTimePeriod)
		{
			var personRequest = new PersonRequest(person, new AbsenceRequest(absence, requestDateTimePeriod));

			personRequest.SetId(Guid.NewGuid());
			_personRequestRepository.Add(personRequest);

			return personRequest;
		}

		private IPerson createAndSetupPerson(DateTime startDateTime, DateTime endDateTime)
		{
			var person = PersonFactory.CreatePersonWithId();
			_personRepository.Add(person);

			var assignmentOne = createAssignment(person, startDateTime, endDateTime, _currentScenario);
			_scheduleRepository.Set(new IScheduleData[] { assignmentOne });

			return person;
		}


		private IPersonAssignment createAssignment(IPerson person, DateTime startDate, DateTime endDate, ICurrentScenario currentScenario)
		{
			return PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(
				currentScenario.Current(),
				person,
				new DateTimePeriod(startDate, endDate));
		}


	}
}
