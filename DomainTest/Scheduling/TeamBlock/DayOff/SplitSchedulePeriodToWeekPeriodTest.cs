using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.DayOff;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.DayOff
{
    [TestFixture]
    public class SplitSchedulePeriodToWeekPeriodTest
    {
        private SplitSchedulePeriodToWeekPeriod _target;

        [SetUp]
        public void Setup()
        {
            _target = new SplitSchedulePeriodToWeekPeriod();
        }

        [Test]
        public void ShouldReturnOneWeekOutOfOneWeekPeriod()
        {
            var dateTimePeriod = new DateOnlyPeriod(2014, 02, 21, 2014, 02, 27);
            var resultDateTimeList = _target.Split(dateTimePeriod);
            Assert.AreEqual(dateTimePeriod, resultDateTimeList[0]);
        }

        [Test]
        public void ShouldReturnTwoWeekOutOfValidSchedulePeriod()
        {
            var dateTimePeriod = new DateOnlyPeriod(2014, 02, 17, 2014, 03, 2);
            var resultDateTimeList = _target.Split(dateTimePeriod);
            Assert.AreEqual(2, resultDateTimeList.Count);
            Assert.AreEqual(new DateOnlyPeriod(2014, 02, 17, 2014, 02, 23), resultDateTimeList[0]);
            Assert.AreEqual(new DateOnlyPeriod(2014, 02, 24, 2014, 03, 2), resultDateTimeList[1]);
        }

        [Test]
        public void ShouldReturnThreeWeekOutOfValidSchedulePeriod()
        {
            var dateTimePeriod = new DateOnlyPeriod(2014, 02, 17, 2014, 03, 9);
            var resultDateTimeList = _target.Split(dateTimePeriod);
            Assert.AreEqual(3, resultDateTimeList.Count);
            Assert.AreEqual(new DateOnlyPeriod(2014, 02, 17, 2014, 02, 23), resultDateTimeList[0]);
            Assert.AreEqual(new DateOnlyPeriod(2014, 02, 24, 2014, 03, 2), resultDateTimeList[1]);
            Assert.AreEqual(new DateOnlyPeriod(2014, 03, 3, 2014, 03, 9), resultDateTimeList[2]);
        }

        [Test]
        public void ShouldReturnValidWeekForInvalidWeek()
        {
            var dateTimePeriod = new DateOnlyPeriod(2014, 02, 17, 2014, 03, 1);
            var resultDateTimeList = _target.Split(dateTimePeriod);
            Assert.AreEqual(2, resultDateTimeList.Count);
            Assert.AreEqual(new DateOnlyPeriod(2014, 02, 17, 2014, 02, 23), resultDateTimeList[0]);
            Assert.AreEqual(new DateOnlyPeriod(2014, 02, 24, 2014, 03, 1), resultDateTimeList[1]);
        }

        [Test]
        public void ShouldReturnValidWeekForWeekEndingOnFriday()
        {
            var dateTimePeriod = new DateOnlyPeriod(2014, 02, 22, 2014, 03, 7);
            var resultDateTimeList = _target.Split(dateTimePeriod);
            Assert.AreEqual(2, resultDateTimeList.Count);
            Assert.AreEqual(new DateOnlyPeriod(2014, 02, 22, 2014, 02, 28), resultDateTimeList[0]);
            Assert.AreEqual(new DateOnlyPeriod(2014, 03, 1, 2014, 03, 7), resultDateTimeList[1]);
        }
    }
}
