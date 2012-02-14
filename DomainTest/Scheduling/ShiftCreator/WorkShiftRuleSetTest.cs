﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.DomainTest.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.ShiftCreator
{
    [TestFixture]
    public class WorkShiftRuleSetTest
    {
        private WorkShiftRuleSet _target;
        private MockRepository _mocks;
        private IWorkShiftTemplateGenerator _generator;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _generator = new WorkShiftTemplateGenerator(ActivityFactory.CreateActivity("sample"),
                                               new TimePeriodWithSegment(10, 0, 12, 0, 60),
                                               new TimePeriodWithSegment(11, 0, 13, 0, 60),
                                               ShiftCategoryFactory.CreateShiftCategory("sample"));

            _target = new WorkShiftRuleSet(_generator);

        }


        [Test]
        public void VerifyProtectedConstructorWorks()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_target.GetType()));
        }

        [Test]
        public void VerifyCanReadProperties()
        {
            Assert.AreSame(_generator, _target.TemplateGenerator);
            Assert.IsNotNull(_target.ExtenderCollection);
            Assert.IsNotNull(_target.LimiterCollection);
            Assert.AreEqual(new Description(), _target.Description);
            Assert.AreEqual(0, _target.RuleSetBagCollection.Count);
			Assert.AreEqual(false, _target.OnlyForRestrictions);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyCannotSetNullGeneratorInConstructor()
        {
            _target = new WorkShiftRuleSet(null);
        }

        [Test]
        public void CanSetProperties()
        {
            Description desc = new Description("asldökf");
            _target.Description = desc;
            Assert.AreEqual(desc, _target.Description);

        	_target.OnlyForRestrictions = true;
			Assert.AreEqual(true, _target.OnlyForRestrictions);
        }

        [Test]
        public void CanReadRuleSetBag()
        {
            IRuleSetBag rsBag = new RuleSetBag();
            rsBag.AddRuleSet(_target);
            Assert.IsTrue(_target.RuleSetBagCollection.Contains(rsBag));
            rsBag.RemoveRuleSet(_target);
            Assert.IsFalse(_target.RuleSetBagCollection.Contains(rsBag));
        }

        [Test]
        public void VerifyCanDeleteExtender()
        {
            IWorkShiftExtender ext = _mocks.StrictMock<IWorkShiftExtender>();
            using (_mocks.Record())
            {
                Expect.Call(ext.Parent).Return(null);
                ext.SetParent(_target);
            }
            using (_mocks.Playback())
            {
                _target.AddExtender(ext);
                Assert.AreEqual(1, _target.ExtenderCollection.Count);
                Assert.AreSame(ext, _target.ExtenderCollection[0]);
                _target.DeleteExtender(ext);
                Assert.AreEqual(0, _target.ExtenderCollection.Count);
            }
        }

        [Test]
        public void VerifyPriorityOnExtender()
        {
            IWorkShiftExtender ext1 =
                new ActivityAbsoluteStartExtender(ActivityFactory.CreateActivity("sdf"), new TimePeriodWithSegment(2, 3, 4, 5, 5),
                                                  new TimePeriodWithSegment(6, 7, 8, 9, 5));
            IWorkShiftExtender ext2 =
                new ActivityAbsoluteStartExtender(ActivityFactory.CreateActivity("sdf"), new TimePeriodWithSegment(12, 13, 14, 15, 5),
                                                  new TimePeriodWithSegment(16, 17, 18, 19, 5));
            Assert.IsNull(ext1.Priority());
            Assert.IsNull(ext2.Priority());
            _target.AddExtender(ext1);
            _target.AddExtender(ext2);
            Assert.AreEqual(1, ext1.Priority());
            Assert.AreEqual(2, ext2.Priority());
        }

        [Test]
        public void VerifyParent()
        {
            IWorkShiftExtender ext =
                new AutoPositionedActivityExtender(ActivityFactory.CreateActivity("sdf"), new TimePeriodWithSegment(), new TimeSpan(2));
            IWorkShiftLimiter limit = new ContractTimeLimiter(new TimePeriod(), TimeSpan.FromMinutes(15));
            _target.AddExtender(ext);
            _target.AddLimiter(limit);
            Assert.AreSame(_target, ext.Parent);
            Assert.AreSame(_target, limit.Parent);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyCannotAddNullAsExtender()
        {
            _target.AddExtender(null);
        }

        [Test]
        public void VerifyCanDeleteLimiter()
        {
            IWorkShiftLimiter limiter = _mocks.StrictMock<IWorkShiftLimiter>();
            using (_mocks.Record())
            {
                Expect.Call(limiter.Parent).Return(null);
                limiter.SetParent(_target);
            }
            using (_mocks.Playback())
            {
                _target.AddLimiter(limiter);
                Assert.AreEqual(1, _target.LimiterCollection.Count);
                Assert.AreSame(limiter, _target.LimiterCollection[0]);
                _target.DeleteLimiter(limiter);
                Assert.AreEqual(0, _target.LimiterCollection.Count);
            }

        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyCannotAddNullAsLimiter()
        {
            _target.AddLimiter(null);
        }


       

        [Test]
        public void VerifyICloneableEntity()
        {
            ((IEntity)_target).SetId(Guid.NewGuid());
            _target.Description = new Description("Original Description");

            // Entity clone testing.

            IWorkShiftRuleSet targetCloned = _target.EntityClone();
            Assert.IsNotNull(targetCloned);
            Assert.AreEqual(_target.Id, targetCloned.Id);
            Assert.AreEqual(_target.Description, targetCloned.Description);

            targetCloned.Description = new Description("Clone of OriginalDescription");
            Assert.AreNotEqual(_target.Description, targetCloned.Description);

            // None entity clone testing.

            targetCloned = _target.NoneEntityClone();
            Assert.IsNotNull(targetCloned);
            Assert.IsNull(targetCloned.Id);
        }

        [Test]
        public void VerifyCollectionsCloned()
        {
            ((IEntity)_target).SetId(Guid.NewGuid());

            IWorkShiftExtender ext = new ActivityRelativeEndExtender(ActivityFactory.CreateActivity("sample"), new TimePeriodWithSegment(2, 0, 5, 0, 10), new TimePeriodWithSegment(9, 0, 10, 0, 10));
            IWorkShiftLimiter limiter = new ContractTimeLimiter(new TimePeriod(11, 12, 13, 15), TimeSpan.FromMinutes(15));
            RuleSetBag rsBag = new RuleSetBag();

            _target.AddExtender(ext);
            _target.AddLimiter(limiter);
            rsBag.AddRuleSet(_target);

            // Entity clone testing.
            IWorkShiftRuleSet targetCloned = _target.EntityClone();
            DoAssertsForCollectionsCloned(_target, targetCloned, true);

            // None entity clone testing.

            targetCloned = _target.NoneEntityClone();
            DoAssertsForCollectionsCloned(_target, targetCloned, false);
        }

        [Test]
        public void VerifyDefaultAccessibility()
        {
            Assert.AreEqual(DefaultAccessibility.Included, _target.DefaultAccessibility);

            _target.DefaultAccessibility = DefaultAccessibility.Excluded;
            Assert.AreEqual(DefaultAccessibility.Excluded, _target.DefaultAccessibility);

            _target.DefaultAccessibility = DefaultAccessibility.Included;
            Assert.AreEqual(DefaultAccessibility.Included, _target.DefaultAccessibility);
        }

        [Test]
        public void VerifyCanAddDaysOfWeek()
        {
            _target.AddAccessibilityDayOfWeek(DayOfWeek.Saturday);
            _target.AddAccessibilityDayOfWeek(DayOfWeek.Sunday);

            Assert.AreEqual(2, _target.AccessibilityDaysOfWeek.Count);
            Assert.IsTrue(_target.AccessibilityDaysOfWeek.Contains(DayOfWeek.Saturday));
            Assert.IsTrue(_target.AccessibilityDaysOfWeek.Contains(DayOfWeek.Sunday));
        }

        [Test]
        public void VerifyCannotAddSameDayOfWeekTwice()
        {
            _target.AddAccessibilityDayOfWeek(DayOfWeek.Sunday);
            _target.AddAccessibilityDayOfWeek(DayOfWeek.Sunday);

            Assert.AreEqual(1, _target.AccessibilityDaysOfWeek.Count);
        }

        [Test]
        public void VerifyCanRemoveDaysOfWeek()
        {
            _target.AddAccessibilityDayOfWeek(DayOfWeek.Saturday);
            _target.AddAccessibilityDayOfWeek(DayOfWeek.Sunday);

            Assert.AreEqual(2, _target.AccessibilityDaysOfWeek.Count);

            _target.RemoveAccessibilityDayOfWeek(DayOfWeek.Sunday);

            Assert.IsFalse(_target.AccessibilityDaysOfWeek.Contains(DayOfWeek.Sunday));
        }

        [Test]
        public void VerifyCanRemoveNonExistingDayOfWeek()
        {
            _target.RemoveAccessibilityDayOfWeek(DayOfWeek.Sunday);

            Assert.AreEqual(0, _target.AccessibilityDaysOfWeek.Count);
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void VerifyDateMustBeUtcInAddAccessibilityDate()
        {
            _target.AddAccessibilityDate(new DateTime(2008, 12, 24));
        }

        [Test]
        public void VerifyCanAddDatesToDefaultAvailability()
        {
            DateTime date1 = new DateTime(2008, 7, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime date2 = new DateTime(2008, 8, 1, 0, 0, 0, DateTimeKind.Utc);

            _target.AddAccessibilityDate(date1);
            _target.AddAccessibilityDate(date2);

            Assert.AreEqual(2, _target.AccessibilityDates.Count);
            Assert.IsTrue(_target.AccessibilityDates.Contains(date1));
            Assert.IsTrue(_target.AccessibilityDates.Contains(date2));
        }

        [Test]
        public void VerifyCannotAddSameDateTwice()
        {
            DateTime date1 = new DateTime(2008, 7, 1, 0, 0, 0, DateTimeKind.Utc);
            _target.AddAccessibilityDate(date1);
            _target.AddAccessibilityDate(date1);

            Assert.AreEqual(1, _target.AccessibilityDates.Count);
        }

        [Test]
        public void VerifyCanRemoveDate()
        {
            DateTime date1 = new DateTime(2008, 7, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime date2 = new DateTime(2008, 8, 1, 0, 0, 0, DateTimeKind.Utc);

            _target.AddAccessibilityDate(date1);
            _target.AddAccessibilityDate(date2);

            Assert.AreEqual(2, _target.AccessibilityDates.Count);
            Assert.IsTrue(_target.AccessibilityDates.Contains(date1));
            Assert.IsTrue(_target.AccessibilityDates.Contains(date2));

            _target.RemoveAccessibilityDate(date1);

            Assert.IsFalse(_target.AccessibilityDates.Contains(date1));
        }

        [Test]
        public void VerifyCanRemoveNonExistingDate()
        {
            DateTime date1 = new DateTime(2008, 7, 1, 0, 0, 0, DateTimeKind.Utc);
            _target.RemoveAccessibilityDate(date1);

            Assert.AreEqual(0, _target.AccessibilityDates.Count);
        }

        [Test]
        public void VerifyCanAddDatesWithTimeToDefaultAvailability()
        {
            DateTime date1 = new DateTime(2008, 7, 1, 10, 0, 0, DateTimeKind.Utc);
            DateTime date2 = new DateTime(2008, 8, 1, 7, 0, 0, DateTimeKind.Utc);

            _target.AddAccessibilityDate(date1);
            _target.AddAccessibilityDate(date2);

            Assert.AreEqual(2, _target.AccessibilityDates.Count);
            Assert.IsTrue(_target.AccessibilityDates.Contains(date1.Date));
            Assert.IsTrue(_target.AccessibilityDates.Contains(date2.Date));
        }

        [Test]
        public void VerifyDateIsValidForWorkShiftRuleSet()
        {

            DateOnly date1 = new DateOnly(2008, 7, 1);
            DateTime date11 = new DateTime(2008, 7, 1, 0, 0, 0, DateTimeKind.Utc);
            DateOnly date2 = new DateOnly(2008, 8, 1);

            _target.AddAccessibilityDate(date11);
            _target.DefaultAccessibility = DefaultAccessibility.Included;

            Assert.IsFalse(_target.IsValidDate(date1));
            Assert.IsTrue(_target.IsValidDate(date2));

            _target.DefaultAccessibility = DefaultAccessibility.Excluded;

            Assert.IsTrue(_target.IsValidDate(date1));
            Assert.IsFalse(_target.IsValidDate(date2));
        }


        [Test]
        public void VerifyDayIsValidForWorkShiftRuleSet()
        {
            DateOnly date1 = new DateOnly(2008, 7, 1);
            DateOnly date2 = new DateOnly(2008, 8, 1);

            _target.AddAccessibilityDayOfWeek(DayOfWeek.Tuesday);
            _target.DefaultAccessibility = DefaultAccessibility.Included;

            Assert.IsFalse(_target.IsValidDate(date1));
            Assert.IsTrue(_target.IsValidDate(date2));

            _target.DefaultAccessibility = DefaultAccessibility.Excluded;

            Assert.IsTrue(_target.IsValidDate(date1));
            Assert.IsFalse(_target.IsValidDate(date2));
        }

        [Test]
        public void VerifyDayAndDateIsValidForWorkShiftRuleSet()
        {
            DateOnly date1 = new DateOnly(2008, 7, 1);
            DateOnly date2 = new DateOnly(2008, 8, 1);
            DateOnly date3 = new DateOnly(2008, 8, 2);
            DateTime date31 = new DateTime(2008, 8, 2, 0, 0, 0, DateTimeKind.Utc);
            _target.AddAccessibilityDayOfWeek(DayOfWeek.Tuesday);
            _target.AddAccessibilityDate(date31);
            _target.DefaultAccessibility = DefaultAccessibility.Included;

            Assert.IsFalse(_target.IsValidDate(date1));
            Assert.IsFalse(_target.IsValidDate(date3));
            Assert.IsTrue(_target.IsValidDate(date2));

            _target.DefaultAccessibility = DefaultAccessibility.Excluded;

            Assert.IsTrue(_target.IsValidDate(date1));
            Assert.IsTrue(_target.IsValidDate(date3));
            Assert.IsFalse(_target.IsValidDate(date2));
        }


        private static void DoAssertsForCollectionsCloned(IWorkShiftRuleSet target, IWorkShiftRuleSet cloned, bool checkForRuleSetBags)
        {

            Assert.AreEqual(1, cloned.ExtenderCollection.Count);
            Assert.AreNotSame(target.ExtenderCollection[0], cloned.ExtenderCollection[0]);
            Assert.AreEqual(cloned, cloned.ExtenderCollection[0].Parent);

            Assert.AreEqual(1, cloned.LimiterCollection.Count);
            Assert.AreNotSame(target.LimiterCollection[0], cloned.LimiterCollection[0]);
            Assert.AreEqual(cloned, cloned.LimiterCollection[0].Parent);

            if (checkForRuleSetBags)
                Assert.IsEmpty(cloned.RuleSetBagCollection);

            cloned.DeleteLimiter(cloned.LimiterCollection[0]);
            cloned.DeleteExtender(cloned.ExtenderCollection[0]);

            Assert.AreEqual(1, target.ExtenderCollection.Count);
            Assert.AreEqual(0, cloned.ExtenderCollection.Count);
            Assert.AreEqual(1, target.LimiterCollection.Count);
            Assert.AreEqual(0, cloned.LimiterCollection.Count);
        }

        [Test]
        public void VerifyMinMaxWorkTimeAddingCorrect()
        {
            IEffectiveRestriction restriction = _mocks.StrictMock<IEffectiveRestriction>();
            IRuleSetProjectionService projectionService = _mocks.StrictMock<IRuleSetProjectionService>();
            IWorkShiftVisualLayerInfo info1 = _mocks.StrictMock<IWorkShiftVisualLayerInfo>();
            IWorkShiftVisualLayerInfo info2 = _mocks.StrictMock<IWorkShiftVisualLayerInfo>();
            IWorkShift shift1 = _mocks.StrictMock<IWorkShift>();
            IWorkShift shift2 = _mocks.StrictMock<IWorkShift>();
            IVisualLayerCollection layercoll1 = _mocks.StrictMock<IVisualLayerCollection>();
            IVisualLayerCollection layercoll2 = _mocks.StrictMock<IVisualLayerCollection>();

            using (_mocks.Record())
            {
                Expect.Call(projectionService.ProjectionCollection(null)).IgnoreArguments().Return(
                    new List<IWorkShiftVisualLayerInfo> { info1, info2 }).Repeat.AtLeastOnce();
                //Expect.Call(restriction.IsLimitedWorkday).Return(true).Repeat.AtLeastOnce();
                //first shift
                Expect.Call(restriction.ValidateWorkShiftInfo(info1)).Return(true).Repeat.AtLeastOnce();
                Expect.Call(info1.WorkShift).Return(shift1).Repeat.AtLeastOnce();
                Expect.Call(shift1.ToTimePeriod()).Return(new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(17)))
                    .Repeat.AtLeastOnce();
                Expect.Call(info1.VisualLayerCollection).Return(layercoll1).Repeat.AtLeastOnce();
                Expect.Call(layercoll1.ContractTime()).Return(TimeSpan.FromHours(7)).Repeat.AtLeastOnce();
                //second shift
                Expect.Call(restriction.ValidateWorkShiftInfo(info2)).Return(true).Repeat.AtLeastOnce();
                Expect.Call(info2.WorkShift).Return(shift2).Repeat.AtLeastOnce();
                Expect.Call(shift2.ToTimePeriod()).Return(new TimePeriod(TimeSpan.FromHours(7), TimeSpan.FromHours(16)))
                    .Repeat.AtLeastOnce();
                Expect.Call(info2.VisualLayerCollection).Return(layercoll2).Repeat.AtLeastOnce();
                Expect.Call(layercoll2.ContractTime()).Return(TimeSpan.FromHours(9)).Repeat.AtLeastOnce();
            }

            IWorkTimeMinMax result; 
            using (_mocks.Playback())
            {
                result = _target.MinMaxWorkTime(projectionService, restriction);
            }
            Assert.AreEqual(TimeSpan.FromHours(7), result.StartTimeLimitation.StartTime);
            Assert.AreEqual(TimeSpan.FromHours(8), result.StartTimeLimitation.EndTime);
            Assert.AreEqual(TimeSpan.FromHours(16), result.EndTimeLimitation.StartTime);
            Assert.AreEqual(TimeSpan.FromHours(17), result.EndTimeLimitation.EndTime);
            Assert.AreEqual(TimeSpan.FromHours(7), result.WorkTimeLimitation.StartTime);
            Assert.AreEqual(TimeSpan.FromHours(9), result.WorkTimeLimitation.EndTime);

        }

        [Test]
        public void VerifyMinMaxWorkTimeDisregardsShiftOutsideRestrictions()
        {
            IEffectiveRestriction restriction = _mocks.StrictMock<IEffectiveRestriction>();
            IRuleSetProjectionService projectionService = _mocks.StrictMock<IRuleSetProjectionService>();
            IWorkShiftVisualLayerInfo info1 = _mocks.StrictMock<IWorkShiftVisualLayerInfo>();
            IWorkShiftVisualLayerInfo info2 = _mocks.StrictMock<IWorkShiftVisualLayerInfo>();
            IWorkShift shift1 = _mocks.StrictMock<IWorkShift>();
            IVisualLayerCollection layercoll1 = _mocks.StrictMock<IVisualLayerCollection>();

            using (_mocks.Record())
            {
                Expect.Call(projectionService.ProjectionCollection(null)).IgnoreArguments().Return(
                    new List<IWorkShiftVisualLayerInfo> { info1, info2 }).Repeat.AtLeastOnce();
                //Expect.Call(restriction.IsLimitedWorkday).Return(true).Repeat.AtLeastOnce();
                //first shift validates OK
                Expect.Call(restriction.ValidateWorkShiftInfo(info1)).Return(true).Repeat.AtLeastOnce();
                Expect.Call(info1.WorkShift).Return(shift1).Repeat.AtLeastOnce();
                Expect.Call(shift1.ToTimePeriod()).Return(new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(17)))
                    .Repeat.AtLeastOnce();
                Expect.Call(info1.VisualLayerCollection).Return(layercoll1).Repeat.AtLeastOnce();
                Expect.Call(layercoll1.ContractTime()).Return(TimeSpan.FromHours(7)).Repeat.AtLeastOnce();
                //second shift validates Not OK
                Expect.Call(restriction.ValidateWorkShiftInfo(info2)).Return(false).Repeat.AtLeastOnce();
            }

            IWorkTimeMinMax result;
            using (_mocks.Playback())
            {
                result = _target.MinMaxWorkTime(projectionService, restriction);
            }
            Assert.AreEqual(TimeSpan.FromHours(8), result.StartTimeLimitation.StartTime);
            Assert.AreEqual(TimeSpan.FromHours(8), result.StartTimeLimitation.EndTime);
            Assert.AreEqual(TimeSpan.FromHours(17), result.EndTimeLimitation.StartTime);
            Assert.AreEqual(TimeSpan.FromHours(17), result.EndTimeLimitation.EndTime);
            Assert.AreEqual(TimeSpan.FromHours(7), result.WorkTimeLimitation.StartTime);
            Assert.AreEqual(TimeSpan.FromHours(7), result.WorkTimeLimitation.EndTime);

        }
    }
}
