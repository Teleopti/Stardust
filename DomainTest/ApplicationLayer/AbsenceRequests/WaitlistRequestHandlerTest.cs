using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[DomainTest]
	[Toggle(Toggles.Wfm_Requests_ProcessWaitlistBefore24hRequests_45767)]
	public class WaitlistRequestHandlerTest : ISetup
	{
		public WaitlistRequestHandler Target;
		public FakePersonRepository PersonRepository;
		public FakeSkillRepository SkillRepository;
		public FakeActivityRepository ActivityRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeQueuedAbsenceRequestRepository QueuedAbsenceRequestRepository;
		public IScheduleStorage ScheduleStorage;
		public MutableNow Now;
		public FakePersonRequestRepository PersonRequestRepository;
		public FakeTenants Tenants;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		private IBusinessUnit businessUnit;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<SchedulerStateScheduleDayChangedCallback>().For<IScheduleDayChangeCallback>();
		}

		public void SetUp()
		{
			Tenants.Has("Teleopti WFM");
			var person = PersonFactory.CreatePerson().WithId(SystemUser.Id);
			PersonRepository.Add(person);
			businessUnit = BusinessUnitFactory.CreateWithId("something");
			BusinessUnitRepository.Add(businessUnit);
		}

		[Test]
		public void ShouldNotApproveIfAlreadyAbsent()
		{
			SetUp();
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

			var personAbsence = new PersonAbsence(agent, scenario, new AbsenceLayer(absence, period));
			PersonAbsenceRepository.Add(personAbsence);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(waitListedAgent, scenario, activity, period, new ShiftCategory("category")));
			

			IPersonRequest waitListedRequest = new PersonRequest(waitListedAgent, new AbsenceRequest(absence, period)).WithId();
			waitListedRequest.Deny("Work Hard!", new PersonRequestAuthorizationCheckerForTest());
			PersonRequestRepository.Add(waitListedRequest);

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(period, new[] {skill.Id.GetValueOrDefault()}, 10)
			});

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 5));

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();
			personRequest.Deny("Denied!", new PersonRequestAuthorizationCheckerForTest());
			PersonRequestRepository.Add(personRequest);
			Target.Handle(new ProcessWaitlistedRequestsEvent { LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault(), LogOnDatasource = "Teleopti WFM" });

			waitListedRequest.IsApproved.Should().Be.True();
			personRequest.IsDenied.Should().Be.True();
		}

		[Test]
		public void ShouldProcessWaitlistedAndApproveRequest()
		{
			SetUp();
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
			personRequest.Deny("Denied!", new PersonRequestAuthorizationCheckerForTest());
			PersonRequestRepository.Add(personRequest);
			Target.Handle(new ProcessWaitlistedRequestsEvent{LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault(), LogOnDatasource = "Teleopti WFM"});

			waitListedRequest.IsApproved.Should().Be.True();
			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldProcessWaitlistedAndDenyRequest()
		{
			SetUp();
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
			personRequest.Deny("Denied!", new PersonRequestAuthorizationCheckerForTest());
			PersonRequestRepository.Add(personRequest);
			Target.Handle(new ProcessWaitlistedRequestsEvent { LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault(), LogOnDatasource = "Teleopti WFM" });

			waitListedRequest.IsApproved.Should().Be.False();
			personRequest.IsApproved.Should().Be.False();
		}

		[Test]
		public void ShouldApproveRequestBasedOnSeniority()
		{
			SetUp();
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
				createSkillCombinationResource(period, new[] {skill.Id.GetValueOrDefault()}, 6)
			});

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 5));

			IPersonRequest personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();
			personRequest.Deny("Denied!", new PersonRequestAuthorizationCheckerForTest());
			PersonRequestRepository.Add(personRequest);
			Target.Handle(new ProcessWaitlistedRequestsEvent { LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault(), LogOnDatasource = "Teleopti WFM" });

			personRequest.IsApproved.Should().Be.True();
			waitListedRequest.IsApproved.Should().Be.False();
		}

		[Test]
		public void ShouldApproveManyWaitlistedRequests()
		{
			SetUp();
			Now.Is(new DateTime(2016, 12, 1, 6, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("phone");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = threshold;
			skill.DefaultResolution = 60;

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
				createSkillCombinationResource(period, new[] {skill.Id.GetValueOrDefault()}, 13)
			});

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 10));

			IPersonRequest personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();
			personRequest.Deny("Work harder!", new PersonRequestAuthorizationCheckerForTest());
			PersonRequestRepository.Add(personRequest);

			Target.Handle(new ProcessWaitlistedRequestsEvent { LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault(), LogOnDatasource = "Teleopti WFM" });

			waitListedRequest1.IsApproved.Should().Be(true);
			waitListedRequest2.IsApproved.Should().Be(true);
			waitListedRequest3.IsApproved.Should().Be(true);
			waitListedRequest4.IsApproved.Should().Be(false);
			personRequest.IsApproved.Should().Be(false);
		}

		[Test]
		public void ShouldApproveRequestBasedOnFirstComeFirstServe()
		{
			SetUp();
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
			personRequest.Deny("Denied!", new PersonRequestAuthorizationCheckerForTest());
			PersonRequestRepository.Add(personRequest);

			Target.Handle(new ProcessWaitlistedRequestsEvent { LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault(), LogOnDatasource = "Teleopti WFM" });

			personRequest.IsApproved.Should().Be.True();
			waitListedRequest.IsApproved.Should().Be.False();
		}

		[Test]
		public void ShouldApproveAndDenyBasedOnStaffingOnDifferentSkills()
		{
			SetUp();
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

			IPersonRequest personRequest = new PersonRequest(agent, new AbsenceRequest(absence, periodWaitlisted)).WithId();
			personRequest.Deny("Work Hard!", new PersonRequestAuthorizationCheckerForTest());
			PersonRequestRepository.Add(personRequest);

			Target.Handle(new ProcessWaitlistedRequestsEvent { LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault(), LogOnDatasource = "Teleopti WFM" });

			waitListedRequest.IsApproved.Should().Be.True();
			agentBRequest.IsApproved.Should().Be.False();
			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldRollbackIfNotApproved()
		{
			SetUp();
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
			var period1 = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);
			var period2 = new DateTimePeriod(2016, 12, 1, 9, 2016, 12, 1, 10);
			var requestPeriod = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 10);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, requestPeriod, new ShiftCategory("category")));
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(waitListedAgent, scenario, activity, requestPeriod, new ShiftCategory("category")));

			IPersonRequest waitListedRequest = new PersonRequest(waitListedAgent, new AbsenceRequest(absence, requestPeriod)).WithId();
			waitListedRequest.Deny("Work Hard!", new PersonRequestAuthorizationCheckerForTest());
			PersonRequestRepository.Add(waitListedRequest);

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(period1, new[] {skill.Id.GetValueOrDefault()}, 5),
				createSkillCombinationResource(period2, new[] {skill.Id.GetValueOrDefault()}, 6)
			});

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(period1.StartDateTime), 5));

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period2)).WithId();
			personRequest.Deny("Denied!", new PersonRequestAuthorizationCheckerForTest());
			PersonRequestRepository.Add(personRequest);
			Target.Handle(new ProcessWaitlistedRequestsEvent { LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault(), LogOnDatasource = "Teleopti WFM" });

			waitListedRequest.IsApproved.Should().Be.False();
			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldHandleShrinkage()
		{
			SetUp();
			Now.Is(new DateTime(2016, 12, 1, 6, 00, 00, DateTimeKind.Utc));
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("phone");

			var absenceWithoutShrinkage = AbsenceFactory.CreateAbsence("HolidayWOS");
			var absenceWithShrinkage = AbsenceFactory.CreateAbsence("HolidayWS");
			var skill = SkillRepository.Has("skillA", activity).WithId();

			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = threshold;
			skill.DefaultResolution = 60;

			var agentWithoutShrinkage = PersonRepository.Has(skill);
			var agentWithShrinkage = PersonRepository.Has(skill);
			var wfcs = new WorkflowControlSet().WithId();
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = absenceWithoutShrinkage,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()

			});
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = absenceWithShrinkage,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdWithShrinkageValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()

			});

			wfcs.AbsenceRequestWaitlistEnabled = true;
			agentWithoutShrinkage.WorkflowControlSet = wfcs;
			agentWithShrinkage.WorkflowControlSet = wfcs;
			var period1 = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);
			var period2 = new DateTimePeriod(2016, 12, 1, 9, 2016, 12, 1, 10);
			var requestPeriod = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 10);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agentWithoutShrinkage, scenario, activity, requestPeriod, new ShiftCategory("category")));
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agentWithShrinkage, scenario, activity, requestPeriod, new ShiftCategory("category")));

			IPersonRequest withoutShrinkageReq = new PersonRequest(agentWithoutShrinkage, new AbsenceRequest(absenceWithoutShrinkage, requestPeriod)).WithId();
			withoutShrinkageReq.Deny("Work Hard!", new PersonRequestAuthorizationCheckerForTest());
			PersonRequestRepository.Add(withoutShrinkageReq);

			IPersonRequest withShrinkageReq = new PersonRequest(agentWithShrinkage, new AbsenceRequest(absenceWithShrinkage, requestPeriod)).WithId();
			withShrinkageReq.Deny("Work Hard!", new PersonRequestAuthorizationCheckerForTest());
			PersonRequestRepository.Add(withShrinkageReq);

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(period1, new[] {skill.Id.GetValueOrDefault()}, 8),
				createSkillCombinationResource(period2, new[] {skill.Id.GetValueOrDefault()}, 8)
			});

			var skillDay = skill.CreateSkillDayWithDemand(scenario, new DateOnly(period1.StartDateTime), 5);
			skillDay.SkillDataPeriodCollection.ForEach(x => x.Shrinkage = new Percent(0.5));
			SkillDayRepository.Has(skillDay);

			Target.Handle(new ProcessWaitlistedRequestsEvent { LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault(), LogOnDatasource = "Teleopti WFM" });

			withoutShrinkageReq.IsApproved.Should().Be.True();
			withShrinkageReq.IsWaitlisted.Should().Be.True();
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