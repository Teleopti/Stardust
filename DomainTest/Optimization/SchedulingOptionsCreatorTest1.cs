﻿using NUnit.Framework;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class SchedulingOptionsCreatorTest1
    {
        private SchedulingOptionsCreator _target;
        private IOptimizationPreferences _optimizationPreferences;
        private ISchedulingOptions _schedulingOptions;

        [SetUp]
        public void Setup()
        {
            _target = new SchedulingOptionsCreator();
            _optimizationPreferences = new OptimizationPreferences();
            _schedulingOptions = new SchedulingOptions();
        }

        [Test]
        public void ShouldTagChangesSetInSchedulingOptions()
        {
            IScheduleTag tag = new ScheduleTag(); 
            Assert.AreNotEqual(_schedulingOptions.TagToUseOnScheduling, tag);
            _optimizationPreferences.General.ScheduleTag = tag;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.AreEqual(_schedulingOptions.TagToUseOnScheduling, tag);
        }

        [Test]
        public void ShouldUseBlockSchedulingSetInSchedulingOptions()
        {
          //  _schedulingOptions.UseBlockScheduling = BlockFinderType.None;
            //Assert.AreEqual(_schedulingOptions.UseBlockScheduling, BlockFinderType.None);
            //_optimizationPreferences.Extra.UseBlockScheduling = true;
            _optimizationPreferences.Extra.BlockFinderTypeValue = BlockFinderType.BetweenDayOff;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            //Assert.AreEqual(_schedulingOptions.UseBlockScheduling, BlockFinderType.BetweenDayOff);
        }

        [Test]
        public void ShouldUseGroupingChangesSetInSchedulingOptions()
        {
            Assert.IsFalse(_schedulingOptions.UseGroupScheduling);
            _optimizationPreferences.Extra.UseTeams = true;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
			Assert.IsTrue(_schedulingOptions.UseGroupScheduling);
        }

        [Test]
        public void ShouldGroupOnGroupPageChangesSetInSchedulingOptions()
        {
            IGroupPageLight groupPage = new GroupPageLight{Name = "Test"}; 
            Assert.AreNotEqual(_schedulingOptions.GroupOnGroupPage, groupPage);
            _optimizationPreferences.Extra.GroupPageOnTeam = groupPage;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.AreEqual(_schedulingOptions.GroupOnGroupPage, groupPage);
        }

        [Test]
        public void ShouldConsiderShortBreaksChangesSetInSchedulingOptions()
        {
            Assert.IsTrue(_schedulingOptions.ConsiderShortBreaks);
            _optimizationPreferences.Rescheduling.ConsiderShortBreaks = false;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.ConsiderShortBreaks);
        }

        [Test]
        public void ShouldOnlyShiftsWhenUnderstaffedChangesSetInSchedulingOptions()
        {
            Assert.IsFalse(_schedulingOptions.OnlyShiftsWhenUnderstaffed);
            _optimizationPreferences.Rescheduling.OnlyShiftsWhenUnderstaffed = true;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsTrue(_schedulingOptions.OnlyShiftsWhenUnderstaffed);
        }

        [Test]
        public void VerifyPreferences()
        {
            _optimizationPreferences.General.UseMustHaves = false;

            _optimizationPreferences.General.UsePreferences = true;
            _optimizationPreferences.General.PreferencesValue = 1;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.UsePreferencesMustHaveOnly);
            Assert.IsFalse(_schedulingOptions.PreferencesDaysOnly);
            Assert.IsTrue(_schedulingOptions.UsePreferences);

            _optimizationPreferences.General.UsePreferences = true;
            _optimizationPreferences.General.PreferencesValue = 0.99;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.UsePreferencesMustHaveOnly);
            Assert.IsFalse(_schedulingOptions.PreferencesDaysOnly);
            Assert.IsFalse(_schedulingOptions.UsePreferences);

            _optimizationPreferences.General.UsePreferences = true;
            _optimizationPreferences.General.PreferencesValue = 0.8;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.UsePreferencesMustHaveOnly);
            Assert.IsFalse(_schedulingOptions.PreferencesDaysOnly);
            Assert.IsFalse(_schedulingOptions.UsePreferences);

            _optimizationPreferences.General.UsePreferences = true;
            _optimizationPreferences.General.PreferencesValue = 0;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.UsePreferencesMustHaveOnly);
            Assert.IsFalse(_schedulingOptions.PreferencesDaysOnly);
            Assert.IsFalse(_schedulingOptions.UsePreferences);

            _optimizationPreferences.General.UsePreferences = false;
            _optimizationPreferences.General.PreferencesValue = 1;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.UsePreferencesMustHaveOnly);
            Assert.IsFalse(_schedulingOptions.PreferencesDaysOnly);
            Assert.IsFalse(_schedulingOptions.UsePreferences);

            _optimizationPreferences.General.UsePreferences = false;
            _optimizationPreferences.General.PreferencesValue = 0.8;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.UsePreferencesMustHaveOnly);
            Assert.IsFalse(_schedulingOptions.PreferencesDaysOnly);
            Assert.IsFalse(_schedulingOptions.UsePreferences);

            _optimizationPreferences.General.UsePreferences = false;
            _optimizationPreferences.General.PreferencesValue = 0;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.UsePreferencesMustHaveOnly);
            Assert.IsFalse(_schedulingOptions.PreferencesDaysOnly);
            Assert.IsFalse(_schedulingOptions.UsePreferences);
        }

        [Test]
        public void VerifyMustHaves()
        {
            _optimizationPreferences.General.UsePreferences = false;

            _optimizationPreferences.General.UseMustHaves = true;
            _optimizationPreferences.General.MustHavesValue = 1;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);

            Assert.IsFalse(_schedulingOptions.UsePreferencesMustHaveOnly);
			Assert.IsFalse(_schedulingOptions.PreferencesDaysOnly);
            Assert.IsFalse(_schedulingOptions.UsePreferences);

            _optimizationPreferences.General.UseMustHaves = true;
            _optimizationPreferences.General.MustHavesValue = 0.8;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.UsePreferencesMustHaveOnly);
            Assert.IsFalse(_schedulingOptions.UsePreferences);
            Assert.IsFalse(_schedulingOptions.PreferencesDaysOnly);

            _optimizationPreferences.General.UseMustHaves = true;
            _optimizationPreferences.General.MustHavesValue = 0;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.UsePreferencesMustHaveOnly);
            Assert.IsFalse(_schedulingOptions.UsePreferences);
            Assert.IsFalse(_schedulingOptions.PreferencesDaysOnly);

            _optimizationPreferences.General.UseMustHaves = false;
            _optimizationPreferences.General.MustHavesValue = 1;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.UsePreferencesMustHaveOnly);
            Assert.IsFalse(_schedulingOptions.UsePreferences);
            Assert.IsFalse(_schedulingOptions.PreferencesDaysOnly);

            _optimizationPreferences.General.UseMustHaves = false;
            _optimizationPreferences.General.MustHavesValue = 0.8;
            Assert.IsFalse(_schedulingOptions.UsePreferencesMustHaveOnly);
            Assert.IsFalse(_schedulingOptions.UsePreferences);
            Assert.IsFalse(_schedulingOptions.PreferencesDaysOnly);

            _optimizationPreferences.General.UseMustHaves = false;
            _optimizationPreferences.General.MustHavesValue = 0;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.UsePreferencesMustHaveOnly);
            Assert.IsFalse(_schedulingOptions.PreferencesDaysOnly);
            Assert.IsFalse(_schedulingOptions.UsePreferences);
        }

        [Test]
        public void VerifyRotations()
        {
            _optimizationPreferences.General.UseRotations = true;
            _optimizationPreferences.General.RotationsValue = 1;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.RotationDaysOnly);
            Assert.IsTrue(_schedulingOptions.UseRotations);

            _optimizationPreferences.General.UseRotations = true;
            _optimizationPreferences.General.RotationsValue = 0.99;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.RotationDaysOnly);
            Assert.IsFalse(_schedulingOptions.UseRotations);

            _optimizationPreferences.General.UseRotations = true;
            _optimizationPreferences.General.RotationsValue = 0.8;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.RotationDaysOnly);
            Assert.IsFalse(_schedulingOptions.UseRotations);

            _optimizationPreferences.General.UseRotations = true;
            _optimizationPreferences.General.RotationsValue = 0;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.RotationDaysOnly);
            Assert.IsFalse(_schedulingOptions.UseRotations);

            _optimizationPreferences.General.UseRotations = false;
            _optimizationPreferences.General.RotationsValue = 1;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.RotationDaysOnly);
            Assert.IsFalse(_schedulingOptions.UseRotations);

            _optimizationPreferences.General.UseRotations = false;
            _optimizationPreferences.General.RotationsValue = 0.8;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.RotationDaysOnly);
            Assert.IsFalse(_schedulingOptions.UseRotations);

            _optimizationPreferences.General.UseRotations = false;
            _optimizationPreferences.General.RotationsValue = 0;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.RotationDaysOnly);
            Assert.IsFalse(_schedulingOptions.UseRotations);
        }

        [Test]
        public void VerifyAvailabilities()
        {
            _optimizationPreferences.General.UseAvailabilities = true;
            _optimizationPreferences.General.AvailabilitiesValue = 1;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.AvailabilityDaysOnly);
            Assert.IsTrue(_schedulingOptions.UseAvailability);

            _optimizationPreferences.General.UseAvailabilities = true;
            _optimizationPreferences.General.AvailabilitiesValue = 0.99;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.AvailabilityDaysOnly);
            Assert.IsFalse(_schedulingOptions.UseAvailability);

            _optimizationPreferences.General.UseAvailabilities = true;
            _optimizationPreferences.General.AvailabilitiesValue = 0.8;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.AvailabilityDaysOnly);
            Assert.IsFalse(_schedulingOptions.UseAvailability);

            _optimizationPreferences.General.UseAvailabilities = true;
            _optimizationPreferences.General.AvailabilitiesValue = 0;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.RotationDaysOnly);
            Assert.IsFalse(_schedulingOptions.UseAvailability);

            _optimizationPreferences.General.UseAvailabilities = false;
            _optimizationPreferences.General.AvailabilitiesValue = 1;
            Assert.IsFalse(_schedulingOptions.AvailabilityDaysOnly);
            Assert.IsFalse(_schedulingOptions.UseAvailability);

            _optimizationPreferences.General.UseAvailabilities = false;
            _optimizationPreferences.General.AvailabilitiesValue = 0.8;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.AvailabilityDaysOnly);
            Assert.IsFalse(_schedulingOptions.UseAvailability);

            _optimizationPreferences.General.UseAvailabilities = false;
            _optimizationPreferences.General.AvailabilitiesValue = 0;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.AvailabilityDaysOnly);
            Assert.IsFalse(_schedulingOptions.UseAvailability);
        }

        [Test]
        public void VerifyStudentAvailabilities()
        {
            _optimizationPreferences.General.UseStudentAvailabilities = true;
            _optimizationPreferences.General.StudentAvailabilitiesValue = 1;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsTrue(_schedulingOptions.UseStudentAvailability);

            _optimizationPreferences.General.UseStudentAvailabilities = true;
            _optimizationPreferences.General.StudentAvailabilitiesValue = 0.99;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.UseStudentAvailability);

            _optimizationPreferences.General.UseStudentAvailabilities = true;
            _optimizationPreferences.General.StudentAvailabilitiesValue = 0.8;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.UseStudentAvailability);

            _optimizationPreferences.General.UseStudentAvailabilities = true;
            _optimizationPreferences.General.StudentAvailabilitiesValue = 0;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.UseStudentAvailability);

            _optimizationPreferences.General.UseStudentAvailabilities = false;
            _optimizationPreferences.General.StudentAvailabilitiesValue = 1;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.UseStudentAvailability);

            _optimizationPreferences.General.UseStudentAvailabilities = false;
            _optimizationPreferences.General.StudentAvailabilitiesValue = 0.8;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.UseStudentAvailability);

            _optimizationPreferences.General.UseStudentAvailabilities = false;
            _optimizationPreferences.General.StudentAvailabilitiesValue = 0;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.UseStudentAvailability);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Levelling"), Test]
        public void VerifyLevellingOptions()
        {
            _optimizationPreferences.Extra.UseLevellingOption = true;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsTrue(_schedulingOptions.UseLevellingPerOption );

            _optimizationPreferences.Extra.UseLevellingSameEndTime  = false;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.UseLevellingSameEndTime);

            _optimizationPreferences.Extra.UseLevellingSameShift  = false ;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsFalse(_schedulingOptions.UseLevellingSameShift);

            _optimizationPreferences.Extra.UseLevellingSameShiftCategory  = true;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsTrue(_schedulingOptions.UseLevellingSameShiftCategory);

            _optimizationPreferences.Extra.UseLevellingSameStartTime  = true;
            _schedulingOptions = _target.CreateSchedulingOptions(_optimizationPreferences);
            Assert.IsTrue(_schedulingOptions.UseLevellingSameStartTime);

        }

    }
}
