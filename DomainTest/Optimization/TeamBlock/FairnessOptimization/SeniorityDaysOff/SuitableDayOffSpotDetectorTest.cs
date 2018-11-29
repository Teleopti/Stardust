using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff;


namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
    [TestFixture]
    public class SuitableDayOffSpotDetectorTest
    {
        private ISuitableDayOffSpotDetector _target;
        private IDictionary<DayOfWeek, int> _weekDayPoints;
        private IList<DateOnly> _dayCollection;

        [SetUp]
        public void Setup()
        {
            _target = new SuitableDayOffSpotDetector();
            _weekDayPoints = new Dictionary<DayOfWeek, int>();
            _weekDayPoints.Add(DayOfWeek.Friday, 10 );
            _weekDayPoints.Add(DayOfWeek.Saturday, 20 );
            _weekDayPoints.Add(DayOfWeek.Sunday, 30 );

            _dayCollection = new List<DateOnly>
                {
                    new DateOnly(2014, 02, 08),
                    new DateOnly(2014, 02, 07),
                    new DateOnly(2014, 02, 09)
                };
        }

        [Test]
        public void ShouldReturnNothingForEmptyList()
        {
            Assert.AreEqual( _target.DetectMostValuableSpot(new List<DateOnly>( ),new Dictionary<DayOfWeek, int>()),DateOnly.MinValue);
        }

        [Test]
        public void ShouldReturnDayWithMostPrioritySingleTry()
        {
            var result = _target.DetectMostValuableSpot(_dayCollection , _weekDayPoints );
            Assert.AreEqual(result, new DateOnly(2014,02,09));
        }

        [Test]
        public void ShouldReturnDayWithMostPriorityTwoTries()
        {
            var temp = _dayCollection;
            var firstResult = new DateOnly(2014, 02, 09);
            var secResult = new DateOnly(2014, 02, 08);
            var result = _target.DetectMostValuableSpot(temp, _weekDayPoints);
            Assert.AreEqual(result,firstResult );

            temp.Remove(firstResult);
            result = _target.DetectMostValuableSpot(temp, _weekDayPoints);
            Assert.AreEqual(result, secResult );

        }
    }
}
