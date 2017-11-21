using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.AgentInfo.Requests
{
	[TestFixture]
	[DomainTest]
	public class AbsenceRequestNoMockTest
	{
		public FakePersonRepository PersonRepository;
		public FakePersonRequestRepository PersonRequestRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeScenarioRepository ScenarioRepository;
		public IScheduleStorage ScheduleRepository;
		
		[Test]
		public void WhenAbsenceRequestIsAcceptedAbsenceShouldBeCorrect()
		{
			var startDateTime = new DateTime(2016, 3, 1, 0, 0, 0, DateTimeKind.Utc);
			var endDateTime = new DateTime(2016, 3, 1, 23, 59, 00, DateTimeKind.Utc);
			var period = new DateTimePeriod(startDateTime, endDateTime);

			var scenario = ScenarioRepository.Has("Default");
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var person = createAndSetupPerson(startDateTime, endDateTime);

			var personRequest = createAbsenceRequest(person, absence, new DateTimePeriod(startDateTime, endDateTime));
			var scheduleDictionary = ScheduleRepository.FindSchedulesForPersons(scenario, new[] {person},
				new ScheduleDictionaryLoadOptions(false, false), period, new[] {person}, false);

			var absenceRequestApprovalService = new AbsenceRequestApprovalService(
				scenario,
				scheduleDictionary,
				NewBusinessRuleCollection.Minimum(), new DoNothingScheduleDayChangeCallBack(), new FakeGlobalSettingDataRepository(), null);

			personRequest.Pending();
			personRequest.Approve(absenceRequestApprovalService, new PersonRequestAuthorizationCheckerForTest());

			var scheduleDay = scheduleDictionary[person].ScheduledDay(new DateOnly(startDateTime));
			var personAbsences = scheduleDay.PersonAbsenceCollection(true);
			var personAbsence = personAbsences[0];

			personAbsence.Layer.Period.Should().Be.EqualTo(period);
		}
		
		private PersonRequest createAbsenceRequest(IPerson person, IAbsence absence, DateTimePeriod requestDateTimePeriod)
		{
			var personRequest = new PersonRequest(person, new AbsenceRequest(absence, requestDateTimePeriod)).WithId();
			PersonRequestRepository.Add(personRequest);

			return personRequest;
		}

		private IPerson createAndSetupPerson(DateTime startDateTime, DateTime endDateTime)
		{
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);

			var assignmentOne = createAssignment(person, startDateTime, endDateTime);
			PersonAssignmentRepository.Has(assignmentOne);

			return person;
		}
		
		private IPersonAssignment createAssignment(IPerson person, DateTime startDate, DateTime endDate)
		{
			return PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(person,
				ScenarioRepository.LoadDefaultScenario(), new DateTimePeriod(startDate, endDate));
		}
	}
}
