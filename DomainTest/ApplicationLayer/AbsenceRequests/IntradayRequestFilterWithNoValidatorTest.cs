using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[DomainTest]
	public class IntradayRequestFilterWithNoValidatorTest : IIsolateSystem
	{
		public IAbsenceRequestProcessor Target;
		public FakePersonRepository PersonRepository;
		public FakeSkillRepository SkillRepository;
		public FakeActivityRepository ActivityRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeCommandDispatcher CommandDispatcher;
		public FakeQueuedAbsenceRequestRepository QueuedAbsenceRequestRepository;
		public IScheduleStorage ScheduleStorage;
		public MutableNow Now;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeCommandDispatcher>().For<ICommandDispatcher>();
		}

		[Test]
		public void ShouldApproveIfNoValidatorIsUsed()
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
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});
			agent.WorkflowControlSet = wfcs;
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));


			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 5));

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest);
			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));

		}

		[Test]
		public void ShouldSetAbsenceToPendingIfAutoGrantIsNotSet()
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
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				AbsenceRequestProcess = new PendingAbsenceRequest()
			});
			agent.WorkflowControlSet = wfcs;
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));


			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 5));

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest);
			CommandDispatcher.LatestCommand.Should().Be.Null();
		}

		[Test]
		public void ShouldApproveRequestIfAnyValidatorIsUsedForIntraday()
		{
			Now.Is(new DateTime(2016, 12, 1, 6, 00, 00, DateTimeKind.Utc));

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
				createSkillCombinationResource(period, new[] {skill.Id.GetValueOrDefault()}, 10)
			});

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 5));

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));
		}

		[Test]
		public void ShouldQueueRequestIfAnyValidatorIsUsed()
		{
			Now.Is(new DateTime(2016, 12, 1, 6, 00, 00, DateTimeKind.Utc));

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
				StaffingThresholdValidator = new BudgetGroupHeadCountValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});
			agent.WorkflowControlSet = wfcs;
			var period = new DateTimePeriod(2016, 12, 5, 8, 2016, 12, 5, 9);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(period, new[] {skill.Id.GetValueOrDefault()}, 10)
			});

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 5));

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest);

			QueuedAbsenceRequestRepository.LoadAll().Count().Should().Be.EqualTo(1);
			CommandDispatcher.LatestCommand.Should().Be.Null();
		}

		private static SkillCombinationResource createSkillCombinationResource(DateTimePeriod period1, Guid[] skillCombinations, double resource)
		{
			return new SkillCombinationResource
			{
				StartDateTime = period1.StartDateTime,
				EndDateTime = period1.EndDateTime,
				Resource = resource,
				SkillCombination = skillCombinations
			};
		}


	}
}

	
