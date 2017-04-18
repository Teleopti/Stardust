using System.Runtime.Serialization;
using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class DateTimeTimeSpanDictionaryTest
    {
        private DateDateTimePeriodDictionary target;

        [Test]
        public void VerifyCanCreateInstance()
        {
            target = new DateDateTimePeriodDictionary();
            Assert.IsNotNull(target);
        }

        [Test]
        public void VerifyCanCreateInstanceOverload()
        {
            target = new DateTimeTimeSpanDictionaryTestClass();
            Assert.IsNotNull(target);
        }

        private class DateTimeTimeSpanDictionaryTestClass : DateDateTimePeriodDictionary
        {
            public DateTimeTimeSpanDictionaryTestClass() : base(null,new StreamingContext())
            {}
        }
    }
}
