using System;
using NUnit.Framework;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class AdherenceDataDtoTest
    {
        private AdherenceDataDto _target;
        private TimeSpan _startTime;
        private TimeSpan _endTime;
        private decimal _readyTimeMinutes = 13;
        private decimal _deviationMinutes = 2;
        private decimal _adherence = (decimal)0.72;
        private decimal _dayAdherence = (decimal)0.92;
        
        [SetUp]
        public void Setup()
        {
            _startTime = new TimeSpan(0, 10, 15, 0);
            _endTime = new TimeSpan(0, 10, 30, 0);
            _target = new AdherenceDataDto(_startTime.Ticks, _endTime.Ticks, _readyTimeMinutes, _deviationMinutes, _adherence);
        }

        [Test]
        public void VerifyPropertiesAndConstructor()
        {
            Assert.IsNotNull(_target);
            Assert.AreEqual(_readyTimeMinutes, _target.ReadyTimeMinutes);
            Assert.AreEqual(_adherence, _target.Adherence);
            Assert.AreEqual(_deviationMinutes, _target.DeviationMinutes);

            _readyTimeMinutes += 2;
            _adherence += 2;
            _deviationMinutes += 2;

            _target.DeviationMinutes = _deviationMinutes;
            _target.Adherence = _adherence;
            _target.ReadyTimeMinutes = _readyTimeMinutes;
            _target.DayAdherence = _dayAdherence;

            Assert.AreEqual(_readyTimeMinutes, _target.ReadyTimeMinutes);
            Assert.AreEqual(_adherence, _target.Adherence);
            Assert.AreEqual(_dayAdherence, _target.DayAdherence);
            Assert.AreEqual(_deviationMinutes, _target.DeviationMinutes);
            Assert.AreEqual(_startTime.Ticks, _target.LocalStartTime);
            Assert.AreEqual(_endTime.Ticks, _target.LocalEndTime);

            _target.LocalStartTime = _startTime.Add(new TimeSpan(1, 0, 0)).Ticks;
            _target.LocalEndTime = _endTime.Add(new TimeSpan(1, 0, 0)).Ticks;

            Assert.AreEqual(_startTime.Add(new TimeSpan(1, 0, 0)).Ticks, _target.LocalStartTime);
            Assert.AreEqual(_endTime.Add(new TimeSpan(1, 0, 0)).Ticks, _target.LocalEndTime);
        }
    }
}
