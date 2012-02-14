using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Common.Time;
using Teleopti.Ccc.WinCode.Common.Time.Timeline;
using Teleopti.Ccc.WinCodeTest.Helpers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Common.Time.Timeline
{
    [TestFixture]
    public class TimelineViewModelTest
    {
        private TimelineViewModel _target;

        [SetUp]
        public void Setup()
        {
            _target = new TimelineViewModel();
        }

        [Test]
        public void VerifyCanCreateAndDefaultValues()
        {
            Assert.IsTrue(_target.Period.ElapsedTime() < TimeSpan.FromDays(7) && _target.Period.ElapsedTime() > TimeSpan.FromHours(1), "Just make sure we have a somwewhat normal default period (not huge, not tick-small,not null etc)");
            Assert.AreEqual(_target.SelectedPeriod, _target.Period, "When not selected, the selected period is the same as the Period");
            Assert.IsTrue(_target.Interval <= _target.Period.ElapsedTime(), "The interval should always be smaller or equal than the period");

            //DefaultIntervals:
            IList<DateTimePeriod> intervalPeriods = _target.Period.Intervals(_target.Interval);
            var periodsFromIntervals = from p in _target.Intervals select p.Period;
            Assert.IsTrue(intervalPeriods.SequenceEqual(periodsFromIntervals), "The default intervals should be created from the Period");
            
            //Default visibility
            Assert.IsFalse(_target.ShowLayers);
            Assert.IsFalse(_target.ShowSelectedPeriod);
            Assert.IsFalse(_target.ShowHoverTime);
            _target.ShowLayers = true;
            _target.ShowSelectedPeriod = true;
            _target.ShowHoverTime = true;
            Assert.IsTrue(_target.ShowLayers);
            Assert.IsTrue(_target.ShowSelectedPeriod);
            Assert.IsTrue(_target.ShowHoverTime);

        }

        [Test]
        public void VerifyIntervalsChangeWhenPeriodAndIntervalChange()
        {
            CollectionListener<IntervalViewModel> listener = new CollectionListener<IntervalViewModel>(_target.Intervals);
            _target.Interval = TimeSpan.FromMinutes(15);
            Assert.IsFalse(listener.CheckChangedItems(0, 0), "Changing the interval should cause the Intervals to be recreated");
            listener.Clear();
            _target.Period = _target.Period.ChangeStartTime(_target.Interval);
            Assert.IsFalse(listener.CheckChangedItems(0, 0), "Changing the period should cause the Intervals to be recreated");
        }

        [Test]
        public void VerifyCanSelectedPeriodComparedToTheActual()
        {
            TimeSpan h = TimeSpan.FromHours(1);
            DateTime targetStart = _target.Period.StartDateTime;
            DateTime targetEnd = _target.Period.EndDateTime;
            DateTimePeriod smallerPeriod = new DateTimePeriod(targetStart.Add(h), targetEnd.Subtract(h));
            DateTimePeriod largerPeriod = new DateTimePeriod(targetStart.Subtract(h), targetEnd.Add(h));
            DateTimePeriod outsideTheActualPeriod = new DateTimePeriod(targetEnd.Add(h), targetEnd.Add(h).Add(h));

            _target.SelectedPeriod = smallerPeriod;
            Assert.AreEqual(smallerPeriod, _target.SelectedPeriod, "Ok to select a period within the actual period");

            _target.SelectedPeriod = outsideTheActualPeriod;
            Assert.AreEqual(smallerPeriod, _target.SelectedPeriod, "Selecting a period aoutside will keep the old selection");

            _target.SelectedPeriod = largerPeriod;
            Assert.AreEqual(_target.Period, _target.SelectedPeriod, "Selecting a period larger than the actual will retuen the actual");

            _target.Period = smallerPeriod;
            Assert.AreEqual(smallerPeriod, _target.SelectedPeriod, "Changing the actual period to a smaller period coerces the selcted period");
        }

        
    }
}