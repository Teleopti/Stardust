using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.DayOff;


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
	    public void ShouldReturnFromMonth()
	    {
			var dateTimePeriod = new DateOnlyPeriod(2015, 12, 1, 2015, 12, 31);
			var resultDateTimeList = _target.Split(dateTimePeriod, DayOfWeek.Monday);
			Assert.AreEqual(5, resultDateTimeList.Count);
			Assert.AreEqual(new DateOnlyPeriod(2015, 11, 30, 2015, 12, 6), resultDateTimeList[0]);
			Assert.AreEqual(new DateOnlyPeriod(2015, 12, 7, 2015, 12, 13), resultDateTimeList[1]);
			Assert.AreEqual(new DateOnlyPeriod(2015, 12, 14, 2015, 12, 20), resultDateTimeList[2]);
			Assert.AreEqual(new DateOnlyPeriod(2015, 12, 21, 2015, 12, 27), resultDateTimeList[3]);
			Assert.AreEqual(new DateOnlyPeriod(2015, 12, 28, 2016, 1, 3), resultDateTimeList[4]);
	    }

		[Test]
		public void ShouldReturnOneWeekOutOfOneWeekPeriod()
		{
			var dateTimePeriod = new DateOnlyPeriod(2015, 11, 30, 2015, 12, 6);
			var resultDateTimeList = _target.Split(dateTimePeriod, DayOfWeek.Monday);
			Assert.AreEqual(dateTimePeriod, resultDateTimeList[0]);
		}


    }
}
