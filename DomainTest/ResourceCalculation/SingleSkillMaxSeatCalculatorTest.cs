using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class SingleSkillMaxSeatCalculatorTest
	{
		private SingleSkillMaxSeatCalculator _singleSkillMaxSeatCalculator;
		private IList<IVisualLayerCollection> _toRemove;
		private IList<IVisualLayerCollection> _toAdd;
		private ISkillSkillStaffPeriodExtendedDictionary _relevantSkillStaffPeriods;
		private ISkillStaffPeriod _skillStaffPeriod;
		private IVisualLayerCollection _visualLayerCollection;
		private ISkill _skill;
		private ISkill _skillMaxSeat;
		private Activity _activity;
		private IPerson _person;
		private DateTime _dateTime;

		[SetUp]
		public void Setup()
		{
			_singleSkillMaxSeatCalculator = new SingleSkillMaxSeatCalculator();
			_toAdd = new List<IVisualLayerCollection>();
			_toRemove = new List<IVisualLayerCollection>();

			_dateTime = new DateTime(1800, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			_skill = SkillFactory.CreateSkill("skill");
			_skillMaxSeat = SkillFactory.CreateSiteSkill("maxSeatSkill");
			_activity = new Activity("activity") { RequiresSeat = true };
			_activity.SetId(Guid.Empty);
			_skill.Activity = _activity;
			_person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(), new List<ISkill> { _skill, _skillMaxSeat });
			_visualLayerCollection = VisualLayerCollectionFactory.CreateForWorkShift(_person, TimeSpan.FromHours(8), TimeSpan.FromHours(9), _skill.Activity);
			_skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_skillMaxSeat, _dateTime, 60, 0, 0);
			_relevantSkillStaffPeriods = new SkillSkillStaffPeriodExtendedDictionary();
			ISkillStaffPeriodDictionary skillStaffPeriodDictionary = new SkillStaffPeriodDictionary(_skillMaxSeat);
			skillStaffPeriodDictionary.Add(_skillStaffPeriod.Period, _skillStaffPeriod);
			_relevantSkillStaffPeriods.Add(_skillMaxSeat, skillStaffPeriodDictionary);
		}

		[Test]
		public void ShouldCalculateAddAndRemove()
		{
			_toAdd.Add(_visualLayerCollection);
			_singleSkillMaxSeatCalculator.Calculate(_relevantSkillStaffPeriods, _toRemove, _toAdd);
			Assert.AreEqual(1, _skillStaffPeriod.CalculatedResource);
			Assert.AreEqual(1, _skillStaffPeriod.Payload.CalculatedUsedSeats);

			_toRemove.Add(_visualLayerCollection);
			_singleSkillMaxSeatCalculator.Calculate(_relevantSkillStaffPeriods, _toRemove, new List<IVisualLayerCollection>());
			Assert.AreEqual(0, _skillStaffPeriod.CalculatedResource);
			Assert.AreEqual(0, _skillStaffPeriod.Payload.CalculatedUsedSeats);
		}

		[Test]
		public void ShouldCalculateFractions()
		{
			_visualLayerCollection = VisualLayerCollectionFactory.CreateForWorkShift(_person, TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(15)), TimeSpan.FromHours(9).Add(TimeSpan.FromMinutes(-15)), _skill.Activity);
			_toAdd.Add(_visualLayerCollection);
			_singleSkillMaxSeatCalculator.Calculate(_relevantSkillStaffPeriods, _toRemove, _toAdd);
			Assert.AreEqual(0.5, _skillStaffPeriod.CalculatedResource);
			Assert.AreEqual(0.5, _skillStaffPeriod.Payload.CalculatedUsedSeats);	
		}

		[Test]
		public void ShouldCalculateFractionsOn30MinIntervals1()
		{
			_visualLayerCollection = VisualLayerCollectionFactory.CreateForWorkShift(_person, TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(15)), TimeSpan.FromHours(9).Add(TimeSpan.FromMinutes(15)), _skill.Activity);
			_skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_skill, _dateTime, 30, 0, 0);
			ISkillStaffPeriodDictionary skillStaffPeriodDictionary = new SkillStaffPeriodDictionary(_skillMaxSeat);
			skillStaffPeriodDictionary.Add(_skillStaffPeriod.Period, _skillStaffPeriod);
			_relevantSkillStaffPeriods.Clear();
			_relevantSkillStaffPeriods.Add(_skillMaxSeat, skillStaffPeriodDictionary);

			_toAdd.Add(_visualLayerCollection);
			_singleSkillMaxSeatCalculator.Calculate(_relevantSkillStaffPeriods, _toRemove, _toAdd);
			Assert.AreEqual(0.5, _skillStaffPeriod.CalculatedResource);
			Assert.AreEqual(0.5, _skillStaffPeriod.Payload.CalculatedUsedSeats);
		}

		[Test]
		public void ShouldCalculateFractionsOn30MinIntervals2()
		{
			_visualLayerCollection = VisualLayerCollectionFactory.CreateForWorkShift(_person, TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(15)), TimeSpan.FromHours(9), _skill.Activity);
			_skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_skill, _dateTime, 30, 0, 0);
			ISkillStaffPeriodDictionary skillStaffPeriodDictionary = new SkillStaffPeriodDictionary(_skillMaxSeat);
			skillStaffPeriodDictionary.Add(_skillStaffPeriod.Period, _skillStaffPeriod);
			_relevantSkillStaffPeriods.Clear();
			_relevantSkillStaffPeriods.Add(_skillMaxSeat, skillStaffPeriodDictionary);

			_toAdd.Add(_visualLayerCollection);
			_singleSkillMaxSeatCalculator.Calculate(_relevantSkillStaffPeriods, _toRemove, _toAdd);
			Assert.AreEqual(0.5, _skillStaffPeriod.CalculatedResource);
			Assert.AreEqual(0.5, _skillStaffPeriod.Payload.CalculatedUsedSeats);
		}

		[Test]
		public void ShouldNotCalculateOnEmptyInput()
		{
			_singleSkillMaxSeatCalculator.Calculate(_relevantSkillStaffPeriods, _toRemove, _toAdd);
			Assert.AreEqual(0, _skillStaffPeriod.CalculatedResource);
			Assert.AreEqual(0, _skillStaffPeriod.Payload.CalculatedUsedSeats);		
		}

		[Test]
		public void ShouldNotCalculateWhenPersonDoNotHaveMaxSeatSkill()
		{
			_person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(), new List<ISkill> { _skill });
			_visualLayerCollection = VisualLayerCollectionFactory.CreateForWorkShift(_person, TimeSpan.FromHours(8), TimeSpan.FromHours(9), _skill.Activity);
		
			_toAdd.Add(_visualLayerCollection);
			_singleSkillMaxSeatCalculator.Calculate(_relevantSkillStaffPeriods, _toRemove, _toAdd);
			Assert.AreEqual(0, _skillStaffPeriod.CalculatedResource);
			Assert.AreEqual(0, _skillStaffPeriod.Payload.CalculatedUsedSeats);		
		}	
	}
}
