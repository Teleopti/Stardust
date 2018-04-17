using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.Authentication;
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
		public FakeLoggedOnUser LoggedOnUser;
		public FakeSkillTypeRepository SkillTypeRepository;
		public FakeSkillCombinationResourceRepository CombinationRepository;
		public SkillIntradayStaffingFactory SkillIntradayStaffingFactory;
		public IRequestApprovalServiceFactory RequestApprovalServiceFactory;
		public ICurrentScenario Scenario;

		readonly ISkillType skillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony)
			.WithId();

		private readonly ISkillType _phoneSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony).WithId();
		private readonly ISkillType _emailSkillType = new SkillTypeEmail(new Description(SkillTypeIdentifier.Email), ForecastSource.Email).WithId();
		private readonly ISkillType _chatSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Chat), ForecastSource.Chat).WithId();
		private readonly TimePeriod _defaultOpenPeriod = new TimePeriod(8, 00, 21, 00);
		private readonly DateOnly _periodStartDate = new DateOnly(2016, 1, 1);

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<FakeCommandDispatcher>().For<ICommandDispatcher>();
			//system.UseTestDouble(new FakeSkillTypeRepository(skillType)).For<ISkillTypeRepository>();
			system.UseTestDouble(new FakeScenarioRepository(new Scenario("default") {DefaultScenario = true}))
				.For<IScenarioRepository>();
		}

		[Test]
		public void ShouldAddActivityOfSkillWhenApproved()
		{
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skill", activity).WithId().DefaultResolution(60);
			var person = createPerson(skill);

			person.WorkflowControlSet.AutoGrantOvertimeRequest = true;

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
			Now.Is(new DateTime(2018, 01, 01));
			var requestPeriod = new DateTimePeriod(2018, 01, 01, 9, 2018, 01, 01, 10);

			var personRequest = createOvertimeRequest(person, requestPeriod);

			var target = RequestApprovalServiceFactory.MakeOvertimeRequestApprovalService(null);

			var result = target.Approve(personRequest.Request);

			result.Count().Should().Be(0);
			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.ActivityId.Should().Be.EqualTo(activity.Id.GetValueOrDefault());
			addOvertimeActivityCommand?.Period.Should().Be.EqualTo(requestPeriod);
		}

		[Test]
		public void ShouldAddActivityOfSkillWhenAutoGrantoIsOnAndApproved()
		{
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skill", activity).WithId().DefaultResolution(60);
			var person = createPerson(skill);

			person.WorkflowControlSet.AutoGrantOvertimeRequest = true;

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
			Now.Is(new DateTime(2018, 01, 01));
			var requestPeriod = new DateTimePeriod(2018, 01, 01, 9, 2018, 01, 01, 10);

			var personRequest = createOvertimeRequest(person, requestPeriod);

			var target = RequestApprovalServiceFactory.MakeOvertimeRequestApprovalService(null);

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
			var person = createPerson(skill, skill2);

			person.WorkflowControlSet.AutoGrantOvertimeRequest = true;

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

			Now.Is(new DateTime(2018, 01, 01));
			var requestPeriod = new DateTimePeriod(2018, 01, 01, 9, 2018, 01, 01, 10);

			var personRequest = createOvertimeRequest(person, requestPeriod);

			var target = RequestApprovalServiceFactory.MakeOvertimeRequestApprovalService(null);

			var result = target.Approve(personRequest.Request);

			result.Count().Should().Be(0);
			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.ActivityId.Should().Be.EqualTo(activity.Id.GetValueOrDefault());
			addOvertimeActivityCommand?.Period.Should().Be.EqualTo(requestPeriod);
		}

		[Test]
		public void ShouldAddActivityOfTheFirstSkillWhenAutoGrantoIsOnAndApproved()
		{
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var activity2 = ActivityRepository.Has("activity2");
			var skill = SkillRepository.Has("skill", activity).WithId().DefaultResolution(60);
			var skill2 = SkillRepository.Has("skill2", activity2).WithId().DefaultResolution(60);
			var person = createPerson(skill, skill2);

			person.WorkflowControlSet.AutoGrantOvertimeRequest = true;

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

			Now.Is(new DateTime(2018, 01, 01));
			var requestPeriod = new DateTimePeriod(2018, 01, 01, 9, 2018, 01, 01, 10);

			var personRequest = createOvertimeRequest(person, requestPeriod);

			var target = RequestApprovalServiceFactory.MakeOvertimeRequestApprovalService(null);

			var result = target.Approve(personRequest.Request);

			result.Count().Should().Be(0);
			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.ActivityId.Should().Be.EqualTo(activity.Id.GetValueOrDefault());
			addOvertimeActivityCommand?.Period.Should().Be.EqualTo(requestPeriod);
		}
		
		[Test]
		public void ShouldAddActivityOfPrimarySkillWhenApproved()
		{
			var scenario = ScenarioRepository.Has("scenario");
			var primaryActivity = ActivityRepository.Has("activity");
			var secondaryActivity = ActivityRepository.Has("activity");
			var primarySkill = SkillRepository.Has("skill", primaryActivity).WithId().DefaultResolution(60).CascadingIndex(1);
			primarySkill.SkillType = skillType;
			var secondarySkill = SkillRepository.Has("skill2", secondaryActivity).WithId().DefaultResolution(60).CascadingIndex(2);
			secondarySkill.SkillType = skillType;
			var person = PersonRepository.Has(primarySkill, secondarySkill);
			person.AddPersonPeriod(
				PersonPeriodFactory.CreatePersonPeriodWithSkills(new DateOnly(2018, 01, 01), primarySkill, secondarySkill));

			var workflowControlSet = new WorkflowControlSet().WithId();
			workflowControlSet.AutoGrantOvertimeRequest = true;
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod()
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 13)
			});
			person.WorkflowControlSet = workflowControlSet;

			var assignmentPeriod = new DateTimePeriod(2018, 01, 01, 8, 2018, 01, 01, 9);
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, primaryActivity,
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

			Now.Is(new DateTime(2018, 01, 01));
			var requestPeriod = new DateTimePeriod(2018, 01, 01, 9, 2018, 01, 01, 10);

			var personRequest = createOvertimeRequest(person, requestPeriod);

			var target = RequestApprovalServiceFactory.MakeOvertimeRequestApprovalService(null);

			var result = target.Approve(personRequest.Request);

			result.Count().Should().Be(0);
			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.ActivityId.Should().Be.EqualTo(primaryActivity.Id.GetValueOrDefault());
			addOvertimeActivityCommand?.Period.Should().Be.EqualTo(requestPeriod);
		}

		[Test]  //This test makes no sence, there is only one activity and it is the same for both primary and secondary skill??
		public void ShouldAddActivityOfPrimarySkillWhennAutoGrantoIsOnAndApproved() 
		{
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var primarySkill = SkillRepository.Has("skill", activity).WithId().DefaultResolution(60).CascadingIndex(1);
			var secondarySkill = SkillRepository.Has("skill2", activity).WithId().DefaultResolution(60).CascadingIndex(2);
			var person = createPerson(primarySkill, secondarySkill);

			person.WorkflowControlSet.AutoGrantOvertimeRequest = true;

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

			Now.Is(new DateTime(2018, 01, 01));
			var requestPeriod = new DateTimePeriod(2018, 01, 01, 9, 2018, 01, 01, 10);

			var personRequest = createOvertimeRequest(person, requestPeriod);

			var target = RequestApprovalServiceFactory.MakeOvertimeRequestApprovalService(null);

			var result = target.Approve(personRequest.Request);

			result.Count().Should().Be(0);
			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.ActivityId.Should().Be.EqualTo(activity.Id.GetValueOrDefault());
			addOvertimeActivityCommand?.Period.Should().Be.EqualTo(requestPeriod);
		}

		[Test] 
		public void ShouldAddActivityOfPrimarySkillWhenAutoGrantoIsOnAndApproved_ThisIsProbablyWhatYourAreTryingToDo()
		{
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var activity2 = ActivityRepository.Has("activity2");
			var primarySkill = SkillRepository.Has("skill", activity).WithId().DefaultResolution(60).CascadingIndex(1);
			var secondarySkill = SkillRepository.Has("skill2", activity2).WithId().DefaultResolution(60).CascadingIndex(2);
			var person = createPerson(primarySkill, secondarySkill);

			person.WorkflowControlSet.AutoGrantOvertimeRequest = true;

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

			Now.Is(new DateTime(2018, 01, 01));
			var requestPeriod = new DateTimePeriod(2018, 01, 01, 9, 2018, 01, 01, 10);

			var personRequest = createOvertimeRequest(person, requestPeriod);

			var target = RequestApprovalServiceFactory.MakeOvertimeRequestApprovalService(null);

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
			var person = createPerson(primarySkill, secondarySkill);

			person.WorkflowControlSet.AutoGrantOvertimeRequest = true;

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

			Now.Is(new DateTime(2018, 1, 1));
			var requestPeriod = new DateTimePeriod(2018, 01, 01, 9, 2018, 01, 01, 10);

			var personRequest = createOvertimeRequest(person, requestPeriod);

			var target = RequestApprovalServiceFactory.MakeOvertimeRequestApprovalService(null);

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
			var person = createPerson(skill);

			person.WorkflowControlSet.AutoGrantOvertimeRequest = true;

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

			Now.Is(new DateTime(2018, 01, 01));
			var requestPeriod = new DateTimePeriod(2018, 01, 01, 23, 2018, 01, 02, 01);

			var personRequest = createOvertimeRequest(person, requestPeriod);

			var target = RequestApprovalServiceFactory.MakeOvertimeRequestApprovalService(null);

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
			var person = createPerson(skill);

			person.WorkflowControlSet.AutoGrantOvertimeRequest = true;

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(2018, 01, 01), 10));

			Now.Is(new DateTime(2018, 01, 01));
			var requestPeriod = new DateTimePeriod(2018, 01, 01, 9, 2018, 01, 01, 10);

			var personRequest = createOvertimeRequest(person, requestPeriod);

			var target = RequestApprovalServiceFactory.MakeOvertimeRequestApprovalService(null);

			var result = target.Approve(personRequest.Request);

			result.Count().Should().Be(0);
			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.ActivityId.Should().Be.EqualTo(activity.Id.GetValueOrDefault());
			addOvertimeActivityCommand?.Period.Should().Be.EqualTo(requestPeriod);
		}

		[Test]
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

			var target = RequestApprovalServiceFactory.MakeOvertimeRequestApprovalService(null);

			var result = target.Approve(personRequest.Request);

			result.Count().Should().Be(1);
			result.First().Message.Should().Be(Resources.ThereIsNoAvailableSkillForOvertime);
		}

		[Test]
		public void ShouldAddActivityOnAgentTimezone()
		{
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skill", activity).WithId().DefaultResolution(60);
			var person = createPerson(skill);
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"));  // better to not use WET since that is UTC+0..

			person.WorkflowControlSet.AutoGrantOvertimeRequest = true;

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

			Now.Is(new DateTime(2018, 01, 01));
			var requestPeriod = new DateTimePeriod(2018, 01, 01, 9, 2018, 01, 01, 10);

			var personRequest = createOvertimeRequest(person, requestPeriod);

			var target = RequestApprovalServiceFactory.MakeOvertimeRequestApprovalService(null);

			var result = target.Approve(personRequest.Request);

			result.Count().Should().Be(0);
			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.ActivityId.Should().Be.EqualTo(activity.Id.GetValueOrDefault());
			addOvertimeActivityCommand?.Period.Should().Be.EqualTo(requestPeriod);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestUseMostUnderStaffedSkill_47853)]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestAtLeastOneCriticalUnderStaffedSkill_74944)]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestSupportMultiSelectionSkillTypes_74945)]
		public void ShouldApprovedAndAssginActivityCorrectlyBaseOnSkillsWhenMultipleSkillTypesSelectionIsEnabledInOvertimeRequestOpenPeriodSetting()
		{
			Now.Is(new DateTime(2018, 01, 01));
			var date = new DateOnly(Now.UtcDateTime());

			setupPerson(0, 24);

			var phoneActivity = createActivity("phone activity");
			var emailActivity = createActivity("email activity");
			var chatActivity = createActivity("chat activity");

			var timeZone = TimeZoneInfoFactory.UtcTimeZoneInfo();

			var partlyUnderStaffingSkillPhone = createSkill("partlyCriticalUnderStaffingSkillPhone", null, timeZone);
			partlyUnderStaffingSkillPhone.SkillType = _phoneSkillType;

			var notUnderStaffingSkillEmail = createSkill("notCriticalUnderStaffingSkillEmail", null, timeZone);
			notUnderStaffingSkillEmail.SkillType = _emailSkillType;
			notUnderStaffingSkillEmail.DefaultResolution = 60;

			var underStaffingSkillChat = createSkill("criticalUnderStaffingSkillChat", null, timeZone);
			underStaffingSkillChat.SkillType = _chatSkillType;

			var personSkillPhone = createPersonSkill(phoneActivity, partlyUnderStaffingSkillPhone);
			var personSkillEmail = createPersonSkill(emailActivity, notUnderStaffingSkillEmail);
			var personSkillChat = createPersonSkill(chatActivity, underStaffingSkillChat);

			addPersonSkillsToPersonPeriod(personSkillPhone, personSkillEmail, personSkillChat);

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new []{ _emailSkillType, _phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 7),
				OrderIndex = 1
			});

			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { _emailSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Deny,
				BetweenDays = new MinMax<int>(0, 5),
				OrderIndex = 2
			});

			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { _chatSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				BetweenDays = new MinMax<int>(0, 5),
				OrderIndex = 3
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var periodStartDateTime = new DateTime(2018, 01, 01, 08, 0, 0, 0, DateTimeKind.Utc);
			var intervalInMinutes = 15;

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(partlyUnderStaffingSkillPhone, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData { ForecastedStaffing = 10d, ScheduledStaffing = 20d, Period = new DateTimePeriod(periodStartDateTime, periodStartDateTime.AddMinutes(intervalInMinutes))},
				new StaffingPeriodData { ForecastedStaffing = 10d, ScheduledStaffing = 2d, Period = new DateTimePeriod(periodStartDateTime.AddMinutes(intervalInMinutes), periodStartDateTime.AddMinutes(intervalInMinutes * 2))},
				new StaffingPeriodData { ForecastedStaffing = 10d, ScheduledStaffing = 2d, Period = new DateTimePeriod(periodStartDateTime.AddMinutes(intervalInMinutes * 2), periodStartDateTime.AddMinutes(intervalInMinutes * 3))},
				new StaffingPeriodData { ForecastedStaffing = 10d, ScheduledStaffing = 20d, Period = new DateTimePeriod(periodStartDateTime.AddMinutes(intervalInMinutes * 3), periodStartDateTime.AddMinutes(intervalInMinutes * 4))},
			}, timeZone);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(notUnderStaffingSkillEmail, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData
				{
					ForecastedStaffing = 10d,
					ScheduledStaffing = 20d,
					Period = new DateTimePeriod(periodStartDateTime, periodStartDateTime.AddMinutes(intervalInMinutes * 4))
				}
			}, timeZone);

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(underStaffingSkillChat, date, new List<StaffingPeriodData>{
				new StaffingPeriodData { ForecastedStaffing = 10d, ScheduledStaffing = 1d, Period = new DateTimePeriod(periodStartDateTime, periodStartDateTime.AddMinutes(intervalInMinutes))},
				new StaffingPeriodData { ForecastedStaffing = 10d, ScheduledStaffing = 1d, Period = new DateTimePeriod(periodStartDateTime.AddMinutes(intervalInMinutes), periodStartDateTime.AddMinutes(intervalInMinutes * 2))},
				new StaffingPeriodData { ForecastedStaffing = 10d, ScheduledStaffing = 1d, Period = new DateTimePeriod(periodStartDateTime.AddMinutes(intervalInMinutes * 2), periodStartDateTime.AddMinutes(intervalInMinutes * 3))},
				new StaffingPeriodData { ForecastedStaffing = 10d, ScheduledStaffing = 1d, Period = new DateTimePeriod(periodStartDateTime.AddMinutes(intervalInMinutes * 3), periodStartDateTime.AddMinutes(intervalInMinutes * 4))},
			}, timeZone);

			var requestPeriod = new DateTimePeriod(2018, 01, 01, 8, 2018, 01, 01, 9);

			var personRequest = createOvertimeRequest(LoggedOnUser.CurrentUser(), requestPeriod);

			var target = RequestApprovalServiceFactory.MakeOvertimeRequestApprovalService(null);

			var result = target.Approve(personRequest.Request);

			result.Count().Should().Be(0);
			var addOvertimeActivityCommand = CommandDispatcher.LatestCommand as AddOvertimeActivityCommand;
			addOvertimeActivityCommand.Should().Not.Be.Null();
			addOvertimeActivityCommand?.ActivityId.Should().Be.EqualTo(chatActivity.Id.GetValueOrDefault());
			addOvertimeActivityCommand?.Period.Should().Be.EqualTo(requestPeriod);
		}

		private IPerson createPerson(params ISkill[] skills)
		{
			foreach (var skill in skills)
			{
				skill.SkillType = skillType;
			}

			var workflowControlSet = new WorkflowControlSet().WithId();

			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod()
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 13)
			});

			var person = PersonRepository.Has(skills);
			person.WorkflowControlSet = workflowControlSet;
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriodWithSkills(new DateOnly(2014, 1, 1), skills));

			return person;
		}

		private void setupPerson(int siteOpenStartHour = 8, int siteOpenEndHour = 17, bool isOpenHoursClosed = false)
		{
			var person = createPersonWithSiteOpenHours(siteOpenStartHour, siteOpenEndHour, isOpenHoursClosed);
			person.PermissionInformation.SetUICulture(CultureInfoFactory.CreateUsCulture());
			person.PermissionInformation.SetCulture(CultureInfoFactory.CreateUsCulture());
			LoggedOnUser.SetFakeLoggedOnUser(person);
			LoggedOnUser.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { _phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 30)
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			SkillTypeRepository.Add(_phoneSkillType);
		}

		private IPerson createPersonWithSiteOpenHours(int startHour, int endHour, bool isOpenHoursClosed = false)
		{
			var team = TeamFactory.CreateTeam("team", "site");
			var siteOpenHour = new SiteOpenHour
			{
				Parent = team.Site,
				TimePeriod = new TimePeriod(startHour, 0, endHour, 0),
				WeekDay = DayOfWeek.Monday,
				IsClosed = isOpenHoursClosed
			};
			team.Site.AddOpenHour(siteOpenHour);
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(_periodStartDate, team);
			return person;
		}

		private IPersonSkill createPersonSkill(IActivity activity, ISkill skill)
		{
			skill.Activity = activity;
			var personSkill = PersonSkillFactory.CreatePersonSkill(skill, 1);
			return personSkill;
		}

		private void addPersonSkillsToPersonPeriod(params IPersonSkill[] personSkills)
		{
			var personPeriod = getOrAddPersonPeriod(Now.ServerDate_DontUse());
			foreach (var personSkill in personSkills)
			{
				personPeriod.AddPersonSkill(personSkill);
			}
		}

		private PersonPeriod getOrAddPersonPeriod(DateOnly startDate)
		{
			var personPeriod = (PersonPeriod)LoggedOnUser.CurrentUser().PersonPeriods(startDate.ToDateOnlyPeriod()).FirstOrDefault();
			if (personPeriod != null) return personPeriod;
			var team = TeamFactory.CreateTeam("team1", "site1");
			personPeriod =
				(PersonPeriod)
				PersonPeriodFactory.CreatePersonPeriod(startDate, PersonContractFactory.CreatePersonContract(), team);
			LoggedOnUser.CurrentUser().AddPersonPeriod(personPeriod);
			return personPeriod;
		}

		private IActivity createActivity(string name)
		{
			var activity = ActivityFactory.CreateActivity(name);
			activity.RequiresSkill = true;
			activity.InWorkTime = true;
			ActivityRepository.Add(activity);
			return activity;
		}

		private ISkill createSkill(string name, TimePeriod? skillOpenHourPeriod = null, TimeZoneInfo timeZone = null, ISkillType skillType = null)
		{
			var skill = SkillFactory.CreateSkill(name, timeZone).WithId();
			skill.SkillType = skillType ?? _phoneSkillType;

			skill.StaffingThresholds = createStaffingThresholds();
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, skillOpenHourPeriod ?? _defaultOpenPeriod);
			SkillRepository.Has(skill);
			return skill;
		}

		private StaffingThresholds createStaffingThresholds()
		{
			return new StaffingThresholds(new Percent(-0.3), new Percent(-0.1), new Percent(0.1));
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
