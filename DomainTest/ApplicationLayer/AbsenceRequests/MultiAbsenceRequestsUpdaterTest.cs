﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[DomainTestWithStaticDependenciesAvoidUse]
	[TestFixture, SetCulture("en-US")]
	public class MultiAbsenceRequestsUpdaterTest : ISetup
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

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeCommandDispatcher>().For<ICommandDispatcher>();
		}

		[Test]
		public void ShouldDenyIfPeriodNotOpenForRequest()
		{
			Now.Is(DateTime.Now);
			var scenario = ScenarioRepository.Has("scnearioName");
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			
			var wfcs = new WorkflowControlSet().WithId();
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod()
											 {
												 Absence = absence,
												 PersonAccountValidator = new AbsenceRequestNoneValidator(),
												 StaffingThresholdValidator = new AbsenceRequestNoneValidator(),
												 Period = new DateOnlyPeriod(2016, 12, 1, 2016, 12, 2),
												 OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2016, 11, 30),
												 AbsenceRequestProcess = new GrantAbsenceRequest()
											 });

			var person = PersonFactory.CreatePerson(wfcs).WithId();

			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, person, new DateTimePeriod(2016, 12, 1, 11, 2016, 12, 2, 20));
			PersonAssignmentRepository.Has(assignment);

			var personRequest = new PersonRequest(person, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 12, 2016, 12, 1, 13))).WithId();
			personRequest.Pending();
			PersonRequestRepository.Add(personRequest);
			
			Target.UpdateAbsenceRequest(new List<Guid>(){personRequest.Id.GetValueOrDefault()});
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
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});

			var person = PersonFactory.CreatePerson(wfcs).WithId();

			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, person, new DateTimePeriod(2016, 12, 1, 11, 2016, 12, 2, 20));
			PersonAssignmentRepository.Has(assignment);
			var absenceLayer = new AbsenceLayer(absence, new DateTimePeriod(2016, 12, 1, 11, 2016, 12, 2, 20));
			var personAbsence = new PersonAbsence(person, scenario, absenceLayer);
			PersonAbsenceRepository.Has(personAbsence);

			var personRequest = new PersonRequest(person, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 12, 2016, 12, 1, 13))).WithId();
			personRequest.Pending();
			PersonRequestRepository.Add(personRequest);

			Target.UpdateAbsenceRequest(new List<Guid>() {personRequest.Id.GetValueOrDefault()});
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
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});
			wfcs.AbsenceRequestExpiredThreshold = 15;
			wfcs.AbsenceRequestWaitlistEnabled = true;

			var person = PersonFactory.CreatePerson(wfcs).WithId();

			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, person, new DateTimePeriod(2016, 12, 1, 11, 2016, 12, 2, 20));
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
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			};

			workflowControlSet.InsertPeriod(absenceRequestOpenPeriod, 0);

			var contractSchedule = new ContractSchedule("_");
			var partTimePercentage = new PartTimePercentage("_");
			var team = new Team { Site = new Site("site") };
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			var category = new ShiftCategory("shiftCategory");
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 150, 150));

			var reqs = new List<IPersonRequest>();
			for (int i = 0; i < 200; i++)
			{
				var agent = PersonRepository.Has(contract, contractSchedule, partTimePercentage, team, schedulePeriod, skill);
				agent.WorkflowControlSet = workflowControlSet;
				var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, agent, new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 17), category, scenario);
				var assignment2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, agent, new DateTimePeriod(2016, 12, 2, 8, 2016, 12, 2, 17), category, scenario);
				PersonAssignmentRepository.Has(assignment);
				PersonAssignmentRepository.Has(assignment2);
				var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 12, 2016, 12, 1, 15))).WithId();
				personRequest.Pending();
				PersonRequestRepository.Add(personRequest);
				reqs.Add(personRequest);
			}

			var newIdentity = new TeleoptiIdentity("test2", null, null, null, null);
			Thread.CurrentPrincipal = new TeleoptiPrincipal(newIdentity, PersonRepository.FindAllSortByName().FirstOrDefault());

			Target.UpdateAbsenceRequest(reqs.Select(x => x.Id.GetValueOrDefault()).ToList());

			reqs.Count(x => x.IsApproved).Should().Be.EqualTo(50); //with 0% threshold
			reqs.Count(x => x.IsDenied).Should().Be.EqualTo(150);
		}

		[Test]
		[Ignore("buggelibug")]
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

			var absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			};

			workflowControlSet.InsertPeriod(absenceRequestOpenPeriod, 0);

			var contractSchedule = new ContractSchedule("_");
			var partTimePercentage = new PartTimePercentage("_");
			var team = new Team { Site = new Site("site") };
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			var category = new ShiftCategory("shiftCategory");
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 150, 170));

			var reqs = new List<IPersonRequest>();
			for (int i = 0; i < 200; i++)
			{
				var agent = PersonRepository.Has(contract, contractSchedule, partTimePercentage, team, schedulePeriod, skill);
				agent.WorkflowControlSet = workflowControlSet;
				var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, agent, new DateTimePeriod(2016, 12, 1, 20, 2016, 12, 2, 04), category, scenario);
				PersonAssignmentRepository.Has(assignment);
				var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 20, 2016, 12, 2, 04))).WithId();
				personRequest.Pending();
				PersonRequestRepository.Add(personRequest);
				reqs.Add(personRequest);
			}

			var newIdentity = new TeleoptiIdentity("test2", null, null, null, null);
			Thread.CurrentPrincipal = new TeleoptiPrincipal(newIdentity, PersonRepository.FindAllSortByName().FirstOrDefault());

			Target.UpdateAbsenceRequest(reqs.Select(x => x.Id.GetValueOrDefault()).ToList());

			reqs.Count(x => x.IsApproved).Should().Be.EqualTo(30); //with 0% threshold
			reqs.Count(x => x.IsDenied).Should().Be.EqualTo(170);
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

			var absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			};

			workflowControlSet.InsertPeriod(absenceRequestOpenPeriod, 0);

			var contractSchedule = new ContractSchedule("_");
			var partTimePercentage = new PartTimePercentage("_");
			var team = new Team { Site = new Site("site") };
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			var category = new ShiftCategory("shiftCategory");
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 150, 150));

			var reqs = new List<IPersonRequest>();
			for (int i = 0; i < 200; i++)
			{
				var agent = PersonRepository.Has(contract, contractSchedule, partTimePercentage, team, schedulePeriod, skill);
				agent.WorkflowControlSet = workflowControlSet;
				var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, agent, new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 17), category, scenario);
				var assignment2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, agent, new DateTimePeriod(2016, 12, 2, 8, 2016, 12, 2, 17), category, scenario);
				PersonAssignmentRepository.Has(assignment);
				PersonAssignmentRepository.Has(assignment2);
				var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 18, 2016, 12, 2, 18))).WithId();
				personRequest.Pending();
				PersonRequestRepository.Add(personRequest);
				reqs.Add(personRequest);
			}

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

			var absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
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
				var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, agent, new DateTimePeriod(2016, 12, 1, 16, 2016, 12, 1, 17), category, scenario);
				var assignment2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, agent, new DateTimePeriod(2016, 12, 2, 16, 2016, 12, 2, 17), category, scenario);
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

	}
}