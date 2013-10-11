using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
    [TestFixture]
    public class DayIntervalGeneratorTest
    {
        [Test]
        public void VerifyCountOfIntervalForTwoDays()
        {
            Assert.AreEqual(DayIntervalGenerator.IntervalForTwoDays(60).Count, 48);
        }

        [Test]
        public void VerifyIntervalForTwoDays()
        {
            var result = DayIntervalGenerator.IntervalForTwoDays(60);
            Assert.AreEqual(result[0], new TimeSpan(0, 0, 0, 0));
            Assert.AreEqual(result[22], new TimeSpan(0, 22, 0, 0));
            Assert.AreEqual(result[47], new TimeSpan(1, 23, 0, 0));
        }

        [Test]
        public void VerifyCountOfIntervalForFirstDay()
        {
            Assert.AreEqual(DayIntervalGenerator.IntervalForFirstDay(60).Count, 24);
        }

        [Test]
        public void VerifyBoundryValuesForIntervalForFirstDay()
        {
            var result = DayIntervalGenerator.IntervalForFirstDay(60);
            Assert.AreEqual(result[0], new TimeSpan(0, 0, 0, 0));
            Assert.AreEqual(result[23], new TimeSpan(0, 23, 0, 0));
        }

        [Test]
        public void VerifyCountOfIntervalForSecondDay()
        {
            Assert.AreEqual(DayIntervalGenerator.IntervalForSecondDay(60).Count, 24);
        }

        [Test]
        public void VerifyBoundryValuesForIntervalForSecondDay()
        {
            var result = DayIntervalGenerator.IntervalForSecondDay(60);
            Assert.AreEqual(result[0], new TimeSpan(1, 0, 0, 0));
            Assert.AreEqual(result[23], new TimeSpan(1, 23, 0, 0));
        }

    }
}
