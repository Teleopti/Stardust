using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation.GroupScheduling
{
	[TestFixture]
	public class RuleSetBagForGroupPersonTest
	{
		private IRuleSetBag _target;
		private MockRepository _mocks;

		[SetUp]
		public void Setup()
		{
			_target = new RuleSetBagForGroupPerson();
			_mocks = new MockRepository();
		}

		[Test]
		public void VerifyDefaultProperties()
		{
			Assert.AreEqual(0, _target.RuleSetCollection.Count);
			Assert.AreEqual(new Description(), _target.Description);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Choosable"), Test, ExpectedException(typeof(NotImplementedException))]
		public void VerifyIsChoosable()
		{
			Assert.IsTrue(_target.IsChoosable);
		}


		[Test]
		public void CanSetProperties()
		{
			Description newDesc = new Description("sdf");
			_target.Description = newDesc;
			Assert.AreEqual(newDesc, _target.Description);
		}

		[Test, ExpectedException(typeof(NotImplementedException))]
		public void VerifyICloneableEntity()
		{
			((IEntity)_target).SetId(Guid.NewGuid());
			foreach (IWorkShiftRuleSet ruleSet in _target.RuleSetCollection)
			{
				_target.RemoveRuleSet(ruleSet);
			}
			IWorkShiftRuleSet ruleSet1 = _mocks.StrictMock<IWorkShiftRuleSet>();
			IWorkShiftRuleSet ruleSet2 = _mocks.StrictMock<IWorkShiftRuleSet>();
			_target.AddRuleSet(ruleSet1);
			_target.AddRuleSet(ruleSet2);

			IRuleSetBag clonedTarget = _target.NoneEntityClone();
			Assert.IsNull(clonedTarget.Id);

			// Change description.
			clonedTarget.Description = new Description("Cloned");
			Assert.AreNotSame(clonedTarget.Description, _target.Description);

			// Check for rule set count.
			Assert.AreEqual(clonedTarget.RuleSetCollection.Count, 2);
			clonedTarget.RemoveRuleSet(ruleSet1);
			Assert.AreEqual(clonedTarget.RuleSetCollection.Count, 1);
			clonedTarget.RemoveRuleSet(ruleSet2);
			Assert.AreEqual(clonedTarget.RuleSetCollection.Count, 0);

			// Check for original.
			Assert.AreEqual(_target.RuleSetCollection.Count, 2);

			clonedTarget = (IRuleSetBag)_target.Clone();
			Assert.IsNotNull(clonedTarget.Id);
		}

		[Test]
		public void CanAddRuleSet()
		{
			IWorkShiftRuleSet ruleSet = _mocks.StrictMock<IWorkShiftRuleSet>();
			_target.AddRuleSet(ruleSet);
			Assert.AreEqual(1, _target.RuleSetCollection.Count);
			Assert.AreSame(ruleSet, _target.RuleSetCollection[0]);
		}

		[Test]
		public void CanRemoveRuleSet()
		{
			IWorkShiftRuleSet ruleSet = _mocks.StrictMock<IWorkShiftRuleSet>();
			_target.AddRuleSet(ruleSet);
			Assert.AreEqual(1, _target.RuleSetCollection.Count);
			_target.RemoveRuleSet(ruleSet);
			Assert.AreEqual(0, _target.RuleSetCollection.Count);
		}

		[Test]
		public void CanClearRuleSet()
		{
			IWorkShiftRuleSet ruleSet = _mocks.StrictMock<IWorkShiftRuleSet>();
			IWorkShiftRuleSet ruleSet2 = _mocks.StrictMock<IWorkShiftRuleSet>();
			_target.AddRuleSet(ruleSet);
			_target.AddRuleSet(ruleSet2);
			_target.ClearRuleSetCollection();
			Assert.AreEqual(0, _target.RuleSetCollection.Count);
		}

		[Test]
		public void CanGetWorkTimeMinMaxFromRuleSet()
		{
			var workShiftWorkTime = new WorkShiftWorkTime(new RuleSetProjectionService(new ShiftCreatorService()));
			IWorkShiftRuleSet ruleSet1 = _mocks.StrictMock<IWorkShiftRuleSet>();
			IWorkShiftRuleSet ruleSet2 = _mocks.StrictMock<IWorkShiftRuleSet>();
			IEffectiveRestriction effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
			_target.AddRuleSet(ruleSet1);
			_target.AddRuleSet(ruleSet2);
			IWorkTimeMinMax minMax1 = new WorkTimeMinMax();
			minMax1.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(7), TimeSpan.FromHours(10));
			minMax1.EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(18), TimeSpan.FromHours(19));
			minMax1.WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(6), TimeSpan.FromHours(11));

			IWorkTimeMinMax minMax2 = new WorkTimeMinMax();
			minMax2.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(6), TimeSpan.FromHours(9));
			minMax2.EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(17), TimeSpan.FromHours(20));
			minMax2.WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(4), TimeSpan.FromHours(8));

			using (_mocks.Record())
			{
				Expect.Call(effectiveRestriction.DayOffTemplate).Return(null);
				Expect.Call(effectiveRestriction.ShiftCategory).Return(null).Repeat.AtLeastOnce();
				Expect.Call(workShiftWorkTime.CalculateMinMax(ruleSet1, effectiveRestriction)).Return(minMax1);
				Expect.Call(workShiftWorkTime.CalculateMinMax(ruleSet2, effectiveRestriction)).Return(minMax2);

				Expect.Call(ruleSet2.IsValidDate(new DateOnly(2008, 1, 1))).IgnoreArguments().Return(true).Repeat.AtLeastOnce();
				Expect.Call(ruleSet1.IsValidDate(new DateOnly(2008, 1, 1))).IgnoreArguments().Return(true).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				var minMax = _target.MinMaxWorkTime(workShiftWorkTime, new DateOnly(2008, 1, 1), effectiveRestriction);
				Assert.IsNotNull(minMax);
				Assert.AreEqual(TimeSpan.FromHours(6), minMax.StartTimeLimitation.StartTime);
				Assert.AreEqual(TimeSpan.FromHours(10), minMax.StartTimeLimitation.EndTime.Value);
				Assert.AreEqual(TimeSpan.FromHours(17), minMax.EndTimeLimitation.StartTime.Value);
				Assert.AreEqual(TimeSpan.FromHours(20), minMax.EndTimeLimitation.EndTime.Value);
				Assert.AreEqual(TimeSpan.FromHours(4), minMax.WorkTimeLimitation.StartTime.Value);
				Assert.AreEqual(TimeSpan.FromHours(11), minMax.WorkTimeLimitation.EndTime.Value);
			}
		}
		[Test]
		public void VerifyMinMaxFromRuleSetIsNullWhenRestrictionIsNull()
		{
			var workShiftWorkTime = new WorkShiftWorkTime(new RuleSetProjectionService(new ShiftCreatorService()));
			IWorkShiftRuleSet ruleSet1 = _mocks.StrictMock<IWorkShiftRuleSet>();
			IWorkShiftRuleSet ruleSet2 = _mocks.StrictMock<IWorkShiftRuleSet>();
			IEffectiveRestriction effectiveRestriction = null;
			_target.AddRuleSet(ruleSet1);
			_target.AddRuleSet(ruleSet2);
			IWorkTimeMinMax minMax1 = new WorkTimeMinMax();
			minMax1.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(7), TimeSpan.FromHours(10));
			minMax1.EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(18), TimeSpan.FromHours(19));
			minMax1.WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(6), TimeSpan.FromHours(11));

			IWorkTimeMinMax minMax2 = new WorkTimeMinMax();
			minMax2.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(6), TimeSpan.FromHours(9));
			minMax2.EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(17), TimeSpan.FromHours(20));
			minMax2.WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(4), TimeSpan.FromHours(8));

			var minMax = _target.MinMaxWorkTime(workShiftWorkTime, new DateOnly(2008, 1, 1), effectiveRestriction);
			Assert.IsNull(minMax);
		}



		[Test]
		public void CanGetShiftCategoriesFromRuleSet()
		{
			IWorkShiftRuleSet ruleSet1 = WorkShiftRuleSetFactory.Create();
			IWorkShiftRuleSet ruleSet2 = WorkShiftRuleSetFactory.Create();
			IWorkShiftRuleSet ruleSet3 = WorkShiftRuleSetFactory.Create();
			IShiftCategory shiftCategoryNight = ShiftCategoryFactory.CreateShiftCategory("Natt");
			IShiftCategory shiftCategoryDay = ShiftCategoryFactory.CreateShiftCategory("Dag");
			ruleSet1.TemplateGenerator.Category = shiftCategoryDay;
			ruleSet2.TemplateGenerator.Category = shiftCategoryNight;
			ruleSet3.TemplateGenerator.Category = shiftCategoryNight;
			_target.AddRuleSet(ruleSet1);
			_target.AddRuleSet(ruleSet2);
			_target.AddRuleSet(ruleSet3);
			IList<IShiftCategory> returnCategories = _target.ShiftCategoriesInBag();

			Assert.AreEqual(2, returnCategories.Count);
		}

		[Test]
		public void DayOffInRestrictionReturnsNullInMinMaxWorkTime()
		{
			var workShiftWorkTime = new WorkShiftWorkTime(new RuleSetProjectionService(new ShiftCreatorService()));
			IEffectiveRestriction effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
			using (_mocks.Record())
			{
				Expect.Call(effectiveRestriction.DayOffTemplate).Return(new DayOffTemplate(new Description("af")));
			}

			using (_mocks.Playback())
			{
				Assert.IsNull(_target.MinMaxWorkTime(workShiftWorkTime, new DateOnly(), effectiveRestriction));
			}

		}

		[Test]
		public void NoValidWorkShiftRuleSetsOnDayReturnsNull()
		{
			var workShiftWorkTime = new WorkShiftWorkTime(new RuleSetProjectionService(new ShiftCreatorService()));
			IEffectiveRestriction effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
			IWorkShiftRuleSet ruleSet = _mocks.StrictMock<IWorkShiftRuleSet>();
			_target.AddRuleSet(ruleSet);

			using (_mocks.Record())
			{
				Expect.Call(effectiveRestriction.DayOffTemplate).Return(null);
				Expect.Call(ruleSet.IsValidDate(new DateOnly())).IgnoreArguments().Return(false);
			}

			using (_mocks.Playback())
			{
				Assert.IsNull(_target.MinMaxWorkTime(workShiftWorkTime, new DateOnly(), effectiveRestriction));
			}
		}
	}
}