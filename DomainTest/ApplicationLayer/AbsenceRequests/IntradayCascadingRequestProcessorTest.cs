using System;
using System.Drawing;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[DomainTest]
	[AllTogglesOn]
	[TestFixture]
	public class IntradayCascadingRequestProcessorTest : ISetup
	{
		public IIntradayRequestProcessor Target;
		public FakePersonRepository PersonRepository;
		public FakeSkillRepository SkillRepository;
		public FakeActivityRepository ActivityRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public FakeScheduleForecastSkillReadModelRepository ScheduleForecastSkillReadModelRepository;
		public FakeCommandDispatcher CommandDispatcher;
		public IScheduleStorage ScheduleStorage;


		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeCommandDispatcher>().For<ICommandDispatcher>();
		}

		[Test]
		public void RobinsTest()
		{
			Assert.Pass();
		}

		
		[Test]
		public void ShouldApproveRequestIfEnoughResourcesOnSkill()
		{
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skillA", activity).WithId();
		
			var agent = PersonRepository.Has(skill);
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

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest, period.StartDateTime);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));
		}


		[Test]
		public void ShouldApproveRequestIfEnoughResourcesOnSkills()
		{
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var skill2 = SkillRepository.Has("skillB", activity).WithId();

			var agent = PersonRepository.Has(skill, skill2);
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

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest, period.StartDateTime);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));
		}


		[Test]
		public void ShouldDenyRequestIfUnderStaffedOnSkill()
		{
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skillA", activity).WithId();

			var agent = PersonRepository.Has(skill);
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

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest, period.StartDateTime);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(DenyRequestCommand));
		}

		[Test]
		public void ShouldDenyRequestIfRequestCausesUnderStaffedOnSkill()
		{
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var skill2 = SkillRepository.Has("skillB", activity).WithId();

			var agent = PersonRepository.Has(skill, skill2);
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

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest, period.StartDateTime);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(DenyRequestCommand));
		}


		[Test]
		public void ShouldApproveIfEnoughStaffingForBothActivities()
		{
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var activity2 = ActivityRepository.Has("activity2");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var skill2 = SkillRepository.Has("skillB", activity).WithId();
			var skill3 = SkillRepository.Has("skillC", activity2).WithId();
			var skill4 = SkillRepository.Has("skillD", activity2).WithId();

			var agent = PersonRepository.Has(skill, skill2, skill3, skill4);
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

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, new DateTimePeriod(period1.StartDateTime, period2.EndDateTime))).WithId();

			Target.Process(personRequest, period1.StartDateTime);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));
		}


		[Test]
		public void ShouldDenyIfEnoughStaffingOnASkillWithTwoActivities()
		{
			var scenario = ScenarioRepository.Has("scenario");

			var activity = ActivityRepository.Has("activity");
			var activity2 = ActivityRepository.Has("activity2");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			var skill2 = SkillRepository.Has("skillB", activity).WithId();
			var skill3 = SkillRepository.Has("skillC", activity2).WithId();
			var skill4 = SkillRepository.Has("skillD", activity2).WithId();

			var agent = PersonRepository.Has(skill, skill2, skill3, skill4);
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

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, new DateTimePeriod(period1.StartDateTime, period2.EndDateTime))).WithId();

			Target.Process(personRequest, period1.StartDateTime);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(DenyRequestCommand));
		}

		[Test]  
		public void ShouldApproveRequestIfShovel()
		{
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill1 = SkillRepository.Has("skillA", activity).WithId();
			var skill2 = SkillRepository.Has("skillB", activity).WithId();
			skill1.SetCascadingIndex(1);
			skill2.SetCascadingIndex(2);


			var agent = PersonRepository.Has(skill2);
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

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest, period.StartDateTime);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));
		}

		[Test]
		public void ShouldApproveRequestIfShovelAndHasUnsortedSkills()
		{
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill1 = SkillRepository.Has("skillA", activity).WithId();
			var skill2 = SkillRepository.Has("skillB", activity).WithId();
			var skill3 = SkillRepository.Has("skillUnsorted", activity).WithId();
			skill1.SetCascadingIndex(1);
			skill2.SetCascadingIndex(2);


			var agent = PersonRepository.Has(skill2);
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

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();

			Target.Process(personRequest, period.StartDateTime);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));
		}

	}

	
}

	
