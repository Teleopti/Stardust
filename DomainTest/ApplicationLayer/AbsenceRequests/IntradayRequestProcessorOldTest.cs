﻿using System;
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
	public class IntradayRequestProcessorOldTest : ISetup
	{
		public IntradayRequestProcessorOld Target;
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
			system.UseTestDouble<IntradayRequestProcessorOld>().For<IIntradayRequestProcessor>();
			system.UseTestDouble<ScheduleForecastSkillReadModelValidator>().For<IScheduleForecastSkillReadModelValidator>();
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
				createSkillCombinationResource(period, new[] {skill.Id.GetValueOrDefault()}, 10)
			});

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

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));
			var skillCombinations =  SkillCombinationResourceRepository.LoadSkillCombinationResources(period).First();
			skillCombinations.StartDateTime.Should().Be.EqualTo(period.StartDateTime);
			skillCombinations.EndDateTime.Should().Be.EqualTo(period.EndDateTime);
			skillCombinations.Resource.Should().Be.EqualTo(9);
			CollectionAssert.AreEqual(skillCombinations.SkillCombination, new[] {skill.Id.GetValueOrDefault()});
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
				createSkillCombinationResource(periodBefore, new[] {skill.Id.GetValueOrDefault()}, 1),
				createSkillCombinationResource(shiftPeriod, new[] {skill.Id.GetValueOrDefault()}, 10),
				createSkillCombinationResource(periodAfter, new[] {skill.Id.GetValueOrDefault()}, 1)
			});

			ScheduleForecastSkillReadModelRepository.Persist(new[]
															 {
																 new SkillStaffingInterval
																 {
																	 StartDateTime = periodBefore.StartDateTime,
																	 EndDateTime = periodBefore.EndDateTime,
																	 Forecast = 5,
																	 SkillId = skill.Id.GetValueOrDefault()
																 },
																 new SkillStaffingInterval
																 {
																	 StartDateTime = shiftPeriod.StartDateTime,
																	 EndDateTime = shiftPeriod.EndDateTime,
																	 Forecast = 5,
																	 SkillId = skill.Id.GetValueOrDefault()
																 },
																 new SkillStaffingInterval
																 {
																	 StartDateTime = periodAfter.StartDateTime,
																	 EndDateTime = periodAfter.EndDateTime,
																	 Forecast = 5,
																	 SkillId = skill.Id.GetValueOrDefault()
																 }
															 }, DateTime.Now);

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, requestPeriod)).WithId();

			Target.Process(personRequest, requestPeriod.StartDateTime);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));
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
				AbsenceRequestProcess = new GrantAbsenceRequest(),
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
																					   SkillCombination = new[] {skill.Id.GetValueOrDefault()}
																				   }
																			   });

			ScheduleForecastSkillReadModelRepository.Persist(new[]
															 {
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.EndDateTime,
																	 Forecast = 5,
																	 ForecastWithShrinkage = 6,
																	 SkillId = skill.Id.GetValueOrDefault()
																 }
															 }, DateTime.Now);

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest, period.StartDateTime);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(DenyRequestCommand));
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
				AbsenceRequestProcess = new GrantAbsenceRequest(),
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
																					   SkillCombination = new[] {skill.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()}
																				   }
																			   });

			ScheduleForecastSkillReadModelRepository.Persist(new[]
															 {
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.EndDateTime,
																	 Forecast = 1,
																	 ForecastWithShrinkage = 1,
																	 SkillId = skill.Id.GetValueOrDefault()
																 },
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.EndDateTime,
																	 Forecast = 1,
																	 ForecastWithShrinkage = 2,
																	 SkillId = skill2.Id.GetValueOrDefault()
																 }
															 }, DateTime.Now);

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest, period.StartDateTime);

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
				createSkillCombinationResource(period, new[] {skill.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()}, 10)

			});

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
			
			var skillCombinations = SkillCombinationResourceRepository.LoadSkillCombinationResources(period).First();
			skillCombinations.StartDateTime.Should().Be.EqualTo(period.StartDateTime);
			skillCombinations.EndDateTime.Should().Be.EqualTo(period.EndDateTime);
			skillCombinations.Resource.Should().Be.EqualTo(9);
			CollectionAssert.AreEqual(skillCombinations.SkillCombination, new[] { skill.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault() });
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
				createSkillCombinationResource(period, new[] {skill.Id.GetValueOrDefault()}, 5)
			});

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

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));

			var skillCombinations = SkillCombinationResourceRepository.LoadSkillCombinationResources(period).First();
			skillCombinations.StartDateTime.Should().Be.EqualTo(period.StartDateTime);
			skillCombinations.EndDateTime.Should().Be.EqualTo(period.EndDateTime);
			skillCombinations.Resource.Should().Be.EqualTo(5);
			CollectionAssert.AreEqual(skillCombinations.SkillCombination, new[] { skill.Id.GetValueOrDefault()});
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
				createSkillCombinationResource(period, new[] {skill.Id.GetValueOrDefault()}, 4)
			});

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

			var skillCombinations = SkillCombinationResourceRepository.LoadSkillCombinationResources(period).First();
			skillCombinations.StartDateTime.Should().Be.EqualTo(period.StartDateTime);
			skillCombinations.EndDateTime.Should().Be.EqualTo(period.EndDateTime);
			skillCombinations.Resource.Should().Be.EqualTo(4);
			CollectionAssert.AreEqual(skillCombinations.SkillCombination, new[] { skill.Id.GetValueOrDefault() });
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
				createSkillCombinationResource(period, new[] {skill.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()}, 10)
			});

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

			var skillCombinations = SkillCombinationResourceRepository.LoadSkillCombinationResources(period).First();
			skillCombinations.StartDateTime.Should().Be.EqualTo(period.StartDateTime);
			skillCombinations.EndDateTime.Should().Be.EqualTo(period.EndDateTime);
			skillCombinations.Resource.Should().Be.EqualTo(10);
			CollectionAssert.AreEqual(skillCombinations.SkillCombination, new[] { skill.Id.GetValueOrDefault(),skill2.Id.GetValueOrDefault() });
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
																				   createSkillCombinationResource(period1,new [] {skill.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()}, 10),
																				   createSkillCombinationResource(period2,new [] {skill.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()}, 10),
																				   createSkillCombinationResource(period1,new [] {skill3.Id.GetValueOrDefault(), skill4.Id.GetValueOrDefault()}, 10),
																				   createSkillCombinationResource(period2,new [] {skill3.Id.GetValueOrDefault(), skill4.Id.GetValueOrDefault()}, 10)
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

			var skillCombinations = SkillCombinationResourceRepository.LoadSkillCombinationResources(new DateTimePeriod(period1.StartDateTime, period2.EndDateTime)).ToList();
			skillCombinations.Count().Should().Be.EqualTo(4);
			skillCombAsserts(skillCombinations[0],period1, new[] { skill.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault() },9);
			skillCombAsserts(skillCombinations[1],period2, new[] { skill.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault() },10);
			skillCombAsserts(skillCombinations[2],period1, new[] { skill3.Id.GetValueOrDefault(), skill4.Id.GetValueOrDefault() },10);
			skillCombAsserts(skillCombinations[3],period2, new[] { skill3.Id.GetValueOrDefault(), skill4.Id.GetValueOrDefault() },9);
		}

		private static SkillCombinationResource createSkillCombinationResource(DateTimePeriod period1, Guid[] skillCombinations,double resource)
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
				createSkillCombinationResource(period1, new[] {skill.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()}, 10),
				createSkillCombinationResource(period2, new[] {skill.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()}, 10),
				createSkillCombinationResource(period1, new[] {skill3.Id.GetValueOrDefault(), skill4.Id.GetValueOrDefault()}, 10),
				createSkillCombinationResource(period2, new[] {skill3.Id.GetValueOrDefault(), skill4.Id.GetValueOrDefault()}, 10)
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
			Now.Is(new DateTime(2016, 12, 22, 22, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill1 = SkillRepository.Has("skillA", activity).WithId();
			var skill2 = SkillRepository.Has("skillB", activity).WithId();
			skill1.SetCascadingIndex(1);
			skill2.SetCascadingIndex(2);
			var threshold = new StaffingThresholds(new Percent(-0.01), new Percent(-0.001), new Percent(0));
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
				createSkillCombinationResource(period, new[] {skill1.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()}, 1),
				createSkillCombinationResource(period, new[] {skill2.Id.GetValueOrDefault()}, 1)
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

			var skillCombinations = SkillCombinationResourceRepository.LoadSkillCombinationResources(period).ToList();
			skillCombinations.Count().Should().Be.EqualTo(2);
			skillCombAsserts(skillCombinations[0], period, new[] { skill1.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault() }, 1);
			skillCombAsserts(skillCombinations[1], period, new[] { skill2.Id.GetValueOrDefault()}, 0);
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
			var threshold = new StaffingThresholds(new Percent(-0.01), new Percent(-0.001), new Percent(0));
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
																					   SkillCombination = new[] {skill1.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()}
																				   },
																					new SkillCombinationResource
																				   {
																					   StartDateTime = period.StartDateTime,
																					   EndDateTime = period.EndDateTime,
																					   Resource = 1,
																					   SkillCombination = new[] {skill1.Id.GetValueOrDefault()}
																				   }
																			   });

			ScheduleForecastSkillReadModelRepository.Persist(new[]
															 {
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.EndDateTime,
																	 Forecast = 3,
																	 SkillId = skill1.Id.GetValueOrDefault()
																 },
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.EndDateTime,
																	 Forecast = 3,
																	 SkillId = skill2.Id.GetValueOrDefault()
																 }
															 }, DateTime.Now);

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest, period.StartDateTime);

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
			var threshold = new StaffingThresholds(new Percent(-0.01), new Percent(-0.001), new Percent(0));
			skill1.StaffingThresholds = skill2.StaffingThresholds = skill3.StaffingThresholds = threshold;
			skill1.DefaultResolution = skill2.DefaultResolution = skill3.DefaultResolution = 60;

			var agent = PersonRepository.Has(skill1,skill3);
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
																					   SkillCombination = new[] {skill1.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault(), skill3.Id.GetValueOrDefault()}
																				   },
																					new SkillCombinationResource
																				   {
																					   StartDateTime = period.StartDateTime,
																					   EndDateTime = period.EndDateTime,
																					   Resource = 1,
																					   SkillCombination = new[] { skill1.Id.GetValueOrDefault(),skill3.Id.GetValueOrDefault()}
																				   }
																			   });

			ScheduleForecastSkillReadModelRepository.Persist(new[]
															 {
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.EndDateTime,
																	 Forecast = 3,
																	 SkillId = skill1.Id.GetValueOrDefault()
																 },
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.EndDateTime,
																	 Forecast = 3,
																	 SkillId = skill2.Id.GetValueOrDefault()
																 },
																  new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.EndDateTime,
																	 Forecast = 3,
																	 SkillId = skill3.Id.GetValueOrDefault()
																 }
															 }, DateTime.Now);

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest, period.StartDateTime);

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
			var threshold = new StaffingThresholds(new Percent(-0.01), new Percent(-0.001), new Percent(0));
			skill1.StaffingThresholds = skill2.StaffingThresholds = threshold;
			skill1.DefaultResolution = skill2.DefaultResolution  = 60;

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
																					   SkillCombination = new[] {skill1.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()}
																				   },
																					new SkillCombinationResource
																				   {
																					   StartDateTime = period.StartDateTime,
																					   EndDateTime = period.EndDateTime,
																					   Resource = 1,
																					   SkillCombination = new[] { skill1.Id.GetValueOrDefault(),}
																				   }
																			   });

			ScheduleForecastSkillReadModelRepository.Persist(new[]
															 {
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.EndDateTime,
																	 Forecast = 3,
																	 SkillId = skill1.Id.GetValueOrDefault()
																 },
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.EndDateTime,
																	 Forecast = 3,
																	 SkillId = skill2.Id.GetValueOrDefault()
																 }
															 }, DateTime.Now);

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest, period.StartDateTime);

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
			var threshold = new StaffingThresholds(new Percent(-0.01), new Percent(-0.001), new Percent(0));
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
																					   SkillCombination = new[] {skill1.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()}
																				   },
																					new SkillCombinationResource
																				   {
																					   StartDateTime = period.StartDateTime,
																					   EndDateTime = period.EndDateTime,
																					   Resource = 1,
																					   SkillCombination = new[] { skill1.Id.GetValueOrDefault(),}
																				   }
																			   });

			ScheduleForecastSkillReadModelRepository.Persist(new[]
															 {
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.EndDateTime,
																	 Forecast = 3,
																	 SkillId = skill1.Id.GetValueOrDefault()
																 },
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.EndDateTime,
																	 Forecast = 3,
																	 SkillId = skill2.Id.GetValueOrDefault()
																 }
															 }, DateTime.Now);

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest, period.StartDateTime);

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
					new[] {skill1.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault(), skill3.Id.GetValueOrDefault()}, 2),
				createSkillCombinationResource(period, new[] {skill2.Id.GetValueOrDefault()}, 1)
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
			var period1 = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);
			var period2 = new DateTimePeriod(2016, 12, 1, 9, 2016, 12, 1, 10);
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 10);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(period1, new[] {skill.Id.GetValueOrDefault()}, 10),
				createSkillCombinationResource(period2, new[] {skill.Id.GetValueOrDefault()}, 10)
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
			ScheduleForecastSkillReadModelRepository.LastCalculatedDate.Add(personRequest.BusinessUnit.Id.GetValueOrDefault(), now.AddHours(-3));
			Target.Process(personRequest, period.StartDateTime);
			var denyCommand = CommandDispatcher.LatestCommand as DenyRequestCommand;
			denyCommand.DenyReason.Should().Contain(UserTexts.Resources.ResourceManager.GetString("DenyReasonTechnicalIssues", agent.PermissionInformation.Culture()));	
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
			ScheduleForecastSkillReadModelRepository.LastCalculatedDate.Add(personRequest.BusinessUnit.Id.GetValueOrDefault(), now.AddHours(-1));
			Target.Process(personRequest, period.StartDateTime);
			var denyCommand = CommandDispatcher.LatestCommand as DenyRequestCommand;
			denyCommand.DenyReason.Should().Contain(UserTexts.Resources.ResourceManager.GetString("DenyReasonTechnicalIssues", agent.PermissionInformation.Culture()));
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
				createSkillCombinationResource(period1, new[] {skill.Id.GetValueOrDefault()}, 10),
				createSkillCombinationResource(period2, new[] {skill2.Id.GetValueOrDefault()}, 10)
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
																	 Forecast = 15,
																	 SkillId = skill2.Id.GetValueOrDefault()
																 }
															 }, DateTime.Now);


			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, new DateTimePeriod(period1.StartDateTime, period2.EndDateTime))).WithId();

			Target.Process(personRequest, period1.StartDateTime);

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

			var agent = PersonRepository.Has( emailSkill);
			var wfcs = new WorkflowControlSet().WithId();
			createWfcs(wfcs, absence);
			agent.WorkflowControlSet = wfcs;
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, emailActivity, period, new ShiftCategory("category")));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(new DateTimePeriod(period.StartDateTime, period.StartDateTime.AddMinutes(15)),
					new[] {phoneSkill.Id.GetValueOrDefault()}, 10),
				createSkillCombinationResource(new DateTimePeriod(period.StartDateTime, period.StartDateTime.AddMinutes(60)),
					new[] {emailSkill.Id.GetValueOrDefault()}, 10)

			});

			ScheduleForecastSkillReadModelRepository.Persist(new[]
															 {
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.StartDateTime.AddMinutes(60),
																	 Forecast = 4,
																	 SkillId = emailSkill.Id.GetValueOrDefault(),
																	 CalculatedResource = 6
																 },
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.StartDateTime.AddMinutes(15),
																	 Forecast = 4,
																	 SkillId = phoneSkill.Id.GetValueOrDefault(),
																	 CalculatedResource = 6
																 }
															 }, DateTime.Now);


			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest, period.StartDateTime);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));
		}


		[Test]
		public void ShouldApproveRequestIfPersonSkillsOnMinResolution()
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


			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(new DateTimePeriod(period.StartDateTime, period.StartDateTime.AddMinutes(15)),new[] {phoneSkill.Id.GetValueOrDefault()}, 10),
				createSkillCombinationResource(new DateTimePeriod(period.StartDateTime.AddMinutes(15), period.StartDateTime.AddMinutes(30)),new[] {phoneSkill.Id.GetValueOrDefault()}, 10),
				createSkillCombinationResource(new DateTimePeriod(period.StartDateTime.AddMinutes(30), period.StartDateTime.AddMinutes(45)),new[] {phoneSkill.Id.GetValueOrDefault()}, 10),
				createSkillCombinationResource(new DateTimePeriod(period.StartDateTime.AddMinutes(45), period.StartDateTime.AddMinutes(60)),new[] {phoneSkill.Id.GetValueOrDefault()}, 10),
				createSkillCombinationResource(emailPerod,new[] {emailSkill.Id.GetValueOrDefault()}, 10)

			});

			ScheduleForecastSkillReadModelRepository.Persist(new[]
															 {
																 new SkillStaffingInterval
																 {
																	 StartDateTime = emailPerod.StartDateTime,
																	 EndDateTime = emailPerod.StartDateTime.AddMinutes(60),
																	 Forecast = 4,
																	 SkillId = emailSkill.Id.GetValueOrDefault()
																 },
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.StartDateTime.AddMinutes(15),
																	 Forecast = 4,
																	 SkillId = phoneSkill.Id.GetValueOrDefault()
																 },
																 new SkillStaffingInterval
																 {
																	StartDateTime = period.StartDateTime.AddMinutes(15),
																	EndDateTime = period.StartDateTime.AddMinutes(30),
																	 Forecast = 4,
																	 SkillId = phoneSkill.Id.GetValueOrDefault()
																 },
																 new SkillStaffingInterval
																 {
																	StartDateTime = period.StartDateTime.AddMinutes(30),
																	EndDateTime = period.StartDateTime.AddMinutes(45),
																	 Forecast = 4,
																	 SkillId = phoneSkill.Id.GetValueOrDefault()
																 },
																 new SkillStaffingInterval
																 {
																	StartDateTime = period.StartDateTime.AddMinutes(45),
																	EndDateTime = period.StartDateTime.AddMinutes(60),
																	 Forecast = 4,
																	 SkillId = phoneSkill.Id.GetValueOrDefault()
																 }
															 }, DateTime.Now);


			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 10))).WithId();

			Target.Process(personRequest, period.StartDateTime);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));
		}


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


			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(new DateTimePeriod(period.StartDateTime, period.StartDateTime.AddMinutes(15)),
					new[] {phoneSkill.Id.GetValueOrDefault()}, 10),
				createSkillCombinationResource(
					new DateTimePeriod(period.StartDateTime.AddMinutes(15), period.StartDateTime.AddMinutes(30)),
					new[] {phoneSkill.Id.GetValueOrDefault()}, 10),
				createSkillCombinationResource(
					new DateTimePeriod(period.StartDateTime.AddMinutes(30), period.StartDateTime.AddMinutes(45)),
					new[] {phoneSkill.Id.GetValueOrDefault()}, 10),
				createSkillCombinationResource(
					new DateTimePeriod(period.StartDateTime.AddMinutes(45), period.StartDateTime.AddMinutes(60)),
					new[] {phoneSkill.Id.GetValueOrDefault()}, 5),
				createSkillCombinationResource(new DateTimePeriod(period.StartDateTime, period.StartDateTime.AddMinutes(30)),
					new[] {emailSkill.Id.GetValueOrDefault()}, 10)

			});

			ScheduleForecastSkillReadModelRepository.Persist(new[]
															 {
																 new SkillStaffingInterval
																 {
																	 StartDateTime = emailPerod.StartDateTime,
																	 EndDateTime = emailPerod.StartDateTime.AddMinutes(60),
																	 Forecast = 4,
																	 SkillId = emailSkill.Id.GetValueOrDefault()
																 },
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.StartDateTime.AddMinutes(15),
																	 Forecast = 4,
																	 SkillId = phoneSkill.Id.GetValueOrDefault()
																 },
																 new SkillStaffingInterval
																 {
																	StartDateTime = period.StartDateTime.AddMinutes(15),
																	EndDateTime = period.StartDateTime.AddMinutes(30),
																	 Forecast = 4,
																	 SkillId = phoneSkill.Id.GetValueOrDefault()
																 },
																 new SkillStaffingInterval
																 {
																	StartDateTime = period.StartDateTime.AddMinutes(30),
																	EndDateTime = period.StartDateTime.AddMinutes(45),
																	 Forecast = 4,
																	 SkillId = phoneSkill.Id.GetValueOrDefault()
																 },
																 new SkillStaffingInterval
																 {
																	StartDateTime = period.StartDateTime.AddMinutes(45),
																	EndDateTime = period.StartDateTime.AddMinutes(60),
																	 Forecast = 10,
																	 SkillId = phoneSkill.Id.GetValueOrDefault()
																 }
															 }, DateTime.Now);


			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 10))).WithId();

			Target.Process(personRequest, period.StartDateTime);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(DenyRequestCommand));
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
			var emailPerod = new DateTimePeriod(new DateTime(2016,12,1,8,0,0,DateTimeKind.Utc) , new DateTime(2016, 12, 1, 8, 30, 0, DateTimeKind.Utc));
			var mainShiftPersonAssing = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, emailActivity, emailPerod, new ShiftCategory("category"));
			PersonAssignmentRepository.Has(mainShiftPersonAssing);

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(
					new DateTimePeriod(emailPerod.StartDateTime, emailPerod.StartDateTime.AddMinutes(60)),
					new[] {emailSkill.Id.GetValueOrDefault()}, 10),
				createSkillCombinationResource(
					new DateTimePeriod(emailPerod.StartDateTime, emailPerod.StartDateTime.AddMinutes(15)),
					new[] {phoneSkill.Id.GetValueOrDefault()}, 10)
			});

			ScheduleForecastSkillReadModelRepository.Persist(new[]
															 {
																 new SkillStaffingInterval
																 {
																	 StartDateTime = emailPerod.StartDateTime,
																	 EndDateTime = emailPerod.StartDateTime.AddMinutes(60),
																	 Forecast = 4,
																	 SkillId = emailSkill.Id.GetValueOrDefault()
																 }
															 }, DateTime.Now);


			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9))).WithId();

			Target.Process(personRequest, emailPerod.StartDateTime);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));

			var skillCombinationResources =
				SkillCombinationResourceRepository.LoadSkillCombinationResources(new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9)).ToList();
			skillCombinationResources.Count().Should().Be.EqualTo(2);
			skillCombAsserts(skillCombinationResources[0], new DateTimePeriod(emailPerod.StartDateTime,emailPerod.StartDateTime.AddMinutes(60)), new[] {emailSkill.Id.GetValueOrDefault()}, 9.5);
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
			workloadDay1.Create(new DateOnly(2008, 7, 1), workload, new List<TimePeriod>() { new TimePeriod(new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0)) });

			phoneSkill.AddWorkload(workload);
			var agent = PersonRepository.Has(phoneSkill);
			var wfcs = new WorkflowControlSet().WithId();
			createWfcs(wfcs, absence);
			agent.WorkflowControlSet = wfcs;
			var period = new DateTimePeriod(2016, 12, 1, 9, 2016, 12, 1, 10);
			var mainShiftPersonAssing = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, phoneActivity, period, new ShiftCategory("category"));
			PersonAssignmentRepository.Has(mainShiftPersonAssing);


			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(new DateTimePeriod(period.StartDateTime, period.StartDateTime.AddMinutes(30)),new[] {phoneSkill.Id.GetValueOrDefault()}, 10),
				createSkillCombinationResource(new DateTimePeriod(period.StartDateTime.AddMinutes(30), period.StartDateTime.AddMinutes(60)),new[] {phoneSkill.Id.GetValueOrDefault()}, 10),
				createSkillCombinationResource(new DateTimePeriod(period.StartDateTime.AddMinutes(60), period.StartDateTime.AddMinutes(90)),new[] {phoneSkill.Id.GetValueOrDefault()}, 10)

			});

			ScheduleForecastSkillReadModelRepository.Persist(new[]
															 {
																new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.StartDateTime.AddMinutes(30),
																	 Forecast = 4,
																	 SkillId = phoneSkill.Id.GetValueOrDefault()
																 },
																 new SkillStaffingInterval
																 {
																	StartDateTime = period.StartDateTime.AddMinutes(30),
																	EndDateTime = period.StartDateTime.AddMinutes(60),
																	 Forecast = 4,
																	 SkillId = phoneSkill.Id.GetValueOrDefault()
																 },
																 new SkillStaffingInterval
																 {
																	StartDateTime = period.StartDateTime.AddMinutes(60),
																	EndDateTime = period.StartDateTime.AddMinutes(90),
																	 Forecast = 4,
																	 SkillId = phoneSkill.Id.GetValueOrDefault()
																 }
															 }, DateTime.Now);


			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 10, 2016, 12, 1, 11))).WithId();

			Target.Process(personRequest, period.StartDateTime);
			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));
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
				AbsenceRequestProcess = new GrantAbsenceRequest(),
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
																					   Resource = 34,
																					   SkillCombination = new[] {skill.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()}
																				   }
																			   });

			ScheduleForecastSkillReadModelRepository.Persist(new[]
															 {
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.EndDateTime,
																	 Forecast = 10,
																	 ForecastWithShrinkage = 11.1,
																	 SkillId = skill.Id.GetValueOrDefault()
																 },
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.EndDateTime,
																	 Forecast = 10,
																	 ForecastWithShrinkage = 20,
																	 SkillId = skill2.Id.GetValueOrDefault()
																 }
															 }, DateTime.Now);

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest, period.StartDateTime);

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
												 AbsenceRequestProcess = new GrantAbsenceRequest(),
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
																					   SkillCombination = new[] {skill.Id.GetValueOrDefault(), skill2.Id.GetValueOrDefault()}
																				   }
																			   });

			ScheduleForecastSkillReadModelRepository.Persist(new[]
															 {
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.EndDateTime,
																	 Forecast = 1,
																	 ForecastWithShrinkage = 1,
																	 SkillId = skill.Id.GetValueOrDefault()
																 },
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.EndDateTime,
																	 Forecast = 1,
																	 ForecastWithShrinkage = 2,
																	 SkillId = skill2.Id.GetValueOrDefault()
																 }
															 }, DateTime.Now);

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest, period.StartDateTime);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));
		}

		private void skillCombAsserts(SkillCombinationResource skillCombinationResource, DateTimePeriod period, Guid[] skillCombinationResources, double resource)
		{
			skillCombinationResource.StartDateTime.Should().Be.EqualTo(period.StartDateTime);
			skillCombinationResource.EndDateTime.Should().Be.EqualTo(period.EndDateTime);
			skillCombinationResource.Resource.Should().Be.EqualTo(resource);
			CollectionAssert.AreEqual(skillCombinationResource.SkillCombination, skillCombinationResources);
		}
	}
}

	
