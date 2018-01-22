using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.OvertimeRequests
{
	[DomainTest]
	public class OvertimeRequestApprovalServiceTest : ISetup
	{
		public IOvertimeRequestUnderStaffingSkillProvider OvertimeRequestUnderStaffingSkillProvider;
		public IOvertimeRequestSkillProvider OvertimeRequestSkillProvider;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeSkillRepository SkillRepository;
		public FakeCommandDispatcher CommandDispatcher;
		public FakePersonRepository PersonRepository;
		public FakeActivityRepository ActivityRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeWorkflowControlSetRepository WorkflowControlSetRepository;
		public FakeMultiplicatorDefinitionSetRepository MultiplicatorDefinitionSetRepository;
		public MutableNow Now;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeCommandDispatcher>().For<ICommandDispatcher>();
		}

		[Test]
		public void ShouldAddActivityOfSkillWhenApproved()
		{
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skill", activity).WithId().DefaultResolution(60);
			var person = PersonRepository.Has(skill);

			var workflowControlSet = new WorkflowControlSet().WithId();
			workflowControlSet.AutoGrantOvertimeRequest = true;
			person.WorkflowControlSet = workflowControlSet;

			var assignmentPeriod = new DateTimePeriod(2018, 01, 01, 8, 2018, 01, 01, 9);
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity,
				assignmentPeriod, new ShiftCategory("category")));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = assignmentPeriod.StartDateTime,
					EndDateTime = assignmentPeriod.EndDateTime,
					Resource = 6,
					SkillCombination = new[] {skill.Id.GetValueOrDefault()}
				}
			});
			
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(assignmentPeriod.StartDateTime), 10));
			var requestPeriod = new DateTimePeriod(2018, 01, 01, 9, 2018, 01, 01, 10);

			var personRequest = createOvertimeRequest(person, requestPeriod);

			var target = new OvertimeRequestApprovalService(
				OvertimeRequestUnderStaffingSkillProvider, OvertimeRequestSkillProvider,
				CommandDispatcher, null, Now);

			var result = target.Approve(personRequest.Request);

			result.Count().Should().Be(0);
			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.ActivityId.Should().Be.EqualTo(activity.Id.GetValueOrDefault());
			addOvertimeActivityCommand?.Period.Should().Be.EqualTo(requestPeriod);
		}

		[Toggle(Toggles.Wfm_Requests_OvertimeRequestHandling_45177)]
		[Test]
		public void ShouldAddActivityOfSkillWhenAutoGrantoIsOnAndApproved()
		{
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skill", activity).WithId().DefaultResolution(60);
			var person = PersonRepository.Has(skill);

			var workflowControlSet = new WorkflowControlSet().WithId();
			workflowControlSet.AutoGrantOvertimeRequest = true;
			person.WorkflowControlSet = workflowControlSet;

			var assignmentPeriod = new DateTimePeriod(2018, 01, 01, 8, 2018, 01, 01, 9);
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity,
				assignmentPeriod, new ShiftCategory("category")));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = assignmentPeriod.StartDateTime,
					EndDateTime = assignmentPeriod.EndDateTime,
					Resource = 6,
					SkillCombination = new[] {skill.Id.GetValueOrDefault()}
				}
			});

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(assignmentPeriod.StartDateTime), 10));
			var requestPeriod = new DateTimePeriod(2018, 01, 01, 9, 2018, 01, 01, 10);

			var personRequest = createOvertimeRequest(person, requestPeriod);

			var target = new OvertimeRequestApprovalService(
				OvertimeRequestUnderStaffingSkillProvider, OvertimeRequestSkillProvider,
				CommandDispatcher, null,Now);

			var result = target.Approve(personRequest.Request);

			result.Count().Should().Be(0);
			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.ActivityId.Should().Be.EqualTo(activity.Id.GetValueOrDefault());
			addOvertimeActivityCommand?.Period.Should().Be.EqualTo(requestPeriod);
		}

		[Test]
		public void ShouldAddActivityOfTheFirstSkillWhenApproved()
		{
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var activity2 = ActivityRepository.Has("activity2");
			var skill = SkillRepository.Has("skill", activity).WithId().DefaultResolution(60);
			var skill2 = SkillRepository.Has("skill2", activity2).WithId().DefaultResolution(60);
			var person = PersonRepository.Has(skill, skill2);

			var workflowControlSet = new WorkflowControlSet().WithId();
			workflowControlSet.AutoGrantOvertimeRequest = true;
			person.WorkflowControlSet = workflowControlSet;

			var assignmentPeriod = new DateTimePeriod(2018, 01, 01, 8, 2018, 01, 01, 9);
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity,
				assignmentPeriod, new ShiftCategory("category")));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = assignmentPeriod.StartDateTime,
					EndDateTime = assignmentPeriod.EndDateTime,
					Resource = 6,
					SkillCombination = new[] {skill.Id.GetValueOrDefault()}
				},
				new SkillCombinationResource
				{
					StartDateTime = assignmentPeriod.StartDateTime,
					EndDateTime = assignmentPeriod.EndDateTime,
					Resource = 6,
					SkillCombination = new[] {skill2.Id.GetValueOrDefault()}
				}
			});

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(assignmentPeriod.StartDateTime), 10));
			SkillDayRepository.Has(skill2.CreateSkillDayWithDemand(scenario, new DateOnly(assignmentPeriod.StartDateTime), 10));

			var requestPeriod = new DateTimePeriod(2018, 01, 01, 9, 2018, 01, 01, 10);

			var personRequest = createOvertimeRequest(person, requestPeriod);

			var target = new OvertimeRequestApprovalService(
				OvertimeRequestUnderStaffingSkillProvider, OvertimeRequestSkillProvider,
				CommandDispatcher, null, Now);

			var result = target.Approve(personRequest.Request);

			result.Count().Should().Be(0);
			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.ActivityId.Should().Be.EqualTo(activity.Id.GetValueOrDefault());
			addOvertimeActivityCommand?.Period.Should().Be.EqualTo(requestPeriod);
		}

		[Toggle(Toggles.Wfm_Requests_OvertimeRequestHandling_45177)]
		[Test]
		public void ShouldAddActivityOfTheFirstSkillWhenAutoGrantoIsOnAndApproved()
		{
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var activity2 = ActivityRepository.Has("activity2");
			var skill = SkillRepository.Has("skill", activity).WithId().DefaultResolution(60);
			var skill2 = SkillRepository.Has("skill2", activity2).WithId().DefaultResolution(60);
			var person = PersonRepository.Has(skill, skill2);

			var workflowControlSet = new WorkflowControlSet().WithId();
			workflowControlSet.AutoGrantOvertimeRequest = true;
			person.WorkflowControlSet = workflowControlSet;

			var assignmentPeriod = new DateTimePeriod(2018, 01, 01, 8, 2018, 01, 01, 9);
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity,
				assignmentPeriod, new ShiftCategory("category")));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = assignmentPeriod.StartDateTime,
					EndDateTime = assignmentPeriod.EndDateTime,
					Resource = 6,
					SkillCombination = new[] {skill.Id.GetValueOrDefault()}
				},
				new SkillCombinationResource
				{
					StartDateTime = assignmentPeriod.StartDateTime,
					EndDateTime = assignmentPeriod.EndDateTime,
					Resource = 6,
					SkillCombination = new[] {skill2.Id.GetValueOrDefault()}
				}
			});

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(assignmentPeriod.StartDateTime), 10));
			SkillDayRepository.Has(skill2.CreateSkillDayWithDemand(scenario, new DateOnly(assignmentPeriod.StartDateTime), 10));

			var requestPeriod = new DateTimePeriod(2018, 01, 01, 9, 2018, 01, 01, 10);

			var personRequest = createOvertimeRequest(person, requestPeriod);

			var target = new OvertimeRequestApprovalService(
				OvertimeRequestUnderStaffingSkillProvider, OvertimeRequestSkillProvider,
				CommandDispatcher, null, Now);

			var result = target.Approve(personRequest.Request);

			result.Count().Should().Be(0);
			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.ActivityId.Should().Be.EqualTo(activity.Id.GetValueOrDefault());
			addOvertimeActivityCommand?.Period.Should().Be.EqualTo(requestPeriod);
		}
		
		[Test]  //This test makes no sence, there is only one activity and it is the same for both primary and secondary skill??
		public void ShouldAddActivityOfPrimarySkillWhenApproved()
		{
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var primarySkill = SkillRepository.Has("skill", activity).WithId().DefaultResolution(60).CascadingIndex(1);
			var secondarySkill = SkillRepository.Has("skill2", activity).WithId().DefaultResolution(60).CascadingIndex(2);
			var person = PersonRepository.Has(primarySkill, secondarySkill);

			var workflowControlSet = new WorkflowControlSet().WithId();
			workflowControlSet.AutoGrantOvertimeRequest = true;
			person.WorkflowControlSet = workflowControlSet;

			var assignmentPeriod = new DateTimePeriod(2018, 01, 01, 8, 2018, 01, 01, 9);
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity,
				assignmentPeriod, new ShiftCategory("category")));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = assignmentPeriod.StartDateTime,
					EndDateTime = assignmentPeriod.EndDateTime,
					Resource = 6,
					SkillCombination = new[] { primarySkill.Id.GetValueOrDefault(), secondarySkill.Id.GetValueOrDefault()}
				}
			});

			SkillDayRepository.Has(primarySkill.CreateSkillDayWithDemand(scenario, new DateOnly(assignmentPeriod.StartDateTime), 10));
			SkillDayRepository.Has(secondarySkill.CreateSkillDayWithDemand(scenario, new DateOnly(assignmentPeriod.StartDateTime), 10));

			var requestPeriod = new DateTimePeriod(2018, 01, 01, 9, 2018, 01, 01, 10);

			var personRequest = createOvertimeRequest(person, requestPeriod);

			var target = new OvertimeRequestApprovalService(
				OvertimeRequestUnderStaffingSkillProvider, OvertimeRequestSkillProvider,
				CommandDispatcher, null, Now);

			var result = target.Approve(personRequest.Request);

			result.Count().Should().Be(0);
			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.ActivityId.Should().Be.EqualTo(activity.Id.GetValueOrDefault());
			addOvertimeActivityCommand?.Period.Should().Be.EqualTo(requestPeriod);
		}

		[Toggle(Toggles.Wfm_Requests_OvertimeRequestHandling_45177)]
		[Test]  //This test makes no sence, there is only one activity and it is the same for both primary and secondary skill??
		public void ShouldAddActivityOfPrimarySkillWhennAutoGrantoIsOnAndApproved() 
		{
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var primarySkill = SkillRepository.Has("skill", activity).WithId().DefaultResolution(60).CascadingIndex(1);
			var secondarySkill = SkillRepository.Has("skill2", activity).WithId().DefaultResolution(60).CascadingIndex(2);
			var person = PersonRepository.Has(primarySkill, secondarySkill);

			var workflowControlSet = new WorkflowControlSet().WithId();
			workflowControlSet.AutoGrantOvertimeRequest = true;
			person.WorkflowControlSet = workflowControlSet;

			var assignmentPeriod = new DateTimePeriod(2018, 01, 01, 8, 2018, 01, 01, 9);
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity,
				assignmentPeriod, new ShiftCategory("category")));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = assignmentPeriod.StartDateTime,
					EndDateTime = assignmentPeriod.EndDateTime,
					Resource = 6,
					SkillCombination = new[] { primarySkill.Id.GetValueOrDefault(), secondarySkill.Id.GetValueOrDefault()}
				}
			});

			SkillDayRepository.Has(primarySkill.CreateSkillDayWithDemand(scenario, new DateOnly(assignmentPeriod.StartDateTime), 10));
			SkillDayRepository.Has(secondarySkill.CreateSkillDayWithDemand(scenario, new DateOnly(assignmentPeriod.StartDateTime), 10));

			var requestPeriod = new DateTimePeriod(2018, 01, 01, 9, 2018, 01, 01, 10);

			var personRequest = createOvertimeRequest(person, requestPeriod);

			var target = new OvertimeRequestApprovalService(
				OvertimeRequestUnderStaffingSkillProvider, OvertimeRequestSkillProvider,
				CommandDispatcher, null, Now);

			var result = target.Approve(personRequest.Request);

			result.Count().Should().Be(0);
			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.ActivityId.Should().Be.EqualTo(activity.Id.GetValueOrDefault());
			addOvertimeActivityCommand?.Period.Should().Be.EqualTo(requestPeriod);
		}

		[Toggle(Toggles.Wfm_Requests_OvertimeRequestHandling_45177)]
		[Test] 
		public void ShouldAddActivityOfPrimarySkillWhenAutoGrantoIsOnAndApproved_ThisIsProbablyWhatYourAreTryingToDo()
		{
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var activity2 = ActivityRepository.Has("activity2");
			var primarySkill = SkillRepository.Has("skill", activity).WithId().DefaultResolution(60).CascadingIndex(1);
			var secondarySkill = SkillRepository.Has("skill2", activity2).WithId().DefaultResolution(60).CascadingIndex(2);
			var person = PersonRepository.Has(primarySkill, secondarySkill);

			var workflowControlSet = new WorkflowControlSet().WithId();
			workflowControlSet.AutoGrantOvertimeRequest = true;
			person.WorkflowControlSet = workflowControlSet;

			var assignmentPeriod = new DateTimePeriod(2018, 01, 01, 8, 2018, 01, 01, 9);
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity2, //scheduled on the activity of the low priority skill
				assignmentPeriod, new ShiftCategory("category")));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = assignmentPeriod.StartDateTime,
					EndDateTime = assignmentPeriod.EndDateTime,
					Resource = 6,
					SkillCombination = new[] { primarySkill.Id.GetValueOrDefault()}
				},
				new SkillCombinationResource
				{
					StartDateTime = assignmentPeriod.StartDateTime,
					EndDateTime = assignmentPeriod.EndDateTime,
					Resource = 6,
					SkillCombination = new[] {secondarySkill.Id.GetValueOrDefault()}
				}
			});

			SkillDayRepository.Has(primarySkill.CreateSkillDayWithDemand(scenario, new DateOnly(assignmentPeriod.StartDateTime), 10));
			SkillDayRepository.Has(secondarySkill.CreateSkillDayWithDemand(scenario, new DateOnly(assignmentPeriod.StartDateTime), 10));

			var requestPeriod = new DateTimePeriod(2018, 01, 01, 9, 2018, 01, 01, 10);

			var personRequest = createOvertimeRequest(person, requestPeriod);

			var target = new OvertimeRequestApprovalService(
				OvertimeRequestUnderStaffingSkillProvider, OvertimeRequestSkillProvider,
				CommandDispatcher, null, Now);

			var result = target.Approve(personRequest.Request);

			result.Count().Should().Be(0);
			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.ActivityId.Should().Be.EqualTo(activity.Id.GetValueOrDefault());  //should get OT on the primary skill activity
			addOvertimeActivityCommand?.Period.Should().Be.EqualTo(requestPeriod);
		}

		[Test]
		public void ShouldAddActivityOfPrimarySkillWhenApproved_ThisIsProbablyWhatYourAreTryingToDo()
		{
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var activity2 = ActivityRepository.Has("activity2");
			var primarySkill = SkillRepository.Has("skill", activity).WithId().DefaultResolution(60).CascadingIndex(1);
			var secondarySkill = SkillRepository.Has("skill2", activity2).WithId().DefaultResolution(60).CascadingIndex(2);
			var person = PersonRepository.Has(primarySkill, secondarySkill);

			var workflowControlSet = new WorkflowControlSet().WithId();
			workflowControlSet.AutoGrantOvertimeRequest = true;
			person.WorkflowControlSet = workflowControlSet;

			var assignmentPeriod = new DateTimePeriod(2018, 01, 01, 8, 2018, 01, 01, 9);
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity2, //scheduled on the activity of the low priority skill
				assignmentPeriod, new ShiftCategory("category")));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = assignmentPeriod.StartDateTime,
					EndDateTime = assignmentPeriod.EndDateTime,
					Resource = 6,
					SkillCombination = new[] { primarySkill.Id.GetValueOrDefault()}
				},
				new SkillCombinationResource
				{
					StartDateTime = assignmentPeriod.StartDateTime,
					EndDateTime = assignmentPeriod.EndDateTime,
					Resource = 6,
					SkillCombination = new[] {secondarySkill.Id.GetValueOrDefault()}
				}
			});

			SkillDayRepository.Has(primarySkill.CreateSkillDayWithDemand(scenario, new DateOnly(assignmentPeriod.StartDateTime), 10));
			SkillDayRepository.Has(secondarySkill.CreateSkillDayWithDemand(scenario, new DateOnly(assignmentPeriod.StartDateTime), 10));

			var requestPeriod = new DateTimePeriod(2018, 01, 01, 9, 2018, 01, 01, 10);

			var personRequest = createOvertimeRequest(person, requestPeriod);

			var target = new OvertimeRequestApprovalService(
				OvertimeRequestUnderStaffingSkillProvider, OvertimeRequestSkillProvider,
				CommandDispatcher, null, Now);

			var result = target.Approve(personRequest.Request);

			result.Count().Should().Be(0);
			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.ActivityId.Should().Be.EqualTo(activity.Id.GetValueOrDefault());  //should get OT on the primary skill activity
			addOvertimeActivityCommand?.Period.Should().Be.EqualTo(requestPeriod);
		}

		[Test]
		public void ShouldApproveCrossDayRequest()
		{
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skill", activity).WithId().DefaultResolution(60);
			var person = PersonRepository.Has(skill);

			var workflowControlSet = new WorkflowControlSet().WithId();
			workflowControlSet.AutoGrantOvertimeRequest = true;
			person.WorkflowControlSet = workflowControlSet;

			var assignmentPeriod = new DateTimePeriod(2018, 01, 01, 22, 2018, 01, 01, 23);
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity,
				assignmentPeriod, new ShiftCategory("category")));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = assignmentPeriod.StartDateTime,
					EndDateTime = assignmentPeriod.EndDateTime,
					Resource = 6,
					SkillCombination = new[] {skill.Id.GetValueOrDefault()}
				}
			});

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(assignmentPeriod.StartDateTime), 10));
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(assignmentPeriod.StartDateTime.AddDays(1)), 10));
			var requestPeriod = new DateTimePeriod(2018, 01, 01, 23, 2018, 01, 02, 01);

			var personRequest = createOvertimeRequest(person, requestPeriod);

			var target = new OvertimeRequestApprovalService(
				OvertimeRequestUnderStaffingSkillProvider, OvertimeRequestSkillProvider,
				CommandDispatcher, null, Now);

			var result = target.Approve(personRequest.Request);

			result.Count().Should().Be(0);
			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.ActivityId.Should().Be.EqualTo(activity.Id.GetValueOrDefault());
			addOvertimeActivityCommand?.Period.Should().Be.EqualTo(requestPeriod);
		}

		[Test]
		public void ShouldApproveWhenScheduledAgentsIsZero()
		{
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skill", activity).WithId().DefaultResolution(60);
			var person = PersonRepository.Has(skill);

			var workflowControlSet = new WorkflowControlSet().WithId();
			workflowControlSet.AutoGrantOvertimeRequest = true;
			person.WorkflowControlSet = workflowControlSet;

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(2018, 01, 01), 10));
			var requestPeriod = new DateTimePeriod(2018, 01, 01, 9, 2018, 01, 01, 10);

			var personRequest = createOvertimeRequest(person, requestPeriod);

			var target = new OvertimeRequestApprovalService(
				OvertimeRequestUnderStaffingSkillProvider, OvertimeRequestSkillProvider,
				CommandDispatcher, null, Now);

			var result = target.Approve(personRequest.Request);

			result.Count().Should().Be(0);
			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.ActivityId.Should().Be.EqualTo(activity.Id.GetValueOrDefault());
			addOvertimeActivityCommand?.Period.Should().Be.EqualTo(requestPeriod);
		}

		[Test] //Looks like you mean agen't has no person period for the request period? Or at least no skills for that period
		public void ShouldNotApproveWhenAgentSkillIsOutOfPersonPeriod()  
		{
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skill", activity).WithId().DefaultResolution(60);

			var person = new Person().WithId();
			PersonRepository.Has(person);

			var workflowControlSet = new WorkflowControlSet().WithId();
			workflowControlSet.AutoGrantOvertimeRequest = true;
			person.WorkflowControlSet = workflowControlSet;

			var assignmentPeriod = new DateTimePeriod(2018, 01, 01, 8, 2018, 01, 01, 9);
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity,
				assignmentPeriod, new ShiftCategory("category")));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = assignmentPeriod.StartDateTime,
					EndDateTime = assignmentPeriod.EndDateTime,
					Resource = 6,
					SkillCombination = new[] {skill.Id.GetValueOrDefault()}
				}
			});

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(assignmentPeriod.StartDateTime), 10));
			var requestPeriod = new DateTimePeriod(2018, 01, 01, 9, 2018, 01, 01, 10);

			var personRequest = createOvertimeRequest(person, requestPeriod);

			var target = new OvertimeRequestApprovalService(
				OvertimeRequestUnderStaffingSkillProvider, OvertimeRequestSkillProvider,
				CommandDispatcher, null, Now);

			var result = target.Approve(personRequest.Request);

			result.Count().Should().Be(1);
			result.First().Message.Should().Be(Resources.NoAvailableSkillForOvertime);
		}

		[Test]
		public void ShouldAddActivityOnAgentTimezone()
		{
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skill", activity).WithId().DefaultResolution(60);
			var person = PersonRepository.Has(skill);
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"));  // better to not use WET since that is UTC+0..

			var workflowControlSet = new WorkflowControlSet().WithId();
			workflowControlSet.AutoGrantOvertimeRequest = true;
			person.WorkflowControlSet = workflowControlSet;

			var assignmentPeriod = new DateTimePeriod(2018, 01, 01, 8, 2018, 01, 01, 9);
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity,
				assignmentPeriod, new ShiftCategory("category")));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				new SkillCombinationResource
				{
					StartDateTime = assignmentPeriod.StartDateTime,
					EndDateTime = assignmentPeriod.EndDateTime,
					Resource = 6,
					SkillCombination = new[] {skill.Id.GetValueOrDefault()}
				}
			});

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(assignmentPeriod.StartDateTime), 10));
			var requestPeriod = new DateTimePeriod(2018, 01, 01, 9, 2018, 01, 01, 10);

			var personRequest = createOvertimeRequest(person, requestPeriod);

			var target = new OvertimeRequestApprovalService(
				OvertimeRequestUnderStaffingSkillProvider, OvertimeRequestSkillProvider,
				CommandDispatcher, null, Now);

			var result = target.Approve(personRequest.Request);

			result.Count().Should().Be(0);
			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.ActivityId.Should().Be.EqualTo(activity.Id.GetValueOrDefault());
			addOvertimeActivityCommand?.Period.Should().Be.EqualTo(requestPeriod);
		}

		private IPersonRequest createOvertimeRequest(IPerson person, DateTimePeriod period)
		{
			var personRequestFactory = new PersonRequestFactory();
			var multiplicatorDefinitionSet = new MultiplicatorDefinitionSet("name", MultiplicatorType.Overtime);
				MultiplicatorDefinitionSetRepository.Has(multiplicatorDefinitionSet);

			var personRequest = personRequestFactory.CreatePersonRequest(person);
			var overTimeRequest = new OvertimeRequest(multiplicatorDefinitionSet, period);
			personRequest.Request = overTimeRequest;
			return personRequest;
		}
	}
}
