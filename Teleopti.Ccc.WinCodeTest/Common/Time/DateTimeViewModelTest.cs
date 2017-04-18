using System;
using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Time;
using Teleopti.Ccc.WinCodeTest.Helpers;

namespace Teleopti.Ccc.WinCodeTest.Common.Time
{
    [TestFixture]
    public class DateTimeViewModelTest
    {
        private DateTimeViewModel _target;
        private DateTime _dateTime;
        private DateTime _newDateTime;
        private PropertyChangedListener _listener;

        [SetUp]
        public void Setup()
        {
            _dateTime = new DateTime(2001, 1, 2, 3, 5, 0);
            _newDateTime = new DateTime(2002, 4, 3, 2, 1, 21);
            _target = new DateTimeViewModel(_dateTime);
            _listener = new PropertyChangedListener();
            _listener.ListenTo(_target);
        }

        [Test]
        public void VerifyConstructors()
        {
            Assert.AreEqual(_dateTime,_target.DateTime);
            _target=new DateTimeViewModel();
            Assert.AreEqual(new DateTime(),_target.DateTime);
        }

        [Test]
        public void VerifyDateProperty()
        {
            _target.Date = _newDateTime.Date;
            Assert.AreEqual(_target.Date,_newDateTime.Date);
            Assert.AreEqual(_target.Time,_dateTime.TimeOfDay,"Make sure only date is changed");
            Assert.IsTrue(_listener.HasFired("Date"));
            Assert.IsTrue(_listener.HasFired("DateTime"));
            Assert.IsFalse(_listener.HasFired("Time"));


        }

        [Test]
        public void VerifyTimeProperty()
        {
            _target.Time = _newDateTime.TimeOfDay;
            Assert.AreEqual(_target.Time, _newDateTime.TimeOfDay);
            Assert.AreEqual(_target.Date, _dateTime.Date, "Make sure only time is changed");
            Assert.IsTrue(_listener.HasFired("Time"));
            Assert.IsTrue(_listener.HasFired("DateTime"));
            Assert.IsFalse(_listener.HasFired("Date"));
        }

        [Test]
        public void VerifyDateTimeProperty()
        {
            _target.DateTime = _newDateTime;
            Assert.AreEqual(_target.DateTime, _newDateTime);
            Assert.IsTrue(_listener.HasFired("Date"));
            Assert.IsTrue(_listener.HasFired("Time"));
            Assert.IsTrue(_listener.HasFired("DateTime"));
        }

        [Test]
        public void VerifyLockDateDefaultValue()
        {
            Assert.IsFalse(_target.DateIsLocked,"Date should not be locked by default");
        }

        [Test]
        public void VerifyLockTimeDefaultValue()
        {
            Assert.IsFalse(_target.TimeIsLocked, "Date should not be locked by default");
        }

        [Test]
        public void VerifyLockDateNotifiesWhenPropertyChanges()
        {
            bool newValue = !_target.DateIsLocked;
            _target.DateIsLocked = newValue;
            Assert.IsTrue(_listener.HasFired("DateIsLocked"));
            Assert.AreEqual(newValue,_target.DateIsLocked);
        }

        [Test]
        public void VerifyLockTimeNotifiesWhenPropertyChanges()
        {
            bool newValue = !_target.TimeIsLocked;
            _target.TimeIsLocked = newValue;
            Assert.IsTrue(_listener.HasFired("TimeIsLocked"));
            Assert.AreEqual(newValue, _target.TimeIsLocked);
        }

        [Test]
        public void VerifyDateDoesNotChangeWhenLocked()
        {
            _target.DateIsLocked = true;
            DateTime oldValue = _target.Date;
            _target.Date = _target.Date.AddDays(1);
            Assert.AreEqual(oldValue,_target.Date);
        }

        [Test]
        public void VerifyTimeDoesNotChangeWhenLocked()
        {
            _target.TimeIsLocked = true;
            TimeSpan oldValue = _target.Time;
            _target.Time = _target.Time.Add(TimeSpan.FromMinutes(15));
            Assert.AreEqual(oldValue, _target.Time);
        }
    }
}
