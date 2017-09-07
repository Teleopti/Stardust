using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[DomainTest]
	[Toggle(Toggles.Wfm_Requests_ProcessWaitlistBefore24hRequests_45767)]
	public class RequestProcessorTest : ISetup
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
		public FakeQueuedAbsenceRequestRepository QueuedAbsenceRequestRepository;
		public IScheduleStorage ScheduleStorage;
		public MutableNow Now;
		public FakePersonRequestRepository PersonRequestRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeCommandDispatcher>().For<ICommandDispatcher>();
		}

		[Test]
		public void ShouldProcessWaitlistedAndApproveRequest()
		{
			Now.Is(new DateTime(2016, 12, 1, 6, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("phone");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = threshold;
			skill.DefaultResolution = 60;
			var agent = PersonRepository.Has(skill);
			var waitListedAgent = PersonRepository.Has(skill);
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
			wfcs.AbsenceRequestWaitlistEnabled = true;
			agent.WorkflowControlSet = wfcs;
			waitListedAgent.WorkflowControlSet = wfcs;
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(waitListedAgent, scenario, activity, period, new ShiftCategory("category")));
			
			IPersonRequest waitListedRequest =  new PersonRequest(waitListedAgent, new AbsenceRequest(absence, period)).WithId();
			waitListedRequest.Deny("Work Hard!", new PersonRequestAuthorizationCheckerForTest());
			PersonRequestRepository.Add(waitListedRequest);

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(period, new[] {skill.Id.GetValueOrDefault()}, 10)
			});

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 5));

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();
			Target.Process(personRequest, Now.UtcDateTime());

			CommandDispatcher.AllComands.Count(x=>x.GetType()== typeof(ApproveRequestCommand)).Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldProcessWaitlistedAndDenyRequest()
		{
			Now.Is(new DateTime(2016, 12, 1, 6, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("phone");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = threshold;
			skill.DefaultResolution = 60;
			var agent = PersonRepository.Has(skill);
			var waitListedAgent = PersonRepository.Has(skill);
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
			wfcs.AbsenceRequestWaitlistEnabled = true;
			agent.WorkflowControlSet = wfcs;
			waitListedAgent.WorkflowControlSet = wfcs;
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(waitListedAgent, scenario, activity, period, new ShiftCategory("category")));

			IPersonRequest waitListedRequest = new PersonRequest(waitListedAgent, new AbsenceRequest(absence, period)).WithId();
			waitListedRequest.Deny("Work Hard!", new PersonRequestAuthorizationCheckerForTest());
			PersonRequestRepository.Add(waitListedRequest);

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(period, new[] {skill.Id.GetValueOrDefault()}, 0.5)
			});

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 5));

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();
			Target.Process(personRequest, Now.UtcDateTime());

			CommandDispatcher.AllComands.Count(x => x.GetType() == typeof(DenyRequestCommand)).Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldApproveOnlyWaitlistedRequest()
		{
			Now.Is(new DateTime(2016, 12, 1, 6, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("phone");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = threshold;
			skill.DefaultResolution = 60;
			var agent = PersonRepository.Has(skill);
			var waitListedAgent = PersonRepository.Has(skill);
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
			wfcs.AbsenceRequestWaitlistEnabled = true;
			agent.WorkflowControlSet = wfcs;
			waitListedAgent.WorkflowControlSet = wfcs;
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(waitListedAgent, scenario, activity, period, new ShiftCategory("category")));

			IPersonRequest waitListedRequest = new PersonRequest(waitListedAgent, new AbsenceRequest(absence, period)).WithId();
			waitListedRequest.Deny("Work Hard!", new PersonRequestAuthorizationCheckerForTest());
			PersonRequestRepository.Add(waitListedRequest);

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(period, new[] {skill.Id.GetValueOrDefault()}, 5.5)
			});

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 5));

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();
			Target.Process(personRequest, Now.UtcDateTime());

			CommandDispatcher.AllComands.Count(x => x.GetType() == typeof(ApproveRequestCommand)).Should().Be.EqualTo(1);
			ApproveRequestCommand command =  (ApproveRequestCommand) CommandDispatcher.AllComands.First(x => x.GetType() == typeof(ApproveRequestCommand));
			command.PersonRequestId.Should().Be.EqualTo(waitListedRequest.Id);
		}

		[Test]
		public void ShouldApproveRequestBasedOnSeniority()
		{
			Now.Is(new DateTime(2016, 12, 1, 6, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("phone");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = threshold;
			skill.DefaultResolution = 60;
			var agent = PersonRepository.Has(skill);
			
			var waitListedAgent = PersonRepository.Has(skill);
			waitListedAgent.ChangePersonPeriodStartDate(new DateOnly(2015,12,1),waitListedAgent.PersonPeriodCollection.First());
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
			wfcs.AbsenceRequestWaitlistEnabled = true;
			wfcs.AbsenceRequestWaitlistProcessOrder = WaitlistProcessOrder.BySeniority;
			agent.WorkflowControlSet = wfcs;
			waitListedAgent.WorkflowControlSet = wfcs;
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(waitListedAgent, scenario, activity, period, new ShiftCategory("category")));

			IPersonRequest waitListedRequest = new PersonRequest(waitListedAgent, new AbsenceRequest(absence, period)).WithId();
			waitListedRequest.Deny("Work Hard!", new PersonRequestAuthorizationCheckerForTest());
			PersonRequestRepository.Add(waitListedRequest);

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(period, new[] {skill.Id.GetValueOrDefault()}, 5.5)
			});

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 5));

			IPersonRequest personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();
			Target.Process(personRequest, Now.UtcDateTime());

			CommandDispatcher.AllComands.Count(x => x.GetType() == typeof(ApproveRequestCommand)).Should().Be.EqualTo(1);
			ApproveRequestCommand command = (ApproveRequestCommand)CommandDispatcher.AllComands.First(x => x.GetType() == typeof(ApproveRequestCommand));
			command.PersonRequestId.Should().Be.EqualTo(personRequest.Id);
		}

		[Test]
		public void ShouldApproveManyWaitlistedRequestBased()
		{
			Now.Is(new DateTime(2016, 12, 1, 6, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("phone");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = threshold;
			skill.DefaultResolution = 60;

			var skillb = SkillRepository.Has("skillB", activity).WithId();
			skillb.StaffingThresholds = threshold;
			skillb.DefaultResolution = 15;


			var agent = PersonRepository.Has(skill);
			var waitListedAgent1 = PersonRepository.Has(skill);
			var waitListedAgent2 = PersonRepository.Has(skill);
			var waitListedAgent3 = PersonRepository.Has(skill);
			var waitListedAgent4 = PersonRepository.Has(skill);


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
			wfcs.AbsenceRequestWaitlistEnabled = true;
			wfcs.AbsenceRequestWaitlistProcessOrder = WaitlistProcessOrder.BySeniority;
			agent.WorkflowControlSet = wfcs;
			waitListedAgent1.WorkflowControlSet = wfcs;
			waitListedAgent2.WorkflowControlSet = wfcs;
			waitListedAgent3.WorkflowControlSet = wfcs;
			waitListedAgent4.WorkflowControlSet = wfcs;

			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(waitListedAgent1, scenario, activity, period, new ShiftCategory("category")));
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(waitListedAgent2, scenario, activity, period, new ShiftCategory("category")));
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(waitListedAgent3, scenario, activity, period, new ShiftCategory("category")));
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(waitListedAgent4, scenario, activity, period, new ShiftCategory("category")));

			IPersonRequest waitListedRequest1 = new PersonRequest(waitListedAgent1, new AbsenceRequest(absence, period)).WithId();
			IPersonRequest waitListedRequest2 = new PersonRequest(waitListedAgent2, new AbsenceRequest(absence, period)).WithId();
			IPersonRequest waitListedRequest3 = new PersonRequest(waitListedAgent3, new AbsenceRequest(absence, period)).WithId();
			IPersonRequest waitListedRequest4 = new PersonRequest(waitListedAgent4, new AbsenceRequest(absence, period)).WithId();
			waitListedRequest1.Deny("Work Hard!", new PersonRequestAuthorizationCheckerForTest());
			waitListedRequest2.Deny("Work Hard!", new PersonRequestAuthorizationCheckerForTest());
			waitListedRequest3.Deny("Work Hard!", new PersonRequestAuthorizationCheckerForTest());
			waitListedRequest4.Deny("Work Hard!", new PersonRequestAuthorizationCheckerForTest());
			PersonRequestRepository.Add(waitListedRequest1);
			PersonRequestRepository.Add(waitListedRequest2);
			PersonRequestRepository.Add(waitListedRequest3);
			PersonRequestRepository.Add(waitListedRequest4);



			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(period, new[] {skill.Id.GetValueOrDefault()}, 12),
				createSkillCombinationResource(period, new[] {skillb.Id.GetValueOrDefault()}, 12)
			});

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 10));

			IPersonRequest personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();
			Target.Process(personRequest, Now.UtcDateTime());

			CommandDispatcher.AllComands.Count(x => x.GetType() == typeof(ApproveRequestCommand)).Should().Be.EqualTo(3);
			CommandDispatcher.AllComands.Count(x => x.GetType() == typeof(DenyRequestCommand)).Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldApproveRemoveResourceSmartly()
		{
			Now.Is(new DateTime(2016, 12, 1, 6, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("phone");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = threshold;
			skill.DefaultResolution = 60;
			var agent = PersonRepository.Has(skill);

			var waitListedAgent = PersonRepository.Has(skill);
			waitListedAgent.ChangePersonPeriodStartDate(new DateOnly(2015, 12, 1), waitListedAgent.PersonPeriodCollection.First());
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
			wfcs.AbsenceRequestWaitlistEnabled = true;
			wfcs.AbsenceRequestWaitlistProcessOrder = WaitlistProcessOrder.BySeniority;
			agent.WorkflowControlSet = wfcs;
			waitListedAgent.WorkflowControlSet = wfcs;
			var periodWaitlisted = new DateTimePeriod(new DateTime( 2016, 12, 1, 8,0,0,DateTimeKind.Utc), new DateTime(2016, 12, 1, 8, 30, 0, DateTimeKind.Utc));
			var period = new DateTimePeriod(new DateTime( 2016, 12, 1, 8,30,0,DateTimeKind.Utc), new DateTime(2016, 12, 1, 9, 0, 0, DateTimeKind.Utc));
			

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(waitListedAgent, scenario, activity, periodWaitlisted, new ShiftCategory("category")));

			IPersonRequest waitListedRequest = new PersonRequest(waitListedAgent, new AbsenceRequest(absence, periodWaitlisted)).WithId();
			waitListedRequest.Deny("Work Hard!", new PersonRequestAuthorizationCheckerForTest());
			PersonRequestRepository.Add(waitListedRequest);

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(new DateTimePeriod(new DateTime( 2016, 12, 1, 8,0,0,DateTimeKind.Utc), new DateTime(2016, 12, 1, 9, 0, 0, DateTimeKind.Utc)), new[] {skill.Id.GetValueOrDefault()}, 5.5)
			});

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 5));

			IPersonRequest personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();
			Target.Process(personRequest, Now.UtcDateTime());

			CommandDispatcher.AllComands.Count(x => x.GetType() == typeof(ApproveRequestCommand)).Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldApproveRequestBasedOnFirstComeFirstServe()
		{
			Now.Is(new DateTime(2016, 12, 1, 6, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("phone");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = threshold;
			skill.DefaultResolution = 60;
			var agent = PersonRepository.Has(skill);

			var waitListedAgent = PersonRepository.Has(skill);
			waitListedAgent.ChangePersonPeriodStartDate(new DateOnly(2015, 12, 1), waitListedAgent.PersonPeriodCollection.First());
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
			wfcs.AbsenceRequestWaitlistEnabled = true;
			wfcs.AbsenceRequestWaitlistProcessOrder = WaitlistProcessOrder.FirstComeFirstServed;
			agent.WorkflowControlSet = wfcs;
			waitListedAgent.WorkflowControlSet = wfcs;
			var periodWaitlisted = new DateTimePeriod(new DateTime(2016, 12, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2016, 12, 1, 8, 30, 0, DateTimeKind.Utc));
			var period = new DateTimePeriod(new DateTime(2016, 12, 1, 8, 30, 0, DateTimeKind.Utc), new DateTime(2016, 12, 1, 9, 0, 0, DateTimeKind.Utc));


			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(waitListedAgent, scenario, activity, periodWaitlisted, new ShiftCategory("category")));

			IPersonRequest waitListedRequest = new PersonRequest(waitListedAgent, new AbsenceRequest(absence, periodWaitlisted)).WithId();
			waitListedRequest.Deny("Work Hard!", new PersonRequestAuthorizationCheckerForTest());
			ReflectionHelper.SetCreatedOn(waitListedRequest, new DateTime(2016, 12, 1, 6, 30, 00, DateTimeKind.Utc));
			PersonRequestRepository.Add(waitListedRequest);

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(new DateTimePeriod(new DateTime( 2016, 12, 1, 8,0,0,DateTimeKind.Utc), new DateTime(2016, 12, 1, 9, 0, 0, DateTimeKind.Utc)), new[] {skill.Id.GetValueOrDefault()}, 5)
			});

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 5));

			IPersonRequest personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();
			ReflectionHelper.SetCreatedOn(personRequest, new DateTime(2016, 12, 1, 6, 00, 00, DateTimeKind.Utc));
			
			Target.Process(personRequest, Now.UtcDateTime());

			CommandDispatcher.AllComands.Count(x => x.GetType() == typeof(ApproveRequestCommand)).Should().Be.EqualTo(1);
			ApproveRequestCommand command = (ApproveRequestCommand)CommandDispatcher.AllComands.First(x => x.GetType() == typeof(ApproveRequestCommand));
			command.PersonRequestId.Should().Be.EqualTo(personRequest.Id);
		}

		[Test]
		public void ShouldApproveAndDenyBasedOnStaffingOnDifferentSkills()
		{
			Now.Is(new DateTime(2016, 12, 1, 6, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("phone");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = threshold;
			skill.DefaultResolution = 60;

			var skillb = SkillRepository.Has("skillB", activity).WithId();
			skillb.StaffingThresholds = threshold;
			skillb.DefaultResolution = 60;

			var agent = PersonRepository.Has(skill);
			var agentb = PersonRepository.Has(skillb);
			var waitListedAgent = PersonRepository.Has(skill);

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
			wfcs.AbsenceRequestWaitlistEnabled = true;
			agent.WorkflowControlSet = wfcs;
			agentb.WorkflowControlSet = wfcs;
			waitListedAgent.WorkflowControlSet = wfcs;
			var periodWaitlisted = new DateTimePeriod(new DateTime(2016, 12, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2016, 12, 1, 8, 30, 0, DateTimeKind.Utc));
			var period = new DateTimePeriod(new DateTime(2016, 12, 1, 8, 30, 0, DateTimeKind.Utc), new DateTime(2016, 12, 1, 9, 0, 0, DateTimeKind.Utc));


			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(waitListedAgent, scenario, activity, periodWaitlisted, new ShiftCategory("category")));
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agentb, scenario, activity, periodWaitlisted, new ShiftCategory("category")));

			IPersonRequest waitListedRequest = new PersonRequest(waitListedAgent, new AbsenceRequest(absence, periodWaitlisted)).WithId();
			waitListedRequest.Deny("Work Hard!", new PersonRequestAuthorizationCheckerForTest());
			PersonRequestRepository.Add(waitListedRequest);

			IPersonRequest agentBRequest = new PersonRequest(agentb, new AbsenceRequest(absence, periodWaitlisted)).WithId();
			agentBRequest.Deny("Work Hard!", new PersonRequestAuthorizationCheckerForTest());
			PersonRequestRepository.Add(agentBRequest);

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(new DateTimePeriod(new DateTime( 2016, 12, 1, 8,0,0,DateTimeKind.Utc), new DateTime(2016, 12, 1, 9, 0, 0, DateTimeKind.Utc)), new[] {skill.Id.GetValueOrDefault()}, 6),
				createSkillCombinationResource(new DateTimePeriod(new DateTime( 2016, 12, 1, 8,0,0,DateTimeKind.Utc), new DateTime(2016, 12, 1, 9, 0, 0, DateTimeKind.Utc)), new[] {skillb.Id.GetValueOrDefault()}, 3)
			});

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 5));
			SkillDayRepository.Has(skillb.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 5));

			IPersonRequest personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest, Now.UtcDateTime());

			CommandDispatcher.AllComands.Count(x => x.GetType() == typeof(ApproveRequestCommand)).Should().Be.EqualTo(2);
			CommandDispatcher.AllComands.Count(x => x.GetType() == typeof(DenyRequestCommand)).Should().Be.EqualTo(1);
			//ApproveRequestCommand command = (ApproveRequestCommand)CommandDispatcher.AllComands.First(x => x.GetType() == typeof(ApproveRequestCommand));
			//command.PersonRequestId.Should().Be.EqualTo(personRequest.Id);
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