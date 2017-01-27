using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Intraday
{
	[DomainTest]
	[TestFixture]
	public class SkillStaffingIntervalProviderTest : ISetup
	{
		public SkillStaffingIntervalProvider Target;
		public IScheduleForecastSkillReadModelRepository ScheduleForecastSkillReadModelRepository;
		public MutableNow Now;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public FakeSkillRepository SkillRepository;
		public FakeActivityRepository ActivityRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<SkillStaffingIntervalProvider>().For<SkillStaffingIntervalProvider>();
		}


		[Test]
		public void ShouldResourceCalculateOnSingleSkill()
		{
			Now.Is(new DateTime(2016, 12, 1, 07, 00, 00, DateTimeKind.Utc));
			
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			skill.DefaultResolution = 60;

			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
																			   {
																				   new SkillCombinationResource
																				   {
																					   StartDateTime = period.StartDateTime,
																					   EndDateTime = period.EndDateTime,
																					   Resource = 10,
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
																	 SkillId = skill.Id.GetValueOrDefault()
																 }
															 }, Now.UtcDateTime());

			var staffingIntervals = Target.StaffingForSkills(new[] {skill.Id.GetValueOrDefault()},period, TimeSpan.FromHours(1));

			staffingIntervals.Count.Should().Be.EqualTo(1);
			staffingIntervals.Single().StaffingLevel.Should().Be.EqualTo(10);
		}

		[Test]
		public void ShouldResourceCalculateOnTwoSkills()
		{
			Now.Is(new DateTime(2016, 12, 1, 07, 00, 00, DateTimeKind.Utc));

			var activity = ActivityRepository.Has("activity");
			var skillA = SkillRepository.Has("skillA", activity).WithId();
			var skillB = SkillRepository.Has("skillB", activity).WithId();
			skillA.DefaultResolution = skillB.DefaultResolution = 60;

			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
																			   {
																				   new SkillCombinationResource
																				   {
																					   StartDateTime = period.StartDateTime,
																					   EndDateTime = period.EndDateTime,
																					   Resource = 10,
																					   SkillCombination = new[] {skillA.Id.GetValueOrDefault(), skillB.Id.GetValueOrDefault() }
																				   }
																			   });

			ScheduleForecastSkillReadModelRepository.Persist(new[]
															 {
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.EndDateTime,
																	 Forecast = 5,
																	 SkillId = skillA.Id.GetValueOrDefault()
																 },
																  new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.EndDateTime,
																	 Forecast = 5,
																	 SkillId = skillB.Id.GetValueOrDefault()
																 }
															 }, Now.UtcDateTime());

			var staffingIntervals = Target.StaffingForSkills(new[] { skillA.Id.GetValueOrDefault(), skillB.Id.GetValueOrDefault() }, period, TimeSpan.FromHours(1));

			staffingIntervals.Count.Should().Be.EqualTo(2);
			staffingIntervals.First().StaffingLevel.Should().Be.EqualTo(5);
			staffingIntervals.Second().StaffingLevel.Should().Be.EqualTo(5);
		}

		[Test]
		public void ShouldOnlyGiveStaffingForAskedSkill()
		{
			Now.Is(new DateTime(2016, 12, 1, 07, 00, 00, DateTimeKind.Utc));

			var activity = ActivityRepository.Has("activity");
			var skillA = SkillRepository.Has("skillA", activity).WithId();
			var skillB = SkillRepository.Has("skillB", activity).WithId();
			skillA.DefaultResolution = skillB.DefaultResolution = 60;

			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
																			   {
																				   new SkillCombinationResource
																				   {
																					   StartDateTime = period.StartDateTime,
																					   EndDateTime = period.EndDateTime,
																					   Resource = 10,
																					   SkillCombination = new[] {skillA.Id.GetValueOrDefault(), skillB.Id.GetValueOrDefault() }
																				   }
																			   });

			ScheduleForecastSkillReadModelRepository.Persist(new[]
															 {
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.EndDateTime,
																	 Forecast = 5,
																	 SkillId = skillA.Id.GetValueOrDefault()
																 },
																  new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.EndDateTime,
																	 Forecast = 5,
																	 SkillId = skillB.Id.GetValueOrDefault()
																 }
															 }, Now.UtcDateTime());

			var staffingIntervals = Target.StaffingForSkills(new[] { skillA.Id.GetValueOrDefault()}, period, TimeSpan.FromHours(1));

			staffingIntervals.Count.Should().Be.EqualTo(1);
			staffingIntervals.Single().StaffingLevel.Should().Be.EqualTo(5);
		}

		[Test]
		public void ShouldHandleDifferentActivities()
		{
			Now.Is(new DateTime(2016, 12, 1, 07, 00, 00, DateTimeKind.Utc));

			var activityA = ActivityRepository.Has("activityA");
			var activityB = ActivityRepository.Has("activityB");
			var skillA = SkillRepository.Has("skillA", activityA).WithId();
			var skillB = SkillRepository.Has("skillB", activityB).WithId();
			skillA.DefaultResolution = skillB.DefaultResolution = 60;

			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
																			   {
																				   new SkillCombinationResource
																				   {
																					   StartDateTime = period.StartDateTime,
																					   EndDateTime = period.EndDateTime,
																					   Resource = 5,
																					   SkillCombination = new[] {skillA.Id.GetValueOrDefault() }
																				   },
																				   new SkillCombinationResource
																				   {
																					   StartDateTime = period.StartDateTime,
																					   EndDateTime = period.EndDateTime,
																					   Resource = 5,
																					   SkillCombination = new[] {skillB.Id.GetValueOrDefault() }
																				   }
																			   });

			ScheduleForecastSkillReadModelRepository.Persist(new[]
															 {
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.EndDateTime,
																	 Forecast = 5,
																	 SkillId = skillA.Id.GetValueOrDefault()
																 },
																  new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.EndDateTime,
																	 Forecast = 5,
																	 SkillId = skillB.Id.GetValueOrDefault()
																 }
															 }, Now.UtcDateTime());

			var staffingIntervals = Target.StaffingForSkills(new[] { skillA.Id.GetValueOrDefault(), skillB.Id.GetValueOrDefault() }, period, TimeSpan.FromHours(1));

			staffingIntervals.Count.Should().Be.EqualTo(2);
			staffingIntervals.First().StaffingLevel.Should().Be.EqualTo(5);
			staffingIntervals.Second().StaffingLevel.Should().Be.EqualTo(5);
		}

		[Test]
		public void ShouldHandlePeriodLargerThanSkillResolution()
		{
			Now.Is(new DateTime(2016, 12, 1, 07, 00, 00, DateTimeKind.Utc));
			const int skillResolution = 30;

			var activity = ActivityRepository.Has("activity");
			var skillA = SkillRepository.Has("skillA", activity).WithId();
			var skillB = SkillRepository.Has("skillB", activity).WithId();
			skillA.DefaultResolution = skillB.DefaultResolution = skillResolution;

			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
																			   {
																				   new SkillCombinationResource
																				   {
																					   StartDateTime = period.StartDateTime,
																					   EndDateTime = period.StartDateTime.AddMinutes(skillResolution),
																					   Resource = 2,
																					   SkillCombination = new[] {skillA.Id.GetValueOrDefault(), skillB.Id.GetValueOrDefault() }
																				   },
																				   new SkillCombinationResource
																				   {
																					   StartDateTime = period.StartDateTime.AddMinutes(skillResolution),
																					   EndDateTime = period.EndDateTime,
																					   Resource = 4,
																					  SkillCombination = new[] {skillA.Id.GetValueOrDefault(), skillB.Id.GetValueOrDefault() }
																				   }
																			   });

			ScheduleForecastSkillReadModelRepository.Persist(new[]
															 {
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.StartDateTime.AddMinutes(skillResolution),
																	 Forecast = 5,
																	 SkillId = skillA.Id.GetValueOrDefault()
																 },
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime.AddMinutes(skillResolution),
																	 EndDateTime = period.EndDateTime,
																	 Forecast = 5,
																	 SkillId = skillA.Id.GetValueOrDefault()
																 },
																  new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.StartDateTime.AddMinutes(skillResolution),
																	 Forecast = 5,
																	 SkillId = skillB.Id.GetValueOrDefault()
																 },
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime.AddMinutes(skillResolution),
																	 EndDateTime = period.EndDateTime,
																	 Forecast = 5,
																	 SkillId = skillB.Id.GetValueOrDefault()
																 }
															 }, Now.UtcDateTime());

			var staffingIntervals = Target.StaffingForSkills(new[] { skillA.Id.GetValueOrDefault(), skillB.Id.GetValueOrDefault() }, period, TimeSpan.FromHours(1));

			staffingIntervals.Count.Should().Be.EqualTo(4);
			staffingIntervals.Single(x => x.StartDateTime == period.StartDateTime && x.Id == skillA.Id.GetValueOrDefault()).StaffingLevel.Should().Be.EqualTo(1);
			staffingIntervals.Single(x => x.StartDateTime == period.StartDateTime && x.Id == skillB.Id.GetValueOrDefault()).StaffingLevel.Should().Be.EqualTo(1);
			staffingIntervals.Single(x => x.StartDateTime == period.StartDateTime.AddMinutes(skillResolution) && x.Id == skillA.Id.GetValueOrDefault()).StaffingLevel.Should().Be.EqualTo(2);
			staffingIntervals.Single(x => x.StartDateTime == period.StartDateTime.AddMinutes(skillResolution) && x.Id == skillB.Id.GetValueOrDefault()).StaffingLevel.Should().Be.EqualTo(2);
		}


		[Test]
		public void ShouldHandleCascadingSkills()
		{
			Now.Is(new DateTime(2016, 12, 1, 07, 00, 00, DateTimeKind.Utc));

			var activity = ActivityRepository.Has("activity");
			var skillA = SkillRepository.Has("skillA", activity).WithId();
			skillA.SetCascadingIndex(1);
			var skillB = SkillRepository.Has("skillB", activity).WithId();
			skillB.SetCascadingIndex(2);
			skillA.DefaultResolution = skillB.DefaultResolution = 60;

			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
																			   {
																				   new SkillCombinationResource
																				   {
																					   StartDateTime = period.StartDateTime,
																					   EndDateTime = period.EndDateTime,
																					   Resource = 10,
																					   SkillCombination = new[] {skillA.Id.GetValueOrDefault(), skillB.Id.GetValueOrDefault() }
																				   }
																			   });

			ScheduleForecastSkillReadModelRepository.Persist(new[]
															 {
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.EndDateTime,
																	 Forecast = 7,
																	 SkillId = skillA.Id.GetValueOrDefault()
																 },
																  new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.EndDateTime,
																	 Forecast = 7,
																	 SkillId = skillB.Id.GetValueOrDefault()
																 }
															 }, Now.UtcDateTime());

			var staffingIntervals = Target.StaffingForSkills(new[] { skillA.Id.GetValueOrDefault(), skillB.Id.GetValueOrDefault() }, period, TimeSpan.FromHours(1));

			staffingIntervals.Count.Should().Be.EqualTo(2);
			staffingIntervals.Single(x => x.StartDateTime == period.StartDateTime && x.Id == skillA.Id.GetValueOrDefault()).StaffingLevel.Should().Be.EqualTo(7);
			staffingIntervals.Single(x => x.StartDateTime == period.StartDateTime && x.Id == skillB.Id.GetValueOrDefault()).StaffingLevel.Should().Be.EqualTo(3);
		}



		[Test]
		[Ignore("WIP")]
		public void ShouldFetchIntervalsForASkillWithinRange()
		{
			Guid skillId = Guid.NewGuid();
			var period = new DateTimePeriod(dateTimeIs(8,0), dateTimeIs(8,30));
			var resolution = TimeSpan.FromMinutes(15);

			IEnumerable<SkillStaffingInterval> items = new List<SkillStaffingInterval>()
			{
				intervalHaving(new TimeSpan(7,45,0),new TimeSpan(8,0,0),5.7,skillId ),
				intervalHaving(new TimeSpan(8,0,0),new TimeSpan(8,15,0),9,skillId ),
				intervalHaving(new TimeSpan(8,15,0),new TimeSpan(8,30,0),2.8,skillId ),
				intervalHaving(new TimeSpan(8,30,0),new TimeSpan(8,45,0),4.3,skillId ),
				intervalHaving(new TimeSpan(8,15,0),new TimeSpan(8,30,0),5.7,Guid.NewGuid() ),
			};
			ScheduleForecastSkillReadModelRepository.Persist(items,DateTime.Now);

			var result = Target.StaffingForSkills(new[] {skillId}, period, resolution);
			result.Count.Should().Be.EqualTo(2);
			result.Should().Contain(lightIntervalHaving(skillId, new TimeSpan(8, 0,0), new TimeSpan( 8, 15,0), 9));
			result.Should().Contain(lightIntervalHaving(skillId, new TimeSpan( 8, 15,0), new TimeSpan(8, 30,0), 2.8));
		}

		[Test]
		[Ignore("WIP")]
		public void ShouldFetchIntervalsForOnlyTargetedSkill()
		{
			var skillId = Guid.NewGuid();
			var period = new DateTimePeriod(dateTimeIs(8, 0), dateTimeIs(8, 30));
			var resolution = TimeSpan.FromMinutes(5);

			IEnumerable<SkillStaffingInterval> items = new List<SkillStaffingInterval>()
			{
				intervalHaving(new TimeSpan(7,45,0),new TimeSpan(8,0,0),5.7,skillId ),
				intervalHaving(new TimeSpan(8,0,0),new TimeSpan(8,15,0),9,skillId ),
				intervalHaving(new TimeSpan(8,15,0),new TimeSpan(8,30,0),2.8,skillId ),
				intervalHaving(new TimeSpan(8,30,0),new TimeSpan(8,45,0),4.3,skillId ),
				intervalHaving(new TimeSpan(8,15,0),new TimeSpan(8,30,0),5.7,Guid.NewGuid() ),
			};
			ScheduleForecastSkillReadModelRepository.Persist(items, DateTime.Now);

			var result = Target.StaffingForSkills(new[] {skillId}, period, resolution);
			result.Count.Should().Be.EqualTo(6);
			result.Should().Contain(lightIntervalHaving(skillId, new TimeSpan(8, 0, 0), new TimeSpan(8, 5, 0), 9));
			result.Should().Contain(lightIntervalHaving(skillId, new TimeSpan(8, 5, 0), new TimeSpan(8, 10, 0), 9));
			result.Should().Contain(lightIntervalHaving(skillId, new TimeSpan(8, 10, 0), new TimeSpan(8, 15, 0), 9));
			result.Should().Contain(lightIntervalHaving(skillId, new TimeSpan(8, 15, 0), new TimeSpan(8, 20, 0), 2.8));
			result.Should().Contain(lightIntervalHaving(skillId, new TimeSpan(8, 20, 0), new TimeSpan(8, 25, 0), 2.8));
			result.Should().Contain(lightIntervalHaving(skillId, new TimeSpan(8, 25, 0), new TimeSpan(8, 30, 0), 2.8));
		}

		[Test]
		[Ignore("WIP")]
		public void ShouldFetchMergedIntervalsWithChanges()
		{
			var skillId = Guid.NewGuid();
			var period = new DateTimePeriod(dateTimeIs(8, 0), dateTimeIs(8, 40));
			var resolution = TimeSpan.FromMinutes(20);

			IEnumerable<SkillStaffingInterval> items = new List<SkillStaffingInterval>()
			{
				intervalHaving(new TimeSpan(8,0,0),new TimeSpan(8,20,0),9,skillId ),
				intervalHaving(new TimeSpan(8,20,0),new TimeSpan(8,40,0),2.8,skillId )
			};
			ScheduleForecastSkillReadModelRepository.Persist(items, DateTime.Now);
			ScheduleForecastSkillReadModelRepository.PersistChange(new StaffingIntervalChange()
			{
				SkillId = skillId,
				StartDateTime = new DateTime(2016, 11, 03, 8, 0, 0, DateTimeKind.Utc),
				EndDateTime = new DateTime(2016, 11, 03, 8, 20, 0, DateTimeKind.Utc),
				StaffingLevel = 3
			});

			var result = Target.StaffingForSkills(new[] {skillId}, period, resolution);
			result.Count.Should().Be.EqualTo(2);
			result.Should().Contain(lightIntervalHaving(skillId, new TimeSpan(8, 0, 0), new TimeSpan(8, 20, 0), 12));
			result.Should().Contain(lightIntervalHaving(skillId, new TimeSpan(8, 20, 0), new TimeSpan(8, 40, 0), 2.8));
		}

		[Test]
		[Ignore("WIP")]
		public void ShouldFetchIntervalsWithinPeriod()
		{
			var skillId = Guid.NewGuid();
			var period = new DateTimePeriod(dateTimeIs(7, 50), dateTimeIs(8, 30));
			var resolution = TimeSpan.FromMinutes(10);

			IEnumerable<SkillStaffingInterval> items = new List<SkillStaffingInterval>()
			{
				intervalHaving(new TimeSpan(7,40,0),new TimeSpan(8,0,0),5,skillId ),
				intervalHaving(new TimeSpan(8,0,0),new TimeSpan(8,20,0),9,skillId ),
				intervalHaving(new TimeSpan(8,20,0),new TimeSpan(8,40,0),2.8,skillId )
			};
			ScheduleForecastSkillReadModelRepository.Persist(items, DateTime.Now);
			
			var result = Target.StaffingForSkills(new[] { skillId }, period, resolution);
			result.Count.Should().Be.EqualTo(4);
			result.Should().Contain(lightIntervalHaving(skillId, new TimeSpan(7, 50, 0), new TimeSpan(8, 0, 0), 5));
			result.Should().Contain(lightIntervalHaving(skillId, new TimeSpan(8, 0, 0), new TimeSpan(8, 10, 0), 9));
			result.Should().Contain(lightIntervalHaving(skillId, new TimeSpan(8, 10, 0), new TimeSpan(8, 20, 0), 9));
			result.Should().Contain(lightIntervalHaving(skillId, new TimeSpan(8, 20, 0), new TimeSpan(8, 30, 0), 2.8));
		}

		[Test]
		[Ignore("WIP")]
		public void ShouldReturnIntervalsForTwoSkills()
		{
			var skill1 = Guid.NewGuid();
			var skill2 = Guid.NewGuid();
			var period = new DateTimePeriod(dateTimeIs(8, 0), dateTimeIs(9, 0));
			var resolution = TimeSpan.FromMinutes(20);

			IEnumerable<SkillStaffingInterval> items = new List<SkillStaffingInterval>()
			{
				intervalHaving(new TimeSpan(8,0,0),new TimeSpan(8,20,0),9,skill1 ),
				intervalHaving(new TimeSpan(8,20,0),new TimeSpan(8,40,0),2.8,skill2 )
			};
			ScheduleForecastSkillReadModelRepository.Persist(items, DateTime.Now);

			var result = Target.StaffingForSkills(new[] { skill1, skill2}, period, resolution);

			result.Count.Should().Be.EqualTo(2);
			result.Should().Contain(lightIntervalHaving(skill1, new TimeSpan(8, 0, 0), new TimeSpan(8, 20, 0), 9));
			result.Should().Contain(lightIntervalHaving(skill2, new TimeSpan(8, 20, 0), new TimeSpan(8, 40, 0), 2.8));
		}

		[Test]
		[Ignore("WIP")]
		public void ShouldMergeSimilarIntervalsForSkillArea()
		{
			var skill1 = Guid.NewGuid();
			var skill2 = Guid.NewGuid();
			var period = new DateTimePeriod(dateTimeIs(8, 0), dateTimeIs(9, 0));
			var resolution = TimeSpan.FromMinutes(10);

			IEnumerable<SkillStaffingInterval> items = new List<SkillStaffingInterval>()
			{
				intervalHaving(new TimeSpan(8,0,0),new TimeSpan(8,20,0),9,skill1 ),
				intervalHaving(new TimeSpan(8,10,0),new TimeSpan(8,20,0),2.8,skill2 )
			};
			ScheduleForecastSkillReadModelRepository.Persist(items, DateTime.Now);

			var result = Target.StaffingForSkills(new []{skill1, skill2}, period, resolution);

			result.Count.Should().Be.EqualTo(3);
			result.Should().Contain(lightIntervalHaving(skill1, new TimeSpan(8, 0, 0), new TimeSpan(8, 10, 0), 9));
			result.Should().Contain(lightIntervalHaving(skill1, new TimeSpan(8, 10, 0), new TimeSpan(8, 20, 0), 9));
			result.Should().Contain(lightIntervalHaving(skill2, new TimeSpan(8, 10, 0), new TimeSpan(8, 20, 0), 2.8));
		}

		private SkillStaffingInterval intervalHaving(TimeSpan from, TimeSpan to, double staffing, Guid skillId)
		{
			var today = new DateTime(2016, 11, 03, 0, 0, 0, DateTimeKind.Utc);
			return new SkillStaffingInterval()
			{
				SkillId = skillId,
				StartDateTime = today.Add(from),
				EndDateTime = today.Add(to),
				StaffingLevel = staffing
			};
		}

		private SkillStaffingIntervalLightModel lightIntervalHaving(Guid skillId, TimeSpan @from, TimeSpan to, double staffing)
		{
			var today = new DateTime(2016, 11, 03, 0, 0, 0, DateTimeKind.Utc);
			return new SkillStaffingIntervalLightModel()
			{
				Id = skillId,
				StartDateTime = today.Add(from),
				EndDateTime = today.Add(to),
				StaffingLevel = staffing
			};
		}

		private DateTime dateTimeIs(int hour, int minutes)
		{
			return new DateTime(2016, 11, 03, hour, minutes, 0, DateTimeKind.Utc);
		}
	}

	
}