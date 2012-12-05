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
	public class SingleSkillCalculatorTest
	{
		private ISingleSkillCalculator _target;
		private IList<IVisualLayerCollection> _toRemove;
		private IList<IVisualLayerCollection> _toAdd;

		[SetUp]
		public void Setup()
		{
			_target = new SingleSkillCalculator();
			_toAdd = new List<IVisualLayerCollection>();
			_toRemove = new List<IVisualLayerCollection>();
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
			
			_target.Calculate(list, relevantSkillStaffPeriods, _toRemove, _toAdd);

			Assert.AreEqual(1, skillStaffPeriod.CalculatedResource);
			Assert.AreEqual(1, skillStaffPeriod.CalculatedLoggedOn);
		}

		[Test]
		public void ShouldAddAndRemoveIfNotEmptyList()
		{
			ISkill rightSkill = SkillFactory.CreateSkill("och");
			IActivity rightActivity = new Activity("och");
			rightActivity.SetId(Guid.Empty);
			rightSkill.Activity = rightActivity;

			IPerson person1 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(), new List<ISkill> { rightSkill });
			IVisualLayerCollection collection1 = VisualLayerCollectionFactory.CreateForWorkShift(person1, TimeSpan.FromHours(8),
																								 TimeSpan.FromHours(9), rightSkill.Activity);
			IList<IVisualLayerCollection> list = new List<IVisualLayerCollection> { collection1 };
			ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods = new SkillSkillStaffPeriodExtendedDictionary();

			DateTime dateTime = new DateTime(1800, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			ISkillStaffPeriod skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(rightSkill, dateTime, 60, 0, 0);
			ISkillStaffPeriodDictionary skillStaffPeriodDictionary = new SkillStaffPeriodDictionary(rightSkill);
			skillStaffPeriodDictionary.Add(skillStaffPeriod.Period, skillStaffPeriod);
			relevantSkillStaffPeriods.Add(rightSkill, skillStaffPeriodDictionary);

			_target.Calculate(new List<IVisualLayerCollection>(), relevantSkillStaffPeriods, _toRemove, list);

			Assert.AreEqual(1, skillStaffPeriod.CalculatedResource);
			Assert.AreEqual(1, skillStaffPeriod.CalculatedLoggedOn);

			_toRemove.Add(collection1);
			_target.Calculate(new List<IVisualLayerCollection>(), relevantSkillStaffPeriods, list, _toAdd);

			Assert.AreEqual(0, skillStaffPeriod.CalculatedResource);
			Assert.AreEqual(0, skillStaffPeriod.CalculatedLoggedOn);
		}

		[Test]
		public void ShouldHandleMultipleShiftsInRelevantProjections()
		{
			ISkill rightSkill = SkillFactory.CreateSkill("och");
			IActivity rightActivity = new Activity("och");
			rightActivity.SetId(Guid.Empty);
			rightSkill.Activity = rightActivity;

			IPerson person1 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(), new List<ISkill> { rightSkill });
			IVisualLayerCollection collection1 = VisualLayerCollectionFactory.CreateForWorkShift(person1, TimeSpan.FromHours(8),
																								 TimeSpan.FromHours(9), rightSkill.Activity);
			IList<IVisualLayerCollection> list = new List<IVisualLayerCollection> { collection1, collection1 };
			ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods = new SkillSkillStaffPeriodExtendedDictionary();

			DateTime dateTime = new DateTime(1800, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			ISkillStaffPeriod skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(rightSkill, dateTime, 60, 0, 0);
			ISkillStaffPeriodDictionary skillStaffPeriodDictionary = new SkillStaffPeriodDictionary(rightSkill);
			skillStaffPeriodDictionary.Add(skillStaffPeriod.Period, skillStaffPeriod);
			relevantSkillStaffPeriods.Add(rightSkill, skillStaffPeriodDictionary);

			_target.Calculate(list, relevantSkillStaffPeriods, _toRemove, _toAdd);

			Assert.AreEqual(2, skillStaffPeriod.CalculatedResource);
			Assert.AreEqual(2, skillStaffPeriod.CalculatedLoggedOn);
		}
		
		[Test]
		public void ShouldEmptyResourceBeforeCalculating()
		{
			ISkill rightSkill = SkillFactory.CreateSkill("och");
			IActivity rightActivity = new Activity("och");
			rightActivity.SetId(Guid.Empty);
			rightSkill.Activity = rightActivity;

			IPerson person1 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(), new List<ISkill> { rightSkill });
			IVisualLayerCollection collection1 = VisualLayerCollectionFactory.CreateForWorkShift(person1, TimeSpan.FromHours(8),
																								 TimeSpan.FromHours(9), rightSkill.Activity);
			IList<IVisualLayerCollection> list = new List<IVisualLayerCollection> { collection1 };
			ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods = new SkillSkillStaffPeriodExtendedDictionary();

			DateTime dateTime = new DateTime(1800, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			ISkillStaffPeriod skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(rightSkill, dateTime, 60, 0, 0);
			ISkillStaffPeriodDictionary skillStaffPeriodDictionary = new SkillStaffPeriodDictionary(rightSkill);
			skillStaffPeriodDictionary.Add(skillStaffPeriod.Period, skillStaffPeriod);
			relevantSkillStaffPeriods.Add(rightSkill, skillStaffPeriodDictionary);

			_target.Calculate(list, relevantSkillStaffPeriods, _toRemove, _toAdd);

			Assert.AreEqual(1, skillStaffPeriod.CalculatedResource);
			Assert.AreEqual(1, skillStaffPeriod.CalculatedLoggedOn);

			list.Clear();
			_target.Calculate(list, relevantSkillStaffPeriods, _toRemove, _toAdd);

			Assert.AreEqual(0, skillStaffPeriod.CalculatedResource);
			Assert.AreEqual(0, skillStaffPeriod.CalculatedLoggedOn);
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

			_target.Calculate(new List<IVisualLayerCollection>(), relevantSkillStaffPeriods, _toRemove, list);

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

			_target.Calculate(new List<IVisualLayerCollection>(), relevantSkillStaffPeriods, _toRemove, list);

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

			_target.Calculate(new List<IVisualLayerCollection>(), relevantSkillStaffPeriods, _toRemove, list);

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

			_target.Calculate(new List<IVisualLayerCollection>(), relevantSkillStaffPeriods, _toRemove, list);

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

			_target.Calculate(new List<IVisualLayerCollection>(), relevantSkillStaffPeriods, _toRemove, list);

			Assert.AreEqual(0.5, skillStaffPeriod.CalculatedResource);
			Assert.AreEqual(0.5, skillStaffPeriod.CalculatedLoggedOn);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldUseAgentPersonalSkillEfficiency()
		{
			ISkill rightSkill = SkillFactory.CreateSkill("och");
			IActivity rightActivity = new Activity("och");
			rightActivity.SetId(Guid.Empty);
			rightSkill.Activity = rightActivity;

			IPerson person1 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(), new List<ISkill> { rightSkill });
			person1.Period(new DateOnly()).PersonSkillCollection[0].SkillPercentage = new Percent(0.5);
			IVisualLayerCollection collection1 = VisualLayerCollectionFactory.CreateForWorkShift(person1, TimeSpan.FromHours(8),
																								 TimeSpan.FromHours(9), rightSkill.Activity);
			IList<IVisualLayerCollection> list = new List<IVisualLayerCollection> { collection1 };
			ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods = new SkillSkillStaffPeriodExtendedDictionary();

			DateTime dateTime = new DateTime(1800, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			ISkillStaffPeriod skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(rightSkill, dateTime, 60, 0, 0);
			ISkillStaffPeriodDictionary skillStaffPeriodDictionary = new SkillStaffPeriodDictionary(rightSkill);
			skillStaffPeriodDictionary.Add(skillStaffPeriod.Period, skillStaffPeriod);
			relevantSkillStaffPeriods.Add(rightSkill, skillStaffPeriodDictionary);

			_target.Calculate(new List<IVisualLayerCollection>(), relevantSkillStaffPeriods, _toRemove, list);

			Assert.AreEqual(0.5, skillStaffPeriod.CalculatedResource);
			Assert.AreEqual(1, skillStaffPeriod.CalculatedLoggedOn);

			_target.Calculate(new List<IVisualLayerCollection>(), relevantSkillStaffPeriods, _toRemove, list);
			Assert.AreEqual(1, skillStaffPeriod.CalculatedResource);
			Assert.AreEqual(2, skillStaffPeriod.CalculatedLoggedOn);

			_target.Calculate(new List<IVisualLayerCollection>(), relevantSkillStaffPeriods, _toRemove, list);
			Assert.AreEqual(1.5, skillStaffPeriod.CalculatedResource);
			Assert.AreEqual(3, skillStaffPeriod.CalculatedLoggedOn);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldNotConsiderInactivePersonalSkills()
		{
			ISkill rightSkill = SkillFactory.CreateSkill("och");
			IActivity rightActivity = new Activity("och");
			rightActivity.SetId(Guid.Empty);
			rightSkill.Activity = rightActivity;

			IPerson person1 = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(), new List<ISkill> { rightSkill });
			person1.Period(new DateOnly()).PersonSkillCollection[0].Active = false;
			IVisualLayerCollection collection1 = VisualLayerCollectionFactory.CreateForWorkShift(person1, TimeSpan.FromHours(8),
																								 TimeSpan.FromHours(9), rightSkill.Activity);
			IList<IVisualLayerCollection> list = new List<IVisualLayerCollection> { collection1 };
			ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods = new SkillSkillStaffPeriodExtendedDictionary();

			DateTime dateTime = new DateTime(1800, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			ISkillStaffPeriod skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(rightSkill, dateTime, 60, 0, 0);
			ISkillStaffPeriodDictionary skillStaffPeriodDictionary = new SkillStaffPeriodDictionary(rightSkill);
			skillStaffPeriodDictionary.Add(skillStaffPeriod.Period, skillStaffPeriod);
			relevantSkillStaffPeriods.Add(rightSkill, skillStaffPeriodDictionary);

			_target.Calculate(new List<IVisualLayerCollection>(), relevantSkillStaffPeriods, _toRemove, list);

			Assert.AreEqual(0, skillStaffPeriod.CalculatedResource);
			Assert.AreEqual(0, skillStaffPeriod.CalculatedLoggedOn);
		}
	}
}