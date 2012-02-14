using System;
using System.Runtime.Serialization;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Time
{
    [TestFixture]
    public class CccTimeZoneInfoTest
    {
        private ICccTimeZoneInfo _target;
        private TimeZoneInfo _master;
        private DateTime _localTime;
        private DateTime _utcTime;

        [SetUp]
        public void Setup()
        {
            _master = TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time");
            _target = new CccTimeZoneInfo(_master);
            _localTime = new DateTime(2008,1,1,1,1,1,DateTimeKind.Local);
            _utcTime = new DateTime(2008, 1, 1, 4, 1, 1, DateTimeKind.Utc);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(_master.BaseUtcOffset, _target.BaseUtcOffset);
            Assert.AreEqual(TimeZoneInfo.ConvertTimeFromUtc(_utcTime, _master), _target.ConvertTimeFromUtc(_utcTime, _target));
            Assert.AreEqual(TimeZoneInfo.ConvertTimeToUtc(_utcTime), _target.ConvertTimeToUtc(_localTime));
            //Assert.AreEqual(_master.Local.ConvertTimeToUtc(_localTime, _master), _target.ConvertTimeToUtc(_localTime, _target));
            Assert.AreEqual(_master.DaylightName, _target.DaylightName);
            Assert.AreEqual(_master.DisplayName, _target.DisplayName);
            Assert.IsTrue( _target.Equals(_target));
            Assert.IsTrue(_target.Equals((object)_target));
            //Assert.AreEqual(_master.GetAmbiguousTimeOffsets(_utcTime), _target.GetAmbiguousTimeOffsets(_utcTime));
            Assert.AreNotEqual(new CccTimeZoneInfo(_master).GetHashCode(), _target.GetHashCode());
            Assert.AreEqual(_master.GetUtcOffset(_localTime), _target.GetUtcOffset(_localTime));
            Assert.AreEqual(_master.GetUtcOffset(new DateTimeOffset()), _target.GetUtcOffset(new DateTimeOffset()));
            Assert.AreEqual(_master.Id, _target.Id);
            Assert.AreEqual(_master.IsAmbiguousTime(_utcTime), _target.IsAmbiguousTime(_utcTime));
            Assert.AreEqual(_master.IsAmbiguousTime(new DateTimeOffset()), _target.IsAmbiguousTime(new DateTimeOffset()));
            Assert.AreEqual(_master.IsDaylightSavingTime(_utcTime), _target.IsDaylightSavingTime(_utcTime));
            Assert.AreEqual(_master.IsDaylightSavingTime(new DateTimeOffset()), _target.IsDaylightSavingTime(new DateTimeOffset()));
            Assert.AreEqual(_master.IsInvalidTime(_utcTime), _target.IsInvalidTime(_utcTime));
            Assert.AreEqual(_master.StandardName, _target.StandardName);
            Assert.AreEqual(_master.SupportsDaylightSavingTime, _target.SupportsDaylightSavingTime);
            Assert.AreEqual(_master.ToSerializedString(), _target.ToSerializedString());
            //Assert.AreEqual(new CccTimeZoneInfo(TimeZoneInfo.Utc).Utc, _target.Utc);
            Assert.AreSame(_master, _target.TimeZoneInfoObject);
        }

        [Test]
        public void VerifyConvertOverDaylightSaving()
        {
            _master = TimeZoneInfo.FindSystemTimeZoneById("Jordan Standard Time");
            DateTime theDate = new DateTime(2010,3,26);
            _target = new CccTimeZoneInfo(_master);

            DateTime ret = _target.ConvertTimeToUtc(theDate, _target);
            Assert.AreEqual(new DateTime(2010,3,25,22,00,0,DateTimeKind.Utc),ret);
            ret = _target.ConvertTimeToUtc(theDate);
            Assert.AreEqual(new DateTime(2010, 3, 25, 22, 00, 0, DateTimeKind.Utc), ret);
        }

        [Test]
        public void VerifySerialize()
        {
            SerializationInfo info = new SerializationInfo(typeof(ICccTimeZoneInfo), new FormatterConverter());
            _target.GetObjectData(info, new StreamingContext());
            Assert.AreEqual(info.GetValue("Id", typeof(string)), _target.Id);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void VerifySerializeWithNoInfo()
        {
            _target.GetObjectData(null, new StreamingContext());
        }

        [Test]
        public void VerifyDateTimeWithWrongKindDoesNotCauseCrash()
        {
            _localTime = new DateTime(2010, 3, 28, 2, 30, 0, DateTimeKind.Utc);
            _target = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            Assert.AreEqual(new DateTime(2010, 3, 28, 1, 0, 0), _target.ConvertTimeToUtc(_localTime));
        }
    }
}
