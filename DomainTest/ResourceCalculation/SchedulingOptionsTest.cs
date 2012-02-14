﻿using NUnit.Framework;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class SchedulingOptionsTest
    {
        private ISchedulingOptions _target;

        [SetUp]
        public void Setup()
        {
            _target = new SchedulingOptions();
        }

        [Test]
        public void VerifyProperties()
        {
            _target.UseRotations = true;
            _target.RotationDaysOnly = true;

            Assert.IsTrue(_target.UseRotations);
            Assert.IsTrue(_target.RotationDaysOnly);

            _target.UseMaximumPersons = true;
            _target.UseMinimumPersons = true;
            Assert.IsTrue(_target.UseMaximumPersons);
            Assert.IsTrue(_target.UseMinimumPersons);

            Assert.AreEqual(0, _target.NotAllowedShiftCategories.Count);

            Assert.AreEqual(WorkShiftLengthHintOption.AverageWorkTime, _target.WorkShiftLengthHintOption);
            _target.WorkShiftLengthHintOption = WorkShiftLengthHintOption.Long;
            Assert.AreEqual(WorkShiftLengthHintOption.Long, _target.WorkShiftLengthHintOption);

            ShiftCategory category = new ShiftCategory("shiftCategory");
            Assert.IsNull(_target.ShiftCategory);
            _target.ShiftCategory = category;
            Assert.AreEqual(category, _target.ShiftCategory);
            Assert.AreEqual(true, _target.AddContractScheduleDaysOff);
            Assert.IsNull(_target.DayOffTemplate);
            IDayOffTemplate template = new DayOffTemplate(new Description("template"));
            _target.DayOffTemplate = template;
            Assert.AreEqual(template, _target.DayOffTemplate);
            Assert.AreEqual(OptimizationRestriction.None, _target.RescheduleOptions);
            _target.RescheduleOptions = OptimizationRestriction.KeepShiftCategory;
            Assert.AreEqual(OptimizationRestriction.KeepShiftCategory, _target.RescheduleOptions);
            _target.SpecificStartAndEndTime = new DateTimePeriod();
            Assert.IsNull(_target.SpecificStartAndEndTime);
            _target.RescheduleOptions = OptimizationRestriction.KeepStartAndEndTime;
            Assert.AreEqual(new DateTimePeriod(), _target.SpecificStartAndEndTime);
            _target.ShiftCategory = category;
            _target.UseBlockScheduling = BlockFinderType.BetweenDayOff;
            Assert.AreEqual(BlockFinderType.BetweenDayOff, _target.UseBlockScheduling);
            Assert.IsNull(_target.ShiftCategory);
            _target.UsePreferences = true;
            _target.UsePreferencesMustHaveOnly = true;
            Assert.IsTrue(_target.UsePreferencesMustHaveOnly);
            _target.UsePreferencesMustHaveOnly = false;
            _target.UsePreferences = true;
            Assert.IsFalse(_target.UsePreferencesMustHaveOnly);
			Assert.IsTrue(_target.UseMaxSeats);
			Assert.IsFalse(_target.DoNotBreakMaxSeats);
        	_target.DoNotBreakMaxSeats = true;
			Assert.IsTrue(_target.DoNotBreakMaxSeats);
        	_target.UseMaxSeats = false;
			Assert.IsFalse(_target.UseMaxSeats);

            Assert.IsFalse(_target.UseGroupOptimizing);
            _target.UseGroupScheduling = true;
            Assert.IsTrue(_target.UseGroupScheduling);
            Assert.IsFalse(_target.UseSameDayOffs);
            _target.UseSameDayOffs = true;
            Assert.IsTrue(_target.UseSameDayOffs);
        }

        [Test]
        public void VerifyAddContractScheduleDaysOff()
        {
            Assert.AreEqual(true, _target.AddContractScheduleDaysOff);
            _target.AddContractScheduleDaysOff = false;
            Assert.AreEqual(false, _target.AddContractScheduleDaysOff);
        }

        [Test]
        public void VerifyRotationDaysOnlyCannotBeTrueIfUseRotationsIsFalse()
        {
            _target.UseRotations = true;
            Assert.IsFalse(_target.RotationDaysOnly);
            _target.RotationDaysOnly = true;
            Assert.IsTrue(_target.RotationDaysOnly);
            _target.UseRotations = false;
            Assert.IsFalse(_target.RotationDaysOnly);
            _target.RotationDaysOnly = true;
            Assert.IsFalse(_target.RotationDaysOnly);

        }

        [Test]
        public void VerifyAvailabilityDaysOnlyCannotBeTrueIfUseAvailabilityIsFalse()
        {
            _target.UseAvailability = true;
            Assert.IsFalse(_target.AvailabilityDaysOnly);
            _target.AvailabilityDaysOnly = true;
            Assert.IsTrue(_target.AvailabilityDaysOnly);
            _target.UseAvailability = false;
            Assert.IsFalse(_target.AvailabilityDaysOnly);
            _target.AvailabilityDaysOnly = true;
            Assert.IsFalse(_target.AvailabilityDaysOnly);
            Assert.IsFalse(_target.UseAvailability);

        }
        
        [Test]
        public void VerifyPreferenceDaysOnlyAndMustHaveCannotBeTrueIfUsePreferencesIsFalse()
        {
            _target.UsePreferences = true;
            Assert.IsFalse(_target.PreferencesDaysOnly);
            Assert.IsFalse(_target.UsePreferencesMustHaveOnly);
            _target.PreferencesDaysOnly = true;
            _target.UsePreferencesMustHaveOnly = true;
            Assert.IsTrue(_target.PreferencesDaysOnly);
            Assert.IsTrue(_target.UsePreferencesMustHaveOnly);
            _target.UsePreferences = false;
            Assert.IsFalse(_target.PreferencesDaysOnly);
            Assert.IsFalse(_target.UsePreferencesMustHaveOnly);
            _target.PreferencesDaysOnly = true;
            _target.UsePreferencesMustHaveOnly = true;
            Assert.IsFalse(_target.PreferencesDaysOnly);
            Assert.IsFalse(_target.UsePreferencesMustHaveOnly);
            Assert.IsFalse(_target.UsePreferences);

        }

        [Test]
        public void VerifyClone()
        {
            ISchedulingOptions cloned = _target.Clone() as ISchedulingOptions;

            Assert.IsNotNull(cloned);
            Assert.AreNotSame(_target, cloned);
        }
    }
}
