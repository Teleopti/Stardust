using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Common.Time;
using Teleopti.Ccc.WinCodeTest.Helpers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Common.Time
{
    [TestFixture]
    public class IntervalCollectionTest
    {
        private IntervalCollection _target;
        private DateTimePeriod _period;
        private DateTime _baseDateTime;
        private TimeSpan _interval;

        [SetUp]
        public void Setup()
        {
            _baseDateTime = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            _period = new DateTimePeriod(_baseDateTime, _baseDateTime.AddDays(1));
            _interval = TimeSpan.FromHours(1);
            _target = new IntervalCollection(_period, _interval);
        }

        [Test]
        public void VerifyCreatesModelsForEachInterval()
        {
            Assert.AreEqual(24, _target.Count());
        }

        [Test]
        public void VerifyCanSetPeriod()
        {
            _target.Change(new DateTimePeriod(_baseDateTime, _baseDateTime.Add(TimeSpan.FromHours(2))));
            Assert.AreEqual(2, _target.Count());
            Assert.AreEqual(_target.First().Period, new DateTimePeriod(_baseDateTime, _baseDateTime.Add(_interval)));
            Assert.AreEqual(_target.Last().Period, new DateTimePeriod(_baseDateTime.Add(_interval), _baseDateTime.Add(_interval).Add(_interval)));
        }

        [Test]
        public void VerifyChangesInCollectionWhenChangingPeriod()
        {
            #region setup
            CollectionListener<IntervalViewModel> listener = new CollectionListener<IntervalViewModel>(_target);

            DateTimePeriod periodWithSamePeriods = _period;
            DateTimePeriod periodWithTwoMoreHours = _period.ChangeEndTime(_interval).ChangeEndTime(_interval);
            DateTimePeriod periodWithTwoLessHours = _period.ChangeEndTime(_interval.Negate()).ChangeStartTime(_interval);
            #endregion

            _target.Change(periodWithSamePeriods);
            Assert.IsTrue(listener.CheckChangedItems(0,0), "Make sure the event hasnt fired");
            Assert.IsTrue(ListsAreEqual(periodWithSamePeriods,_interval));

            _target.Change(periodWithTwoMoreHours);
            Assert.IsTrue(ListsAreEqual(periodWithTwoMoreHours,_interval), "Two new Periods should have beeen added");
            Assert.IsTrue(listener.CheckChangedItems(2,0), "should not fire more than two times");

            listener.Clear();
            _target.Change(periodWithTwoLessHours);
            Assert.IsTrue(ListsAreEqual(periodWithTwoLessHours,_interval));
            Assert.IsTrue(listener.CheckChangedItems(0,4),"Should not have changed more than 4 times");
        }

        [Test]
        public void VerifyChangesInCollectionWhenChangingInterval()
        {           
            TimeSpan tenMinutes = TimeSpan.FromMinutes(10);
            _target.Change(tenMinutes);
            Assert.IsTrue(ListsAreEqual(_period,tenMinutes));

        }

        #region helpers
        //Lists are equal if they have the same Periods at the same place...
        private bool ListsAreEqual(DateTimePeriod period,TimeSpan interval)
        {
            return (_target.Select(d => d.Period)).SequenceEqual(period.Intervals(interval));
        }
        #endregion
    }
}
