using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class SingleSkillCalculatorTest
	{
		private ISingleSkillCalculator _target;

		[SetUp]
		public void Setup()
		{
			_target = new SingleSkillCalculator();
		}

		[Test]
		public void ShouldCalculateIfPersonSkillAndRightActivity()
		{
			ISkill rightSkill = SkillFactory.CreateSkill("och");
			IActivity rightActivity = new Activity("och");
			rightActivity.SetId(Guid.Empty);
			rightSkill.Activity = rightActivity;

			IPerson person1 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(), new List<ISkill> { rightSkill });
			IVisualLayerCollection collection1 = VisualLayerCollectionFactory.CreateForWorkShift(person1, TimeSpan.FromHours(8),
			                                                                                     TimeSpan.FromHours(9), rightSkill.Activity);
			IList<IVisualLayerCollection> list = new List<IVisualLayerCollection>{collection1};
			ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods = new SkillSkillStaffPeriodExtendedDictionary();

			DateTime dateTime = new DateTime(1800, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			ISkillStaffPeriod skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(rightSkill, dateTime, 60, 0, 0);
			ISkillStaffPeriodDictionary skillStaffPeriodDictionary = new SkillStaffPeriodDictionary(rightSkill);
			skillStaffPeriodDictionary.Add(skillStaffPeriod.Period, skillStaffPeriod);
			relevantSkillStaffPeriods.Add(rightSkill, skillStaffPeriodDictionary);
			
			_target.Calculate(list, relevantSkillStaffPeriods);

			Assert.AreEqual(1, skillStaffPeriod.CalculatedResource);
			Assert.AreEqual(1, skillStaffPeriod.CalculatedLoggedOn);
		}

		[Test]
		public void ShouldNotCalculateIfNotPersonSkill()
		{
			ISkill rightSkill = SkillFactory.CreateSkill("och");
			IActivity rightActivity = new Activity("och");
			rightActivity.SetId(Guid.Empty);
			rightSkill.Activity = rightActivity;

			IPerson person1 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(), new List<ISkill> { rightSkill });
			IVisualLayerCollection collection1 = VisualLayerCollectionFactory.CreateForWorkShift(person1, TimeSpan.FromHours(8),
																								 TimeSpan.FromHours(9), new Activity("other"));
			IList<IVisualLayerCollection> list = new List<IVisualLayerCollection> { collection1 };
			ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods = new SkillSkillStaffPeriodExtendedDictionary();

			DateTime dateTime = new DateTime(1800, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			ISkillStaffPeriod skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(rightSkill, dateTime, 60, 0, 0);
			ISkillStaffPeriodDictionary skillStaffPeriodDictionary = new SkillStaffPeriodDictionary(rightSkill);
			skillStaffPeriodDictionary.Add(skillStaffPeriod.Period, skillStaffPeriod);
			relevantSkillStaffPeriods.Add(rightSkill, skillStaffPeriodDictionary);

			_target.Calculate(list, relevantSkillStaffPeriods);

			Assert.AreEqual(0, skillStaffPeriod.CalculatedResource);
			Assert.AreEqual(0, skillStaffPeriod.CalculatedLoggedOn);
		}

		[Test]
		public void ShouldNotCalculateIfSkillStaffPeriodOnAnotherSkill()
		{
			ISkill rightSkill = SkillFactory.CreateSkill("och");
			rightSkill.SetId(Guid.Empty);
			IActivity rightActivity = new Activity("och");
			rightActivity.SetId(Guid.Empty);
			rightSkill.Activity = rightActivity;

			ISkill otherSkill = SkillFactory.CreateSkill("other");

			IPerson person1 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(), new List<ISkill> { rightSkill });
			IVisualLayerCollection collection1 = VisualLayerCollectionFactory.CreateForWorkShift(person1, TimeSpan.FromHours(8),
																								 TimeSpan.FromHours(9), rightSkill.Activity);
			IList<IVisualLayerCollection> list = new List<IVisualLayerCollection> { collection1 };
			ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods = new SkillSkillStaffPeriodExtendedDictionary();

			DateTime dateTime = new DateTime(1800, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			ISkillStaffPeriod skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(rightSkill, dateTime, 60, 0, 0);
			ISkillStaffPeriodDictionary skillStaffPeriodDictionary = new SkillStaffPeriodDictionary(otherSkill);
			skillStaffPeriodDictionary.Add(skillStaffPeriod.Period, skillStaffPeriod);
			relevantSkillStaffPeriods.Add(otherSkill, skillStaffPeriodDictionary);

			_target.Calculate(list, relevantSkillStaffPeriods);

			Assert.AreEqual(0, skillStaffPeriod.CalculatedResource);
			Assert.AreEqual(0, skillStaffPeriod.CalculatedLoggedOn);
		}

		[Test]
		public void ShouldCalculateFractionsOfInterval()
		{
			ISkill rightSkill = SkillFactory.CreateSkill("och");
			IActivity rightActivity = new Activity("och");
			rightActivity.SetId(Guid.Empty);
			rightSkill.Activity = rightActivity;

			IPerson person1 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(), new List<ISkill> { rightSkill });
			IVisualLayerCollection collection1 = VisualLayerCollectionFactory.CreateForWorkShift(person1, TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(15)),
																								 TimeSpan.FromHours(9).Add(TimeSpan.FromMinutes(-15)), rightSkill.Activity);
			IList<IVisualLayerCollection> list = new List<IVisualLayerCollection> { collection1 };
			ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods = new SkillSkillStaffPeriodExtendedDictionary();

			DateTime dateTime = new DateTime(1800, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			ISkillStaffPeriod skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(rightSkill, dateTime, 60, 0, 0);
			ISkillStaffPeriodDictionary skillStaffPeriodDictionary = new SkillStaffPeriodDictionary(rightSkill);
			skillStaffPeriodDictionary.Add(skillStaffPeriod.Period, skillStaffPeriod);
			relevantSkillStaffPeriods.Add(rightSkill, skillStaffPeriodDictionary);

			_target.Calculate(list, relevantSkillStaffPeriods);

			Assert.AreEqual(0.5, skillStaffPeriod.CalculatedResource);
			Assert.AreEqual(0.5, skillStaffPeriod.CalculatedLoggedOn);
		}

		[Test]
		public void ShouldCalculateFractionsOfIntervalWhenIntervalLengthIs30()
		{
			ISkill rightSkill = SkillFactory.CreateSkill("och");
			IActivity rightActivity = new Activity("och");
			rightActivity.SetId(Guid.Empty);
			rightSkill.Activity = rightActivity;

			IPerson person1 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(), new List<ISkill> { rightSkill });
			IVisualLayerCollection collection1 = VisualLayerCollectionFactory.CreateForWorkShift(person1, TimeSpan.FromHours(8),
																								 TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(15)), rightSkill.Activity);
			IList<IVisualLayerCollection> list = new List<IVisualLayerCollection> { collection1 };
			ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods = new SkillSkillStaffPeriodExtendedDictionary();

			DateTime dateTime = new DateTime(1800, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			ISkillStaffPeriod skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(rightSkill, dateTime, 30, 0, 0);
			ISkillStaffPeriodDictionary skillStaffPeriodDictionary = new SkillStaffPeriodDictionary(rightSkill);
			skillStaffPeriodDictionary.Add(skillStaffPeriod.Period, skillStaffPeriod);
			relevantSkillStaffPeriods.Add(rightSkill, skillStaffPeriodDictionary);

			_target.Calculate(list, relevantSkillStaffPeriods);

			Assert.AreEqual(0.5, skillStaffPeriod.CalculatedResource);
			Assert.AreEqual(0.5, skillStaffPeriod.CalculatedLoggedOn);
		}

		[Test]
		public void ShouldCalculateFractionsOfIntervalWhenIntervalLengthIs30AndAnotherTwist()
		{
			ISkill rightSkill = SkillFactory.CreateSkill("och");
			IActivity rightActivity = new Activity("och");
			rightActivity.SetId(Guid.Empty);
			rightSkill.Activity = rightActivity;

			IPerson person1 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(), new List<ISkill> { rightSkill });
			IVisualLayerCollection collection1 = VisualLayerCollectionFactory.CreateForWorkShift(person1, TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(15)),
																								 TimeSpan.FromHours(9), rightSkill.Activity);
			IList<IVisualLayerCollection> list = new List<IVisualLayerCollection> { collection1 };
			ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods = new SkillSkillStaffPeriodExtendedDictionary();

			DateTime dateTime = new DateTime(1800, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			ISkillStaffPeriod skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(rightSkill, dateTime, 30, 0, 0);
			ISkillStaffPeriodDictionary skillStaffPeriodDictionary = new SkillStaffPeriodDictionary(rightSkill);
			skillStaffPeriodDictionary.Add(skillStaffPeriod.Period, skillStaffPeriod);
			relevantSkillStaffPeriods.Add(rightSkill, skillStaffPeriodDictionary);

			_target.Calculate(list, relevantSkillStaffPeriods);

			Assert.AreEqual(0.5, skillStaffPeriod.CalculatedResource);
			Assert.AreEqual(0.5, skillStaffPeriod.CalculatedLoggedOn);
		}
	}
}