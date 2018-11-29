using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;


namespace Teleopti.Ccc.DomainTest.Optimization.WeeklyRestSolver
{
    [TestFixture]
    public class IdentifyDayOffWithHighestSpanTest
    {
        private IdentifyDayOffWithHighestSpan _target;

        [SetUp]
        public void SetUp()
        {
            _target = new IdentifyDayOffWithHighestSpan();
        }


        [Test]
        public void ReturnMinValueIfNoDataProvided()
        {
            var result = _target.GetHighProbableDayOffPosition(new Dictionary<DateOnly, TimeSpan>());
            Assert.AreEqual(new DateOnly(), result);
        }

        [Test]
        public void ReturnADayOffWithHighestSpan()
        {
            IDictionary<DateOnly, TimeSpan> possiblePositionsToFix = new Dictionary<DateOnly, TimeSpan>();
            possiblePositionsToFix.Add(new DateOnly(2014, 03, 25), TimeSpan.FromHours(10));
            possiblePositionsToFix.Add(new DateOnly(2014, 03, 26), TimeSpan.FromHours(9));
            possiblePositionsToFix.Add(new DateOnly(2014, 03, 27), TimeSpan.FromHours(12));
            var result = _target.GetHighProbableDayOffPosition(possiblePositionsToFix);
            Assert.AreEqual(new DateOnly(2014, 03, 27), result);
        }

        [Test]
        public void ReturnFirstFoundDayOffWithSameSpan()
        {
            IDictionary<DateOnly, TimeSpan> possiblePositionsToFix = new Dictionary<DateOnly, TimeSpan>();
            possiblePositionsToFix.Add(new DateOnly(2014, 03, 25), TimeSpan.FromHours(10));
            possiblePositionsToFix.Add(new DateOnly(2014, 03, 26), TimeSpan.FromHours(10));
            possiblePositionsToFix.Add(new DateOnly(2014, 03, 27), TimeSpan.FromHours(10));
            var result = _target.GetHighProbableDayOffPosition(possiblePositionsToFix);
            Assert.AreEqual(new DateOnly(2014, 03, 25), result);
        }

    }
}
