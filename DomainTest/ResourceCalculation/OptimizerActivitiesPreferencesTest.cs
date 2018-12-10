using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ResourceCalculation;

using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class OptimizerActivitiesPreferencesTest
    {
        private OptimizerActivitiesPreferences _target;
        private IList<IActivity> _activities;
        private IActivity _activity;

        [SetUp]
        public void Setup()
        {
            _activity = new Activity("activity");
            _activities = new List<IActivity>{_activity};
            _target = new OptimizerActivitiesPreferences();
        }

        [Test]
        public void VerifyDefaultValues()
        {
            Assert.AreEqual(false, _target.KeepShiftCategory);
            Assert.AreEqual(false, _target.KeepStartTime);
            Assert.AreEqual(false, _target.KeepEndTime);
            Assert.AreEqual(false, _target.AllowAlterBetween.HasValue);
            Assert.AreEqual(0, _target.DoNotMoveActivities.Count);
        }

        [Test]
        public void VerifySetDoNotMoveActivities()
        {
            _target.SetDoNotMoveActivities(_activities);
            Assert.IsTrue(_target.DoNotMoveActivities.Contains(_activity));
        }

        [Test]
        public void VerifyClone()
        {
            var clone = _target.Clone() as OptimizerActivitiesPreferences;
            Assert.IsNotNull(clone);
            Assert.AreNotSame(_target, clone);
        }

        [Test]
        public void VerifyUtcPeriodFromDateAndTimePeriod()
        {
            DateOnly dateOnly = new DateOnly(2010, 1, 1);
            TimeZoneInfo timeZoneInfo = (TimeZoneInfo.Utc);
            Assert.IsFalse(_target.UtcPeriodFromDateAndTimePeriod(dateOnly, timeZoneInfo).HasValue);

            TimePeriod timePeriod = new TimePeriod(new TimeSpan(0, 30, 0), new TimeSpan(6, 0, 0));
            DateTimePeriod expectedPeriod = new DateTimePeriod(new DateTime(2010, 1, 1, 0, 30, 0, DateTimeKind.Utc),
                                                               new DateTime(2010, 1, 1, 6, 0, 0, DateTimeKind.Utc));
            _target.AllowAlterBetween = timePeriod;
            Assert.AreEqual(expectedPeriod, _target.UtcPeriodFromDateAndTimePeriod(dateOnly, timeZoneInfo));
        }
    }
}
