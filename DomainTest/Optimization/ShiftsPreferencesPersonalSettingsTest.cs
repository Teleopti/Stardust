﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class ShiftsPreferencesPersonalSettingsTest
    {
        private ShiftsPreferencesPersonalSettings _target;
        private IShiftPreferences _shiftPreferencesSource;
        private IShiftPreferences _shiftPreferencesTarget;
        private List<IActivity> _activityList;

        [SetUp]
        public void Setup()
        {
            _shiftPreferencesSource = new ShiftPreferences();
            _shiftPreferencesTarget = new ShiftPreferences();
            _target = new ShiftsPreferencesPersonalSettings();
            _activityList = new List<IActivity>();
        }

        [Test]
        public void VerifyDefaultValues()
        {
            _target.MapTo(_shiftPreferencesTarget, _activityList);
            Assert.AreEqual(false, _shiftPreferencesTarget.AlterBetween );
            Assert.AreEqual(false, _shiftPreferencesTarget.KeepEndTimes );
            Assert.AreEqual(false, _shiftPreferencesTarget.KeepShiftCategories);
            Assert.AreEqual(false, _shiftPreferencesTarget.KeepShifts);
            Assert.AreEqual(0.8d, _shiftPreferencesTarget.KeepShiftsValue );
            Assert.AreEqual(false, _shiftPreferencesTarget.KeepStartTimes);
            Assert.AreEqual(new List<IActivity>(), _shiftPreferencesTarget.SelectedActivities);
            Assert.AreEqual(new TimePeriod(), _shiftPreferencesTarget.SelectedTimePeriod );
        }

        [Test]
        public void MappingShouldGetAndSetSimpleProperties()
        {
            _shiftPreferencesSource.KeepEndTimes = !_shiftPreferencesTarget.KeepEndTimes;
            _shiftPreferencesSource.AlterBetween = !_shiftPreferencesTarget.AlterBetween;
            _shiftPreferencesSource.KeepEndTimes = !_shiftPreferencesTarget.KeepEndTimes;
            _shiftPreferencesSource.KeepShiftCategories = !_shiftPreferencesTarget.KeepShiftCategories;
            _shiftPreferencesSource.KeepShifts = !_shiftPreferencesTarget.KeepShifts;
            _shiftPreferencesSource.KeepShiftsValue = 10.0d;
            _shiftPreferencesSource.KeepStartTimes = !_shiftPreferencesTarget.KeepStartTimes;
            
           

            _target.MapFrom(_shiftPreferencesSource );
            _target.MapTo(_shiftPreferencesTarget,_activityList  );

            Assert.AreEqual(_shiftPreferencesSource.AlterBetween , _shiftPreferencesTarget.AlterBetween );
            Assert.AreEqual(_shiftPreferencesSource.KeepEndTimes, _shiftPreferencesTarget.KeepEndTimes);
            Assert.AreEqual(_shiftPreferencesSource.KeepShiftCategories, _shiftPreferencesTarget.KeepShiftCategories);
            Assert.AreEqual(_shiftPreferencesSource.KeepShifts, _shiftPreferencesTarget.KeepShifts);
            Assert.AreEqual(_shiftPreferencesSource.KeepShiftsValue, _shiftPreferencesTarget.KeepShiftsValue);
            Assert.AreEqual(_shiftPreferencesSource.KeepStartTimes, _shiftPreferencesTarget.KeepStartTimes);
           
            
        }

        [Test]
        public void MappingShouldGetAndSetListProperties()
        {
            var activityList = new List<IActivity>();
            var activity = new Activity("Test");
            activity.SetId(new Guid( ));
            activityList.Add(activity);
            var timePeriod = new TimePeriod(10, 10, 12, 12);
            _shiftPreferencesSource.SelectedActivities = activityList;
            _shiftPreferencesSource.SelectedTimePeriod = timePeriod;
           

            _target.MapFrom(_shiftPreferencesSource);
            _target.MapTo(_shiftPreferencesTarget, activityList);

            Assert.AreEqual(_shiftPreferencesSource.SelectedActivities.Count , _shiftPreferencesTarget.SelectedActivities.Count );
            Assert.AreEqual(_shiftPreferencesSource.SelectedTimePeriod, _shiftPreferencesTarget.SelectedTimePeriod);
           

        }

    }
}
