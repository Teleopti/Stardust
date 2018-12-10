using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff;


namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
    [TestFixture]
    public class SuitableDayOffsToGiveAwayTest
    {
        private ISuitableDayOffsToGiveAway _target;
        private IDictionary<DayOfWeek, int> _weekDayPoints;
        private IList<DateOnly> _dayCollection;
	    private DateOnly _day1;
	    private DateOnly _day2;
	    private DateOnly _day3;
	    private DateOnly _day4;

	    [SetUp]
        public void Setup()
        {
            _target = new SuitableDayOffsToGiveAway();
            _weekDayPoints = new Dictionary<DayOfWeek, int>();
            _weekDayPoints.Add(DayOfWeek.Friday, 10);
            _weekDayPoints.Add(DayOfWeek.Saturday, 20);
            _weekDayPoints.Add(DayOfWeek.Sunday, 30);
	        _day1 = new DateOnly(2014, 02, 07);
	        _day2 = new DateOnly(2014, 02, 08);
	        _day3 = new DateOnly(2014, 02, 09);
	        _day4 = new DateOnly(2014, 02, 10);
            _dayCollection = new List<DateOnly>
                {
                  _day1, _day2, _day3, _day4
                };
        }
		
        [Test]
        public void ShouldReturnDaysWithLowestToHighest()
        {
			_weekDayPoints.Add(DayOfWeek.Monday, 1);
            var result = _target.DetectMostValuableSpot(_dayCollection, _weekDayPoints);
            Assert.AreEqual(result, new List<DateOnly>{_day4, _day1, _day2, _day3});
        }
		
		[Test]
        public void ShouldNotReturnDaysWithoutPoints()
        {
            var result = _target.DetectMostValuableSpot(_dayCollection, _weekDayPoints);
            Assert.AreEqual(result, new List<DateOnly>{_day1, _day2, _day3});
        }
    }
}
