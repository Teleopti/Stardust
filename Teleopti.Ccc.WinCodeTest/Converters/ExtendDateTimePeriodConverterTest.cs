using System;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Converters.DateTimeConverter;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Converters
{
    [TestFixture]
    public class ExtendDateTimePeriodConverterTest
    {
        private ExtendDateTimePeriodConverter _target;
        private DateTime _end;
        private DateTime _start;
        private DateTimePeriod _period;
        private TimeSpan _span;

        [SetUp]
        public void Setup()
        {
            _start = new DateTime(2001,1,1,0,0,0,DateTimeKind.Utc);
            _end = new DateTime(2001,1,2,0,0,0,DateTimeKind.Utc);
            _period = new DateTimePeriod(_start,_end);
            _target = new ExtendDateTimePeriodConverter();
            _span = TimeSpan.FromHours(1);
        }

        [Test]
        public void VerifyExtendsPeriodByParameter()
        {
            DateTimePeriod result = (DateTimePeriod)_target.Convert(_period, _period.GetType(), _span, null);
            Assert.AreEqual(_start.Subtract(_span), result.StartDateTime);
            Assert.AreEqual(_end.Add(_span), result.EndDateTime);
        }

        [Test]
        public void VerifyConvertBack()
        {
            DateTimePeriod result = (DateTimePeriod)_target.ConvertBack(_period, _period.GetType(), _span, null);
            Assert.AreEqual(_start.Add(_span), result.StartDateTime);
            Assert.AreEqual(_end.Subtract(_span), result.EndDateTime);
        }

        [Test]
        public void VerifyConvertsToTripleLengthWithoutParameter()
        {
            TimeSpan elapsedTime = _period.ElapsedTime();
            DateTimePeriod result = (DateTimePeriod)_target.Convert(_period, _period.GetType(), null, null);
            Assert.AreEqual(_start.Subtract(elapsedTime), result.StartDateTime);
            Assert.AreEqual(_end.Add(elapsedTime), result.EndDateTime);
        }

        [Test]
        public void VerifyReturnsMin()
        {
            DateTime minimum = DateTime.MinValue.ToUniversalTime();
            DateTimePeriod minPeriod = new DateTimePeriod(minimum,_period.EndDateTime);

            DateTimePeriod result = (DateTimePeriod)_target.Convert(minPeriod, _period.GetType(), TimeSpan.FromDays(1000), null);
            Assert.AreEqual(minimum, result.StartDateTime);
        }

        [Test]
        public void VerifyReturnsMax()
        {
            DateTime maximum = DateTime.MaxValue.ToUniversalTime();
            DateTimePeriod maxPeriod = new DateTimePeriod(_period.StartDateTime,maximum);

            DateTimePeriod result = (DateTimePeriod)_target.Convert(maxPeriod, _period.GetType(), TimeSpan.FromDays(1000), null);
            Assert.AreEqual(maximum, result.EndDateTime);
        }

        [Test]
        public void VerifyDoesNothingWhenSpanIsToLong()
        {
            DateTimePeriod result = (DateTimePeriod)_target.ConvertBack(_period, _period.GetType(), _period.ElapsedTime(), null);
            Assert.AreEqual(_period, result);
        }

    }
}
