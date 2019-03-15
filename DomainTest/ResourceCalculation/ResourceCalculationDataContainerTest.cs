using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
	public class ResourceCalculationDataContainerTest
    {
		private ResourceCalculationDataContainer _target;
	    private readonly DateOnly _date = new DateOnly(2013, 8, 16);
		private readonly DateTimePeriod _period = new DateTimePeriod(new DateTime(2013, 8, 16, 12, 0, 0, DateTimeKind.Utc), new DateTime(2013, 8, 16, 12, 15, 0, DateTimeKind.Utc));
		private readonly IActivity _activity = ActivityFactory.CreateActivity("Phone").WithId();
		private readonly ISkill _skill = SkillFactory.CreateSkill("Skill1").WithId();
	    private IPerson _person;

	    [SetUp]
        public void Setup()
		{
			_skill.Activity = _activity;
			_person = PersonFactory.CreatePersonWithPersonPeriod(_date, new[] { _skill });
            _target = new ResourceCalculationDataContainer(Enumerable.Empty<ExternalStaff>(), new PersonSkillProvider(), 15, false);
        }

		[Test]
		public void ShouldGetResourcesOnSkill()
		{
			_target.AddResources(_person, _date,
			                     new ResourceLayer
				                     {
					                     PayloadId = _activity.Id.GetValueOrDefault(),
					                     Period = _period,
					                     RequiresSeat = false,
					                     Resource = 0.8
				                     });
			var result = _target.SkillResources(_skill, _period);
			result.Item2.Should().Be.EqualTo(1);
			result.Item1.Should().Be.EqualTo(0.8);
		}

		[Test]
		public void ShouldNotGetResourcesWhereSeatIsNotRequired()
		{
			_target.AddResources(_person, _date,
								 new ResourceLayer
								 {
									 PayloadId = _activity.Id.GetValueOrDefault(),
									 Period = _period,
									 RequiresSeat = false,
									 Resource = 0.8
								 });
			var result = _target.ActivityResourcesWhereSeatRequired(_skill, _period);
			result.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetAffectedSkillsWithProficiency()
		{
			_person.ChangeSkillProficiency(_skill,new Percent(0.9), _person.Period(_date));
			_target.AddResources(_person, _date,
								 new ResourceLayer
								 {
									 PayloadId = _activity.Id.GetValueOrDefault(),
									 Period = _period,
									 RequiresSeat = false,
									 Resource = 0.8
								 });
			var result = _target.AffectedResources(_activity, _period);
			var affectedSkill = result.First().Value;
			affectedSkill.SkillEffiencies[_skill.Id.GetValueOrDefault()].Should().Be.EqualTo(0.9);
			affectedSkill.Resource.Should().Be.EqualTo(0.8);
			affectedSkill.Skills.First().Should().Be.EqualTo(_skill);
		}

		[Test]
		public void ShouldGetAffectedSkillsDuplicateProficiency()
		{
			var skillClone = _skill.NoneEntityClone();
			_person = PersonFactory.CreatePersonWithPersonPeriod(_date, new[] { _skill, skillClone });
			skillClone.SetId(_skill.Id);
			_person.Period(_date).PersonSkillCollection.OfType<IPersonSkillModify>().ForEach(
				s =>
				{
					s.Active = true;
					s.SkillPercentage = new Percent(0.9);
				});
			
			_target.AddResources(_person, _date,
								 new ResourceLayer
								 {
									 PayloadId = _activity.Id.GetValueOrDefault(),
									 Period = _period,
									 RequiresSeat = false,
									 Resource = 0.8
								 });
			var result = _target.AffectedResources(_activity, _period);
			var affectedSkill = result.First().Value;
			affectedSkill.SkillEffiencies[_skill.Id.GetValueOrDefault()].Should().Be.EqualTo(0.9);
			affectedSkill.Resource.Should().Be.EqualTo(0.8);
			affectedSkill.Skills.First().Should().Be.EqualTo(_skill);
		}

		[Test]
		public void ShouldRemoveSkillsWithProficiency()
		{
			_person.ChangeSkillProficiency(_skill, new Percent(0.9), _person.Period(_date));
			_target.AddResources(_person, _date,
								 new ResourceLayer
								 {
									 PayloadId = _activity.Id.GetValueOrDefault(),
									 Period = _period,
									 RequiresSeat = false,
									 Resource = 0.8
								 });
			_target.AddResources(PersonFactory.CreatePersonWithPersonPeriod(_date,new []{_skill}), _date,
								 new ResourceLayer
								 {
									 PayloadId = _activity.Id.GetValueOrDefault(),
									 Period = _period,
									 RequiresSeat = false,
									 Resource = 0.7
								 });
			_target.RemoveResources(_person, _date, new ResourceLayer
			{
				PayloadId = _activity.Id.GetValueOrDefault(),
				Period = _period,
				RequiresSeat = false,
				Resource = 0.8
			});
			var result = _target.AffectedResources(_activity, _period);
			var affectedSkill = result.First().Value;
			affectedSkill.SkillEffiencies[_skill.Id.GetValueOrDefault()].Should().Be.IncludedIn(0.99d,1.01d);
			affectedSkill.Resource.Should().Be.EqualTo(0.7);
			affectedSkill.Skills.First().Should().Be.EqualTo(_skill);
		}

		[Test]
		public void ShouldRemoveOnePersonsSkillsWithProficiency()
		{
			_person.ChangeSkillProficiency(_skill, new Percent(0.9), _person.Period(_date));
			_target.AddResources(_person, _date,
								 new ResourceLayer
								 {
									 PayloadId = _activity.Id.GetValueOrDefault(),
									 Period = _period,
									 RequiresSeat = false,
									 Resource = 0.8
								 });
			_target.RemoveResources(_person, _date, new ResourceLayer
			{
				PayloadId = _activity.Id.GetValueOrDefault(),
				Period = _period,
				RequiresSeat = false,
				Resource = 0.8
			});
			var result = _target.AffectedResources(_activity, _period);
			var affectedSkill = result.First().Value;
			affectedSkill.SkillEffiencies[_skill.Id.GetValueOrDefault()].Should().Be.EqualTo(0d);
			affectedSkill.Resource.Should().Be.EqualTo(0d);
			affectedSkill.Skills.First().Should().Be.EqualTo(_skill);
		}

		[Test]
		public void ShouldGetCorrectEfficiencyForAffectedSkillsWhenCalledMoreThanOnce()
		{
			_target.AddResources(_person, _date,
								 new ResourceLayer
								 {
									 PayloadId = _activity.Id.GetValueOrDefault(),
									 Period = _period,
									 RequiresSeat = false,
									 Resource = 0.8
								 });

			_target.AddResources(_person, _date,
											 new ResourceLayer
											 {
												 PayloadId = _activity.Id.GetValueOrDefault(),
												 Period = _period.MovePeriod(TimeSpan.FromMinutes(15)),
												 RequiresSeat = false,
												 Resource = 0.8
											 });

			var result = _target.AffectedResources(_activity, _period.ChangeEndTime(TimeSpan.FromMinutes(15)));
			result = _target.AffectedResources(_activity, _period.ChangeEndTime(TimeSpan.FromMinutes(15)));
			var affectedSkill = result.First().Value;
			affectedSkill.Resource.Should().Be.EqualTo(0.8d);
			affectedSkill.Skills.First().Should().Be.EqualTo(_skill);
			affectedSkill.SkillEffiencies[_skill.Id.Value].Should().Be.EqualTo(1d);
		}

		[Test]
		public void ShouldGetAffectedSkills()
		{
			_target.AddResources(_person, _date,
								 new ResourceLayer
								 {
									 PayloadId = _activity.Id.GetValueOrDefault(),
									 Period = _period,
									 RequiresSeat = false,
									 Resource = 0.8
								 });
			var result = _target.AffectedResources(_activity, _period);
			var affectedSkill = result.First().Value;
			affectedSkill.Resource.Should().Be.EqualTo(0.8);
			affectedSkill.Skills.First().Should().Be.EqualTo(_skill);
		}

		[Test]
		public void ShouldNotUseSameInstanceOfDictionaryForSkillEfficiencies()
		{
			_target.AddResources(_person, _date,
								 new ResourceLayer
								 {
									 PayloadId = _activity.Id.GetValueOrDefault(),
									 Period = _period,
									 RequiresSeat = false,
									 Resource = 0.8
								 });
			var firstResult = _target.AffectedResources(_activity, _period);
			var secondResult = _target.AffectedResources(_activity, _period);
			firstResult.First().Value.SkillEffiencies.Should().Not.Be.SameInstanceAs(secondResult.First().Value.SkillEffiencies);
		}

		[Test]
		public void ShouldGetAffectedSkillsForMultipleIntervalsWithEfficiency()
		{
			_person.ChangeSkillProficiency(_skill, new Percent(0.9), _person.Period(_date));
			_target.AddResources(_person, _date,
								 new ResourceLayer
								 {
									 PayloadId = _activity.Id.GetValueOrDefault(),
									 Period = _period,
									 RequiresSeat = false,
									 Resource = 0.8
								 });
			_target.AddResources(_person, _date,
								 new ResourceLayer
								 {
									 PayloadId = _activity.Id.GetValueOrDefault(),
									 Period = _period.MovePeriod(TimeSpan.FromMinutes(15)),
									 RequiresSeat = false,
									 Resource = 0.7
								 });
			var result = _target.AffectedResources(_activity, _period.ChangeEndTime(TimeSpan.FromMinutes(15)));
			var affectedSkill = result.First().Value;
			affectedSkill.SkillEffiencies[_skill.Id.GetValueOrDefault()].Should().Be.EqualTo(0.9);
			affectedSkill.Resource.Should().Be.EqualTo(0.75);
			affectedSkill.Skills.First().Should().Be.EqualTo(_skill);
		}

		[Test]
		public void ShouldGetResourcesWhereSeatIsRequired()
		{
			_target.AddResources(_person, _date,
								 new ResourceLayer
								 {
									 PayloadId = _activity.Id.GetValueOrDefault(),
									 Period = _period,
									 RequiresSeat = true,
									 Resource = 0.8
								 });
			var result = _target.ActivityResourcesWhereSeatRequired(_skill, _period);
			result.Should().Be.EqualTo(0.8);
		}

		[Test]
		public void ShouldRemoveResourcesProperly()
		{
			_target.AddResources(_person, _date,
								 new ResourceLayer
								 {
									 PayloadId = _activity.Id.GetValueOrDefault(),
									 Period = _period,
									 RequiresSeat = false,
									 Resource = 0.8
								 });
			_target.RemoveResources(_person, _date, new ResourceLayer
				{
					PayloadId = _activity.Id.GetValueOrDefault(),
					Period = _period,
					RequiresSeat = false,
					Resource = 0.8
				});
			var result = _target.SkillResources(_skill, _period);
			result.Item2.Should().Be.EqualTo(0);
			result.Item1.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldSumResourcesOnSkillForTwoIntervals()
		{
			_target.AddResources(_person, _date,
								 new ResourceLayer
								 {
									 PayloadId = _activity.Id.GetValueOrDefault(),
									 Period = _period,
									 RequiresSeat = false,
									 Resource = 0.8
								 });
			_target.AddResources(_person, _date,
								 new ResourceLayer
								 {
									 PayloadId = _activity.Id.GetValueOrDefault(),
									 Period = _period.MovePeriod(TimeSpan.FromMinutes(15)),
									 RequiresSeat = false,
									 Resource = 0.7
								 });
			var result = _target.SkillResources(_skill, _period.ChangeEndTime(TimeSpan.FromMinutes(15)));
			result.Item2.Should().Be.EqualTo(1);
			result.Item1.Should().Be.EqualTo(0.75);
		}

		[Test]
		public void ShouldConsiderProficiencyWhenGettingSkillResources()
		{
			_person.ChangeSkillProficiency(_skill,new Percent(0.75), _person.Period(_date));
			_target.AddResources(_person, _date,
								 new ResourceLayer
								 {
									 PayloadId = _activity.Id.GetValueOrDefault(),
									 Period = _period,
									 RequiresSeat = false,
									 Resource = 0.8
								 });
			var result = _target.SkillResources(_skill, _period);
			result.Item2.Should().Be.EqualTo(1);
			Math.Round(result.Item1, 5).Should().Be.EqualTo(0.60000);
		}

		[Test]
		public void ShouldGetZeroResourcesOnSkillWithOtherActivity()
		{
			_skill.Activity = ActivityFactory.CreateActivity("Phone 2").WithId();
			_target.AddResources(_person, _date,
								 new ResourceLayer
								 {
									 PayloadId = _activity.Id.GetValueOrDefault(),
									 Period = _period,
									 RequiresSeat = false,
									 Resource = 0.8
								 });
			var result = _target.SkillResources(_skill, _period);
			result.Item2.Should().Be.EqualTo(0);
			result.Item1.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetFractionResourcesOnResourceWithPartOfInterval()
		{

			_target.AddResources(_person, _date,
								 new ResourceLayer
								 {
									 PayloadId = _activity.Id.GetValueOrDefault(),
									 Period = _period,
									 RequiresSeat = false,
									 Resource = 0.8,
									 FractionPeriod = _period.ChangeStartTime(TimeSpan.FromMinutes(3))
								 });
			_target.AddResources(_person, _date,
								 new ResourceLayer
								 {
									 PayloadId = _activity.Id.GetValueOrDefault(),
									 Period = _period.MovePeriod(TimeSpan.FromMinutes(15)),
									 RequiresSeat = false,
									 Resource = 0.6,
									 FractionPeriod = _period.MovePeriod(TimeSpan.FromMinutes(15)).ChangeStartTime(TimeSpan.FromMinutes(-6))
								 });
			var result = _target.IntraIntervalResources(_skill, _period.ChangeEndTime(TimeSpan.FromMinutes(15)));
			result.Should()
			      .Have.SameValuesAs(new[]
				      {
					      _period.ChangeStartTime(TimeSpan.FromMinutes(3)),
					      _period.MovePeriod(TimeSpan.FromMinutes(15)).ChangeStartTime(TimeSpan.FromMinutes(-6))
				      });
		}

		[Test]
		public void ShouldNotThrowWhenRemoveResourcesForFractionPeriods()
		{
			_target.AddResources(_person, _date, new ResourceLayer
			{
				PayloadId = _activity.Id.GetValueOrDefault(),
				Period = _period,
				RequiresSeat = false,
				Resource = 0.8
			});
			_target.RemoveResources(_person, _date, new ResourceLayer
			{
				FractionPeriod = _period.ChangeEndTime(TimeSpan.FromMinutes(-8)),
				PayloadId = _activity.Id.GetValueOrDefault(),
				Period = _period,
				RequiresSeat = false,
				Resource = 0.8
			});
			_target.RemoveResources(_person, _date, new ResourceLayer
			{
				FractionPeriod = _period.ChangeStartTime(TimeSpan.FromMinutes(7)),
				PayloadId = _activity.Id.GetValueOrDefault(),
				Period = _period,
				RequiresSeat = false,
				Resource = 0.8
			});
			var result = _target.SkillResources(_skill, _period);
			result.Item1.Should().Be.EqualTo(0);
			result.Item2.Should().Be.EqualTo(0);
		}
		
		[Test]
		public void ShouldRemoveResourcesCorrectlyForFractionPeriods()
		{
			_target.AddResources(_person, _date, new ResourceLayer
			{
				PayloadId = _activity.Id.GetValueOrDefault(),
				Period = _period,
				RequiresSeat = false,
				Resource = 0.8
			});
			var person2 = PersonFactory.CreatePersonWithPersonPeriod(_date, new[] { _skill });
			_target.AddResources(person2, _date, new ResourceLayer
			{
				PayloadId = _activity.Id.GetValueOrDefault(),
				Period = _period,
				RequiresSeat = false,
				Resource = 0.8
			});
			
			_target.RemoveResources(_person, _date, new ResourceLayer
			{
				FractionPeriod = _period.ChangeEndTime(TimeSpan.FromMinutes(-8)),
				PayloadId = _activity.Id.GetValueOrDefault(),
				Period = _period,
				RequiresSeat = false,
				Resource = 0.8
			});
			_target.RemoveResources(_person, _date, new ResourceLayer
			{
				FractionPeriod = _period.ChangeStartTime(TimeSpan.FromMinutes(7)),
				PayloadId = _activity.Id.GetValueOrDefault(),
				Period = _period,
				RequiresSeat = false,
				Resource = 0.8
			});
			
			var result = _target.SkillResources(_skill, _period);
			Math.Round(result.Item1, 1).Should().Be.EqualTo(0);
			Math.Round(result.Item2, 1).Should().Be.EqualTo(1);
		}

		[Test]
	    public void ShouldGetTheDistributedCountForHourSkill()
	    {
		    var period = new DateTimePeriod(2013, 08, 06, 12, 2013, 8, 06, 13);
			_person.ChangeSkillProficiency(_skill, new Percent(0.9), _person.Period(_date));
			_target.AddResources(_person, _date,
								 new ResourceLayer
								 {
									 PayloadId = _activity.Id.GetValueOrDefault(),
									 Period = new DateTimePeriod(period.StartDateTime,period.StartDateTime.AddMinutes(15)),
									 RequiresSeat = false,
									 Resource = 1
								 });

			_target.AddResources(_person, _date,
								 new ResourceLayer
								 {
									 PayloadId = _activity.Id.GetValueOrDefault(),
									 Period = new DateTimePeriod(period.StartDateTime.AddMinutes(15), period.StartDateTime.AddMinutes(30)),
									 RequiresSeat = false,
									 Resource = 1
								 });

			_target.AddResources(_person, _date,
								 new ResourceLayer
								 {
									 PayloadId = _activity.Id.GetValueOrDefault(),
									 Period = new DateTimePeriod(period.StartDateTime.AddMinutes(30), period.StartDateTime.AddMinutes(45)),
									 RequiresSeat = false,
									 Resource = 1
								 });

			_target.AddResources(_person, _date,
								 new ResourceLayer
								 {
									 PayloadId = _activity.Id.GetValueOrDefault(),
									 Period = new DateTimePeriod(period.StartDateTime.AddMinutes(45), period.StartDateTime.AddMinutes(60)),
									 RequiresSeat = false,
									 Resource = 1
								 });
			var result = _target.AffectedResources(_activity, period );
			var affectedSkill = result.First().Value;
			affectedSkill.Resource.Should().Be.EqualTo(1);
			affectedSkill.Count.Should().Be.EqualTo(1);
		}
    }
}
