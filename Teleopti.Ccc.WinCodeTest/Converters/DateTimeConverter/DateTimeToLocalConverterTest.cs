using System;
using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Converters.DateTimeConverter;

namespace Teleopti.Ccc.WinCodeTest.Converters.DateTimeConverter
{
    [TestFixture]
    public class DateTimeToLocalConverterTest
    {
        private DateTimeToLocalConverter _target=new DateTimeToLocalConverter();
        DateTime _timeToConvert = new DateTime(2001, 1, 1, 12, 12, 0);

        [Test]
        public void VerifyThatTheConverterJustPassesTheValueInTheTransformMethod()
        {
           Assert.AreEqual(_target.Transform(_timeToConvert, null), _timeToConvert);
           Assert.AreEqual(_target.TransformBack(_timeToConvert, null), _timeToConvert);
        }
    }
}
