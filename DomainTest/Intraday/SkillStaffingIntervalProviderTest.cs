using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.IocCommon;
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
		public ISkillAreaRepository SkillAreaRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<SkillStaffingIntervalProvider>().For<SkillStaffingIntervalProvider>();
		}

		[Test]
		public void ShouldFetchIntervalsForASkillWithinRange()
		{
			var skillId = Guid.NewGuid();
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

			var result = Target.StaffingForSkill(skillId, period, resolution);
			result.Count.Should().Be.EqualTo(2);
			result.Should().Contain(lightIntervalHaving(new TimeSpan(8, 0,0), new TimeSpan( 8, 15,0), 9, skillId));
			result.Should().Contain(lightIntervalHaving(new TimeSpan( 8, 15,0), new TimeSpan(8, 30,0), 2.8, skillId));
		}

		[Test]
		public void ShouldFetchIntervalsForASkillDifferent()
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

			var result = Target.StaffingForSkill(skillId, period, resolution);
			result.Count.Should().Be.EqualTo(6);
			result.Should().Contain(lightIntervalHaving(new TimeSpan(8, 0, 0), new TimeSpan(8, 5, 0), 3, skillId));
			result.Should().Contain(lightIntervalHaving(new TimeSpan(8, 5, 0), new TimeSpan(8, 10, 0), 3, skillId));
			result.Should().Contain(lightIntervalHaving(new TimeSpan(8, 10, 0), new TimeSpan(8, 15, 0), 3, skillId));
			result.Should().Contain(lightIntervalHaving(new TimeSpan(8, 15, 0), new TimeSpan(8, 20, 0), 0.93333, skillId));
			result.Should().Contain(lightIntervalHaving(new TimeSpan(8, 20, 0), new TimeSpan(8, 25, 0), 0.93333, skillId));
			result.Should().Contain(lightIntervalHaving(new TimeSpan(8, 25, 0), new TimeSpan(8, 30, 0), 0.93333, skillId));
		}

		[Test]
		public void ShouldFetchMergedIntervalsWithChanges()
		{
			var skillId = Guid.NewGuid();
			var period = new DateTimePeriod(dateTimeIs(8, 0), dateTimeIs(8, 30));
			var resolution = TimeSpan.FromMinutes(10);

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

			var result = Target.StaffingForSkill(skillId, period, resolution);
			result.Count.Should().Be.EqualTo(4);
			result.Should().Contain(lightIntervalHaving(new TimeSpan(8, 0, 0), new TimeSpan(8, 10, 0), 6, skillId));
			result.Should().Contain(lightIntervalHaving(new TimeSpan(8, 10, 0), new TimeSpan(8, 20, 0), 6, skillId));
			result.Should().Contain(lightIntervalHaving(new TimeSpan(8, 20, 0), new TimeSpan(8, 30, 0), 1.4, skillId));
			result.Should().Contain(lightIntervalHaving(new TimeSpan(8, 30, 0), new TimeSpan(8, 40, 0), 1.4, skillId));
		}

		[Test]
		public void ShouldReturnIntervalsForASkillArea()
		{
			var skill1 = Guid.NewGuid();
			var skill2 = Guid.NewGuid();
			ICollection<SkillInIntraday> skills = new List<SkillInIntraday>() {new SkillInIntraday() {Id = skill1} ,  new SkillInIntraday() { Id = skill2 } };
			var skillArea = new SkillArea() {Name = "something", Skills = skills};
			skillArea.SetId(Guid.NewGuid());
			SkillAreaRepository.Add(skillArea);
			
			var period = new DateTimePeriod(dateTimeIs(8, 0), dateTimeIs(9, 0));
			var resolution = TimeSpan.FromMinutes(20);

			IEnumerable<SkillStaffingInterval> items = new List<SkillStaffingInterval>()
			{
				intervalHaving(new TimeSpan(8,0,0),new TimeSpan(8,20,0),9,skill1 ),
				intervalHaving(new TimeSpan(8,20,0),new TimeSpan(8,40,0),2.8,skill2 )
			};
			ScheduleForecastSkillReadModelRepository.Persist(items, DateTime.Now);

			var result = Target.StaffingForSkillArea(skillArea.Id.GetValueOrDefault(), period, resolution);

			result.Count.Should().Be.EqualTo(2);
			result.Should().Contain(lightIntervalHaving(new TimeSpan(8, 0, 0), new TimeSpan(8, 20, 0), 9, skillArea.Id.GetValueOrDefault()));
			result.Should().Contain(lightIntervalHaving(new TimeSpan(8, 20, 0), new TimeSpan(8, 40, 0), 2.8, skillArea.Id.GetValueOrDefault()));
		}

		[Test]
		public void ShouldReturnIntervalsForASkillAreaWithDifferentIntervals()
		{
			var skill1 = Guid.NewGuid();
			var skill2 = Guid.NewGuid();
			ICollection<SkillInIntraday> skills = new List<SkillInIntraday>() { new SkillInIntraday() { Id = skill1 }, new SkillInIntraday() { Id = skill2 } };
			var skillArea = new SkillArea() { Name = "something", Skills = skills };
			skillArea.SetId(Guid.NewGuid());
			SkillAreaRepository.Add(skillArea);

			var period = new DateTimePeriod(dateTimeIs(8, 0), dateTimeIs(9, 0));
			var resolution = TimeSpan.FromMinutes(10);

			IEnumerable<SkillStaffingInterval> items = new List<SkillStaffingInterval>()
			{
				intervalHaving(new TimeSpan(8,0,0),new TimeSpan(8,20,0),9,skill1 ),
				intervalHaving(new TimeSpan(8,20,0),new TimeSpan(8,30,0),2.8,skill2 )
			};
			ScheduleForecastSkillReadModelRepository.Persist(items, DateTime.Now);

			var result = Target.StaffingForSkillArea(skillArea.Id.GetValueOrDefault(), period, resolution);

			result.Count.Should().Be.EqualTo(3);
			result.Should().Contain(lightIntervalHaving(new TimeSpan(8, 0, 0), new TimeSpan(8, 10, 0), 4.5, skillArea.Id.GetValueOrDefault()));
			result.Should().Contain(lightIntervalHaving(new TimeSpan(8, 10, 0), new TimeSpan(8, 20, 0), 4.5, skillArea.Id.GetValueOrDefault()));
			result.Should().Contain(lightIntervalHaving(new TimeSpan(8, 20, 0), new TimeSpan(8, 30, 0), 2.8, skillArea.Id.GetValueOrDefault()));
		}

		[Test]
		public void ShouldMergeSimilarIntervalsForSkillArea()
		{
			var skill1 = Guid.NewGuid();
			var skill2 = Guid.NewGuid();
			ICollection<SkillInIntraday> skills = new List<SkillInIntraday>() { new SkillInIntraday() { Id = skill1 }, new SkillInIntraday() { Id = skill2 } };
			var skillArea = new SkillArea() { Name = "something", Skills = skills };
			skillArea.SetId(Guid.NewGuid());
			SkillAreaRepository.Add(skillArea);

			var period = new DateTimePeriod(dateTimeIs(8, 0), dateTimeIs(9, 0));
			var resolution = TimeSpan.FromMinutes(10);

			IEnumerable<SkillStaffingInterval> items = new List<SkillStaffingInterval>()
			{
				intervalHaving(new TimeSpan(8,0,0),new TimeSpan(8,20,0),9,skill1 ),
				intervalHaving(new TimeSpan(8,10,0),new TimeSpan(8,20,0),2.8,skill2 )
			};
			ScheduleForecastSkillReadModelRepository.Persist(items, DateTime.Now);

			var result = Target.StaffingForSkillArea(skillArea.Id.GetValueOrDefault(), period, resolution);

			result.Count.Should().Be.EqualTo(2);
			result.Should().Contain(lightIntervalHaving(new TimeSpan(8, 0, 0), new TimeSpan(8, 10, 0), 4.5, skillArea.Id.GetValueOrDefault()));
			result.Should().Contain(lightIntervalHaving(new TimeSpan(8, 10, 0), new TimeSpan(8, 20, 0), 7.3, skillArea.Id.GetValueOrDefault()));
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

		private SkillStaffingIntervalLightModel lightIntervalHaving(TimeSpan from, TimeSpan to, double staffing, Guid skillId)
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