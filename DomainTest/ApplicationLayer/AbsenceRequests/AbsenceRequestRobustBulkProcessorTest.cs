using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[DomainTest]
	public class AbsenceRequestRobustBulkProcessorTest : IIsolateSystem
	{
		public IPersonRequestRepository PersonRequestRepository;
		public IQueuedAbsenceRequestRepository QueuedAbsenceRequestRepository;
		public MultiAbsenceRequestsHandler Target;
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public MutableNow Now;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakeWorkflowControlSetRepository WorkflowControlSetRepository;
		public FakeBudgetGroupRepository FakeBudgetGroup;
		public FakeBudgetDayRepository FakeBudgetDay;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<MultiAbsenceRequestsHandler>().For<IHandleEvent<NewMultiAbsenceRequestsCreatedEvent>>();
			isolate.UseTestDouble<FakeCommandDispatcher>().For<ICommandDispatcher>();
			isolate.UseTestDouble<FakeASMScheduleChangeTimeRepository>().For<IASMScheduleChangeTimeRepository>();
		}

		//[Test]
		//public void ShouldPickUpWaitlistPeriodForAllQueuedRequestsWithSameSentTimestamp()
		//{
		//	var dateTimeNow = new DateTime(2016, 12, 01, 8, 0, 0, 2);
		//	//var truncatedSent = new DateTime(2016, 12, 01, 8, 0, 0, 0);
		//	Now.Is(dateTimeNow);
		//	var bu = new Domain.Common.BusinessUnit("bu").WithId();
		//	BusinessUnitRepository.Has(bu);
		//	var scenario = new Scenario("scnearioName").WithId();
		//	scenario.DefaultScenario = true;
		//	scenario.SetBusinessUnit(bu);
		//	ScenarioRepository.Has(scenario);

		//	var absence = AbsenceFactory.CreateAbsence("Holiday").WithId();

		//	var wfcs = new WorkflowControlSet().WithId();
		//	wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod()
		//	{
		//		Absence = absence,
		//		PersonAccountValidator = new AbsenceRequestNoneValidator(),
		//		StaffingThresholdValidator = new StaffingThresholdValidator(),
		//		Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
		//		OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
		//		AbsenceRequestProcess = new GrantAbsenceRequest()
		//	});
		//	wfcs.AbsenceRequestWaitlistEnabled = true;
		//	WorkflowControlSetRepository.Add(wfcs);
		//	var firstDay = new DateOnly(2016, 12, 01);
		//	var activity = ActivityRepository.Has("activityName");
		//	var skill = SkillRepository.Has("skillName", activity);

		//	SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 0));

		//	var person = PersonRepository.Has(new Contract("contract"), new ContractSchedule("_"),
		//		new PartTimePercentage("_"), new Team {Site = new Site("site")},
		//		new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), skill);
		//	person.WorkflowControlSet = wfcs;
		//	var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario,
		//		new DateTimePeriod(2016, 12, 1, 10, 2016, 12, 2, 20));
		//	PersonAssignmentRepository.Has(assignment);

		//	var personRequest = new PersonRequest(person,
		//		new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 12, 2016, 12, 1, 13))).WithId();
		//	personRequest.Pending();
		//	PersonRequestRepository.Add(personRequest);

		//	var personRequestWaitlisted = new PersonRequest(person,
		//		new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 10, 2016, 12, 1, 11))).WithId();
		//	personRequestWaitlisted.Pending();
		//	personRequestWaitlisted.Deny("", new PersonRequestAuthorizationCheckerForTest(), person,
		//		PersonRequestDenyOption.AutoDeny);
		//	PersonRequestRepository.Add(personRequestWaitlisted);

		//	addToQueue(personRequest, RequestValidatorsFlag.None);

		//	QueuedAbsenceRequestRepository.Add(
		//		new QueuedAbsenceRequest
		//		{
		//			PersonRequest = Guid.Empty,
		//			Sent = truncatedSent, //truncated by DB
		//			StartDateTime = personRequestWaitlisted.Request.Period.StartDateTime,
		//			EndDateTime = personRequestWaitlisted.Request.Period.EndDateTime
		//		});

		//	var requestList = new List<Guid> {personRequest.Id.GetValueOrDefault()};
		//	Target.Handle(new NewMultiAbsenceRequestsCreatedEvent
		//		{PersonRequestIds = requestList, Sent = Now.UtcDateTime()});
		//	personRequestWaitlisted.IsApproved.Should().Be.EqualTo(true);
		//	personRequest.IsApproved.Should().Be.EqualTo(true);
		//}

		[Test]
		public void ShouldHandleRequestWithSpecificValidators()

		{
			Now.Is("2016-12-01 08:00");
			DateTime sent = Now.UtcDateTime();
			var bu = new Domain.Common.BusinessUnit("bu").WithId();
			BusinessUnitRepository.Has(bu);
			var scenario = new Scenario("scnearioName").WithId();
			scenario.DefaultScenario = true;
			scenario.SetBusinessUnit(bu);
			ScenarioRepository.Has(scenario);

			var absence = AbsenceFactory.CreateAbsence("Holiday").WithId();

			var wfcs = new WorkflowControlSet().WithId();
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new AbsenceRequestNoneValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});
			wfcs.AbsenceRequestWaitlistEnabled = true;
			WorkflowControlSetRepository.Add(wfcs);
			var firstDay = new DateOnly(2016, 12, 01);
			var activity = ActivityRepository.Has("activityName");
			var skill = SkillRepository.Has("skillName", activity);

			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 0));
			var person = PersonRepository.Has(new Contract("contract"), new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), skill);
			person.WorkflowControlSet = wfcs;

			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(2016, 12, 1, 10, 2016, 12, 2, 20));
			PersonAssignmentRepository.Has(assignment);

			var personRequest = new PersonRequest(person, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 12, 2016, 12, 1, 13))).WithId();
			personRequest.Pending();
			PersonRequestRepository.Add(personRequest);

			var newIdentity = new TeleoptiIdentity("test2", null, null, null, null);
			Thread.CurrentPrincipal = new TeleoptiPrincipalForLegacy(newIdentity, PersonRepository.FindAllSortByName().FirstOrDefault());

			addToQueue(personRequest, RequestValidatorsFlag.IntradayValidator, sent);


			var secondDay= new DateOnly(2016, 12, 15);
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, secondDay, 0));

			var assignment2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(2016, 12, 15, 10, 2016, 12, 16, 20));
			PersonAssignmentRepository.Has(assignment2);

			var personRequest2 = new PersonRequest(person, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 15, 12, 2016, 12, 15, 13))).WithId();
			personRequest2.Pending();
			personRequest2.Pending();
			personRequest2.Deny("", new PersonRequestAuthorizationCheckerForTest(), person, PersonRequestDenyOption.AutoDeny);
			PersonRequestRepository.Add(personRequest2);

			addPlaceholderToQueue(personRequest2, sent);

			var thirdDay = new DateOnly(2016, 12, 25);
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, thirdDay, 0));

			var assignment3 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(2016, 12, 25, 10, 2016, 12, 26, 20));
			PersonAssignmentRepository.Has(assignment3);

			var personRequest3 = new PersonRequest(person, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 25, 12, 2016, 12, 25, 13))).WithId();
			personRequest3.Pending();
			personRequest3.Pending();
			personRequest3.Deny("", new PersonRequestAuthorizationCheckerForTest(), person, PersonRequestDenyOption.AutoDeny);
			PersonRequestRepository.Add(personRequest3);

			addPlaceholderToQueue(personRequest3, sent);


			Target.Handle(new NewMultiAbsenceRequestsCreatedEvent
			{
				PersonRequestIds = new List<Guid>
				{
					personRequest.Id.GetValueOrDefault()
				},
				Timestamp = DateTime.Parse("2016-08-08T11:06:00.7366909Z"),
				Sent = Now.UtcDateTime()
			});
			personRequest.IsApproved.Should().Be.EqualTo(true);
			personRequest2.IsApproved.Should().Be.EqualTo(false);
			personRequest3.IsApproved.Should().Be.EqualTo(false);

		}



		private void addToQueue(IPersonRequest personRequest, RequestValidatorsFlag requestValidatorsFlag, DateTime sent)
		{
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				PersonRequest = personRequest.Id.GetValueOrDefault(),
				MandatoryValidators = requestValidatorsFlag,
				StartDateTime = personRequest.Request.Period.StartDateTime,
				EndDateTime = personRequest.Request.Period.EndDateTime,
				Sent = sent
			});
		}

		private static string createDenyMessage(CultureInfo culture, CultureInfo uiCulture, TimeZoneInfo timeZone, DateTime dateTime)
		{
			var detail = new UnderstaffingDetails();
			var val = new StaffingThresholdValidator();
			detail.AddUnderstaffingPeriod(new DateTimePeriod(dateTime, dateTime.AddHours(1)));
			return val.GetUnderStaffingPeriodsString(detail, culture, uiCulture, timeZone);
		}

		private void addPlaceholderToQueue(IPersonRequest personRequest, DateTime sent)
		{
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				PersonRequest = Guid.Empty,
				StartDateTime = personRequest.Request.Period.StartDateTime.Date,
				EndDateTime = personRequest.Request.Period.StartDateTime.Date.AddDays(1).AddSeconds(-1),
				Sent = sent
			});
		}
	}
}