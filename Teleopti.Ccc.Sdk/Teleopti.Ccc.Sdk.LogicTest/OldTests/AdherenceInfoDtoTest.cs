using System;
using NUnit.Framework;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class AdherenceInfoDtoTest
    {
        private AdherenceInfoDto _target;
        private long _availableTime;
        private DateOnlyDto _dateOnlyDto;
        private long _idleTime;
        private long _loggedInTime;
        private long _scheduleWorkCtiTime;

        [SetUp]
        public void Setup()
        {
            _target = new AdherenceInfoDto();
            _availableTime = TimeSpan.FromMinutes(12).Ticks;
            _dateOnlyDto = new DateOnlyDto();
            _dateOnlyDto.DateTime = DateTime.Today;
            _idleTime = TimeSpan.FromMinutes(5).Ticks;
            _loggedInTime = TimeSpan.FromMinutes(22).Ticks;
            _scheduleWorkCtiTime = TimeSpan.FromMinutes(17).Ticks;
        }
        [Test]
        public void VerifyCanCreate()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifySetAndGetProperties()
        {
            _target.AvailableTime = _availableTime;
            _target.DateOnlyDto = _dateOnlyDto;
            _target.IdleTime = _idleTime;
            _target.LoggedInTime = _loggedInTime;
            _target.ScheduleWorkCtiTime = _scheduleWorkCtiTime;

            Assert.AreEqual(_availableTime, _target.AvailableTime);
            Assert.AreEqual(_dateOnlyDto, _target.DateOnlyDto);
            Assert.AreEqual(_idleTime, _target.IdleTime);
            Assert.AreEqual(_loggedInTime, _target.LoggedInTime);
            Assert.AreEqual(_scheduleWorkCtiTime, _target.ScheduleWorkCtiTime);
        }
    }
}
