using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class ResourceCalculationDataContainerFromSkillCombinationsTest
	{
		private ResourceCalculationDataContainerFromSkillCombinations _target;
		private readonly DateOnly _date = new DateOnly(2013, 8, 16);
		private DateTimePeriod _period = new DateTimePeriod(new DateTime(2013, 8, 16, 12, 0, 0, DateTimeKind.Utc), new DateTime(2013, 8, 16, 13, 0, 0, DateTimeKind.Utc));
		private readonly IActivity _activity = ActivityFactory.CreateActivity("Phone").WithId();
		private readonly ISkill _skill = SkillFactory.CreateSkill("Skill1").WithId();
		private readonly ISkill _skill15Min = SkillFactory.CreateSkill("Skill15").WithId();
		
		private IPerson _person;

		[SetUp]
		public void Setup()
		{
			_skill.Activity = _activity;
			_skill.DefaultResolution = 60;
			_person = PersonFactory.CreatePersonWithPersonPeriod(_date, new[] { _skill });
			var resource = new SkillCombinationResource
			{
				StartDateTime = _period.StartDateTime,
				EndDateTime = _period.EndDateTime,
				SkillCombination = new[] { _skill.Id.GetValueOrDefault() },
				Resource = 10
			};
			_target = new ResourceCalculationDataContainerFromSkillCombinations(new List<SkillCombinationResource>() { resource }, new[] { _skill,_skill15Min }, false);
		}

		[Test]
		public void ShouldRemoveQuarterResourcePartOfSkillCombinationResource()
		{
			_period = _period.MovePeriod(TimeSpan.FromHours(1));
			_target.AddResources(_person, _date,
				new ResourceLayer
				{
					PayloadId = _activity.Id.GetValueOrDefault(),
					Period = new DateTimePeriod(_period.StartDateTime, _period.StartDateTime.AddMinutes(15)),
					Resource = 10
				});
			_target.AddResources(_person, _date,
				new ResourceLayer
				{
					PayloadId = _activity.Id.GetValueOrDefault(),
					Period = new DateTimePeriod(_period.StartDateTime.AddMinutes(15), _period.StartDateTime.AddMinutes(30)),
					Resource = 10
				});
			_target.AddResources(_person, _date,
				new ResourceLayer
				{
					PayloadId = _activity.Id.GetValueOrDefault(),
					Period = new DateTimePeriod(_period.StartDateTime.AddMinutes(30), _period.StartDateTime.AddMinutes(45)),
					Resource = 10
				});
			_target.AddResources(_person, _date,
				new ResourceLayer
				{
					PayloadId = _activity.Id.GetValueOrDefault(),
					Period = new DateTimePeriod(_period.StartDateTime.AddMinutes(45), _period.StartDateTime.AddMinutes(60)),
					Resource = 10
				});
			_target.RemoveResources(_person, _date,
				new ResourceLayer
				{
					PayloadId = _activity.Id.GetValueOrDefault(),
					Period = new DateTimePeriod(_period.StartDateTime.AddMinutes(15), _period.StartDateTime.AddMinutes(30)),
					Resource = 1
				});

			var result = _target.AffectedResources(_activity, _period);
			var affectedSkill = result.First().Value;

			affectedSkill.Resource.Should().Be.EqualTo(9.75);
			affectedSkill.Skills.First().Should().Be.EqualTo(_skill);
		}

		[Test]
		public void ShouldSumResourcesOnSkillResolution()
		{
			_period = _period.MovePeriod(TimeSpan.FromHours(1));
			_target.AddResources(_person, _date,
				new ResourceLayer
				{
					PayloadId = _activity.Id.GetValueOrDefault(),
					Period = new DateTimePeriod(_period.StartDateTime, _period.StartDateTime.AddMinutes(15)),
					Resource = 10
				});
			_target.AddResources(_person, _date,
				new ResourceLayer
				{
					PayloadId = _activity.Id.GetValueOrDefault(),
					Period = new DateTimePeriod(_period.StartDateTime.AddMinutes(15), _period.StartDateTime.AddMinutes(30)),
					Resource = 10
				});
			_target.AddResources(_person, _date,
				new ResourceLayer
				{
					PayloadId = _activity.Id.GetValueOrDefault(),
					Period = new DateTimePeriod(_period.StartDateTime.AddMinutes(30), _period.StartDateTime.AddMinutes(45)),
					Resource = 20
				});
			_target.AddResources(_person, _date,
				new ResourceLayer
				{
					PayloadId = _activity.Id.GetValueOrDefault(),
					Period = new DateTimePeriod(_period.StartDateTime.AddMinutes(45), _period.StartDateTime.AddMinutes(60)),
					Resource = 20
				});
			var result = _target.AffectedResources(_activity, _period);

			result.Values.First().Resource.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldRemoveHalfResourceOfSkillCombinationResource()
		{
			_period = _period.MovePeriod(TimeSpan.FromHours(1));
			_target.AddResources(_person, _date,
				new ResourceLayer
				{
					PayloadId = _activity.Id.GetValueOrDefault(),
					Period = new DateTimePeriod(_period.StartDateTime, _period.StartDateTime.AddMinutes(15)),
					Resource = 10
				});
			_target.AddResources(_person, _date,
				new ResourceLayer
				{
					PayloadId = _activity.Id.GetValueOrDefault(),
					Period = new DateTimePeriod(_period.StartDateTime.AddMinutes(15), _period.StartDateTime.AddMinutes(30)),
					Resource = 10
				});
			_target.AddResources(_person, _date,
				new ResourceLayer
				{
					PayloadId = _activity.Id.GetValueOrDefault(),
					Period = new DateTimePeriod(_period.StartDateTime.AddMinutes(30), _period.StartDateTime.AddMinutes(45)),
					Resource = 10
				});
			_target.AddResources(_person, _date,
				new ResourceLayer
				{
					PayloadId = _activity.Id.GetValueOrDefault(),
					Period = new DateTimePeriod(_period.StartDateTime.AddMinutes(45), _period.StartDateTime.AddMinutes(60)),
					Resource = 10
				});
			_target.RemoveResources(_person, _date,
				new ResourceLayer
				{
					PayloadId = _activity.Id.GetValueOrDefault(),
					Period = new DateTimePeriod(_period.StartDateTime, _period.StartDateTime.AddMinutes(15)),
					Resource = 1
				});
			_target.RemoveResources(_person, _date,
				new ResourceLayer
				{
					PayloadId = _activity.Id.GetValueOrDefault(),
					Period = new DateTimePeriod(_period.StartDateTime.AddMinutes(15), _period.StartDateTime.AddMinutes(30)),
					Resource = 1
				});

			var result = _target.AffectedResources(_activity, _period);
			var affectedSkill = result.First().Value;
			
			affectedSkill.Resource.Should().Be.EqualTo(9.5);
			affectedSkill.Skills.First().Should().Be.EqualTo(_skill);
		}

		[Test]
		public void ShouldAddQuarterResourcePartOfSkillCombinationResource()
		{
			_period = _period.MovePeriod(TimeSpan.FromHours(1));
			_target.AddResources(_person, _date,
				new ResourceLayer
				{
					PayloadId = _activity.Id.GetValueOrDefault(),
					Period = new DateTimePeriod(_period.StartDateTime, _period.StartDateTime.AddMinutes(15)),
					Resource = 10
				});
			_target.AddResources(_person, _date,
				new ResourceLayer
				{
					PayloadId = _activity.Id.GetValueOrDefault(),
					Period = new DateTimePeriod(_period.StartDateTime.AddMinutes(15), _period.StartDateTime.AddMinutes(30)),
					Resource = 10
				});
			_target.AddResources(_person, _date,
				new ResourceLayer
				{
					PayloadId = _activity.Id.GetValueOrDefault(),
					Period = new DateTimePeriod(_period.StartDateTime.AddMinutes(30), _period.StartDateTime.AddMinutes(45)),
					Resource = 11
				});
			_target.AddResources(_person, _date,
				new ResourceLayer
				{
					PayloadId = _activity.Id.GetValueOrDefault(),
					Period = new DateTimePeriod(_period.StartDateTime.AddMinutes(45), _period.StartDateTime.AddMinutes(60)),
					Resource = 11
				});
			var result = _target.AffectedResources(_activity, _period);
			var affectedSkill = result.First().Value;
			affectedSkill.Resource.Should().Be.EqualTo(10.5);
			affectedSkill.Skills.First().Should().Be.EqualTo(_skill);
		}
		
		[Test]
		public void ShouldIncludeQuarterResourcesInSum()
		{
			_target.AddResources(_person, _date,
				new ResourceLayer
				{
					PayloadId = _activity.Id.GetValueOrDefault(),
					Period = new DateTimePeriod(_period.StartDateTime, _period.StartDateTime.AddMinutes(15)),
					Resource = 1
				});
			_target.AddResources(_person, _date,
				new ResourceLayer
				{
					PayloadId = _activity.Id.GetValueOrDefault(),
					Period = new DateTimePeriod(_period.StartDateTime.AddMinutes(15), _period.StartDateTime.AddMinutes(30)),
					Resource = 1
				});
			_target.AddResources(_person, _date,
				new ResourceLayer
				{
					PayloadId = _activity.Id.GetValueOrDefault(),
					Period = new DateTimePeriod(_period.StartDateTime.AddMinutes(30), _period.StartDateTime.AddMinutes(45)),
					Resource = 1
				});
			_target.AddResources(_person, _date,
				new ResourceLayer
				{
					PayloadId = _activity.Id.GetValueOrDefault(),
					Period = new DateTimePeriod(_period.StartDateTime.AddMinutes(45), _period.StartDateTime.AddMinutes(60)),
					Resource = 1
				});
			

			var result = _target.AffectedResources(_activity, _period);
			var affectedSkill = result.First().Value;
			
			affectedSkill.Resource.Should().Be.EqualTo(11);
			affectedSkill.Skills.First().Should().Be.EqualTo(_skill);
		}

		[Test]
		public void ShouldRemoveResourcesFromHourWhenQuarterIsMinResolution()
		{
			
			_target.RemoveResources(_person, _date,
				new ResourceLayer
				{
					PayloadId = _activity.Id.GetValueOrDefault(),
					Period = new DateTimePeriod(_period.StartDateTime, _period.StartDateTime.AddMinutes(15)),
					Resource = 1
				});
			_target.RemoveResources(_person, _date,
				new ResourceLayer
				{
					PayloadId = _activity.Id.GetValueOrDefault(),
					Period = new DateTimePeriod(_period.StartDateTime.AddMinutes(15), _period.StartDateTime.AddMinutes(30)),
					Resource = 1
				});
			
			
			var result = _target.AffectedResources(_activity, _period);
			var affectedSkill = result.First().Value;

			affectedSkill.Resource.Should().Be.EqualTo(9.5);
			affectedSkill.Skills.First().Should().Be.EqualTo(_skill);
		}

		[Test]
		public void ShouldAddResourcesToHourWhenQuarterIsMinResolutionAndSkillIsHour()
		{

			_target.AddResources(_person, _date,
				new ResourceLayer
				{
					PayloadId = _activity.Id.GetValueOrDefault(),
					Period = new DateTimePeriod(_period.StartDateTime, _period.StartDateTime.AddMinutes(15)),
					Resource = 1
				});
			_target.AddResources(_person, _date,
				new ResourceLayer
				{
					PayloadId = _activity.Id.GetValueOrDefault(),
					Period = new DateTimePeriod(_period.StartDateTime.AddMinutes(15), _period.StartDateTime.AddMinutes(30)),
					Resource = 1
				});


			var result = _target.AffectedResources(_activity, _period);
			var affectedSkill = result.First().Value;

			affectedSkill.Resource.Should().Be.EqualTo(10.5);
			affectedSkill.Skills.First().Should().Be.EqualTo(_skill);
		}

		[Test]
		public void ShouldAddResourcesToHourWhenQuarterIsMinResolutionANdHourIsMissing()
		{
			_target = new ResourceCalculationDataContainerFromSkillCombinations(new List<SkillCombinationResource>() , new[] { _skill, _skill15Min }, false);
			_target.AddResources(_person, _date,
				new ResourceLayer
				{
					PayloadId = _activity.Id.GetValueOrDefault(),
					Period = new DateTimePeriod(_period.StartDateTime, _period.StartDateTime.AddMinutes(15)),
					Resource = 10
				});
			_target.AddResources(_person, _date,
				new ResourceLayer
				{
					PayloadId = _activity.Id.GetValueOrDefault(),
					Period = new DateTimePeriod(_period.StartDateTime.AddMinutes(15), _period.StartDateTime.AddMinutes(30)),
					Resource = 10
				});
			_target.AddResources(_person, _date,
				new ResourceLayer
				{
					PayloadId = _activity.Id.GetValueOrDefault(),
					Period = new DateTimePeriod(_period.StartDateTime.AddMinutes(30), _period.StartDateTime.AddMinutes(45)),
					Resource = 11
				});
			_target.AddResources(_person, _date,
				new ResourceLayer
				{
					PayloadId = _activity.Id.GetValueOrDefault(),
					Period = new DateTimePeriod(_period.StartDateTime.AddMinutes(45), _period.StartDateTime.AddMinutes(60)),
					Resource = 11
				});

			var result = _target.AffectedResources(_activity, _period);
			var affectedSkill = result.First().Value;

			affectedSkill.Resource.Should().Be.EqualTo(10.5);
			affectedSkill.Skills.First().Should().Be.EqualTo(_skill);
		}
		
	}

	
}
