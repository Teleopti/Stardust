using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
	public class ResourceCalculationDataContainerFromStorageTest
    {
		private ResourceCalculationDataContainerFromStorage _target;
		private readonly DateTimePeriod _period = new DateTimePeriod(new DateTime(2013, 8, 16, 12, 0, 0, DateTimeKind.Utc), new DateTime(2013, 8, 16, 12, 15, 0, DateTimeKind.Utc));
		private readonly IActivity _activity = ActivityFactory.CreateActivity("Phone");
		private readonly ISkill _skill = SkillFactory.CreateSkill("Skill1");
	    private ActivitySkillCombinationFromStorage[] _activitySkillCombinations;

	    [SetUp]
        public void Setup()
		{
			_skill.Activity = _activity;
			_skill.SetId(Guid.NewGuid());
			_activity.SetId(Guid.NewGuid());
		    _activitySkillCombinations = new[]
			    {
				    new ActivitySkillCombinationFromStorage
					    {
						    Activity = _activity.Id.GetValueOrDefault(),
						    ActivityRequiresSeat = false,
						    Id = 1,
						    Skills = _skill.Id.ToString()
					    }
			    };
		    _target = new ResourceCalculationDataContainerFromStorage();
        }

		[Test]
		public void ShouldGetResourcesOnSkill()
		{
			addResourcesForTest(_period,0.8);
			var result = _target.SkillResources(_skill, _period);
			result.Item2.Should().Be.EqualTo(0.8);
			result.Item1.Should().Be.EqualTo(0.8);
		}

		[Test]
		public void ShouldNotGetResourcesWhereSeatIsNotRequired()
		{
			addResourcesForTest(_period, 0.8);
			var result = _target.ActivityResourcesWhereSeatRequired(_skill, _period);
			result.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldClearResources()
		{
			addResourcesForTest(_period, 0.8);
			_target.HasItems().Should().Be.True();
			_target.Clear();
			_target.HasItems().Should().Be.False();
		}

		[Test]
		public void ShouldGetAffectedSkillsWithProficiency()
		{
			addResourcesForTest(_period, 0.8,
			                    new[]
				                    {
					                    new SkillEfficienciesFromStorage
						                    {
							                    Amount = 0.9,
							                    ParentId = 2,
							                    SkillId = _skill.Id.GetValueOrDefault()
						                    }
				                    });
			
			var result = _target.AffectedResources(_activity, _period);
			var affectedSkill = result.First().Value;
			affectedSkill.SkillEffiencies[_skill.Id.GetValueOrDefault()].Should().Be.EqualTo(0.9);
			affectedSkill.Resource.Should().Be.EqualTo(0.8);
			affectedSkill.Skills.First().Should().Be.EqualTo(_skill);
		}

		[Test]
		public void ShouldGetAffectedSkills()
		{
			addResourcesForTest(_period, 0.8); 
			var result = _target.AffectedResources(_activity, _period);
			var affectedSkill = result.First().Value;
			affectedSkill.SkillEffiencies.Count.Should().Be.EqualTo(0);
			affectedSkill.Resource.Should().Be.EqualTo(0.8);
			affectedSkill.Skills.First().Should().Be.EqualTo(_skill);
		}

		[Test]
		public void ShouldGetAffectedSkillsForMultipleIntervalsWithEfficiency()
		{
			addResourcesForTest(_period, 0.8, _period.MovePeriod(TimeSpan.FromMinutes(15)), 0.7,
			                    new[]
				                    {
					                    new SkillEfficienciesFromStorage
						                    {
							                    Amount = 0.9,
							                    ParentId = 2,
							                    SkillId = _skill.Id.GetValueOrDefault()
						                    },
					                    new SkillEfficienciesFromStorage
						                    {
							                    Amount = 0.9,
							                    ParentId = 3,
							                    SkillId = _skill.Id.GetValueOrDefault()
						                    }
				                    });

			var result = _target.AffectedResources(_activity, _period.ChangeEndTime(TimeSpan.FromMinutes(15)));
			var affectedSkill = result.First().Value;
			affectedSkill.SkillEffiencies[_skill.Id.GetValueOrDefault()].Should().Be.EqualTo(1.8);
			affectedSkill.Resource.Should().Be.EqualTo(1.5);
			affectedSkill.Skills.First().Should().Be.EqualTo(_skill);
		}

		[Test]
		public void ShouldGetResourcesWhereSeatIsRequired()
		{
			_activitySkillCombinations[0].ActivityRequiresSeat = true;
			addResourcesForTest(_period, 0.8);

			var result = _target.ActivityResourcesWhereSeatRequired(_skill, _period);
			result.Should().Be.EqualTo(0.8);
		}

		[Test]
		public void ShouldSumResourcesOnSkillForTwoIntervals()
		{
			addResourcesForTest(_period, 0.8, _period.MovePeriod(TimeSpan.FromMinutes(15)), 0.7);

			var result = _target.SkillResources(_skill, _period.ChangeEndTime(TimeSpan.FromMinutes(15)));
			result.Item2.Should().Be.EqualTo(0.75);
			result.Item1.Should().Be.EqualTo(0.75);
		}

		[Test]
		public void ShouldGetZeroResourcesOnSkillWithOtherActivity()
		{
			_skill.Activity = ActivityFactory.CreateActivity("Phone 2");
			_skill.Activity.SetId(Guid.NewGuid());

			addResourcesForTest(_period, 0.8);

			var result = _target.SkillResources(_skill, _period);
			result.Item2.Should().Be.EqualTo(0);
			result.Item1.Should().Be.EqualTo(0);
		}

		private void addResourcesForTest(DateTimePeriod period, double resources, IEnumerable<SkillEfficienciesFromStorage> skillEfficiencies = null)
		{
			_target.AddResources(new ResourcesFromStorage(
						 new[]
					                     {
						                     new ResourcesForCombinationFromStorage
							                     {
								                     ActivitySkillCombinationId = 1,
								                     Heads = 1,
								                     Id = 2,
								                     PeriodEnd = period.EndDateTime,
								                     PeriodStart = period.StartDateTime,
													 Resources = resources
							                     }
					                     }, _activitySkillCombinations,
						 skillEfficiencies ?? Enumerable.Empty<SkillEfficienciesFromStorage>(), new[] { _skill }), 15);
		}

		private void addResourcesForTest(DateTimePeriod period, double resources, DateTimePeriod period2, double resources2, IEnumerable<SkillEfficienciesFromStorage> skillEfficiencies = null)
		{
			_target.AddResources(new ResourcesFromStorage(
						 new[]
					                     {
						                     new ResourcesForCombinationFromStorage
							                     {
								                     ActivitySkillCombinationId = 1,
								                     Heads = 1,
								                     Id = 2,
								                     PeriodEnd = period.EndDateTime,
								                     PeriodStart = period.StartDateTime,
													 Resources = resources
							                     },
												 
						                     new ResourcesForCombinationFromStorage
							                     {
								                     ActivitySkillCombinationId = 1,
								                     Heads = 1,
								                     Id = 3,
								                     PeriodEnd = period2.EndDateTime,
								                     PeriodStart = period2.StartDateTime,
													 Resources = resources2
							                     }
					                     }, _activitySkillCombinations,
						 skillEfficiencies ?? Enumerable.Empty<SkillEfficienciesFromStorage>(), new[] { _skill }), 15);
		}
    }
}
