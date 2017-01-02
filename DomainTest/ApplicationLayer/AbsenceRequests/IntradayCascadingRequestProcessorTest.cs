using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[DomainTest]
	[TestFixture]
	public class IntradayCascadingRequestProcessorTest : ISetup
	{
		public IntradayCascadingRequestProcessor Target;
		public FakePersonRepository PersonRepository;
		public FakeSkillRepository SkillRepository;
		public FakeActivityRepository ActivityRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public FakeScheduleForecastSkillReadModelRepository ScheduleForecastSkillReadModelRepository;
		public FakeCommandDispatcher CommandDispatcher;
		public IScheduleStorage ScheduleStorage;
		public MutableNow Now;


		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeCommandDispatcher>().For<ICommandDispatcher>();
			system.UseTestDouble<IntradayCascadingRequestProcessor>().For<IIntradayRequestProcessor>();
			system.UseTestDouble<ScheduleForecastSkillReadModelValidator>().For<IScheduleForecastSkillReadModelValidator>();
		}

		[Test]
		public void RobinsTest()
		{
			Assert.Pass();
		}

		
		[Test]
		public void ShouldApproveRequestIfEnoughResourcesOnSkill()
		{
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = threshold;
			var agent = PersonRepository.Has(skill);
			var wfcs = new WorkflowControlSet().WithId();
			Now.Is(new DateTime(2016, 12, 1, 7, 0, 0));
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2020, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2020, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});
			agent.WorkflowControlSet = wfcs;
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9); 
			
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, agent, period, new ShiftCategory("category"), scenario));
			
			SkillCombinationResourceRepository.PersistSkillCombinationResource(new [] {new SkillCombinationResource
																			   {
																				   StartDateTime = period.StartDateTime,
																				   EndDateTime = period.EndDateTime,
																				   Resource = 10,
																				   SkillCombination = new []{skill.Id.GetValueOrDefault()}
																			   } });

			ScheduleForecastSkillReadModelRepository.Persist(new []{ new SkillStaffingInterval
															 {
																 StartDateTime = period.StartDateTime,
																 EndDateTime = period.EndDateTime,
																 Forecast = 5,
																 SkillId = skill.Id.GetValueOrDefault()
															 } }, DateTime.Now);
			
			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest, period.StartDateTime);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));
		}


		[Test]
		public void ShouldApproveRequestIfEnoughResourcesOnSkills()
		{
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var skill2 = SkillRepository.Has("skillB", activity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = threshold;
			skill2.StaffingThresholds = threshold;
			Now.Is(new DateTime(2016, 12, 1, 7, 0, 0));

			var agent = PersonRepository.Has(skill, skill2);
			var wfcs = new WorkflowControlSet().WithId();
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2020, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2020, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});
			agent.WorkflowControlSet = wfcs;
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);
			
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, agent, period, new ShiftCategory("category"), scenario));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(new[] {new SkillCombinationResource
																			   {
																				   StartDateTime = period.StartDateTime,
																				   EndDateTime = period.EndDateTime,
																				   Resource = 10,
																				   SkillCombination = new []{skill.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()}
																			   } });

			ScheduleForecastSkillReadModelRepository.Persist(new[]
															 {
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.EndDateTime,
																	 Forecast = 4,
																	 SkillId = skill.Id.GetValueOrDefault()
																 },
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.EndDateTime,
																	 Forecast = 4,
																	 SkillId = skill2.Id.GetValueOrDefault()
																 }
															 }, DateTime.Now);
			
			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest, period.StartDateTime);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));
		}


		[Test]
		public void ShouldDenyRequestIfUnderStaffedOnSkill()
		{
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = threshold;
			Now.Is(new DateTime(2016, 12, 1, 7, 0, 0));

			var agent = PersonRepository.Has(skill);
			var wfcs = new WorkflowControlSet().WithId();
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2020, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2020, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});
			agent.WorkflowControlSet = wfcs;
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);
			
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, agent, period, new ShiftCategory("category"), scenario));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(new[] {new SkillCombinationResource
																			   {
																				   StartDateTime = period.StartDateTime,
																				   EndDateTime = period.EndDateTime,
																				   Resource = 4,
																				   SkillCombination = new []{skill.Id.GetValueOrDefault()}
																			   } });

			ScheduleForecastSkillReadModelRepository.Persist(new[]
															 {
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.EndDateTime,
																	 Forecast = 5,
																	 SkillId = skill.Id.GetValueOrDefault()
																 }
															 }, DateTime.Now);
			
			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest, period.StartDateTime);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(DenyRequestCommand));
		}

		[Test]
		public void ShouldDenyRequestIfRequestCausesUnderStaffedOnSkill()
		{
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var skill2 = SkillRepository.Has("skillB", activity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = threshold;
			skill2.StaffingThresholds = threshold;
			Now.Is(new DateTime(2016, 12, 1, 7, 0, 0));

			var agent = PersonRepository.Has(skill, skill2);
			var wfcs = new WorkflowControlSet().WithId();
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2020, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2020, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});
			agent.WorkflowControlSet = wfcs;
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);
			
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, agent, period, new ShiftCategory("category"), scenario));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(new[] {new SkillCombinationResource
																			   {
																				   StartDateTime = period.StartDateTime,
																				   EndDateTime = period.EndDateTime,
																				   Resource = 10,
																				   SkillCombination = new []{skill.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()}
																			   } });

			ScheduleForecastSkillReadModelRepository.Persist(new[]
															 {
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.EndDateTime,
																	 Forecast = 4.6,
																	 SkillId = skill.Id.GetValueOrDefault()
																 },
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.EndDateTime,
																	 Forecast = 4.6,
																	 SkillId = skill2.Id.GetValueOrDefault()
																 }
															 }, DateTime.Now);
			
			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest, period.StartDateTime);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(DenyRequestCommand));
		}


		[Test]
		public void ShouldApproveIfEnoughStaffingForBothActivities()
		{
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var activity2 = ActivityRepository.Has("activity2");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var skill2 = SkillRepository.Has("skillB", activity).WithId();
			var skill3 = SkillRepository.Has("skillC", activity2).WithId();
			var skill4 = SkillRepository.Has("skillD", activity2).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = threshold;
			skill2.StaffingThresholds = threshold;
			skill3.StaffingThresholds = threshold;
			skill4.StaffingThresholds = threshold;
			Now.Is(new DateTime(2016, 12, 1, 7, 0, 0));

			var agent = PersonRepository.Has(skill, skill2, skill3, skill4);
			var wfcs = new WorkflowControlSet().WithId();
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2020, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2020, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});
			agent.WorkflowControlSet = wfcs;
			var period1 = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);
			var period2 = new DateTimePeriod(2016, 12, 1, 9, 2016, 12, 1, 10);

			var ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, agent, period1, new ShiftCategory("category"), scenario);
			ass.AddActivity(activity2, period2);
			PersonAssignmentRepository.Has(ass);
			
			SkillCombinationResourceRepository.PersistSkillCombinationResource(new[]
																			   {
																				   new SkillCombinationResource
																				   {
																					   StartDateTime = period1.StartDateTime,
																					   EndDateTime = period1.EndDateTime,
																					   Resource = 10,
																					   SkillCombination = new[] {skill.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()}
																				   },
																					new SkillCombinationResource
																				   {
																					   StartDateTime = period2.StartDateTime,
																					   EndDateTime = period2.EndDateTime,
																					   Resource = 10,
																					   SkillCombination = new[] {skill.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()}
																				   },
																				   new SkillCombinationResource
																				   {
																					   StartDateTime = period1.StartDateTime,
																					   EndDateTime = period1.EndDateTime,
																					   Resource = 10,
																					   SkillCombination = new[] {skill3.Id.GetValueOrDefault(), skill4.Id.GetValueOrDefault()}
																				   },
																				   new SkillCombinationResource
																				   {
																					   StartDateTime = period2.StartDateTime,
																					   EndDateTime = period2.EndDateTime,
																					   Resource = 10,
																					   SkillCombination = new[] {skill3.Id.GetValueOrDefault(), skill4.Id.GetValueOrDefault()}
																				   }
																			   });

			ScheduleForecastSkillReadModelRepository.Persist(new[]
															 {
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period1.StartDateTime,
																	 EndDateTime = period1.EndDateTime,
																	 Forecast = 4,
																	 SkillId = skill.Id.GetValueOrDefault()
																 },
																  new SkillStaffingInterval
																 {
																	 StartDateTime = period1.StartDateTime,
																	 EndDateTime = period1.EndDateTime,
																	 Forecast = 4,
																	 SkillId = skill2.Id.GetValueOrDefault()
																 },
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period2.StartDateTime,
																	 EndDateTime = period2.EndDateTime,
																	 Forecast = 4,
																	 SkillId = skill3.Id.GetValueOrDefault()
																 },
																  new SkillStaffingInterval
																 {
																	 StartDateTime = period2.StartDateTime,
																	 EndDateTime = period2.EndDateTime,
																	 Forecast = 4,
																	 SkillId = skill4.Id.GetValueOrDefault()
																 }
															 }, DateTime.Now);

			
			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, new DateTimePeriod(period1.StartDateTime, period2.EndDateTime))).WithId();

			Target.Process(personRequest, period1.StartDateTime);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));
		}


		[Test]
		public void ShouldDenyIfEnoughStaffingOnASkillWithTwoActivities()
		{
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var activity2 = ActivityRepository.Has("activity2");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var skill2 = SkillRepository.Has("skillB", activity).WithId();
			var skill3 = SkillRepository.Has("skillC", activity2).WithId();
			var skill4 = SkillRepository.Has("skillD", activity2).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = threshold;
			skill2.StaffingThresholds = threshold;
			skill3.StaffingThresholds = threshold;
			skill4.StaffingThresholds = threshold;
			Now.Is(new DateTime(2016, 12, 1, 7, 0, 0));

			var agent = PersonRepository.Has(skill, skill2, skill3, skill4);
			var wfcs = new WorkflowControlSet().WithId();
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2020, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2020, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});
			agent.WorkflowControlSet = wfcs;
			var period1 = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);
			var period2 = new DateTimePeriod(2016, 12, 1, 9, 2016, 12, 1, 10);

			var ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, agent, period1, new ShiftCategory("category"), scenario);
			ass.AddActivity(activity2, period2);
			PersonAssignmentRepository.Has(ass);

			SkillCombinationResourceRepository.PersistSkillCombinationResource(new[]
																			   {
																				   new SkillCombinationResource
																				   {
																					   StartDateTime = period1.StartDateTime,
																					   EndDateTime = period1.EndDateTime,
																					   Resource = 10,
																					   SkillCombination = new[] {skill.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()}
																				   },
																					new SkillCombinationResource
																				   {
																					   StartDateTime = period2.StartDateTime,
																					   EndDateTime = period2.EndDateTime,
																					   Resource = 10,
																					   SkillCombination = new[] {skill.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()}
																				   },
																				   new SkillCombinationResource
																				   {
																					   StartDateTime = period1.StartDateTime,
																					   EndDateTime = period1.EndDateTime,
																					   Resource = 10,
																					   SkillCombination = new[] {skill3.Id.GetValueOrDefault(), skill4.Id.GetValueOrDefault()}
																				   },
																				   new SkillCombinationResource
																				   {
																					   StartDateTime = period2.StartDateTime,
																					   EndDateTime = period2.EndDateTime,
																					   Resource = 10,
																					   SkillCombination = new[] {skill3.Id.GetValueOrDefault(), skill4.Id.GetValueOrDefault()}
																				   }
																			   });

			ScheduleForecastSkillReadModelRepository.Persist(new[]
															 {
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period1.StartDateTime,
																	 EndDateTime = period1.EndDateTime,
																	 Forecast = 5,
																	 SkillId = skill.Id.GetValueOrDefault()
																 },
																  new SkillStaffingInterval
																 {
																	 StartDateTime = period1.StartDateTime,
																	 EndDateTime = period1.EndDateTime,
																	 Forecast = 5,
																	 SkillId = skill2.Id.GetValueOrDefault()
																 },
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period2.StartDateTime,
																	 EndDateTime = period2.EndDateTime,
																	 Forecast = 5,
																	 SkillId = skill3.Id.GetValueOrDefault()
																 },
																  new SkillStaffingInterval
																 {
																	 StartDateTime = period2.StartDateTime,
																	 EndDateTime = period2.EndDateTime,
																	 Forecast = 5,
																	 SkillId = skill4.Id.GetValueOrDefault()
																 }
															 }, DateTime.Now);
			
			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, new DateTimePeriod(period1.StartDateTime, period2.EndDateTime))).WithId();

			Target.Process(personRequest, period1.StartDateTime);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(DenyRequestCommand));
		}

		[Test]  
		public void ShouldApproveRequestIfShovel()
		{
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill1 = SkillRepository.Has("skillA", activity).WithId();
			var skill2 = SkillRepository.Has("skillB", activity).WithId();
			skill1.SetCascadingIndex(1);
			skill2.SetCascadingIndex(2);
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill1.StaffingThresholds = threshold;
			skill2.StaffingThresholds = threshold;
			Now.Is(new DateTime(2016, 12, 1, 7, 0, 0));

			var agent = PersonRepository.Has(skill2);
			var wfcs = new WorkflowControlSet().WithId();
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2020, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2020, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});
			agent.WorkflowControlSet = wfcs;
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, agent, period, new ShiftCategory("category"), scenario));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(new[]
																			   {
																				   new SkillCombinationResource
																				   {
																					   StartDateTime = period.StartDateTime,
																					   EndDateTime = period.EndDateTime,
																					   Resource = 1,
																					   SkillCombination = new[] {skill1.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()}
																				   },
																				 new SkillCombinationResource
																				   {
																					   StartDateTime = period.StartDateTime,
																					   EndDateTime = period.EndDateTime,
																					   Resource = 1,
																					   SkillCombination = new[] { skill2.Id.GetValueOrDefault()}
																				   },
																			   });

			ScheduleForecastSkillReadModelRepository.Persist(new[]
															 {
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.EndDateTime,
																	 Forecast = 0,
																	 SkillId = skill1.Id.GetValueOrDefault()
																 },
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.EndDateTime,
																	 Forecast = 1,
																	 SkillId = skill2.Id.GetValueOrDefault()
																 }
															 }, DateTime.Now);
			
			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest, period.StartDateTime);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));
		}

		[Test]
		public void ShouldApproveRequestIfShovelAndHasUnsortedSkills()
		{
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill1 = SkillRepository.Has("skillA", activity).WithId();
			var skill2 = SkillRepository.Has("skillB", activity).WithId();
			var skill3 = SkillRepository.Has("skillUnsorted", activity).WithId();
			skill1.SetCascadingIndex(1);
			skill2.SetCascadingIndex(2);
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill1.StaffingThresholds = threshold;
			skill2.StaffingThresholds = threshold;
			skill3.StaffingThresholds = threshold;
			Now.Is(new DateTime(2016, 12, 1, 7, 0, 0));

			var agent = PersonRepository.Has(skill2);
			var wfcs = new WorkflowControlSet().WithId();
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2020, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2020, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});
			agent.WorkflowControlSet = wfcs;
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, agent, period, new ShiftCategory("category"), scenario));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(new[]
																			   {
																				   new SkillCombinationResource
																				   {
																					   StartDateTime = period.StartDateTime,
																					   EndDateTime = period.EndDateTime,
																					   Resource = 2,
																					   SkillCombination = new[] {skill1.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault(), skill3.Id.GetValueOrDefault() }
																				   },
																				 new SkillCombinationResource
																				   {
																					   StartDateTime = period.StartDateTime,
																					   EndDateTime = period.EndDateTime,
																					   Resource = 1,
																					   SkillCombination = new[] { skill2.Id.GetValueOrDefault()}
																				   },
																			   });

			ScheduleForecastSkillReadModelRepository.Persist(new[]
															 {
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.EndDateTime,
																	 Forecast = 0,
																	 SkillId = skill1.Id.GetValueOrDefault()
																 },
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.EndDateTime,
																	 Forecast = 1,
																	 SkillId = skill2.Id.GetValueOrDefault()
																 },
																  new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.EndDateTime,
																	 Forecast = 0,
																	 SkillId = skill3.Id.GetValueOrDefault()
																 }
															 }, DateTime.Now);
			
			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest, period.StartDateTime);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));
		}

		[Test]
		public void ShouldUpdateDeltaIfRequestIsApproved()
		{
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = threshold;
			Now.Is(new DateTime(2016, 12, 1, 7, 0, 0));

			var agent = PersonRepository.Has(skill);
			var wfcs = new WorkflowControlSet().WithId();
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2020, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2020, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});
			agent.WorkflowControlSet = wfcs;
			var period1 = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);
			var period2 = new DateTimePeriod(2016, 12, 1, 9, 2016, 12, 1, 10);
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 10);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, agent, period, new ShiftCategory("category"), scenario));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(new[]
																			   {
																				   new SkillCombinationResource
																				   {
																					   StartDateTime = period1.StartDateTime,
																					   EndDateTime = period1.EndDateTime,
																					   Resource = 10,
																					   SkillCombination = new[] {skill.Id.GetValueOrDefault()}
																				   },
																				   new SkillCombinationResource
																				   {
																					   StartDateTime = period2.StartDateTime,
																					   EndDateTime = period2.EndDateTime,
																					   Resource = 10,
																					   SkillCombination = new[] {skill.Id.GetValueOrDefault()}
																				   }
																			   });

			ScheduleForecastSkillReadModelRepository.Persist(new[]
															 {
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period1.StartDateTime,
																	 EndDateTime = period1.EndDateTime,
																	 Forecast = 5,
																	 SkillId = skill.Id.GetValueOrDefault()
																 },
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period2.StartDateTime,
																	 EndDateTime = period2.EndDateTime,
																	 Forecast = 5,
																	 SkillId = skill.Id.GetValueOrDefault()
																 }
															 }, DateTime.Now);
			
			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest, period.StartDateTime);

			SkillCombinationResourceRepository.LoadSkillCombinationResources(period).First().Resource.Should().Be.EqualTo(9);
			SkillCombinationResourceRepository.LoadSkillCombinationResources(period).Second().Resource.Should().Be.EqualTo(9);
		}

		[Test]
		public void DenyIfReadModelHasNoSkillCombinations()
		{
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var threshold = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0));
			skill.StaffingThresholds = threshold;
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);
			var now = new DateTime(2016, 12, 1, 7, 0, 0);

			var agent = PersonRepository.Has(skill);
			var wfcs = new WorkflowControlSet().WithId();
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 12, 1, 2020, 12, 2),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2020, 11, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});
			agent.WorkflowControlSet = wfcs;
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, agent, period, new ShiftCategory("category"), scenario));

			
			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Now.Is(now);
			ScheduleForecastSkillReadModelRepository.LastCalculatedDate.Add(personRequest.BusinessUnit.Id.GetValueOrDefault(), now.AddHours(-1));
			Target.Process(personRequest, period.StartDateTime);
			var denyCommand = CommandDispatcher.LatestCommand as DenyRequestCommand;
			denyCommand.DenyReason.Should().Contain("Please contact system administrator");
		}
	}
}

	
