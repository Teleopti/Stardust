using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Time
{
    [TestFixture]
    public class TimeZoneInfoExtensionsTest
    {
        private TimeZoneInfo _target;
        private DateTime _localTime;

        [SetUp]
        public void Setup()
        {
            _localTime = new DateTime(2008,1,1,1,1,1,DateTimeKind.Local);
        }

        [Test]
        public void VerifyConvertOverDaylightSaving()
        {
					_target = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
	        var theDate = new DateTime(2014, 3, 28, 2, 0, 0);

            DateTime ret = _target.SafeConvertTimeToUtc(theDate);
	        Assert.AreEqual(new DateTime(2014, 3, 28, 1, 00, 0, DateTimeKind.Utc), ret);
        }

        [Test]
        public void VerifyDateTimeWithWrongKindDoesNotCauseCrash()
        {
            _localTime = new DateTime(2010, 3, 28, 2, 30, 0, DateTimeKind.Utc);
            _target = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            Assert.AreEqual(new DateTime(2010, 3, 28, 1, 0, 0), _target.SafeConvertTimeToUtc(_localTime));
        }
    }
}
