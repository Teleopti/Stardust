using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common;
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
	public class RequestProcessorTest
	{
		public IRequestProcessor Target;
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
			
			IPersonRequest waitListedRequest = createPersonRequest(waitListedAgent, absence, period);
			waitListedRequest.Deny("Work Hard!", new PersonRequestAuthorizationCheckerForTest());
			PersonRequestRepository.Add(waitListedRequest);

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(period, new[] {skill.Id.GetValueOrDefault()}, 10)
			});

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 5));

			var personRequest = createPersonRequest(agentWithoutWaitlist, absence, period);
			Target.Process(personRequest);

			personRequest.IsApproved.Should().Be(true);
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

			var personRequest = createPersonRequest(agentWithNewRequest, absence, period);
			Target.Process(personRequest);

			personRequest.IsDenied.Should().Be(true);
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

			var personRequest = createPersonRequest(agentWithNewRequest, absence, period);
			Target.Process(personRequest);

			personRequest.IsApproved.Should().Be(true);
		}

		[SetCulture("en-US")]
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

			var skillCombinationResources = new List<SkillCombinationResource>();
			var resolution = skill.DefaultResolution;
			for (var dateTime = period.StartDateTime;
				dateTime.CompareTo(period.EndDateTime) <= 0;
				dateTime = dateTime.AddMinutes(resolution))
			{
				skillCombinationResources.Add(createSkillCombinationResource(new DateTimePeriod(dateTime, dateTime.AddMinutes(resolution)), new[] { skill.Id.GetValueOrDefault() }, 1));
			}
			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), skillCombinationResources.ToArray());

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(period.StartDateTime), 1));

			var personRequest = createPersonRequest(agent, absence, period);
			Target.Process(personRequest);

			personRequest.IsDenied.Should().Be(true);
			personRequest.DenyReason.Should().Be("Insufficient staffing for : 12/4/2017 8:00:00 AM - 12/4/2017 9:00:00 AM");
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

		private PersonRequest createPersonRequest(Person agent, IAbsence absence, DateTimePeriod period)
		{
			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();
			personRequest.Pending();
			PersonRequestRepository.Add(personRequest);
			return personRequest;
		}
	}
}