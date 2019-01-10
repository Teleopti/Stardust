using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[DomainTest]
	[TestWithStaticDependenciesDONOTUSE]
	[TestFixture, SetCulture("en-US")]
	public class MultiAbsenceRequestsUpdaterTest : IIsolateSystem
	{
		public IMultiAbsenceRequestsUpdater Target;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public FakePersonRequestRepository PersonRequestRepository;
		public FakePersonRepository PersonRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public MutableNow Now;
		public FakePersonAbsenceAccountRepository PersonAbsenceAccountRepository;
		public FakeBudgetGroupRepository BudgetGroupRepository;
		public FakeBudgetDayRepository BudgetDayRepository;
		
		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeCommandDispatcher>().For<ICommandDispatcher>();
			isolate.UseTestDouble<FakeBudgetGroupRepository>().For<IBudgetGroupRepository>();
			isolate.UseTestDouble<FakeBudgetDayRepository>().For<IBudgetDayRepository>();
			isolate.UseTestDouble<FakeASMScheduleChangeTimeRepository>().For<IASMScheduleChangeTimeRepository>();
		}

		[Test]
		public void ShouldDenyIfPeriodNotOpenForRequest()
		{
			Now.Is(DateTime.Now);
			var scenario = ScenarioRepository.Has("scnearioName");
			var absence = AbsenceFactory.CreateAbsence("Holiday");

			var wfcs = new WorkflowControlSet().WithId();
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new AbsenceRequestNoneValidator(),
				Period = new DateOnlyPeriod(2016, 12, 1, 2016, 12, 2),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2016, 11, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});

			var person = PersonFactory.CreatePerson(wfcs).WithId();

			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(2016, 12, 1, 11, 2016, 12, 2, 20));
			PersonAssignmentRepository.Has(assignment);

			var personRequest = new PersonRequest(person, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 12, 2016, 12, 1, 13))).WithId();
			personRequest.Pending();
			PersonRequestRepository.Add(personRequest);

			Target.UpdateAbsenceRequest(new List<Guid> { personRequest.Id.GetValueOrDefault() });
			personRequest.DenyReason.Should().Be.EqualTo(Resources.RequestDenyReasonClosedPeriod);
		}


		[Test]
		public void ShouldDenyIfPersonIsAlreadyAbsent()
		{
			Now.Is(new DateTime(2016, 12, 1, 10, 0, 0));
			var scenario = ScenarioRepository.Has("scnearioName");
			var absence = AbsenceFactory.CreateAbsence("Holiday");

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

			var person = PersonFactory.CreatePerson(wfcs).WithId();

			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(2016, 12, 1, 11, 2016, 12, 2, 20));
			PersonAssignmentRepository.Has(assignment);
			var absenceLayer = new AbsenceLayer(absence, new DateTimePeriod(2016, 12, 1, 11, 2016, 12, 2, 20));
			var personAbsence = new PersonAbsence(person, scenario, absenceLayer);
			PersonAbsenceRepository.Has(personAbsence);

			var personRequest = new PersonRequest(person, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 12, 2016, 12, 1, 13))).WithId();
			personRequest.Pending();
			PersonRequestRepository.Add(personRequest);

			Target.UpdateAbsenceRequest(new List<Guid> { personRequest.Id.GetValueOrDefault() });
			personRequest.DenyReason.Should().Be.EqualTo(Resources.RequestDenyReasonAlreadyAbsent);
		}

		[Test]
		public void ShouldDenyIfPersonIsAlreadyAbsentMidnight()
		{
			Now.Is(new DateTime(2016, 12, 1, 10, 0, 0));
			var scenario = ScenarioRepository.Has("scnearioName");
			var absence = AbsenceFactory.CreateAbsence("Holiday");

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

			var person = PersonFactory.CreatePerson(wfcs).WithId();

			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(2016, 12, 1, 11, 2016, 12, 2, 20));
			PersonAssignmentRepository.Has(assignment);
			var absenceLayer = new AbsenceLayer(absence, new DateTimePeriod(2016, 12, 1, 11, 2016, 12, 2, 20));
			var personAbsence = new PersonAbsence(person, scenario, absenceLayer);
			PersonAbsenceRepository.Has(personAbsence);

			var personRequest = new PersonRequest(person, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 2, 12, 2016, 12, 2, 13))).WithId();
			personRequest.Pending();
			PersonRequestRepository.Add(personRequest);

			Target.UpdateAbsenceRequest(new List<Guid> { personRequest.Id.GetValueOrDefault() });
			personRequest.DenyReason.Should().Be.EqualTo(Resources.RequestDenyReasonAlreadyAbsent);
		}

		[Test]
		public void ShouldDenyExpiredRequestWithWaitlistingEnabled()
		{
			Now.Is(new DateTime(2016, 12, 1, 10, 0, 0));
			var scenario = ScenarioRepository.Has("scnearioName");
			var absence = AbsenceFactory.CreateAbsence("Holiday");

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
			wfcs.AbsenceRequestExpiredThreshold = 15;
			wfcs.AbsenceRequestWaitlistEnabled = true;

			var person = PersonFactory.CreatePerson(wfcs).WithId();

			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(2016, 12, 1, 11, 2016, 12, 2, 20));
			PersonAssignmentRepository.Has(assignment);

			var personRequest = new PersonRequest(person, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 10, 2016, 12, 1, 13))).WithId();
			personRequest.Pending();
			PersonRequestRepository.Add(personRequest);

			Target.UpdateAbsenceRequest(new List<Guid>() { personRequest.Id.GetValueOrDefault() });

			personRequest.IsDenied.Should().Be.True();
			personRequest.IsWaitlisted.Should().Be.False();
			personRequest.DenyReason.Should().Be.EqualTo(string.Format(Resources.RequestDenyReasonRequestExpired, personRequest.Request.Period.StartDateTime, 15));
		}

		[Test]
		public void ShouldOnlyApprove50RequestsSoNotUnderstaffed()
		{
			Now.Is(new DateTime(2016, 12, 1, 10, 0, 0));
			var firstDay = new DateOnly(2016, 12, 1);
			var activity = ActivityRepository.Has("activityName");
			var skill = SkillRepository.Has("skillName", activity);
			skill.StaffingThresholds = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));

			var scenario = ScenarioRepository.Has("scnearioName");
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(168), TimeSpan.FromHours(1), TimeSpan.FromHours(1))
			};
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var workflowControlSet = new WorkflowControlSet().WithId();

			var absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			};

			workflowControlSet.InsertPeriod(absenceRequestOpenPeriod, 0);

			var contractSchedule = new ContractSchedule("_");
			var partTimePercentage = new PartTimePercentage("_");
			var team = new Team { Site = new Site("site") };
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			var category = new ShiftCategory("shiftCategory");
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 150, 150));

			var reqs = Enumerable.Range(0, 200).Select(i =>
			{
				var agent = PersonRepository.Has(contract, contractSchedule, partTimePercentage, team, schedulePeriod, skill);
				agent.WorkflowControlSet = workflowControlSet;
				var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity,
					new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 17), category);
				var assignment2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity,
					new DateTimePeriod(2016, 12, 2, 8, 2016, 12, 2, 17), category);
				PersonAssignmentRepository.Has(assignment);
				PersonAssignmentRepository.Has(assignment2);
				var personRequest =
					new PersonRequest(agent, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 12, 2016, 12, 1, 15))).WithId();
				personRequest.Pending();
				PersonRequestRepository.Add(personRequest);
				return personRequest;
			}).ToArray();

			var newIdentity = new TeleoptiIdentity("test2", null, null, null, null);
			Thread.CurrentPrincipal = new TeleoptiPrincipal(newIdentity, PersonRepository.FindAllSortByName().FirstOrDefault());

			Target.UpdateAbsenceRequest(reqs.Select(x => x.Id.GetValueOrDefault()).ToList());

			reqs.Count(x => x.IsApproved).Should().Be.EqualTo(50); //with 0% threshold
			reqs.Count(x => x.IsDenied).Should().Be.EqualTo(150);
		}

		[Test]
		public void ShouldHandleDifferentSkillSetups()
		{
			Now.Is(new DateTime(2016, 12, 1, 10, 0, 0));
			var firstDay = new DateOnly(2016, 12, 1);
			var activity = ActivityRepository.Has("activityName");
			var skill1 = SkillRepository.Has("skillName1", activity);
			var skill2 = SkillRepository.Has("skillName2", activity);
			skill1.StaffingThresholds = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill2.StaffingThresholds = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));

			var scenario = ScenarioRepository.Has("scnearioName");
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(168), TimeSpan.FromHours(1), TimeSpan.FromHours(1))
			};
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var workflowControlSet = new WorkflowControlSet().WithId();

			var absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			};

			workflowControlSet.InsertPeriod(absenceRequestOpenPeriod, 0);

			var contractSchedule = new ContractSchedule("_");
			var partTimePercentage = new PartTimePercentage("_");
			var team = new Team { Site = new Site("site") };
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			var category = new ShiftCategory("shiftCategory");
			SkillDayRepository.Has(skill1.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 150));
			SkillDayRepository.Has(skill2.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 150));

			var reqs = Enumerable.Range(0, 200).SelectMany(i =>
			{
				var agent1 = PersonRepository.Has(contract, contractSchedule, partTimePercentage, team, schedulePeriod, skill1);
				var agent2 = PersonRepository.Has(contract, contractSchedule, partTimePercentage, team, schedulePeriod, skill2);
				agent1.WorkflowControlSet = workflowControlSet;
				agent2.WorkflowControlSet = workflowControlSet;
				var assignment1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent1, scenario, activity,
					new DateTimePeriod(2016, 12, 1, 20, 2016, 12, 2, 04), category);
				var assignment2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent2, scenario, activity,
					new DateTimePeriod(2016, 12, 1, 20, 2016, 12, 2, 04), category);
				PersonAssignmentRepository.Has(assignment1);
				PersonAssignmentRepository.Has(assignment2);
				var personRequest1 =
					new PersonRequest(agent1, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 20, 2016, 12, 2, 04))).WithId
						();
				var personRequest2 =
					new PersonRequest(agent2, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 20, 2016, 12, 2, 04))).WithId
						();
				personRequest1.Pending();
				personRequest2.Pending();
				PersonRequestRepository.Add(personRequest1);
				PersonRequestRepository.Add(personRequest2);

				return new[] { personRequest1, personRequest2 };
			}).ToArray();

			var newIdentity = new TeleoptiIdentity("test2", null, null, null, null);
			Thread.CurrentPrincipal = new TeleoptiPrincipal(newIdentity, PersonRepository.FindAllSortByName().FirstOrDefault());

			Target.UpdateAbsenceRequest(reqs.Select(x => x.Id.GetValueOrDefault()).ToList());

			reqs.Count(x => x.IsApproved).Should().Be.EqualTo(100); // 50 of each skill
			reqs.Count(x => x.IsDenied).Should().Be.EqualTo(300);
		}

		[Test]
		public void ShouldHandleMidnightShifts()
		{
			Now.Is(new DateTime(2016, 12, 1, 10, 0, 0));
			var firstDay = new DateOnly(2016, 12, 1);
			var activity = ActivityRepository.Has("activityName");
			var skill = SkillRepository.Has("skillName", activity);
			skill.StaffingThresholds = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));

			var scenario = ScenarioRepository.Has("scnearioName");
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(168), TimeSpan.FromHours(1), TimeSpan.FromHours(1))
			};
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var workflowControlSet = new WorkflowControlSet().WithId();

			var absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			};

			workflowControlSet.InsertPeriod(absenceRequestOpenPeriod, 0);

			var contractSchedule = new ContractSchedule("_");
			var partTimePercentage = new PartTimePercentage("_");
			var team = new Team { Site = new Site("site") };
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			var category = new ShiftCategory("shiftCategory");
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 150, 170));

			var reqs = Enumerable.Range(0, 200).Select(i =>
			{
				var agent = PersonRepository.Has(contract, contractSchedule, partTimePercentage, team, schedulePeriod, skill);
				agent.WorkflowControlSet = workflowControlSet;
				var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity,
					new DateTimePeriod(2016, 12, 1, 20, 2016, 12, 2, 04), category);
				PersonAssignmentRepository.Has(assignment);
				var personRequest =
					new PersonRequest(agent, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 20, 2016, 12, 2, 04))).WithId();
				personRequest.Pending();
				PersonRequestRepository.Add(personRequest);
				return personRequest;
			}).ToArray();

			var newIdentity = new TeleoptiIdentity("test2", null, null, null, null);
			Thread.CurrentPrincipal = new TeleoptiPrincipal(newIdentity, PersonRepository.FindAllSortByName().FirstOrDefault());

			Target.UpdateAbsenceRequest(reqs.Select(x => x.Id.GetValueOrDefault()).ToList());

			reqs.Count(x => x.IsApproved).Should().Be.EqualTo(30); //with 0% threshold
			reqs.Count(x => x.IsDenied).Should().Be.EqualTo(170);
		}

		[Test]
		public void ShouldHandleMidnightShiftsWhenAbsenceIsOnSecondDay()
		{
			Now.Is(new DateTime(2016, 12, 1, 10, 0, 0));
			var firstDay = new DateOnly(2016, 12, 1);
			var activity = ActivityRepository.Has("activityName");
			var skill = SkillRepository.Has("skillName", activity);
			skill.StaffingThresholds = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));

			var scenario = ScenarioRepository.Has("scnearioName");
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(168), TimeSpan.FromHours(1), TimeSpan.FromHours(1))
			};
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var workflowControlSet = new WorkflowControlSet().WithId();

			var absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			};

			workflowControlSet.InsertPeriod(absenceRequestOpenPeriod, 0);

			var contractSchedule = new ContractSchedule("_");
			var partTimePercentage = new PartTimePercentage("_");
			var team = new Team { Site = new Site("site") };
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			var category = new ShiftCategory("shiftCategory");
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 150, 170));

			var reqs = Enumerable.Range(0, 200).Select(i =>
			{
				var agent = PersonRepository.Has(contract, contractSchedule, partTimePercentage, team, schedulePeriod, skill);
				agent.WorkflowControlSet = workflowControlSet;
				var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity,
					new DateTimePeriod(2016, 12, 1, 20, 2016, 12, 2, 04), category);
				PersonAssignmentRepository.Has(assignment);
				var personRequest =
					new PersonRequest(agent, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 2, 03, 2016, 12, 2, 04))).WithId();
				personRequest.Pending();
				PersonRequestRepository.Add(personRequest);
				return personRequest;
			}).ToArray();

			var newIdentity = new TeleoptiIdentity("test2", null, null, null, null);
			Thread.CurrentPrincipal = new TeleoptiPrincipal(newIdentity, PersonRepository.FindAllSortByName().FirstOrDefault());

			Target.UpdateAbsenceRequest(reqs.Select(x => x.Id.GetValueOrDefault()).ToList());

			reqs.Count(x => x.IsApproved).Should().Be.EqualTo(30); //with 0% threshold
			reqs.Count(x => x.IsDenied).Should().Be.EqualTo(170);
		}

		[Test]
		public void ShouldHandleMidnightShiftsOnOtherAgents()
		{
			Now.Is(new DateTime(2016, 12, 1, 10, 0, 0));
			var firstDay = new DateOnly(2016, 12, 1);
			var activity = ActivityRepository.Has("activityName");
			var skill = SkillRepository.Has("skillName", activity);
			skill.StaffingThresholds = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));

			var scenario = ScenarioRepository.Has("scnearioName");
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(168), TimeSpan.FromHours(1), TimeSpan.FromHours(1))
			};
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var workflowControlSet = new WorkflowControlSet().WithId();

			var absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			};

			workflowControlSet.InsertPeriod(absenceRequestOpenPeriod, 0);

			var contractSchedule = new ContractSchedule("_");
			var partTimePercentage = new PartTimePercentage("_");
			var team = new Team { Site = new Site("site") };
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			var category = new ShiftCategory("shiftCategory");
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 10, 12));

			for (int i = 0; i < 10; i++)
			{
				var agent = PersonRepository.Has(contract, contractSchedule, partTimePercentage, team, schedulePeriod, skill);
				agent.WorkflowControlSet = workflowControlSet;
				var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity,
					new DateTimePeriod(2016, 12, 1, 20, 2016, 12, 2, 04), category);
				PersonAssignmentRepository.Has(assignment);
			}

			var reqs = Enumerable.Range(0, 10).Select(i =>
			{
				var agent = PersonRepository.Has(contract, contractSchedule, partTimePercentage, team, schedulePeriod, skill);
				agent.WorkflowControlSet = workflowControlSet;
				var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity,
					new DateTimePeriod(2016, 12, 2, 01, 2016, 12, 2, 04), category);
				PersonAssignmentRepository.Has(assignment);
				var personRequest =
					new PersonRequest(agent, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 2, 03, 2016, 12, 2, 04))).WithId();
				personRequest.Pending();
				PersonRequestRepository.Add(personRequest);
				return personRequest;
			}).ToArray();

			var newIdentity = new TeleoptiIdentity("test2", null, null, null, null);
			Thread.CurrentPrincipal = new TeleoptiPrincipal(newIdentity, PersonRepository.FindAllSortByName().FirstOrDefault());

			Target.UpdateAbsenceRequest(reqs.Select(x => x.Id.GetValueOrDefault()).ToList());

			reqs.Count(x => x.IsApproved).Should().Be.EqualTo(8); //with 0% threshold
			reqs.Count(x => x.IsDenied).Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldNotApproveAllRequestsIfRequestsStartsAfterEndOfShiftAndEndsTheNextDay()
		{
			Now.Is(new DateTime(2016, 12, 1, 10, 0, 0));
			var firstDay = new DateOnly(2016, 12, 1);
			var activity = ActivityRepository.Has("activityName");
			var skill = SkillRepository.Has("skillName", activity);
			skill.StaffingThresholds = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));

			var scenario = ScenarioRepository.Has("scnearioName");
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(168), TimeSpan.FromHours(1), TimeSpan.FromHours(1))
			};
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var workflowControlSet = new WorkflowControlSet().WithId();

			var absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			};

			workflowControlSet.InsertPeriod(absenceRequestOpenPeriod, 0);

			var contractSchedule = new ContractSchedule("_");
			var partTimePercentage = new PartTimePercentage("_");
			var team = new Team { Site = new Site("site") };
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			var category = new ShiftCategory("shiftCategory");
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 150, 150));

			var reqs = Enumerable.Range(0, 200).Select(i =>
			{
				var agent = PersonRepository.Has(contract, contractSchedule, partTimePercentage, team, schedulePeriod, skill);
				agent.WorkflowControlSet = workflowControlSet;
				var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity,
					new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 17), category);
				var assignment2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity,
					new DateTimePeriod(2016, 12, 2, 8, 2016, 12, 2, 17), category);
				PersonAssignmentRepository.Has(assignment);
				PersonAssignmentRepository.Has(assignment2);
				var personRequest =
					new PersonRequest(agent, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 18, 2016, 12, 2, 18))).WithId();
				personRequest.Pending();
				PersonRequestRepository.Add(personRequest);
				return personRequest;
			}).ToArray();

			var newIdentity = new TeleoptiIdentity("test2", null, null, null, null);
			Thread.CurrentPrincipal = new TeleoptiPrincipal(newIdentity, PersonRepository.FindAllSortByName().FirstOrDefault());

			Target.UpdateAbsenceRequest(reqs.Select(x => x.Id.GetValueOrDefault()).ToList());

			reqs.Count(x => x.IsApproved).Should().Be.EqualTo(50); //with 0% threshold
			reqs.Count(x => x.IsDenied).Should().Be.EqualTo(150);
		}

		[Test] //debug friendly, assert the same as above
		public void ShouldNotApproveAllRequestsIfRequestsStartsAfterEndOfShiftAndEndsTheNextDayDebugFriendly()
		{
			Now.Is(new DateTime(2016, 12, 1, 10, 0, 0));
			var firstDay = new DateOnly(2016, 12, 1);
			var activity = ActivityRepository.Has("activityName");
			var skill = SkillRepository.Has("skillName", activity);
			skill.StaffingThresholds = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));

			var scenario = ScenarioRepository.Has("scnearioName");
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(168), TimeSpan.FromHours(1), TimeSpan.FromHours(1))
			};
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var workflowControlSet = new WorkflowControlSet().WithId();

			var absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			};

			workflowControlSet.InsertPeriod(absenceRequestOpenPeriod, 0);

			var contractSchedule = new ContractSchedule("_");
			var partTimePercentage = new PartTimePercentage("_");
			var team = new Team { Site = new Site("site") };
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			var category = new ShiftCategory("shiftCategory");
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1));

			var reqs = new List<IPersonRequest>();
			for (int i = 0; i < 2; i++)
			{
				var agent = PersonRepository.Has(contract, contractSchedule, partTimePercentage, team, schedulePeriod, skill);
				agent.WorkflowControlSet = workflowControlSet;
				var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, new DateTimePeriod(2016, 12, 1, 16, 2016, 12, 1, 17), category);
				var assignment2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, new DateTimePeriod(2016, 12, 2, 16, 2016, 12, 2, 17), category);
				PersonAssignmentRepository.Has(assignment);
				PersonAssignmentRepository.Has(assignment2);
				var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 23, 2016, 12, 2, 18))).WithId();
				personRequest.Pending();
				PersonRequestRepository.Add(personRequest);
				reqs.Add(personRequest);
			}

			var newIdentity = new TeleoptiIdentity("test2", null, null, null, null);
			Thread.CurrentPrincipal = new TeleoptiPrincipal(newIdentity, PersonRepository.FindAllSortByName().FirstOrDefault());

			Target.UpdateAbsenceRequest(reqs.Select(x => x.Id.GetValueOrDefault()).ToList());

			reqs.Count(x => x.IsApproved).Should().Be.EqualTo(1); //with 0% threshold
			reqs.Count(x => x.IsDenied).Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldOnlyValidateWhenAgentHasAnActivity()
		{
			Now.Is(new DateTime(2016, 12, 2, 10, 0, 0));
			var firstDay = new DateOnly(2016, 12, 1);
			var activity = ActivityRepository.Has("activityName");
			var skill = SkillRepository.Has("skillName", activity);
			skill.StaffingThresholds = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));

			var scenario = ScenarioRepository.Has("scnearioName");
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(168), TimeSpan.FromHours(1), TimeSpan.FromHours(1))
			};
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var workflowControlSet = new WorkflowControlSet().WithId();

			var absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			};

			workflowControlSet.InsertPeriod(absenceRequestOpenPeriod, 0);

			var contractSchedule = new ContractSchedule("_");
			var partTimePercentage = new PartTimePercentage("_");
			var team = new Team { Site = new Site("site") };
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			var category = new ShiftCategory("shiftCategory");
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 2, 1, 1, 2));

			var reqs = new List<IPersonRequest>();
			for (var i = 0; i < 2; i++)
			{
				var agent = PersonRepository.Has(contract, contractSchedule, partTimePercentage, team, schedulePeriod, skill);
				agent.WorkflowControlSet = workflowControlSet;
				var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, new DateTimePeriod(2016, 12, 1, 23, 2016, 12, 2, 04), category);
				var assignment2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, new DateTimePeriod(2016, 12, 2, 23, 2016, 12, 3, 04), category);
				var assignment3 = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, new DateTimePeriod(2016, 12, 3, 23, 2016, 12, 4, 04), category);
				PersonAssignmentRepository.Has(assignment);
				PersonAssignmentRepository.Has(assignment2);
				PersonAssignmentRepository.Has(assignment3);
				var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 2, 23, 2016, 12, 3, 04))).WithId();
				personRequest.Pending();
				PersonRequestRepository.Add(personRequest);
				reqs.Add(personRequest);
			}

			var newIdentity = new TeleoptiIdentity("test2", null, null, null, null);
			Thread.CurrentPrincipal = new TeleoptiPrincipal(newIdentity, PersonRepository.FindAllSortByName().FirstOrDefault());

			Target.UpdateAbsenceRequest(reqs.Select(x => x.Id.GetValueOrDefault()).ToList());

			reqs.Count(x => x.IsApproved).Should().Be.EqualTo(1); //with 0% threshold
			reqs.Count(x => x.IsDenied).Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotDenyIfAgentHasNoShift()
		{
			Now.Is(new DateTime(2016, 12, 1, 10, 0, 0));
			var firstDay = new DateOnly(2016, 12, 1);
			var activity = ActivityRepository.Has("activityName");
			var skill = SkillRepository.Has("skillName", activity);
			skill.StaffingThresholds = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));

			var scenario = ScenarioRepository.Has("scnearioName");
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(168), TimeSpan.FromHours(1), TimeSpan.FromHours(1))
			};
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var workflowControlSet = new WorkflowControlSet().WithId();

			var absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			};

			workflowControlSet.InsertPeriod(absenceRequestOpenPeriod, 0);

			var contractSchedule = new ContractSchedule("_");
			var partTimePercentage = new PartTimePercentage("_");
			var team = new Team { Site = new Site("site") };
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			var category = new ShiftCategory("shiftCategory");
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 2, 2));

			var reqs = new List<IPersonRequest>();

			var agent = PersonRepository.Has(contract, contractSchedule, partTimePercentage, team, schedulePeriod, skill);
			agent.WorkflowControlSet = workflowControlSet;
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 17), category);
			var assignment2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, new DateTimePeriod(2016, 12, 2, 8, 2016, 12, 2, 17), category);
			PersonAssignmentRepository.Has(assignment);
			PersonAssignmentRepository.Has(assignment2);

			var agent2 = PersonRepository.Has(contract, contractSchedule, partTimePercentage, team, schedulePeriod, skill);
			agent2.WorkflowControlSet = workflowControlSet;
			var personRequest2 = new PersonRequest(agent2, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 12, 2016, 12, 1, 15))).WithId();
			personRequest2.Pending();
			PersonRequestRepository.Add(personRequest2);
			reqs.Add(personRequest2);
			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 12, 2016, 12, 1, 15))).WithId();
			personRequest.Pending();
			PersonRequestRepository.Add(personRequest);
			reqs.Add(personRequest);

			var newIdentity = new TeleoptiIdentity("test2", null, null, null, null);
			Thread.CurrentPrincipal = new TeleoptiPrincipal(newIdentity, PersonRepository.FindAllSortByName().FirstOrDefault());

			Target.UpdateAbsenceRequest(reqs.Select(x => x.Id.GetValueOrDefault()).ToList());

			reqs.Count(x => x.IsApproved).Should().Be.EqualTo(1); //with 0% threshold
			reqs.Count(x => x.IsDenied).Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldDenyIfDenyFromToPeriodOverridesAutoGrantRollingPeriod()
		{
			Now.Is(DateTime.Now);
			ScenarioRepository.Has("scnearioName");
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var tomorrow = DateOnly.Today.AddDays(1);
			var theDayAfterTomorrow = DateOnly.Today.AddDays(2);

			var wfcs = new WorkflowControlSet().WithId();
			var openForRequestsPeriod = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(30));

			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenRollingPeriod
			{
				BetweenDays = new MinMax<int>(0, 1),
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new AbsenceRequestNoneValidator(),
				OpenForRequestsPeriod = openForRequestsPeriod,
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});

			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new AbsenceRequestNoneValidator(),
				Period = new DateOnlyPeriod(tomorrow, theDayAfterTomorrow),
				OpenForRequestsPeriod = openForRequestsPeriod,
				AbsenceRequestProcess = new DenyAbsenceRequest()
			});

			var person = PersonFactory.CreatePerson(wfcs).WithId();

			var personRequest = new PersonRequest(person, new AbsenceRequest(absence, tomorrow.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone()))).WithId();
			personRequest.Pending();
			PersonRequestRepository.Add(personRequest);

			Target.UpdateAbsenceRequest(new List<Guid> { personRequest.Id.GetValueOrDefault() });

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo("RequestDenyReasonAutodeny");
		}

		[Test]
		public void ShouldApproveOnlyIfShovel()
		{
			Now.Is(new DateTime(2016, 12, 1, 10, 0, 0));
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var skillA = SkillRepository.Has("skillA", activity).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillB = SkillRepository.Has("skillB", activity).WithId().CascadingIndex(1).IsOpenBetween(8, 9);
			var skillC = SkillRepository.Has("skillB", activity).WithId().CascadingIndex(2).IsOpenBetween(8, 9);
			Now.Is(new DateTime(2016, 12, 1, 7, 0, 0));

			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);
			SkillDayRepository.Has(skillA.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, new DateOnly(period.StartDateTime), 0.08));
			SkillDayRepository.Has(skillB.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, new DateOnly(period.StartDateTime), 0.08));
			SkillDayRepository.Has(skillC.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, new DateOnly(period.StartDateTime), 1));


			var agent = PersonRepository.Has(skillA, skillB, skillC);
			var agent2 = PersonRepository.Has(skillA, skillB, skillC);
			var agent3 = PersonRepository.Has(skillA, skillB, skillC);
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent2, scenario, activity, period, new ShiftCategory("category")));
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent3, scenario, activity, period, new ShiftCategory("category")));

			var wfcs = new WorkflowControlSet().WithId();
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});
			agent.WorkflowControlSet = wfcs;

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();
			personRequest.Pending();
			PersonRequestRepository.Add(personRequest);

			Target.UpdateAbsenceRequest(new List<Guid> { personRequest.Id.GetValueOrDefault() });

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldApproveWithShrinkage()
		{
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var skillA = SkillRepository.Has("skillA", activity).WithId().CascadingIndex(1);
			var skillB = SkillRepository.Has("skillB", activity).WithId().CascadingIndex(2);
			Now.Is(new DateTime(2016, 12, 1, 7, 0, 0));

			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);
			SkillDayRepository.Has(skillA.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 0.5));
			var skillday = skillB.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 0.5);
			skillday.SkillDataPeriodCollection.ForEach(x => x.Shrinkage = new Percent(0.5));
			SkillDayRepository.Has(skillday);


			var agent = PersonRepository.Has(skillA, skillB);
			var agent2 = PersonRepository.Has(skillA, skillB);
			var agent3 = PersonRepository.Has(skillA, skillB);
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent2, scenario, activity, period, new ShiftCategory("category")));
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent3, scenario, activity, period, new ShiftCategory("category")));

			var wfcs = new WorkflowControlSet().WithId();
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdWithShrinkageValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});
			agent.WorkflowControlSet = wfcs;

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();
			personRequest.Pending();
			PersonRequestRepository.Add(personRequest);

			Target.UpdateAbsenceRequest(new List<Guid> { personRequest.Id.GetValueOrDefault() });

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldDenyWhenPersonalAccountIsOnAndBalanceIsNotEnoughButOverZero()
		{
			Now.Is(new DateTime(2016, 12, 1, 10, 0, 0));
			var scenario = ScenarioRepository.Has("scnearioName");
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			absence.InContractTime = true;
			absence.InWorkTime = true;
			absence.InPaidTime = true;

			var wfcs = new WorkflowControlSet().WithId();
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				PersonAccountValidator = new PersonAccountBalanceValidator(),
				StaffingThresholdValidator = new AbsenceRequestNoneValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});
			wfcs.AbsenceRequestWaitlistEnabled = true;

			var person = PersonFactory.CreatePerson(wfcs).WithId();

			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(2016, 12, 1, 11, 2016, 12, 2, 20));
			PersonAssignmentRepository.Has(assignment);

			var personRequest = new PersonRequest(person, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 18))).WithId();
			personRequest.Pending();
			PersonRequestRepository.Add(personRequest);

			createAccount(person, absence, createAccountDay(new DateOnly(2016, 12, 1), TimeSpan.FromMinutes(360)));

			Target.UpdateAbsenceRequest(new List<Guid> { personRequest.Id.GetValueOrDefault() });

			personRequest.IsDenied.Should().Be.True();
			personRequest.IsWaitlisted.Should().Be.False();
			personRequest.DenyReason.Should().Be.EqualTo(Resources.RequestDenyReasonPersonAccount);
		}

		[Test]
		public void ShouldDenyWhenPersonAccountIsOnAndPersonAccountIsMissing()
		{
			Now.Is(new DateTime(2016, 12, 1, 10, 0, 0));
			var scenario = ScenarioRepository.Has("scnearioName");
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			absence.InContractTime = true;
			absence.InWorkTime = true;
			absence.InPaidTime = true;
			absence.Tracker = Tracker.CreateTimeTracker();

			var wfcs = new WorkflowControlSet().WithId();
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				PersonAccountValidator = new PersonAccountBalanceValidator(),
				StaffingThresholdValidator = new AbsenceRequestNoneValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});
			wfcs.AbsenceRequestWaitlistEnabled = true;

			var person = PersonFactory.CreatePerson(wfcs).WithId();

			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(2016, 12, 1, 11, 2016, 12, 2, 20));
			PersonAssignmentRepository.Has(assignment);

			var personRequest = new PersonRequest(person, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 18))).WithId();
			personRequest.Pending();
			PersonRequestRepository.Add(personRequest);

			//createAccount(person, absence, createAccountDay(new DateOnly(2016, 12, 1), TimeSpan.FromMinutes(360)));

			Target.UpdateAbsenceRequest(new List<Guid> { personRequest.Id.GetValueOrDefault() });

			personRequest.IsDenied.Should().Be.True();
			personRequest.IsWaitlisted.Should().Be.False();
			personRequest.DenyReason.Should().Be.EqualTo(Resources.RequestDenyReasonPersonAccount);
		}

		[Test]
		public void ShouldNotDenyWhenThereAreTwoBudgetDaysInTheSameDay()
		{
			Now.Is(new DateTime(2017, 6, 5));

			var budgetDay = new DateOnly(2017, 6, 5);
			var budgetGroup = getBudgetGroup();
			var scenario = ScenarioRepository.Has("scenarioName");
			var budgetDayOne = new BudgetDay(budgetGroup, scenario, budgetDay)
			{
				FulltimeEquivalentHours = 8d,
				ShrinkedAllowance = 1,
				AbsenceOverride = 1d,
				IsClosed = false,
				UpdatedOn = new DateTime()
			};
			var budgetDayTwo = new BudgetDay(budgetGroup, scenario, budgetDay)
			{
				FulltimeEquivalentHours = 10d,
				ShrinkedAllowance = 1,
				AbsenceOverride = 2d,
				IsClosed = false,
				UpdatedOn = new DateTime().AddDays(1)
			};
			BudgetGroupRepository.Add(budgetGroup);
			BudgetDayRepository.Add(budgetDayOne);
			BudgetDayRepository.Add(budgetDayTwo);

			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(budgetDay);
			personPeriod.BudgetGroup = budgetGroup;

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			absence.InContractTime = true;
			var wfcs = new WorkflowControlSet().WithId();
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new BudgetGroupAllowanceValidator(),
				Period = new DateOnlyPeriod(2017, 6, 4, 2017, 6, 11),
				OpenForRequestsPeriod = new DateOnlyPeriod(2017, 6, 4, 2017, 6, 11),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});

			var person = PersonFactory.CreatePerson(wfcs).WithId();
			person.AddPersonPeriod(personPeriod);
			PersonRepository.Add(person);

			var period = new DateTimePeriod(2017, 6, 5, 8, 2017, 6, 5, 17);
			var activity = ActivityRepository.Has("activity");
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity,
				period, new ShiftCategory("category")));

			var personRequest = new PersonRequest(person, new AbsenceRequest(absence, period)).WithId();
			personRequest.Pending();
			PersonRequestRepository.Add(personRequest);

			Target.UpdateAbsenceRequest(new List<Guid> { personRequest.Id.GetValueOrDefault() });
			personRequest.IsApproved.Should().Be(true);

		}

		[Test]
		public void ShouldApproveRequestThatFitIntoBudget()
		{
			Now.Is(new DateTime(2017, 6, 5));

			var budgetDay = new DateOnly(2017, 6, 5);
			var budgetGroup = getBudgetGroup();
			var scenario = ScenarioRepository.Has("scenarioName");
			var budgetDayOne = new BudgetDay(budgetGroup, scenario, budgetDay)
			{
				FulltimeEquivalentHours = 8d,
				ShrinkedAllowance = 1,
				AbsenceOverride = 1d,
				IsClosed = false,
				UpdatedOn = new DateTime()
			};
			BudgetGroupRepository.Add(budgetGroup);
			BudgetDayRepository.Add(budgetDayOne);

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			absence.InContractTime = true;
			var wfcs = new WorkflowControlSet().WithId();
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new BudgetGroupAllowanceValidator(),
				Period = new DateOnlyPeriod(2017, 6, 4, 2017, 6, 11),
				OpenForRequestsPeriod = new DateOnlyPeriod(2017, 6, 4, 2017, 6, 11),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});

			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(budgetDay);
			personPeriod.BudgetGroup = budgetGroup;
			var person = PersonFactory.CreatePerson(wfcs).WithId();
			person.AddPersonPeriod(personPeriod);
			PersonRepository.Add(person);

			var personPeriod2 = PersonPeriodFactory.CreatePersonPeriod(budgetDay);
			personPeriod2.BudgetGroup = budgetGroup;
			var person2 = PersonFactory.CreatePerson(wfcs).WithId();
			person2.AddPersonPeriod(personPeriod2);
			PersonRepository.Add(person2);

			var startTime = new DateTime(2017, 06, 05, 0, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(startTime, startTime.AddDays(1).AddMinutes(-1));

			var personAssignmentPeriod = new DateTimePeriod(2017, 6, 5, 8, 2017, 6, 5, 16);

			var activity = ActivityRepository.Has("activity");
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity,
				personAssignmentPeriod, new ShiftCategory("category")));

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(person2, scenario, activity,
				personAssignmentPeriod, new ShiftCategory("category")));

			var personRequest = new PersonRequest(person, new AbsenceRequest(absence, period)).WithId();
			personRequest.Pending();
			PersonRequestRepository.Add(personRequest);

			var personRequest2 = new PersonRequest(person2, new AbsenceRequest(absence, period)).WithId();
			personRequest2.Pending();
			PersonRequestRepository.Add(personRequest2);

			Target.UpdateAbsenceRequest(new List<Guid> { personRequest.Id.GetValueOrDefault(), personRequest2.Id.GetValueOrDefault() });

			var requests = PersonRequestRepository.RequestRepository;
			requests.Count(r => r.IsApproved).Should().Be(1);
			requests.Count(r => r.IsDenied).Should().Be(1);
		}

		[Test]
		public void ShouldNotDenyWhenRequestDayIsNotWorkingDayAndWithoutAssignment()
		{
			Now.Is(new DateTime(2017, 6, 5));

			var budgetDay = new DateOnly(2017, 6, 5);
			var budgetGroup = getBudgetGroup();
			var scenario = ScenarioRepository.Has("scenarioName");
			var budgetDayOne = new BudgetDay(budgetGroup, scenario, budgetDay)
			{
				FulltimeEquivalentHours = 10d,
				ShrinkedAllowance = 1,
				AbsenceOverride = 1d,
				IsClosed = false,
				UpdatedOn = new DateTime()
			};
			BudgetGroupRepository.Add(budgetGroup);
			BudgetDayRepository.Add(budgetDayOne);

			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(budgetDay);
			personPeriod.BudgetGroup = budgetGroup;
			personPeriod.PersonContract = PersonContractFactory.CreatePersonContract();
			personPeriod.PersonContract.ContractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			personPeriod.PersonContract.ContractSchedule.ContractScheduleWeeks.First().Add(DayOfWeek.Monday, false);

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			absence.InContractTime = true;
			var wfcs = new WorkflowControlSet().WithId();
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new BudgetGroupAllowanceValidator(),
				Period = new DateOnlyPeriod(2017, 6, 4, 2017, 6, 11),
				OpenForRequestsPeriod = new DateOnlyPeriod(2017, 6, 4, 2017, 6, 11),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});

			var personOne = PersonFactory.CreatePerson(wfcs).WithId();
			personOne.AddPersonPeriod(personPeriod);
			PersonRepository.Add(personOne);

			var period = new DateTimePeriod(2017, 6, 5, 8, 2017, 6, 5, 17);
			var activity = ActivityRepository.Has("activity");
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(personOne, scenario, activity,
				period, new ShiftCategory("category")));

			var personRequestOne = new PersonRequest(personOne, new AbsenceRequest(absence, period)).WithId();
			personRequestOne.Pending();
			PersonRequestRepository.Add(personRequestOne);

			Target.UpdateAbsenceRequest(new List<Guid> { personRequestOne.Id.GetValueOrDefault() });
			personRequestOne.IsApproved.Should().Be(true);

			var absenceLayer = new AbsenceLayer(absence, period);
			var personAbsence = new PersonAbsence(personOne, scenario, absenceLayer);
			PersonAbsenceRepository.Has(personAbsence);

			var personTwo = PersonFactory.CreatePerson(wfcs).WithId();
			personTwo.AddPersonPeriod(personPeriod);
			PersonRepository.Add(personTwo);

			var personRequestTwo = new PersonRequest(personTwo, new AbsenceRequest(absence, period)).WithId();
			personRequestTwo.Pending();
			PersonRequestRepository.Add(personRequestTwo);

			Target.UpdateAbsenceRequest(new List<Guid> { personRequestTwo.Id.GetValueOrDefault() });
			personRequestTwo.IsApproved.Should().Be(true);
		}

		[Test]
		public void ShouldDenyWhenRequestDayIsNotWorkingDayAndWithAssignment()
		{
			Now.Is(new DateTime(2017, 6, 5));

			var budgetDay = new DateOnly(2017, 6, 5);
			var budgetGroup = getBudgetGroup();
			var scenario = ScenarioRepository.Has("scenarioName");
			var budgetDayOne = new BudgetDay(budgetGroup, scenario, budgetDay)
			{
				FulltimeEquivalentHours = 10d,
				ShrinkedAllowance = 1,
				AbsenceOverride = 1d,
				IsClosed = false,
				UpdatedOn = new DateTime()
			};
			BudgetGroupRepository.Add(budgetGroup);
			BudgetDayRepository.Add(budgetDayOne);

			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(budgetDay);
			personPeriod.BudgetGroup = budgetGroup;
			personPeriod.PersonContract = PersonContractFactory.CreatePersonContract();
			personPeriod.PersonContract.ContractSchedule = ContractScheduleFactory.CreateWorkingWeekContractSchedule();
			personPeriod.PersonContract.ContractSchedule.ContractScheduleWeeks.First().Add(DayOfWeek.Monday, false);

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			absence.InContractTime = true;
			var wfcs = new WorkflowControlSet().WithId();
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new BudgetGroupAllowanceValidator(),
				Period = new DateOnlyPeriod(2017, 6, 4, 2017, 6, 11),
				OpenForRequestsPeriod = new DateOnlyPeriod(2017, 6, 4, 2017, 6, 11),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});

			var personOne = PersonFactory.CreatePerson(wfcs).WithId();
			personOne.AddPersonPeriod(personPeriod);
			PersonRepository.Add(personOne);

			var period = new DateTimePeriod(2017, 6, 5, 8, 2017, 6, 5, 17);
			var activity = ActivityRepository.Has("activity");
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(personOne, scenario, activity,
				period, new ShiftCategory("category")));

			var personRequestOne = new PersonRequest(personOne, new AbsenceRequest(absence, period)).WithId();
			personRequestOne.Pending();
			PersonRequestRepository.Add(personRequestOne);

			Target.UpdateAbsenceRequest(new List<Guid> { personRequestOne.Id.GetValueOrDefault() });
			personRequestOne.IsApproved.Should().Be(true);

			var absenceLayer = new AbsenceLayer(absence, period);
			var personAbsence = new PersonAbsence(personOne, scenario, absenceLayer);
			PersonAbsenceRepository.Has(personAbsence);

			var personTwo = PersonFactory.CreatePerson(wfcs).WithId();
			personTwo.AddPersonPeriod(personPeriod);
			PersonRepository.Add(personTwo);

			var personRequestTwo = new PersonRequest(personTwo, new AbsenceRequest(absence, period)).WithId();
			personRequestTwo.Pending();
			PersonRequestRepository.Add(personRequestTwo);
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(personTwo, scenario, activity,
				period, new ShiftCategory("category")));

			Target.UpdateAbsenceRequest(new List<Guid> { personRequestTwo.Id.GetValueOrDefault() });
			personRequestTwo.IsApproved.Should().Be(false);
		}

		[Test]
		public void ShouldDenyAbsenceRequestWhenOnlyOneAgentIsScheduled()
		{
			Now.Is(new DateTime(2017, 12, 1, 6, 00, 00, DateTimeKind.Utc));

			var period = new DateTimePeriod(2017, 12, 4, 8, 2017, 12, 4, 9);
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var activity = ActivityRepository.Has("phone");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var threshold = new StaffingThresholds(new Percent(-0.2), new Percent(-0.1), new Percent(0));
			skill.StaffingThresholds = threshold;
			var agent = PersonRepository.Has(skill);
			var workflowControlSet = new WorkflowControlSet().WithId();
			workflowControlSet.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2017, 1, 1, 2017, 12, 31),
				OpenForRequestsPeriod = new DateOnlyPeriod(2017, 1, 1, 2017, 12, 31),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});
			agent.WorkflowControlSet = workflowControlSet;

			var scenario = ScenarioRepository.Has("scenario");
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));
			
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 1));

			var personRequest = createPersonRequest(agent, absence, period);
			Target.UpdateAbsenceRequest(new List<Guid> { personRequest.Id.GetValueOrDefault() });

			personRequest.IsDenied.Should().Be(true);
			personRequest.DenyReason.Should().Be("Insufficient staffing for : 12/4/2017 8:00:00 AM - 12/4/2017 9:00:00 AM");

		}
		
		private PersonRequest createPersonRequest(Person agent, IAbsence absence, DateTimePeriod period)
		{
			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();
			personRequest.Pending();
			PersonRequestRepository.Add(personRequest);
			return personRequest;
		}

		private static IBudgetGroup getBudgetGroup()
		{
			var budgetGroup = new BudgetGroup { Name = "BG1" };
			budgetGroup.SetId(Guid.NewGuid());
			return budgetGroup;
		}

		private void createAccount(IPerson person, IAbsence absence, params IAccount[] accountDays)
		{
			var personAbsenceAccount = new PersonAbsenceAccount(person, absence);
			personAbsenceAccount.Absence.Tracker = Tracker.CreateTimeTracker();
			foreach (var accountDay in accountDays)
			{
				personAbsenceAccount.Add(accountDay);
			}
			PersonAbsenceAccountRepository.Add(personAbsenceAccount);
		}

		private static AccountDay createAccountDay(DateOnly startDate, TimeSpan balance)
		{
			return new AccountDay(startDate)
			{
				Accrued = TimeSpan.FromMinutes(480),
				LatestCalculatedBalance = balance
			};
		}
	}
}