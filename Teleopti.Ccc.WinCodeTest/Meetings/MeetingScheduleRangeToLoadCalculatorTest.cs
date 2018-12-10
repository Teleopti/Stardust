using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Meetings
{
    [TestFixture]
    public class MeetingScheduleRangeToLoadCalculatorTest
    {
        private MeetingScheduleRangeToLoadCalculator _target;
        private DateTimePeriod _period;
        private IPerson _person;

        [SetUp]
        public void Setup()
        {
            _person = PersonFactory.CreatePerson();
            _period = new DateTimePeriod(2009, 10, 20, 2009, 10, 21);
            _target = new MeetingScheduleRangeToLoadCalculator(_period);
        }

        [Test]
        public void VerifyPeriodsAreReturned()
        {
            Assert.AreEqual(_period,_target.RequestedPeriod);
            Assert.AreEqual(_period, _target.SchedulerRangeToLoad(_person));
            Assert.AreEqual(_period, _target.SchedulerRangeToLoad(null));
        }

        [Test]
        public void ShouldBePossibleToSetJusticeLimit()
        {
            _target.JusticeValue = 10;
            Assert.That(_target.JusticeValue , Is.EqualTo(10));
        }
    }
}
