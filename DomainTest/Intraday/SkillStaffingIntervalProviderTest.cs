﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
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

			var staffingIntervals = Target.StaffingForSkills(new[] {skill.Id.GetValueOrDefault()},period, TimeSpan.FromHours(1), false);

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

			var staffingIntervals = Target.StaffingForSkills(new[] { skillA.Id.GetValueOrDefault(), skillB.Id.GetValueOrDefault() }, period, TimeSpan.FromHours(1), false);

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

			var staffingIntervals = Target.StaffingForSkills(new[] { skillA.Id.GetValueOrDefault()}, period, TimeSpan.FromHours(1), false);

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

			var staffingIntervals = Target.StaffingForSkills(new[] { skillA.Id.GetValueOrDefault(), skillB.Id.GetValueOrDefault() }, period, TimeSpan.FromHours(1), false);

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

			var staffingIntervals = Target.StaffingForSkills(new[] { skillA.Id.GetValueOrDefault(), skillB.Id.GetValueOrDefault() }, period, TimeSpan.FromMinutes(skillResolution), false);

			staffingIntervals.Count.Should().Be.EqualTo(4);
			staffingIntervals.Single(x => x.StartDateTime == period.StartDateTime && x.Id == skillA.Id.GetValueOrDefault()).StaffingLevel.Should().Be.EqualTo(1);
			staffingIntervals.Single(x => x.StartDateTime == period.StartDateTime && x.Id == skillB.Id.GetValueOrDefault()).StaffingLevel.Should().Be.EqualTo(1);
			staffingIntervals.Single(x => x.StartDateTime == period.StartDateTime.AddMinutes(skillResolution) && x.Id == skillA.Id.GetValueOrDefault()).StaffingLevel.Should().Be.EqualTo(2);
			staffingIntervals.Single(x => x.StartDateTime == period.StartDateTime.AddMinutes(skillResolution) && x.Id == skillB.Id.GetValueOrDefault()).StaffingLevel.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldHandleDifferentSkillResolutions()
		{
			Now.Is(new DateTime(2016, 12, 1, 07, 00, 00, DateTimeKind.Utc));

			var activityA = ActivityRepository.Has("activityA");
			var activityB = ActivityRepository.Has("activityB");
			var skillA = SkillRepository.Has("skillA", activityA).WithId();
			var skillB = SkillRepository.Has("skillB", activityB).WithId();
			skillA.DefaultResolution = 30;
			skillB.DefaultResolution = 60;

			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
																			   {
																				   new SkillCombinationResource
																				   {
																					   StartDateTime = period.StartDateTime,
																					   EndDateTime = period.StartDateTime.AddMinutes(30),
																					   Resource = 2,
																					   SkillCombination = new[] {skillA.Id.GetValueOrDefault()}
																				   },
																				   new SkillCombinationResource
																				   {
																					   StartDateTime = period.StartDateTime.AddMinutes(30),
																					   EndDateTime = period.EndDateTime,
																					   Resource = 2,
																					   SkillCombination = new[] {skillA.Id.GetValueOrDefault()}
																				   },
																				   new SkillCombinationResource
																				   {
																					   StartDateTime = period.StartDateTime,
																					   EndDateTime = period.EndDateTime,
																					   Resource = 4,
																					   SkillCombination = new[] {skillB.Id.GetValueOrDefault()}
																				   }
																			   });

			ScheduleForecastSkillReadModelRepository.Persist(new[]
															 {
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.StartDateTime.AddMinutes(30),
																	 Forecast = 5,
																	 SkillId = skillA.Id.GetValueOrDefault()
																 },
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime.AddMinutes(30),
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

			var staffingIntervals = Target.StaffingForSkills(new[] { skillA.Id.GetValueOrDefault(), skillB.Id.GetValueOrDefault() }, period, TimeSpan.FromMinutes(30), false);

			staffingIntervals.Count.Should().Be.EqualTo(4);
			staffingIntervals.Single(x => x.StartDateTime == period.StartDateTime && x.Id == skillA.Id.GetValueOrDefault()).StaffingLevel.Should().Be.EqualTo(2);
			staffingIntervals.Single(x => x.StartDateTime == period.StartDateTime && x.Id == skillB.Id.GetValueOrDefault()).StaffingLevel.Should().Be.EqualTo(4);
			staffingIntervals.Single(x => x.StartDateTime == period.StartDateTime.AddMinutes(30) && x.Id == skillA.Id.GetValueOrDefault()).StaffingLevel.Should().Be.EqualTo(2);
			staffingIntervals.Single(x => x.StartDateTime == period.StartDateTime.AddMinutes(30) && x.Id == skillB.Id.GetValueOrDefault()).StaffingLevel.Should().Be.EqualTo(4);
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

			var staffingIntervals = Target.StaffingForSkills(new[] { skillA.Id.GetValueOrDefault(), skillB.Id.GetValueOrDefault() }, period, TimeSpan.FromHours(1), false);

			staffingIntervals.Count.Should().Be.EqualTo(2);
			staffingIntervals.Single(x => x.StartDateTime == period.StartDateTime && x.Id == skillA.Id.GetValueOrDefault()).StaffingLevel.Should().Be.EqualTo(7);
			staffingIntervals.Single(x => x.StartDateTime == period.StartDateTime && x.Id == skillB.Id.GetValueOrDefault()).StaffingLevel.Should().Be.EqualTo(3);
		}


		[Test]
		public void ShouldFetchMergedIntervalsWithChanges()
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
																					   Resource = 5.7,
																					   SkillCombination = new[] {skill.Id.GetValueOrDefault()}
																				   }
																			   });

			SkillCombinationResourceRepository.PersistChanges(new[]
															  {
																  new SkillCombinationResource
																  {
																	  StartDateTime = period.StartDateTime,
																	  EndDateTime = period.EndDateTime,
																	  Resource = 1,
																	  SkillCombination = new[] {skill.Id.GetValueOrDefault()}
																  },
																   new SkillCombinationResource
																  {
																	  StartDateTime = period.StartDateTime,
																	  EndDateTime = period.EndDateTime,
																	  Resource = 3,
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

			var staffingIntervals = Target.StaffingForSkills(new[] { skill.Id.GetValueOrDefault() }, period, TimeSpan.FromHours(1), false);

			staffingIntervals.Count.Should().Be.EqualTo(1);
			staffingIntervals.Single().StaffingLevel.Should().Be.EqualTo(8.7);
		}


		[Test]
		public void ShouldResourceCalculateOnTwoSkillsWithSingleCombinations()
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
																					   Resource = 9,
																					   SkillCombination = new[] {skillA.Id.GetValueOrDefault()}
																				   },
																					new SkillCombinationResource
																				   {
																					   StartDateTime = period.StartDateTime,
																					   EndDateTime = period.EndDateTime,
																					   Resource = 2.8,
																					   SkillCombination = new[] {skillB.Id.GetValueOrDefault()}
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

			var staffingIntervals = Target.StaffingForSkills(new[] { skillA.Id.GetValueOrDefault(), skillB.Id.GetValueOrDefault() }, period, TimeSpan.FromHours(1), false);

			staffingIntervals.Count.Should().Be.EqualTo(2);

			staffingIntervals.Single(x => x.StartDateTime == period.StartDateTime && x.Id == skillA.Id.GetValueOrDefault()).StaffingLevel.Should().Be.EqualTo(9);
			staffingIntervals.Single(x => x.StartDateTime == period.StartDateTime && x.Id == skillB.Id.GetValueOrDefault()).StaffingLevel.Should().Be.EqualTo(2.8);
		}
		
	}

	
}