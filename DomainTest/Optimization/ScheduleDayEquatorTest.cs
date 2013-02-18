﻿using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    /// <summary>
    /// Test for ScheduleDayEquator
    /// </summary>
    /// <remarks>
    /// See also ScheduleDayEquatorTest.doc document about testcases
    /// </remarks>
    [TestFixture]
    public class ScheduleDayEquatorTest
    {
        private ScheduleDayEquator _target;

        [SetUp]
        public void Setup()
        {
            _target = new ScheduleDayEquator();
        }

	
		#region Day Off and MainShift testcases when days has different significant parts

		[Test]
        public void ShouldBothDayOffAndMainShiftEqualIfOriginalHaveDayOffCurrentHasMainShift()
        {
            SchedulePartFactoryForDomain schedulePartFactory = CreateSchedulePartFactory();

            IScheduleDay original = schedulePartFactory.CreatePartWithMainShift();
            IScheduleDay current = schedulePartFactory.CreatePartWithMainShift();
            SetIdOnShiftCategories(original, current, Guid.NewGuid());
            Assert.IsTrue(_target.MainShiftEquals(original, current));

            IDayOffTemplate dayOffTemplate = new DayOffTemplate(new Description("DayOff"));
            original.CreateAndAddDayOff(dayOffTemplate);

            Assert.IsTrue(_target.DayOffEquals(original, current));
            Assert.IsTrue(_target.MainShiftEquals(original, current));

            original.DeleteMainShift(original);
            Assert.AreEqual(0, original.PersonAssignmentCollection().Count);

            Assert.IsTrue(_target.DayOffEquals(original, current));
            Assert.IsTrue(_target.MainShiftEquals(original, current));
        }

        [Test]
        public void ShouldDayOffBeDifferentMainShiftEqualIfOriginalHaveMainShiftCurrentHasDayOff()
        {
            SchedulePartFactoryForDomain schedulePartFactory = CreateSchedulePartFactory();

            IScheduleDay original = schedulePartFactory.CreatePartWithMainShift();
            IScheduleDay current = schedulePartFactory.CreatePartWithMainShift();
            SetIdOnShiftCategories(original, current, Guid.NewGuid());
            Assert.IsTrue(_target.MainShiftEquals(original, current));

            IDayOffTemplate dayOffTemplate = new DayOffTemplate(new Description("DayOff"));
            current.CreateAndAddDayOff(dayOffTemplate);

            Assert.IsFalse(_target.DayOffEquals(original, current));
            Assert.IsTrue(_target.MainShiftEquals(original, current));

            current.DeleteMainShift(current);
            Assert.AreEqual(0, current.PersonAssignmentCollection().Count);

            Assert.IsFalse(_target.DayOffEquals(original, current));
            Assert.IsTrue(_target.MainShiftEquals(original, current));
        }

        #endregion

        #region MainShift testcases when both days has MainShift as significant part

        [Test]
        public void ShouldMainShiftBeDifferentIfOriginalReplacesMainShift()
        {
            SchedulePartFactoryForDomain schedulePartFactory = CreateSchedulePartFactory();

            IScheduleDay original = schedulePartFactory.CreatePartWithMainShift();
            IScheduleDay current = schedulePartFactory.CreatePartWithMainShift();

            SetIdOnShiftCategories(original, current, Guid.NewGuid());
            IMainShift newMainShift = new MainShift(original.PersonAssignmentCollection()[0].MainShift.ShiftCategory);
            original.AddMainShift(newMainShift);

            Assert.IsTrue(_target.DayOffEquals(original, current));
            Assert.IsFalse(_target.MainShiftEquals(original, current));
        }

        [Test]
        public void ShouldMainShiftBeDifferentIfCurrentReplacesMainShift()
        {
            SchedulePartFactoryForDomain schedulePartFactory = CreateSchedulePartFactory();

            IScheduleDay original = schedulePartFactory.CreatePartWithMainShift();
            IScheduleDay current = schedulePartFactory.CreatePartWithMainShift();

            SetIdOnShiftCategories(original, current, Guid.NewGuid());
            IMainShift newMainShift = new MainShift(current.PersonAssignmentCollection()[0].MainShift.ShiftCategory);
            current.AddMainShift(newMainShift);

            Assert.IsTrue(_target.DayOffEquals(original, current));
            Assert.IsFalse(_target.MainShiftEquals(original, current));
        }

        [Test]
        public void ShouldMainShiftBeEqualIfHasCopyOfTheSameMainShift()
        {
            SchedulePartFactoryForDomain schedulePartFactory = CreateSchedulePartFactory();

            IScheduleDay original = schedulePartFactory.CreatePartWithMainShift();
            IScheduleDay current = schedulePartFactory.CreatePartWithMainShift();
            SetIdOnShiftCategories(original, current, Guid.NewGuid());

            Assert.IsTrue(_target.MainShiftEquals(original, current));
        }

        [Test]
        public void ShouldMainShiftBeDifferentIfDifferentNumberOfActivities()
        {
            SchedulePartFactoryForDomain schedulePartFactory = CreateSchedulePartFactory();

            IScheduleDay original = schedulePartFactory.CreatePartWithMainShift();
            IScheduleDay current = schedulePartFactory.CreatePartWithMainShiftWithDifferentActivities();
            SetIdOnShiftCategories(original, current, Guid.NewGuid());

            Assert.IsTrue(_target.DayOffEquals(original, current));
            Assert.IsFalse(_target.MainShiftEquals(original, current));
        }

        [Test]
        public void ShouldMainShiftBeDifferentIfSameNumberOfLayerDifferentOrder()
        {
            SchedulePartFactoryForDomain schedulePartFactory = CreateSchedulePartFactory();

            IScheduleDay original = schedulePartFactory.CreatePartWithMainShift();
            IScheduleDay current = schedulePartFactory.CreatePartWithMainShift();
            IPersonAssignment personAssingment = current.PersonAssignmentCollection()[0];
            Assert.AreEqual(2, personAssingment.MainShift.LayerCollection.Count);

            // change order
            ILayer<IActivity> activity1 = personAssingment.MainShift.LayerCollection[0];
            ILayer<IActivity> activity2 = personAssingment.MainShift.LayerCollection[1];
            personAssingment.MainShift.LayerCollection.Clear();
            Assert.AreEqual(0, personAssingment.MainShift.LayerCollection.Count);
            personAssingment.MainShift.LayerCollection.Add(activity2);
            personAssingment.MainShift.LayerCollection.Add(activity1);

            Assert.IsTrue(_target.DayOffEquals(original, current));
            Assert.IsFalse(_target.MainShiftEquals(original, current));
        }

        [Test]
        public void ShouldMainShiftBeDifferentIfOneActivityHasChangedPeriodLength()
        {
            SchedulePartFactoryForDomain schedulePartFactory = CreateSchedulePartFactory();

            IScheduleDay scheduleDay1 = schedulePartFactory.CreatePartWithMainShift();
            IScheduleDay scheduleDay2 = schedulePartFactory.CreatePartWithMainShift();
            SetIdOnShiftCategories(scheduleDay1, scheduleDay2, Guid.NewGuid());

            IActivity activity = ActivityFactory.CreateActivity("Hej");
            DateTimePeriod layerPeriod =
                scheduleDay2.PersonAssignmentCollection()[0].MainShift.LayerCollection[1].Period.ChangeEndTime(TimeSpan.FromHours(1));
            IShiftCategory category = scheduleDay1.PersonAssignmentCollection()[0].MainShift.ShiftCategory;
            scheduleDay2.PersonAssignmentCollection()[0].SetMainShift(MainShiftFactory.CreateMainShift(activity, layerPeriod, category));

            Assert.IsTrue(_target.DayOffEquals(scheduleDay1, scheduleDay2));
            Assert.IsFalse(_target.MainShiftEquals(scheduleDay1, scheduleDay2));
        }

        [Test]
        public void ShouldMainShiftBeDifferentIfSameMainShiftButDifferentShiftCategory()
        {
            SchedulePartFactoryForDomain schedulePartFactory = CreateSchedulePartFactory();
            IScheduleDay original = schedulePartFactory.CreatePartWithMainShift();
            original.PersonAssignmentCollection()[0].MainShift.ShiftCategory.SetId(Guid.NewGuid());
            IScheduleDay current = schedulePartFactory.CreatePartWithMainShift();
            current.PersonAssignmentCollection()[0].MainShift.ShiftCategory.SetId(Guid.NewGuid());

            Assert.IsTrue(_target.DayOffEquals(original, current));
            Assert.IsFalse(_target.MainShiftEquals(original, current));

        }

        [Test]
        public void ShouldBothDayOffAndMainShiftBeEqualIfOriginalHaveDayOffCurrentHasDayOff()
        {
            SchedulePartFactoryForDomain schedulePartFactory = CreateSchedulePartFactory();

            IScheduleDay original = schedulePartFactory.CreatePartWithMainShift();
            IScheduleDay current = schedulePartFactory.CreatePartWithMainShift();
            IScheduleDay current2 = schedulePartFactory.CreatePartWithMainShift();
            SetIdOnShiftCategories(original, current, Guid.NewGuid());
            Assert.IsTrue(_target.MainShiftEquals(original, current));

            IDayOffTemplate dayOffTemplate = new DayOffTemplate(new Description("DayOff"));
            original.CreateAndAddDayOff(dayOffTemplate);
            current.CreateAndAddDayOff(dayOffTemplate);
            current2.CreateAndAddDayOff(dayOffTemplate);

            Assert.IsTrue(_target.DayOffEquals(original, current));
            Assert.IsTrue(_target.MainShiftEquals(original, current));
            Assert.IsTrue(_target.DayOffEquals(original, current2));
            Assert.IsTrue(_target.MainShiftEquals(original, current2));

            original.DeleteMainShift(original);
            Assert.AreEqual(0, original.PersonAssignmentCollection().Count);

            Assert.IsTrue(_target.DayOffEquals(original, current));
            Assert.IsTrue(_target.MainShiftEquals(original, current));

            current.DeleteMainShift(current);
            Assert.AreEqual(0, current.PersonAssignmentCollection().Count);

            Assert.IsTrue(_target.DayOffEquals(original, current));
            Assert.IsTrue(_target.MainShiftEquals(original, current));
        }

        [Test]
        public void ShouldBothDayOffAndMainShiftBeEqualIfOriginalHasDayOffCurrentHasDayOffWithMainShiftDeleted()
        {
            SchedulePartFactoryForDomain schedulePartFactory = CreateSchedulePartFactory();

            IScheduleDay original = schedulePartFactory.CreatePartWithMainShift();
            IScheduleDay current = schedulePartFactory.CreatePartWithMainShift();
            SetIdOnShiftCategories(original, current, Guid.NewGuid());
            Assert.IsTrue(_target.MainShiftEquals(original, current));

            IDayOffTemplate dayOffTemplate = new DayOffTemplate(new Description("DayOff"));
            original.CreateAndAddDayOff(dayOffTemplate);
            current.CreateAndAddDayOff(dayOffTemplate);

            Assert.IsTrue(_target.DayOffEquals(original, current));
            Assert.IsTrue(_target.MainShiftEquals(original, current));

            original.DeleteMainShift(original);
            current.DeleteMainShift(current);
            Assert.AreEqual(0, original.PersonAssignmentCollection().Count);
            Assert.AreEqual(0, current.PersonAssignmentCollection().Count);

            Assert.IsTrue(_target.DayOffEquals(original, current));
            Assert.IsTrue(_target.MainShiftEquals(original, current));
        }

        [Test]
        public void ShouldDayOffBeEqualMainShiftDifferentIfOriginalHasNoneCurrentHasMainShift()
        {
            SchedulePartFactoryForDomain schedulePartFactory = CreateSchedulePartFactory();

            IScheduleDay original = schedulePartFactory.CreatePartWithMainShift();
            IScheduleDay current = schedulePartFactory.CreatePartWithMainShift();
            SetIdOnShiftCategories(original, current, Guid.NewGuid());

            Assert.IsTrue(_target.MainShiftEquals(original, current));
            original.DeleteMainShift(original);

            Assert.IsTrue(_target.DayOffEquals(original, current));
            Assert.IsFalse(_target.MainShiftEquals(original, current));
        }

        #endregion

		#region Bugfix 19056

		/// <summary>
		/// Bugfix for 19056> Error "ScheduleDayEquator.MainShiftEquals" during optimization on days with only a personal shift.
		/// </summary>
		[Test]
		public void ShouldReturnFalseIfCurrentPersonAssignmentHasNoMainShift()
		{
			SchedulePartFactoryForDomain schedulePartFactory = CreateSchedulePartFactory();

			IScheduleDay original = schedulePartFactory.CreatePartWithMainShift();
			IScheduleDay current = schedulePartFactory.CreatePartWithoutMainShift();
			
			schedulePartFactory.AddPersonalLayer(current);

			Assert.IsFalse(_target.MainShiftEquals(original, current));
		}

		[Test]
		public void ShouldReturnFalseIfOriginalPersonAssignmentHasNoMainShift()
		{
			SchedulePartFactoryForDomain schedulePartFactory = CreateSchedulePartFactory();

			IScheduleDay original = schedulePartFactory.CreatePartWithoutMainShift();
			IScheduleDay current = schedulePartFactory.CreatePartWithMainShift();

			schedulePartFactory.AddPersonalLayer(original);

			Assert.IsFalse(_target.MainShiftEquals(original, current));
		}

		[Test]
		public void ShouldReturnFalseIfNorOriginalNorCurrentPersonAssignmentHasNoMainShift()
		{
			SchedulePartFactoryForDomain schedulePartFactory = CreateSchedulePartFactory();

			IScheduleDay original = schedulePartFactory.CreatePartWithoutMainShift();
			IScheduleDay current = schedulePartFactory.CreatePartWithoutMainShift();

			schedulePartFactory.AddPersonalLayer(original);
			schedulePartFactory.AddPersonalLayer(current);

			Assert.IsFalse(_target.MainShiftEquals(original, current));
		}

		#endregion

		#region "Bug 20756"
		[Test]
		public void ShouldReturnFalseIfCurrentIsNoneAndOriginalIsMainShift()
		{
			var schedulePartFactory = CreateSchedulePartFactory();

			var original = schedulePartFactory.CreatePartWithMainShift();
			var current = schedulePartFactory.CreatePartWithoutMainShift();
			
			Assert.IsFalse(_target.MainShiftEquals(original, current));
		}
		
		#endregion
		
		private static void SetIdOnShiftCategories(IScheduleDay scheduleDay1, IScheduleDay scheduleDay2, Guid shiftCategoryId)
        {
            foreach (IPersonAssignment assignment in scheduleDay1.PersonAssignmentCollection())
            {
                ((IAggregateRoot)assignment.MainShift.ShiftCategory).SetId(shiftCategoryId);
            }
            foreach (IPersonAssignment assignment in scheduleDay2.PersonAssignmentCollection())
            {
                ((IAggregateRoot)assignment.MainShift.ShiftCategory).SetId(shiftCategoryId);
            }
        }

        private static SchedulePartFactoryForDomain CreateSchedulePartFactory()
        {
            IPerson person = PersonFactory.CreatePerson();
            IScenario scenario = new Scenario("TestScenario");
            DateTime startDate = new DateTime(2010, 01, 10, 0, 0, 0, DateTimeKind.Utc);
            DateTime endDate = startDate.AddDays(10);
            DateTimePeriod dateTimePeriod = new DateTimePeriod(startDate, endDate);
            ISkill skill = SkillFactory.CreateSkill("TestSkill");

            return new SchedulePartFactoryForDomain(person, scenario,
                                                    dateTimePeriod, skill);
        }
    }
}
