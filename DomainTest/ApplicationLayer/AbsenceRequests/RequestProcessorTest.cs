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
		public void ShouldProcessRequestIfWaitlistedDisabled()
		{
			Now.Is(new DateTime(2016, 12, 1, 6, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("phone");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = threshold;
			skill.DefaultResolution = 60;
			var agentWithoutWaitlist = PersonRepository.Has(skill);
			var waitListedAgent = PersonRepository.Has(skill);
			var wfcsWithWaitlist = new WorkflowControlSet().WithId();
			var wfcsWithoutWaitlist = new WorkflowControlSet().WithId();
			wfcsWithWaitlist.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			
			});
			wfcsWithoutWaitlist.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()

			});
			wfcsWithWaitlist.AbsenceRequestWaitlistEnabled = true;
			wfcsWithoutWaitlist.AbsenceRequestWaitlistEnabled = false;
			agentWithoutWaitlist.WorkflowControlSet = wfcsWithoutWaitlist;
			waitListedAgent.WorkflowControlSet = wfcsWithWaitlist;

			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agentWithoutWaitlist, scenario, activity, period, new ShiftCategory("category")));
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(waitListedAgent, scenario, activity, period, new ShiftCategory("category")));
			
			IPersonRequest waitListedRequest =  new PersonRequest(waitListedAgent, new AbsenceRequest(absence, period)).WithId();
			waitListedRequest.Deny("Work Hard!", new PersonRequestAuthorizationCheckerForTest());
			PersonRequestRepository.Add(waitListedRequest);

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(period, new[] {skill.Id.GetValueOrDefault()}, 10)
			});

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 5));

			var personRequest = new PersonRequest(agentWithoutWaitlist, new AbsenceRequest(absence, period)).WithId();
			Target.Process(personRequest, Now.UtcDateTime());

			CommandDispatcher.AllComands.Count(x=>x.GetType()== typeof(ApproveRequestCommand)).Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldDenyIfWaitlistedEnabledAndWaitlistedRequest()
		{
			Now.Is(new DateTime(2016, 12, 1, 6, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			
			var activity = ActivityRepository.Has("phone");
			var skill = SkillRepository.Has("skillA", activity).WithId();
		
			var agentWithNewRequest = PersonRepository.Has(skill);

			var wfcsWithWaitlist = new WorkflowControlSet().WithId();
			wfcsWithWaitlist.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()

			});
			wfcsWithWaitlist.AbsenceRequestWaitlistEnabled = true;
			agentWithNewRequest.WorkflowControlSet = wfcsWithWaitlist;
			
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);

			PersonRequestRepository.HasWaitlisted = true;

			var personRequest = new PersonRequest(agentWithNewRequest, new AbsenceRequest(absence, period)).WithId();
			Target.Process(personRequest, Now.UtcDateTime());

			CommandDispatcher.AllComands.Count(x => x.GetType() == typeof(DenyRequestCommand)).Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotBotherAboutWaitlistedRequestThatAreOld()
		{
			Now.Is(new DateTime(2016, 12, 1, 6, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("phone");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = threshold;
			skill.DefaultResolution = 60;
			var agentWithNewRequest = PersonRepository.Has(skill);
			var waitListedAgent = PersonRepository.Has(skill);
			var wfcsWithWaitlist = new WorkflowControlSet().WithId();
			wfcsWithWaitlist.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()

			});
			wfcsWithWaitlist.AbsenceRequestWaitlistEnabled = true;
			agentWithNewRequest.WorkflowControlSet = wfcsWithWaitlist;
			waitListedAgent.WorkflowControlSet = wfcsWithWaitlist;

			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);
			var oldPeriod = new DateTimePeriod(2016, 12, 1, 5, 2016, 12, 1, 9);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agentWithNewRequest, scenario, activity, period, new ShiftCategory("category")));
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(waitListedAgent, scenario, activity, oldPeriod, new ShiftCategory("category")));

			IPersonRequest waitListedRequest = new PersonRequest(waitListedAgent, new AbsenceRequest(absence, oldPeriod)).WithId();
			waitListedRequest.Deny("Work Hard!", new PersonRequestAuthorizationCheckerForTest());
			PersonRequestRepository.Add(waitListedRequest);

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(period, new[] {skill.Id.GetValueOrDefault()}, 10)
			});

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 5));

			var personRequest = new PersonRequest(agentWithNewRequest, new AbsenceRequest(absence, period)).WithId();
			Target.Process(personRequest, Now.UtcDateTime());

			CommandDispatcher.AllComands.Count(x => x.GetType() == typeof(ApproveRequestCommand)).Should().Be.EqualTo(1);
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