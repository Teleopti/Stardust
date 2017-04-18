using System;
using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Converters.DateTimeConverter;

namespace Teleopti.Ccc.WinCodeTest.Converters.DateTimeConverter
{
    [TestFixture]
    public class TimeSpanToDateTimeConverterTest
    {
        private TimeSpanToDateTimeConverter _target;
        private DateTime _dateTime;
        private TimeSpan _span;

        [SetUp]
        public void Setup()
        {
            _target = new TimeSpanToDateTimeConverter();
            _span = TimeSpan.FromMinutes(15);
            _dateTime = new DateTime(2001, 1, 1);
        }

        [Test]
        public void VerifyCombinesParameterAndValue()
        {
            Assert.AreEqual(_dateTime.Add(_span),_target.Convert(_span,typeof(TimeSpan),_dateTime,null));
        }

        [Test]
        public void VerifyWorksWithoutSupplyingADateTimeParameter()
        {
            Assert.IsTrue(_target.Convert(_span,typeof(TimeSpan),null,null)is DateTime);
        }

        [Test]
        public void VerifyConvertBackWithDefaultValue()
        {
            DateTime converted = (DateTime)_target.Convert(_span, typeof (TimeSpan), null, null);

            TimeSpan convertedBack = (TimeSpan)_target.ConvertBack(converted, typeof (DateTime), null, null);
            Assert.AreEqual(_span, convertedBack);
        }

    }
}
