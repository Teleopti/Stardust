using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[DomainTest]
	public class IntradayRequestProcessorTest_useRemoveScheduleForecastSkillChangeReadModel : IIsolateSystem
	{
		public IRequestProcessor Target;
		public FakePersonRepository PersonRepository;
		public FakeSkillRepository SkillRepository;
		public FakeActivityRepository ActivityRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeCommandDispatcher CommandDispatcher;
		public IScheduleStorage ScheduleStorage;
		public MutableNow Now;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeCommandDispatcher>().For<ICommandDispatcher>();
		}

		[Test]
		public void ShouldApproveRequestIfEnoughResourcesOnSkill()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = threshold;
			skill.DefaultResolution = 60;
			var agent = PersonRepository.Has(skill);
			var wfcs = new WorkflowControlSet().WithId();
			createWfcs(wfcs, absence);
			agent.WorkflowControlSet = wfcs;
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(period, new HashSet<Guid> { skill.Id.GetValueOrDefault()}, 10)
			});

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 5));

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));
		}

		[Test]
		public void ShouldDenyIfStaffingIsZero()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = threshold;
			skill.DefaultResolution = 60;
			var agent = PersonRepository.Has(skill);
			var wfcs = new WorkflowControlSet().WithId();
			createWfcs(wfcs, absence);
			agent.WorkflowControlSet = wfcs;
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(period, new HashSet<Guid> { skill.Id.GetValueOrDefault()}, 0)
			});

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 0.1));

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(DenyRequestCommand));
			var denyCommand = CommandDispatcher.LatestCommand as DenyRequestCommand;
			denyCommand.DenyReason.Should().StartWith(UserTexts.Resources.ResourceManager.GetString("InsufficientStaffingHours", agent.PermissionInformation.UICulture()).Substring(0, 10));
			
		}

		private static void createWfcs(WorkflowControlSet wfcs, IAbsence absence)
		{
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});
		}

		[Test]
		public void ShouldOnlyValidateIntervalsInRequestPeriod()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = threshold;
			skill.DefaultResolution = 60;
			var agent = PersonRepository.Has(skill);
			var wfcs = new WorkflowControlSet().WithId();
			createWfcs(wfcs, absence);
			agent.WorkflowControlSet = wfcs;
			var periodBefore = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);
			var periodAfter = new DateTimePeriod(2016, 12, 1, 10, 2016, 12, 1, 11);
			var shiftPeriod = new DateTimePeriod(2016, 12, 1, 9, 2016, 12, 1, 10);
			var requestPeriod = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 11);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, shiftPeriod, new ShiftCategory("category")));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(periodBefore, new HashSet<Guid> { skill.Id.GetValueOrDefault()}, 1),
				createSkillCombinationResource(shiftPeriod, new HashSet<Guid> { skill.Id.GetValueOrDefault()}, 10),
				createSkillCombinationResource(periodAfter, new HashSet<Guid> { skill.Id.GetValueOrDefault()}, 1)
			});

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(periodBefore.StartDateTime), 5));

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, requestPeriod)).WithId();

			Target.Process(personRequest);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));
		}

		[Test]
		public void ShouldDenyRequestIfShrinkage()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = threshold;
			skill.DefaultResolution = 60;
			var agent = PersonRepository.Has(skill);
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
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = period.StartDateTime,
					EndDateTime = period.EndDateTime,
					Resource = 6,
					SkillCombination = new HashSet<Guid> { skill.Id.GetValueOrDefault()}
				}
			});

			var skillday = skill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 5);
			skillday.SkillDataPeriodCollection.ForEach(x => x.Shrinkage = new Percent(0.1));
			SkillDayRepository.Has(skillday);

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(DenyRequestCommand));
		}


		[Test]
		public void ShouldApproveWithShrinkageAdvanced()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var skill2 = SkillRepository.Has("skillB", activity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = skill2.StaffingThresholds = threshold;
			skill.DefaultResolution = skill2.DefaultResolution = 60;
			var agent = PersonRepository.Has(skill, skill2);
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
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = period.StartDateTime,
					EndDateTime = period.EndDateTime,
					Resource = 4,
					SkillCombination = new HashSet<Guid> { skill.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()}
				}
			});

			var skillday = skill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 1);
			skillday.SkillDataPeriodCollection.ForEach(x => x.Shrinkage = new Percent(0.1));
			SkillDayRepository.Has(skillday);

			var skillday2 = skill2.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 1);
			skillday2.SkillDataPeriodCollection.ForEach(x => x.Shrinkage = new Percent(0.4));
			SkillDayRepository.Has(skillday2);

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));
		}

		[Test]
		public void ShouldApproveWithShrinkageAndCascading()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			skill.SetCascadingIndex(1);
			var skill2 = SkillRepository.Has("skillB", activity).WithId();
			skill2.SetCascadingIndex(2);
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = skill2.StaffingThresholds = threshold;
			skill.DefaultResolution = skill2.DefaultResolution = 60;
			var agent = PersonRepository.Has(skill, skill2);
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
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = period.StartDateTime,
					EndDateTime = period.EndDateTime,
					Resource = 10,
					SkillCombination = new HashSet<Guid> { skill.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()}
				}
			});

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 1));

			var skillday2 = skill2.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 1);
			skillday2.SkillDataPeriodCollection.ForEach(x => x.Shrinkage = new Percent(0.5));
			SkillDayRepository.Has(skillday2);

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));
		}

		[Test]
		public void ShouldDenyWithShrinkageAndCascading()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			skill.SetCascadingIndex(1);
			var skill2 = SkillRepository.Has("skillB", activity).WithId();
			skill2.SetCascadingIndex(2);
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = skill2.StaffingThresholds = threshold;
			skill.DefaultResolution = skill2.DefaultResolution = 60;
			var agent = PersonRepository.Has(skill, skill2);
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
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = period.StartDateTime,
					EndDateTime = period.EndDateTime,
					Resource = 3,
					SkillCombination = new HashSet<Guid> { skill.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()}
				}
			});


			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 1));

			var skillday2 = skill2.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 1);
			skillday2.SkillDataPeriodCollection.ForEach(x => x.Shrinkage = new Percent(0.5));
			SkillDayRepository.Has(skillday2);

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(DenyRequestCommand));
		}

		[Test]
		public void ShouldApproveRequestIfEnoughResourcesOnSkills()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var skill2 = SkillRepository.Has("skillB", activity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = skill2.StaffingThresholds = threshold;
			skill.DefaultResolution = skill2.DefaultResolution = 60;

			var agent = PersonRepository.Has(skill, skill2);
			var wfcs = new WorkflowControlSet().WithId();
			createWfcs(wfcs, absence);
			agent.WorkflowControlSet = wfcs;
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(period, new HashSet<Guid> { skill.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()}, 10)

			});

			skill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 4);

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));
		}

		[Test]
		public void ShouldApproveRequestIfActivityDoesntRequireSkill()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var lunch = ActivityRepository.Has("lunch");
			lunch.RequiresSkill = false;
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = threshold;
			skill.DefaultResolution = 60;
			var agent = PersonRepository.Has(skill);
			var wfcs = new WorkflowControlSet().WithId();
			createWfcs(wfcs, absence);
			agent.WorkflowControlSet = wfcs;
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, lunch, period, new ShiftCategory("category")));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(period, new HashSet<Guid> { skill.Id.GetValueOrDefault()}, 5)
			});

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 5));

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));

			var skillCombinations = SkillCombinationResourceRepository.LoadSkillCombinationResources(period).First();
			skillCombinations.StartDateTime.Should().Be.EqualTo(period.StartDateTime);
			skillCombinations.EndDateTime.Should().Be.EqualTo(period.EndDateTime);
			skillCombinations.Resource.Should().Be.EqualTo(5);
			CollectionAssert.AreEqual(skillCombinations.SkillCombination, new HashSet<Guid> { skill.Id.GetValueOrDefault()});
		}

		[Test]
		public void ShouldDenyRequestIfUnderStaffedOnSkill()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = threshold;
			skill.DefaultResolution = 60;

			var agent = PersonRepository.Has(skill);
			var wfcs = new WorkflowControlSet().WithId();
			createWfcs(wfcs, absence);
			agent.WorkflowControlSet = wfcs;
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(period, new HashSet<Guid> { skill.Id.GetValueOrDefault()}, 4)
			});
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 5));

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(DenyRequestCommand));

			var skillCombinations = SkillCombinationResourceRepository.LoadSkillCombinationResources(period).First();
			skillCombinations.StartDateTime.Should().Be.EqualTo(period.StartDateTime);
			skillCombinations.EndDateTime.Should().Be.EqualTo(period.EndDateTime);
			skillCombinations.Resource.Should().Be.EqualTo(4);
			CollectionAssert.AreEqual(skillCombinations.SkillCombination, new HashSet<Guid> { skill.Id.GetValueOrDefault()});
		}

		[Test]
		public void ShouldDenyRequestIfRequestCausesUnderStaffedOnSkill()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var skill2 = SkillRepository.Has("skillB", activity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = skill2.StaffingThresholds = threshold;
			skill.DefaultResolution = skill2.DefaultResolution = 60;

			var agent = PersonRepository.Has(skill, skill2);
			var wfcs = new WorkflowControlSet().WithId();
			createWfcs(wfcs, absence);
			agent.WorkflowControlSet = wfcs;
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(period, new HashSet<Guid> { skill.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()}, 10)
			});

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 4.6));
			SkillDayRepository.Has(skill2.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 4.6));

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(DenyRequestCommand));

			var skillCombinations = SkillCombinationResourceRepository.LoadSkillCombinationResources(period).First();
			skillCombinations.StartDateTime.Should().Be.EqualTo(period.StartDateTime);
			skillCombinations.EndDateTime.Should().Be.EqualTo(period.EndDateTime);
			skillCombinations.Resource.Should().Be.EqualTo(10);
			CollectionAssert.AreEqual(skillCombinations.SkillCombination, new HashSet<Guid> { skill.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()});
		}

		[Test]
		public void ShouldApproveIfEnoughStaffingForBothActivities()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var activity2 = ActivityRepository.Has("activity2");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var skill2 = SkillRepository.Has("skillB", activity).WithId();
			var skill3 = SkillRepository.Has("skillC", activity2).WithId();
			var skill4 = SkillRepository.Has("skillD", activity2).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = skill2.StaffingThresholds = skill3.StaffingThresholds = skill4.StaffingThresholds = threshold;
			skill.DefaultResolution = skill2.DefaultResolution = skill3.DefaultResolution = skill4.DefaultResolution = 60;

			var agent = PersonRepository.Has(skill, skill2, skill3, skill4);
			var wfcs = new WorkflowControlSet().WithId();
			createWfcs(wfcs, absence);
			agent.WorkflowControlSet = wfcs;
			var period1 = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);
			var period2 = new DateTimePeriod(2016, 12, 1, 9, 2016, 12, 1, 10);

			var ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period1, new ShiftCategory("category"));
			ass.AddActivity(activity2, period2);
			PersonAssignmentRepository.Has(ass);

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(period1, new HashSet<Guid> { skill.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()}, 10),
				createSkillCombinationResource(period2, new HashSet<Guid> { skill.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()}, 10),
				createSkillCombinationResource(period1, new HashSet<Guid> { skill3.Id.GetValueOrDefault(), skill4.Id.GetValueOrDefault()}, 10),
				createSkillCombinationResource(period2, new HashSet<Guid> { skill3.Id.GetValueOrDefault(), skill4.Id.GetValueOrDefault()}, 10)
			});
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(period1.StartDateTime), 4));
			SkillDayRepository.Has(skill2.CreateSkillDayWithDemand(scenario, new DateOnly(period1.StartDateTime), 4));
			SkillDayRepository.Has(skill3.CreateSkillDayWithDemand(scenario, new DateOnly(period1.StartDateTime), 4));
			SkillDayRepository.Has(skill4.CreateSkillDayWithDemand(scenario, new DateOnly(period1.StartDateTime), 4));


			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, new DateTimePeriod(period1.StartDateTime, period2.EndDateTime))).WithId();

			Target.Process(personRequest);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));
		}

		private static SkillCombinationResource createSkillCombinationResource(DateTimePeriod period1, HashSet<Guid> skillCombinations, double resource)
		{
			return new SkillCombinationResource
			{
				StartDateTime = period1.StartDateTime,
				EndDateTime = period1.EndDateTime,
				Resource = resource,
				SkillCombination = skillCombinations
			};
		}

		[Test]
		public void ShouldDenyIfEnoughStaffingOnASkillWithTwoActivities()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var activity2 = ActivityRepository.Has("activity2");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var skill2 = SkillRepository.Has("skillB", activity).WithId();
			var skill3 = SkillRepository.Has("skillC", activity2).WithId();
			var skill4 = SkillRepository.Has("skillD", activity2).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = skill2.StaffingThresholds = skill3.StaffingThresholds = skill4.StaffingThresholds = threshold;
			skill.DefaultResolution = skill2.DefaultResolution = skill3.DefaultResolution = skill4.DefaultResolution = 60;

			var agent = PersonRepository.Has(skill, skill2, skill3, skill4);
			var wfcs = new WorkflowControlSet().WithId();
			createWfcs(wfcs, absence);
			agent.WorkflowControlSet = wfcs;
			var period1 = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);
			var period2 = new DateTimePeriod(2016, 12, 1, 9, 2016, 12, 1, 10);

			var ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period1, new ShiftCategory("category"));
			ass.AddActivity(activity2, period2);
			PersonAssignmentRepository.Has(ass);

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(period1, new HashSet<Guid> { skill.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()}, 10),
				createSkillCombinationResource(period2, new HashSet<Guid> { skill.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()}, 10),
				createSkillCombinationResource(period1, new HashSet<Guid> { skill3.Id.GetValueOrDefault(), skill4.Id.GetValueOrDefault()}, 10),
				createSkillCombinationResource(period2, new HashSet<Guid> { skill3.Id.GetValueOrDefault(), skill4.Id.GetValueOrDefault()}, 10)
			});

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(period1.StartDateTime), 5));
			SkillDayRepository.Has(skill2.CreateSkillDayWithDemand(scenario, new DateOnly(period1.StartDateTime), 5));
			SkillDayRepository.Has(skill3.CreateSkillDayWithDemand(scenario, new DateOnly(period1.StartDateTime), 5));
			SkillDayRepository.Has(skill4.CreateSkillDayWithDemand(scenario, new DateOnly(period1.StartDateTime), 5));

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, new DateTimePeriod(period1.StartDateTime, period2.EndDateTime))).WithId();

			Target.Process(personRequest);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(DenyRequestCommand));
		}

		[Test]
		public void ShouldApproveRequestIfShovel()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill1 = SkillRepository.Has("skillA", activity).WithId();
			var skill2 = SkillRepository.Has("skillB", activity).WithId();
			skill1.SetCascadingIndex(1);
			skill2.SetCascadingIndex(2);
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill1.StaffingThresholds = skill2.StaffingThresholds = threshold;
			skill1.DefaultResolution = skill2.DefaultResolution = 60;

			var agent = PersonRepository.Has(skill2);
			var wfcs = new WorkflowControlSet().WithId();
			createWfcs(wfcs, absence);
			agent.WorkflowControlSet = wfcs;
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(period, new HashSet<Guid> { skill1.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()}, 1),
				createSkillCombinationResource(period, new HashSet<Guid> { skill2.Id.GetValueOrDefault()}, 1)
			});

			SkillDayRepository.Has(skill1.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 0));
			SkillDayRepository.Has(skill2.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 1));

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));
		}

		[Test]
		public void ShouldOnlyValidateOnPersonSkillAfterShovel()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill1 = SkillRepository.Has("skillA", activity).WithId();
			var skill2 = SkillRepository.Has("skillB", activity).WithId();
			skill1.SetCascadingIndex(1);
			skill2.SetCascadingIndex(2);
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill1.StaffingThresholds = skill2.StaffingThresholds = threshold;
			skill1.DefaultResolution = skill2.DefaultResolution = 60;

			var agent = PersonRepository.Has(skill1);
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
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = period.StartDateTime,
					EndDateTime = period.EndDateTime,
					Resource = 4,
					SkillCombination = new HashSet<Guid> { skill1.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()}
				},
				new SkillCombinationResource
				{
					StartDateTime = period.StartDateTime,
					EndDateTime = period.EndDateTime,
					Resource = 1,
					SkillCombination = new HashSet<Guid> { skill1.Id.GetValueOrDefault()}
				}
			});

			SkillDayRepository.Has(skill1.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 3));
			SkillDayRepository.Has(skill2.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 3));

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));
		}

		[Test]
		public void ShouldTreatNonCascadingAsPrimarySkillsAdvanced()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill1 = SkillRepository.Has("skillA", activity).WithId();
			var skill2 = SkillRepository.Has("skillB", activity).WithId();
			var skill3 = SkillRepository.Has("skill", activity).WithId();
			skill1.SetCascadingIndex(1);
			skill2.SetCascadingIndex(2);
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill1.StaffingThresholds = skill2.StaffingThresholds = skill3.StaffingThresholds = threshold;
			skill1.DefaultResolution = skill2.DefaultResolution = skill3.DefaultResolution = 60;

			var agent = PersonRepository.Has(skill1, skill3);
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
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = period.StartDateTime,
					EndDateTime = period.EndDateTime,
					Resource = 6,
					SkillCombination = new HashSet<Guid> { skill1.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault(), skill3.Id.GetValueOrDefault()}
				},
				new SkillCombinationResource
				{
					StartDateTime = period.StartDateTime,
					EndDateTime = period.EndDateTime,
					Resource = 1,
					SkillCombination = new HashSet<Guid> { skill1.Id.GetValueOrDefault(), skill3.Id.GetValueOrDefault()}
				}
			});
			SkillDayRepository.Has(skill1.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 3));
			SkillDayRepository.Has(skill2.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 3));
			SkillDayRepository.Has(skill3.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 3));

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));
		}

		[Test]
		public void ShouldApproveRequestIfOnlyUnsortedSkills()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill1 = SkillRepository.Has("skillA", activity).WithId();
			var skill2 = SkillRepository.Has("skillB", activity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill1.StaffingThresholds = skill2.StaffingThresholds = threshold;
			skill1.DefaultResolution = skill2.DefaultResolution = 60;

			var agent = PersonRepository.Has(skill1);
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
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = period.StartDateTime,
					EndDateTime = period.EndDateTime,
					Resource = 6,
					SkillCombination = new HashSet<Guid> { skill1.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()}
				},
				new SkillCombinationResource
				{
					StartDateTime = period.StartDateTime,
					EndDateTime = period.EndDateTime,
					Resource = 1,
					SkillCombination = new HashSet<Guid> { skill1.Id.GetValueOrDefault()}
				}
			});
			SkillDayRepository.Has(skill1.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 3));
			SkillDayRepository.Has(skill2.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 3));

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));
		}

		[Test]
		public void ShouldDenyRequestIfOnlyUnsortedSkills()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill1 = SkillRepository.Has("skillA", activity).WithId();
			var skill2 = SkillRepository.Has("skillB", activity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill1.StaffingThresholds = skill2.StaffingThresholds = threshold;
			skill1.DefaultResolution = skill2.DefaultResolution = 60;

			var agent = PersonRepository.Has(skill1);
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
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = period.StartDateTime,
					EndDateTime = period.EndDateTime,
					Resource = 5,
					SkillCombination = new HashSet<Guid> { skill1.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()}
				},
				new SkillCombinationResource
				{
					StartDateTime = period.StartDateTime,
					EndDateTime = period.EndDateTime,
					Resource = 1,
					SkillCombination = new HashSet<Guid> { skill1.Id.GetValueOrDefault()}
				}
			});

			SkillDayRepository.Has(skill1.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 3));
			SkillDayRepository.Has(skill2.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 3));

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(DenyRequestCommand));
		}

		[Test]
		public void ShouldApproveRequestIfShovelAndHasUnsortedSkills()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill1 = SkillRepository.Has("skillA", activity).WithId();
			var skill2 = SkillRepository.Has("skillB", activity).WithId();
			var skill3 = SkillRepository.Has("skillUnsorted", activity).WithId();
			skill1.SetCascadingIndex(1);
			skill2.SetCascadingIndex(2);
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill1.StaffingThresholds = skill2.StaffingThresholds = skill3.StaffingThresholds = threshold;
			skill1.DefaultResolution = skill2.DefaultResolution = skill3.DefaultResolution = 60;

			var agent = PersonRepository.Has(skill2);
			var wfcs = new WorkflowControlSet().WithId();
			createWfcs(wfcs, absence);
			agent.WorkflowControlSet = wfcs;
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(period,
											   new HashSet<Guid> { skill1.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault(), skill3.Id.GetValueOrDefault()}, 2),
				createSkillCombinationResource(period, new HashSet<Guid> { skill2.Id.GetValueOrDefault()}, 1)
			});

			SkillDayRepository.Has(skill1.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 0));
			SkillDayRepository.Has(skill2.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 1));
			SkillDayRepository.Has(skill3.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 0));

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));
		}

		[Test, SetCulture("en-US")]
		public void DenyIfReadModelDataIsTooOld()
		{
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = threshold;
			skill.DefaultResolution = 60;
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);
			var now = new DateTime(2016, 12, 1, 7, 0, 0);

			var agent = PersonRepository.Has(skill);
			var wfcs = new WorkflowControlSet().WithId();
			createWfcs(wfcs, absence);
			agent.WorkflowControlSet = wfcs;
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Now.Is(now);
			SkillCombinationResourceRepository.SetLastCalculatedTime(now.AddHours(-3));
			Target.Process(personRequest);
			var denyCommand = CommandDispatcher.LatestCommand as DenyRequestCommand;
			denyCommand.DenyReason.Should().Contain(UserTexts.Resources.ResourceManager.GetString("DenyReasonSystemBusy", agent.PermissionInformation.Culture()));
		}

		[Test, SetCulture("en-US")]
		public void DenyIfReadModelDataIsNotTooOldButNoSkillCombinationsInReadModel()
		{
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = threshold;
			skill.DefaultResolution = 60;
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);
			var now = new DateTime(2016, 12, 1, 7, 0, 0);

			var agent = PersonRepository.Has(skill);
			var wfcs = new WorkflowControlSet().WithId();
			createWfcs(wfcs, absence);
			agent.WorkflowControlSet = wfcs;
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));


			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Now.Is(now);
			Target.Process(personRequest);
			var denyCommand = CommandDispatcher.LatestCommand as DenyRequestCommand;
			denyCommand.DenyReason.Should().Contain(UserTexts.Resources.ResourceManager.GetString("DenyReasonNoSkillCombinationsFound", agent.PermissionInformation.Culture()));
		}

		[Test]
		public void ShouldApproveIfMySkillsAreOkButOthersAreUnderstaffed()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var activity2 = ActivityRepository.Has("activity2");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var skill2 = SkillRepository.Has("skillB", activity2).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = skill2.StaffingThresholds = threshold;
			skill.DefaultResolution = skill2.DefaultResolution = 60;

			var agent = PersonRepository.Has(skill);
			var wfcs = new WorkflowControlSet().WithId();
			createWfcs(wfcs, absence);
			agent.WorkflowControlSet = wfcs;
			var period1 = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);
			var period2 = new DateTimePeriod(2016, 12, 1, 9, 2016, 12, 1, 10);

			var ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period1, new ShiftCategory("category"));
			PersonAssignmentRepository.Has(ass);

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(period1, new HashSet<Guid> { skill.Id.GetValueOrDefault()}, 10),
				createSkillCombinationResource(period2, new HashSet<Guid> { skill2.Id.GetValueOrDefault()}, 10)
			});

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(period1.StartDateTime), 4));
			SkillDayRepository.Has(skill2.CreateSkillDayWithDemand(scenario, new DateOnly(period1.StartDateTime), 15));


			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, new DateTimePeriod(period1.StartDateTime, period2.EndDateTime))).WithId();

			Target.Process(personRequest);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));
		}

		[Test]
		public void ShouldApproveRequestIfSkillsHaveDifferentResolution()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var phoneActivity = ActivityRepository.Has("phone");
			var emailActivity = ActivityRepository.Has("email");
			var phoneSkill = SkillRepository.Has("skillA", phoneActivity).WithId();
			var emailSkill = SkillRepository.Has("skillB", emailActivity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			phoneSkill.StaffingThresholds = emailSkill.StaffingThresholds = threshold;
			phoneSkill.DefaultResolution = 15;
			emailSkill.DefaultResolution = 60;

			var agent = PersonRepository.Has(emailSkill);
			var wfcs = new WorkflowControlSet().WithId();
			createWfcs(wfcs, absence);
			agent.WorkflowControlSet = wfcs;
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, emailActivity, period, new ShiftCategory("category")));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(new DateTimePeriod(period.StartDateTime, period.StartDateTime.AddMinutes(15)),
											   new HashSet<Guid> {phoneSkill.Id.GetValueOrDefault()}, 10),
				createSkillCombinationResource(new DateTimePeriod(period.StartDateTime, period.StartDateTime.AddMinutes(60)),
											   new HashSet<Guid> {emailSkill.Id.GetValueOrDefault()}, 10)

			});
			SkillDayRepository.Has(emailSkill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 6));
			SkillDayRepository.Has(phoneSkill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 6));


			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));
		}

		[Test]
		public void ShouldApproveRequestIfPersonSkillsOnMinResolution()
		{
			Now.Is(new DateTime(2016, 12, 1, 07, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var phoneActivity = ActivityRepository.Has("phone");
			var emailActivity = ActivityRepository.Has("email");
			var phoneSkill = SkillRepository.Has("skillA", phoneActivity).WithId();
			var emailSkill = SkillRepository.Has("skillB", emailActivity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			phoneSkill.StaffingThresholds = emailSkill.StaffingThresholds = threshold;
			phoneSkill.DefaultResolution = 15;
			emailSkill.DefaultResolution = 60;

			var agent = PersonRepository.Has(emailSkill, phoneSkill);
			var wfcs = new WorkflowControlSet().WithId();
			createWfcs(wfcs, absence);
			agent.WorkflowControlSet = wfcs;
			var emailPerod = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);
			var period = new DateTimePeriod(2016, 12, 1, 9, 2016, 12, 1, 10);
			var mainShiftPersonAssing = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, emailActivity, emailPerod, new ShiftCategory("category"));
			mainShiftPersonAssing.AddActivity(phoneActivity, period);

			PersonAssignmentRepository.Has(mainShiftPersonAssing);


			var skillCombinationsPhone = new HashSet<Guid> {phoneSkill.Id.GetValueOrDefault()};
			var skillCombinationsEmail = new HashSet<Guid> {emailSkill.Id.GetValueOrDefault()};
			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(new DateTimePeriod(emailPerod.StartDateTime, emailPerod.StartDateTime.AddMinutes(15)), skillCombinationsPhone, 10),
				createSkillCombinationResource(new DateTimePeriod(emailPerod.StartDateTime.AddMinutes(15), emailPerod.StartDateTime.AddMinutes(30)), skillCombinationsPhone, 10),
				createSkillCombinationResource(new DateTimePeriod(emailPerod.StartDateTime.AddMinutes(30), emailPerod.StartDateTime.AddMinutes(45)), skillCombinationsPhone, 10),
				createSkillCombinationResource(new DateTimePeriod(emailPerod.StartDateTime.AddMinutes(45), emailPerod.StartDateTime.AddMinutes(60)), skillCombinationsPhone, 10),
				createSkillCombinationResource(new DateTimePeriod(period.StartDateTime, period.StartDateTime.AddMinutes(15)), skillCombinationsPhone, 10),
				createSkillCombinationResource(new DateTimePeriod(period.StartDateTime.AddMinutes(15), period.StartDateTime.AddMinutes(30)), skillCombinationsPhone, 10),
				createSkillCombinationResource(new DateTimePeriod(period.StartDateTime.AddMinutes(30), period.StartDateTime.AddMinutes(45)), skillCombinationsPhone, 10),
				createSkillCombinationResource(new DateTimePeriod(period.StartDateTime.AddMinutes(45), period.StartDateTime.AddMinutes(60)), skillCombinationsPhone, 10),
				createSkillCombinationResource(emailPerod, skillCombinationsEmail, 10),
				createSkillCombinationResource(period, skillCombinationsEmail, 10)

			});

			SkillDayRepository.Has(phoneSkill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 4));
			SkillDayRepository.Has(emailSkill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 4));

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 10))).WithId();

			Target.Process(personRequest);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));
		}

		[Test]
		public void ShouldRemoveResourceOnScheduledInterval()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var emailActivity = ActivityRepository.Has("email");
			var phoneActivity = ActivityRepository.Has("phone");
			var emailSkill = SkillRepository.Has("skillB", emailActivity).WithId();
			var phoneSkill = SkillRepository.Has("skillA", phoneActivity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			emailSkill.DefaultResolution = 60;
			emailSkill.StaffingThresholds = threshold;

			var agent = PersonRepository.Has(emailSkill, phoneSkill);
			var wfcs = new WorkflowControlSet().WithId();
			createWfcs(wfcs, absence);
			agent.WorkflowControlSet = wfcs;
			var emailPerod = new DateTimePeriod(new DateTime(2016, 12, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2016, 12, 1, 8, 30, 0, DateTimeKind.Utc));
			var mainShiftPersonAssing = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, emailActivity, emailPerod, new ShiftCategory("category"));
			PersonAssignmentRepository.Has(mainShiftPersonAssing);

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(
					new DateTimePeriod(emailPerod.StartDateTime, emailPerod.StartDateTime.AddMinutes(60)),
					new HashSet<Guid> {emailSkill.Id.GetValueOrDefault()}, 10),
				createSkillCombinationResource(
					new DateTimePeriod(emailPerod.StartDateTime, emailPerod.StartDateTime.AddMinutes(15)),
					new HashSet<Guid> {phoneSkill.Id.GetValueOrDefault()}, 10)
			});

			SkillDayRepository.Has(emailSkill.CreateSkillDayWithDemand(scenario, new DateOnly(emailPerod.StartDateTime), 4));


			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9))).WithId();

			Target.Process(personRequest);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));
		}

		[Test]
		public void ShouldApproveRequestIfTheAgentIsNotScheduled()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var phoneActivity = ActivityRepository.Has("phone");
			var phoneSkill = SkillRepository.Has("skillA", phoneActivity).WithId();
			phoneSkill.DefaultResolution = 30;
			var workload = WorkloadFactory.CreateWorkload(phoneSkill);
			WorkloadDay workloadDay1 = new WorkloadDay();
			workloadDay1.Create(new DateOnly(2008, 7, 1), workload, new List<TimePeriod>() {new TimePeriod(new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0))});

			phoneSkill.AddWorkload(workload);
			var agent = PersonRepository.Has(phoneSkill);
			var wfcs = new WorkflowControlSet().WithId();
			createWfcs(wfcs, absence);
			agent.WorkflowControlSet = wfcs;
			var period = new DateTimePeriod(2016, 12, 1, 9, 2016, 12, 1, 10);
			var mainShiftPersonAssing = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, phoneActivity, period, new ShiftCategory("category"));
			PersonAssignmentRepository.Has(mainShiftPersonAssing);
			
			var skillCombinations = new HashSet<Guid> {phoneSkill.Id.GetValueOrDefault()};
			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(new DateTimePeriod(period.StartDateTime, period.StartDateTime.AddMinutes(30)), skillCombinations, 10),
				createSkillCombinationResource(new DateTimePeriod(period.StartDateTime.AddMinutes(30), period.StartDateTime.AddMinutes(60)), skillCombinations, 10),
				createSkillCombinationResource(new DateTimePeriod(period.StartDateTime.AddMinutes(60), period.StartDateTime.AddMinutes(90)), skillCombinations, 10)

			});

			SkillDayRepository.Has(phoneSkill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 4));


			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 10, 2016, 12, 1, 11))).WithId();

			Target.Process(personRequest);
			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));
		}

		[Test]
		public void ShouldGetSkilldaysForCorrectDays()
		{
			Now.Is(new DateTime(2017, 5, 1, 22, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillSetupHelper.CreateSkill(60, "skill", new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1)), false, activity);
			skill.TimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			SkillRepository.Has(skill);

			var agent = PersonRepository.Has(skill);
			var wfcs = new WorkflowControlSet().WithId();
			createWfcs(wfcs, absence);
			agent.WorkflowControlSet = wfcs;
			agent.PermissionInformation.SetDefaultTimeZone(skill.TimeZone);
			var period = new DateTimePeriod(2017, 5, 1, 21, 2017, 5, 2, 9);
			var requestPeriod = new DateTimePeriod(2017, 5, 1, 21, 2017, 5, 1, 23);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));

			var periodDayOne = new DateTimePeriod(2017, 5, 1, 21, 2017, 5, 1, 22);
			var periodDayTwo1 = new DateTimePeriod(2017, 5, 1, 22, 2017, 5, 1, 23);
			var periodDayTwo2 = new DateTimePeriod(2017, 5, 1, 23, 2017, 5, 2, 0);

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(periodDayOne, new HashSet<Guid> { skill.Id.GetValueOrDefault()}, 100),
				createSkillCombinationResource(periodDayTwo1, new HashSet<Guid> { skill.Id.GetValueOrDefault()}, 1),
				createSkillCombinationResource(periodDayTwo2, new HashSet<Guid> { skill.Id.GetValueOrDefault()}, 1)
			});


			var skillday = SkillSetupHelper.CreateSkillDay(skill, scenario, Now.UtcDateTime(), new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1)), false);
			var skillday2 = SkillSetupHelper.CreateSkillDay(skill, scenario, Now.UtcDateTime().AddDays(1), new TimePeriod(8, 0, 18, 00), false);
			SkillDayRepository.Has(new [] { skillday, skillday2});

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, requestPeriod)).WithId();

			Target.Process(personRequest);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(DenyRequestCommand));
		}

		
		/* xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx */

		[Test]
		public void ShouldDenyRequestIfUnderstaffedOnPhone()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var phoneActivity = ActivityRepository.Has("phone");
			var emailActivity = ActivityRepository.Has("email");
			var phoneSkill = SkillRepository.Has("skillA", phoneActivity).WithId();
			var emailSkill = SkillRepository.Has("skillB", emailActivity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			phoneSkill.StaffingThresholds = emailSkill.StaffingThresholds = threshold;
			phoneSkill.DefaultResolution = 15;
			emailSkill.DefaultResolution = 60;

			var agent = PersonRepository.Has(emailSkill, phoneSkill);
			var wfcs = new WorkflowControlSet().WithId();
			createWfcs(wfcs, absence);
			agent.WorkflowControlSet = wfcs;
			var emailPerod = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);
			var period = new DateTimePeriod(2016, 12, 1, 9, 2016, 12, 1, 10);
			var mainShiftPersonAssing = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, emailActivity, emailPerod, new ShiftCategory("category"));
			mainShiftPersonAssing.AddActivity(phoneActivity, period);

			PersonAssignmentRepository.Has(mainShiftPersonAssing);


			var skillCombinations = new HashSet<Guid> {phoneSkill.Id.GetValueOrDefault()};
			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(new DateTimePeriod(period.StartDateTime, period.StartDateTime.AddMinutes(15)),
											   skillCombinations, 10),
				createSkillCombinationResource(
					new DateTimePeriod(period.StartDateTime.AddMinutes(15), period.StartDateTime.AddMinutes(30)),
					skillCombinations, 10),
				createSkillCombinationResource(
					new DateTimePeriod(period.StartDateTime.AddMinutes(30), period.StartDateTime.AddMinutes(45)),
					skillCombinations, 10),
				createSkillCombinationResource(
					new DateTimePeriod(period.StartDateTime.AddMinutes(45), period.StartDateTime.AddMinutes(60)),
					skillCombinations, 2),
				createSkillCombinationResource(new DateTimePeriod(period.StartDateTime, period.StartDateTime.AddMinutes(30)),
											   new HashSet<Guid> {emailSkill.Id.GetValueOrDefault()}, 10)

			});

			SkillDayRepository.Has(phoneSkill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 4));
			SkillDayRepository.Has(emailSkill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 4));


			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 10))).WithId();

			Target.Process(personRequest);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(DenyRequestCommand));
		}

		[Test]
		public void ShouldBeDeniedWhenMidnightRequestAndNoShiftTodayAndUnderstaffedTomorrow()
		{
			Now.Is(new DateTime(2016, 12, 1, 22, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = threshold;
			skill.DefaultResolution = 60;
			var agent = PersonRepository.Has(skill);
			var wfcs = new WorkflowControlSet().WithId();
			createWfcs(wfcs, absence);
			agent.WorkflowControlSet = wfcs;
			var period = new DateTimePeriod(2016, 12, 2, 8, 2016, 12, 2, 9);
			var requestPeriod = new DateTimePeriod(2016, 12, 1, 23, 2016, 12, 2, 9);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = period.StartDateTime,
					EndDateTime = period.EndDateTime,
					Resource = 5,
					SkillCombination = new HashSet<Guid> { skill.Id.GetValueOrDefault()}
				}
			});

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 10));

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, requestPeriod)).WithId();

			Target.Process(personRequest);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(DenyRequestCommand));
		}

		[Test]
		public void ShouldBeDeniedWhenMidnightShiftAndUnderStaffedAndRequestIsTomorrow()
		{
			Now.Is(new DateTime(2016, 12, 1, 22, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = threshold;
			skill.DefaultResolution = 60;
			var agent = PersonRepository.Has(skill);
			var wfcs = new WorkflowControlSet().WithId();
			createWfcs(wfcs, absence);
			agent.WorkflowControlSet = wfcs;
			var period = new DateTimePeriod(2016, 12, 1, 20, 2016, 12, 2, 9);
			var requestPeriod = new DateTimePeriod(2016, 12, 2, 1, 2016, 12, 2, 2);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = requestPeriod.StartDateTime,
					EndDateTime = requestPeriod.EndDateTime,
					Resource = 5,
					SkillCombination = new HashSet<Guid> { skill.Id.GetValueOrDefault()}
				}
			});
			
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(period.EndDateTime), 10));

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, requestPeriod)).WithId();

			Target.Process(personRequest);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(DenyRequestCommand));
		}


		[Test]
		public void ShouldBeApprovedWhenNotScheduledToday()
		{
			Now.Is(new DateTime(2016, 12, 1, 8, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = threshold;
			skill.DefaultResolution = 60;
			var agent = PersonRepository.Has(skill);
			var wfcs = new WorkflowControlSet().WithId();
			createWfcs(wfcs, absence);
			agent.WorkflowControlSet = wfcs;
			var period = new DateTimePeriod(2016, 12, 1, 9, 2016, 12, 1, 10);

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = period.StartDateTime,
					EndDateTime = period.EndDateTime,
					Resource = 5,
					SkillCombination = new HashSet<Guid> { skill.Id.GetValueOrDefault()}
				}
			});

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 10));

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));
		}

	}
}

	
