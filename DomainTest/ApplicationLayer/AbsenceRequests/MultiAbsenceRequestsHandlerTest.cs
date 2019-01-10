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
	public class MultiAbsenceRequestsHandlerTest : IIsolateSystem
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

		[Test]
		public void ShouldHandleRequestWithSpecificValidators()
		{
			Now.Is("2016-12-01 08:00");
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
			WorkflowControlSetRepository.Add(wfcs);
			var firstDay = new DateOnly(2016, 12, 01);
			var activity = ActivityRepository.Has("activityName");
			var skill = SkillRepository.Has("skillName", activity);

			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 10));
			var person = PersonRepository.Has(new Contract("contract"), new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), skill);
			person.WorkflowControlSet = wfcs;

			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(2016, 12, 1, 11, 2016, 12, 2, 20));
			PersonAssignmentRepository.Has(assignment);

			var personRequest = new PersonRequest(person, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 12, 2016, 12, 1, 13))).WithId();
			personRequest.Pending();
			PersonRequestRepository.Add(personRequest);
			
			var newIdentity = new TeleoptiIdentity("test2", null, null, null, null);
			Thread.CurrentPrincipal = new TeleoptiPrincipal(newIdentity, PersonRepository.FindAllSortByName().FirstOrDefault());

			addToQueue(personRequest, RequestValidatorsFlag.IntradayValidator);

			Target.Handle(new NewMultiAbsenceRequestsCreatedEvent
			{
				PersonRequestIds = new List<Guid> {personRequest.Id.GetValueOrDefault()},
				Timestamp = DateTime.Parse("2016-08-08T11:06:00.7366909Z"),
				Sent = DateTime.UtcNow
			});

			Assert.IsTrue(personRequest.IsPending);
			var msg = createDenyMessage(person.PermissionInformation.Culture(), person.PermissionInformation.UICulture(), person.PermissionInformation.DefaultTimeZone(), new DateTime(2016, 12, 01, 12, 0, 0).Utc());
			Assert.IsTrue(personRequest.GetMessage(new NoFormatting()) == msg);
		}

		[Test]
		public void ShouldHandleRequestsWithDifferentSpecificValidators()
		{

			Now.Is("2016-12-01 08:00");
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
			WorkflowControlSetRepository.Add(wfcs);
			var firstDay = new DateOnly(2016, 12, 01);
			var activity = ActivityRepository.Has("activityName");
			var skill = SkillRepository.Has("skillName", activity);

			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 10));

			var person = PersonRepository.Has(new Contract("contract"), new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), skill);
			person.WorkflowControlSet = wfcs;

			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(2016, 12, 1, 11, 2016, 12, 2, 20));
			PersonAssignmentRepository.Has(assignment);

			var personRequest = new PersonRequest(person, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 12, 2016, 12, 1, 13))).WithId();
			personRequest.Pending();
			PersonRequestRepository.Add(personRequest);

			var personRequest2 = new PersonRequest(person, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 13, 2016, 12, 1, 14))).WithId();
			personRequest2.Pending();
			PersonRequestRepository.Add(personRequest2);

			var newIdentity = new TeleoptiIdentity("test2", null, null, null, null);
			Thread.CurrentPrincipal = new TeleoptiPrincipal(newIdentity, PersonRepository.FindAllSortByName().FirstOrDefault());

			addToQueue(personRequest, RequestValidatorsFlag.IntradayValidator);
			addToQueue(personRequest2, RequestValidatorsFlag.BudgetAllotmentValidator);

			Target.Handle(new NewMultiAbsenceRequestsCreatedEvent
			{
				PersonRequestIds = new List<Guid> { personRequest.Id.GetValueOrDefault(), personRequest2.Id.GetValueOrDefault() },
				Timestamp = DateTime.Parse("2016-08-08T11:06:00.7366909Z"),
				Sent = DateTime.UtcNow
			});

			Assert.IsTrue(personRequest.IsPending);
			var msg = createDenyMessage(person.PermissionInformation.Culture(), person.PermissionInformation.UICulture(), person.PermissionInformation.DefaultTimeZone(), new DateTime(2016, 12, 01, 12, 0, 0).Utc());
			Assert.IsTrue(personRequest.GetMessage(new NoFormatting()) == msg);

			Assert.IsTrue(personRequest2.IsPending);
			var msg2 = Resources.ResourceManager.GetString("BudgetGroupMissing", person.PermissionInformation.Culture()) ?? Resources.BudgetGroupMissing;
			Assert.IsTrue(personRequest2.GetMessage(new NoFormatting()) == msg2);
		}

		[Test, Ignore("different without toggle?")]
		public void ShouldPickUpWaitlistPeriodForAllQueuedRequestsWithSameSentTimestamp()
		{
			var dateTimeNow = new DateTime(2016, 12, 01, 8, 0, 0, 2);
			var truncatedSent = new DateTime(2016, 12, 01, 8, 0, 0, 0);
			Now.Is(dateTimeNow);
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
				StaffingThresholdValidator = new StaffingThresholdValidator(),
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

			var personRequestWaitlisted = new PersonRequest(person, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 10, 2016, 12, 1, 11))).WithId();
			personRequestWaitlisted.Pending();
			personRequestWaitlisted.Deny("", new PersonRequestAuthorizationCheckerForTest(), person, PersonRequestDenyOption.AutoDeny);
			PersonRequestRepository.Add(personRequestWaitlisted);

			addToQueue(personRequest, RequestValidatorsFlag.None);

			QueuedAbsenceRequestRepository.Add(
				new QueuedAbsenceRequest
				{
					PersonRequest = Guid.Empty,
					Sent = truncatedSent,  //truncated by DB
					StartDateTime = personRequestWaitlisted.Request.Period.StartDateTime,
					EndDateTime = personRequestWaitlisted.Request.Period.EndDateTime
				});

			var requestList = new List<Guid>{personRequest.Id.GetValueOrDefault()};
			Target.Handle(new NewMultiAbsenceRequestsCreatedEvent{PersonRequestIds = requestList, Sent = Now.UtcDateTime()});
			personRequestWaitlisted.IsApproved.Should().Be.EqualTo(true);
			personRequest.IsApproved.Should().Be.EqualTo(true);
		}

		[Test, Ignore("different without toggle?")]
		public void ShouldHandleNullInSentTimestamp()
		{
			var dateTimeNow = new DateTime(2016, 12, 01, 8, 0, 0, 2);
			var truncatedSent = new DateTime(2016, 12, 01, 8, 0, 0, 0);
			Now.Is(dateTimeNow);
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
				StaffingThresholdValidator = new StaffingThresholdValidator(),
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

			var personRequestWaitlisted = new PersonRequest(person, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 10, 2016, 12, 1, 11))).WithId();
			personRequestWaitlisted.Pending();
			personRequestWaitlisted.Deny("", new PersonRequestAuthorizationCheckerForTest(), person, PersonRequestDenyOption.AutoDeny);
			PersonRequestRepository.Add(personRequestWaitlisted);

			addToQueue(personRequest, RequestValidatorsFlag.None);

			QueuedAbsenceRequestRepository.Add(
				new QueuedAbsenceRequest
				{
					PersonRequest = Guid.Empty,
					Sent = truncatedSent,  //truncated by DB
					StartDateTime = personRequestWaitlisted.Request.Period.StartDateTime,
					EndDateTime = personRequestWaitlisted.Request.Period.EndDateTime
				});

			QueuedAbsenceRequestRepository.Add(
				new QueuedAbsenceRequest
				{
					PersonRequest = Guid.Empty,
					//Sent = null,
					StartDateTime = personRequestWaitlisted.Request.Period.StartDateTime,
					EndDateTime = personRequestWaitlisted.Request.Period.EndDateTime
				});

			var requestList = new List<Guid> { personRequest.Id.GetValueOrDefault() };
			Target.Handle(new NewMultiAbsenceRequestsCreatedEvent { PersonRequestIds = requestList, Sent = Now.UtcDateTime() });
			personRequestWaitlisted.IsApproved.Should().Be.EqualTo(true);
			personRequest.IsApproved.Should().Be.EqualTo(true);
		}

		public void ShouldFilterWaitlistedRequests()
		{
			var dateTimeNow = new DateTime(2016, 12, 01, 8, 0, 0, 0);
			Now.Is(dateTimeNow);
			var bu = new Domain.Common.BusinessUnit("bu").WithId();
			BusinessUnitRepository.Has(bu);
			var scenario = new Scenario("scnearioName").WithId();
			scenario.DefaultScenario = true;
			scenario.SetBusinessUnit(bu);
			ScenarioRepository.Has(scenario);

			var absence = AbsenceFactory.CreateAbsence("Holiday").WithId();
			var absenceIllness = AbsenceFactory.CreateAbsence("Illness").WithId();
			var wfcs = new WorkflowControlSet().WithId();
			
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});

			var arp = new AbsenceRequestOpenDatePeriod
			{
				Absence = absenceIllness,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new BudgetGroupHeadCountValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			};
			wfcs.AddOpenAbsenceRequestPeriod(arp);

			wfcs.AbsenceRequestWaitlistEnabled = true;
			WorkflowControlSetRepository.Add(wfcs);
			var firstDay = new DateOnly(2016, 12, 01);
			var activity = ActivityRepository.Has("activityName");
			var skill = SkillRepository.Has("skillName", activity);

			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 0));

			var budgetGroup = new BudgetGroup().WithId();
			var budgetDay = new BudgetDay(budgetGroup, scenario, new DateOnly(2016, 12, 1));

			budgetDay.ShrinkedAllowance = 10;

			FakeBudgetGroup.Add(budgetGroup);
			FakeBudgetDay.Add(budgetDay);

			var person = PersonRepository.Has(new Contract("contract"), new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), skill);
			person.WorkflowControlSet = wfcs;
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(2016, 12, 1, 10, 2016, 12, 2, 20));
			PersonAssignmentRepository.Has(assignment);

			person.PersonPeriodCollection.First().BudgetGroup = budgetGroup;

			var personRequestWaitlisted = new PersonRequest(person, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 10, 2016, 12, 1, 11))).WithId();
			personRequestWaitlisted.Pending();
			personRequestWaitlisted.Deny("", new PersonRequestAuthorizationCheckerForTest(), person, PersonRequestDenyOption.AutoDeny);
			PersonRequestRepository.Add(personRequestWaitlisted);

			var requestUsingBudgetGroup = new PersonRequest(person, new AbsenceRequest(absenceIllness, new DateTimePeriod(2016, 12, 1, 14, 2016, 12, 1, 15))).WithId();
			requestUsingBudgetGroup.Pending();
			requestUsingBudgetGroup.Deny("", new PersonRequestAuthorizationCheckerForTest(), person, PersonRequestDenyOption.AutoDeny);
			PersonRequestRepository.Add(requestUsingBudgetGroup);

			QueuedAbsenceRequestRepository.Add(
				new QueuedAbsenceRequest
				{
					PersonRequest = Guid.Empty,
					Sent = dateTimeNow,  //truncated by DB
					StartDateTime = personRequestWaitlisted.Request.Period.StartDateTime,
					EndDateTime = new DateTime(2016, 12, 3)
				});

			var requestList = new List<Guid> { Guid.Empty };
			Target.Handle(new NewMultiAbsenceRequestsCreatedEvent { PersonRequestIds = requestList, Sent = Now.UtcDateTime() });
			personRequestWaitlisted.IsWaitlisted.Should().Be.True();
			requestUsingBudgetGroup.IsApproved.Should().Be.True();
		}

		public void ShouldHandleRequestsWithSeconds()
		{
			var dateTimeNow = new DateTime(2016, 12, 01, 8, 0, 0, 0);
			Now.Is(dateTimeNow);
			var bu = new Domain.Common.BusinessUnit("bu").WithId();
			BusinessUnitRepository.Has(bu);
			var scenario = new Scenario("scenarioName").WithId();
			scenario.DefaultScenario = true;
			scenario.SetBusinessUnit(bu);
			ScenarioRepository.Has(scenario);

			var absence = AbsenceFactory.CreateAbsence("Holiday").WithId();
			var absenceIllness = AbsenceFactory.CreateAbsence("Illness").WithId();
			var wfcs = new WorkflowControlSet().WithId();

			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});

			var arp = new AbsenceRequestOpenDatePeriod
			{
				Absence = absenceIllness,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new BudgetGroupHeadCountValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			};
			wfcs.AddOpenAbsenceRequestPeriod(arp);

			wfcs.AbsenceRequestWaitlistEnabled = true;
			WorkflowControlSetRepository.Add(wfcs);
			var firstDay = new DateOnly(2016, 12, 01);
			var activity = ActivityRepository.Has("activityName");
			var skill = SkillRepository.Has("skillName", activity);

			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 0));

			var budgetGroup = new BudgetGroup().WithId();
			var budgetDay = new BudgetDay(budgetGroup, scenario, new DateOnly(2016, 12, 1));

			budgetDay.ShrinkedAllowance = 10;

			FakeBudgetGroup.Add(budgetGroup);
			FakeBudgetDay.Add(budgetDay);

			var person = PersonRepository.Has(new Contract("contract"), new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), skill);
			person.WorkflowControlSet = wfcs;
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(2016, 12, 1, 10, 2016, 12, 2, 20));
			PersonAssignmentRepository.Has(assignment);

			person.PersonPeriodCollection.First().BudgetGroup = budgetGroup;

			var personRequestWaitlisted = new PersonRequest(person, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 10, 2016, 12, 1, 11))).WithId();
			personRequestWaitlisted.Pending();
			personRequestWaitlisted.Deny("", new PersonRequestAuthorizationCheckerForTest(), person, PersonRequestDenyOption.AutoDeny);
			PersonRequestRepository.Add(personRequestWaitlisted);

			var startTime = new DateTime(2016, 12, 1, 14, 0, 0, DateTimeKind.Utc);
			var requestUsingBudgetGroup = new PersonRequest(person, new AbsenceRequest(absenceIllness, new DateTimePeriod(startTime, startTime.AddHours(5).AddSeconds(59)))).WithId();

			requestUsingBudgetGroup.Pending();
			requestUsingBudgetGroup.Deny("", new PersonRequestAuthorizationCheckerForTest(), person, PersonRequestDenyOption.AutoDeny);
			PersonRequestRepository.Add(requestUsingBudgetGroup);

			QueuedAbsenceRequestRepository.Add(
				new QueuedAbsenceRequest
				{
					PersonRequest = Guid.Empty,
					Sent = dateTimeNow,  //truncated by DB
					StartDateTime = personRequestWaitlisted.Request.Period.StartDateTime,
					EndDateTime = new DateTime(2016, 12, 3)
				});

			var requestList = new List<Guid> { Guid.Empty };
			Target.Handle(new NewMultiAbsenceRequestsCreatedEvent { PersonRequestIds = requestList, Sent = Now.UtcDateTime() });
			personRequestWaitlisted.IsWaitlisted.Should().Be.True();
			requestUsingBudgetGroup.IsApproved.Should().Be.True();
		}

		public void ShouldFilterWaitlistedRequestsAndSkipWithNoWCS_48682()
		{
			var dateTimeNow = new DateTime(2016, 12, 01, 8, 0, 0, 0);
			Now.Is(dateTimeNow);
			var bu = new Domain.Common.BusinessUnit("bu").WithId();
			BusinessUnitRepository.Has(bu);
			var scenario = new Scenario("scenarioName").WithId();
			scenario.DefaultScenario = true;
			scenario.SetBusinessUnit(bu);
			ScenarioRepository.Has(scenario);

			var absence = AbsenceFactory.CreateAbsence("Holiday").WithId();
			var absenceIllness = AbsenceFactory.CreateAbsence("Illness").WithId();
			var wfcs = new WorkflowControlSet().WithId();

			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});

			var arp = new AbsenceRequestOpenDatePeriod
			{
				Absence = absenceIllness,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new BudgetGroupHeadCountValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			};
			wfcs.AddOpenAbsenceRequestPeriod(arp);

			wfcs.AbsenceRequestWaitlistEnabled = true;
			WorkflowControlSetRepository.Add(wfcs);
			var firstDay = new DateOnly(2016, 12, 01);
			var activity = ActivityRepository.Has("activityName");
			var skill = SkillRepository.Has("skillName", activity);

			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 0));

			var person = PersonRepository.Has(new Contract("contract"), new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), skill);
			var person2 = PersonRepository.Has(new Contract("contract"), new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), skill);
			person.WorkflowControlSet = wfcs;
			person2.WorkflowControlSet = wfcs;
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(2016, 12, 1, 10, 2016, 12, 2, 20));
			PersonAssignmentRepository.Has(assignment);

			var personRequestWaitlisted = new PersonRequest(person, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 10, 2016, 12, 1, 11))).WithId();
			personRequestWaitlisted.Pending();
			personRequestWaitlisted.Deny("", new PersonRequestAuthorizationCheckerForTest(), person, PersonRequestDenyOption.AutoDeny);
			PersonRequestRepository.Add(personRequestWaitlisted);

			var requestInWaitlistButNoWfc = new PersonRequest(person2, new AbsenceRequest(absenceIllness, new DateTimePeriod(2016, 12, 1, 14, 2016, 12, 1, 15))).WithId();
			requestInWaitlistButNoWfc.Pending();
			requestInWaitlistButNoWfc.Deny("", new PersonRequestAuthorizationCheckerForTest(), person2, PersonRequestDenyOption.AutoDeny);
			PersonRequestRepository.Add(requestInWaitlistButNoWfc);

			person2.WorkflowControlSet = null;

			QueuedAbsenceRequestRepository.Add(
				new QueuedAbsenceRequest
				{
					PersonRequest = Guid.Empty,
					Sent = dateTimeNow,  //truncated by DB
					StartDateTime = personRequestWaitlisted.Request.Period.StartDateTime,
					EndDateTime = new DateTime(2016, 12, 3)
				});

			var requestList = new List<Guid> { Guid.Empty };
			Target.Handle(new NewMultiAbsenceRequestsCreatedEvent { PersonRequestIds = requestList, Sent = Now.UtcDateTime() });
			personRequestWaitlisted.IsWaitlisted.Should().Be.True();
			requestInWaitlistButNoWfc.IsApproved.Should().Be.False();
		}

		private void addToQueue(IPersonRequest personRequest, RequestValidatorsFlag requestValidatorsFlag)
		{
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				PersonRequest = personRequest.Id.GetValueOrDefault(),
				MandatoryValidators = requestValidatorsFlag,
				StartDateTime = personRequest.Request.Period.StartDateTime,
				EndDateTime = personRequest.Request.Period.EndDateTime,
				Sent = Now.UtcDateTime()
			});
		}

		private static string createDenyMessage(CultureInfo culture, CultureInfo uiCulture, TimeZoneInfo timeZone, DateTime dateTime)
		{
			var detail = new UnderstaffingDetails();
			var val = new StaffingThresholdValidator();
			detail.AddUnderstaffingPeriod(new DateTimePeriod(dateTime, dateTime.AddHours(1)));
			return val.GetUnderStaffingPeriodsString(detail, culture, uiCulture, timeZone);
		}


	}
	
}
