using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Time
{
    [TestFixture]
    public class DateTimePeriodMergerTest
    {
        private DateTimePeriodMerger _target;
        private readonly DateTime dt1995_1_1 = new DateTime(1995, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private readonly DateTime dt1995_5_31 = new DateTime(1995, 5, 31, 0, 0, 0, DateTimeKind.Utc);
        private readonly DateTime dt1995_12_31 = new DateTime(1995, 12, 31, 0, 0, 0, DateTimeKind.Utc);
        private readonly DateTime dt1996_6_1 = new DateTime(1996, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        private readonly DateTime dt1996_12_31 = new DateTime(1996, 12, 31, 0, 0, 0, DateTimeKind.Utc);
        private readonly DateTime dt1997_1_1 = new DateTime(1997, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private readonly DateTime dt1997_12_31 = new DateTime(1997, 12, 31, 0, 0, 0, DateTimeKind.Utc);

        private DateTimePeriod p1995_1_1_1995_5_31;
        private DateTimePeriod p1995_1_1_1995_12_31;
        private DateTimePeriod p1996_6_1_1996_12_31;
        private DateTimePeriod p1997_1_1_1997_12_31;

        private IList<DateTimePeriod> _periods;

        [SetUp]
        public void Setup()
        {
            // input data, see expected result later
            /*
            ------
            ------------
                              ------
                                    ------------
            */

            p1995_1_1_1995_5_31 = new DateTimePeriod(dt1995_1_1, dt1995_5_31);
            p1995_1_1_1995_12_31 = new DateTimePeriod(dt1995_1_1, dt1995_12_31);
            p1996_6_1_1996_12_31 = new DateTimePeriod(dt1996_6_1, dt1996_12_31);
            p1997_1_1_1997_12_31 = new DateTimePeriod(dt1997_1_1, dt1997_12_31);

            _periods = new List<DateTimePeriod>();
            _periods.Add(p1996_6_1_1996_12_31);
            _periods.Add(p1995_1_1_1995_12_31);
            _periods.Add(p1997_1_1_1997_12_31);
            _periods.Add(p1995_1_1_1995_5_31);

            _target = new DateTimePeriodMerger(_periods);
        }

        /// <summary>
        /// Merges the date time periods.
        /// </summary>
        /// <returns>the merged list</returns>
        [Test]
        public void VerifyMergePeriods()
        {

            // expected result
            /*
            ------------
                              ------------------
            */

            IList<DateTimePeriod> mergedPeriods = _target.MergeDays();
            Assert.AreEqual(2, mergedPeriods.Count);
            Assert.AreEqual(dt1995_1_1, mergedPeriods[0].StartDateTime);
            Assert.AreEqual(dt1995_12_31, mergedPeriods[0].EndDateTime);
            Assert.AreEqual(dt1996_6_1, mergedPeriods[1].StartDateTime);
            Assert.AreEqual(dt1997_12_31, mergedPeriods[1].EndDateTime);
        }


        [Test]
        public void VerifyOrderPeriods()
        {
            IList<DateTimePeriod> resultList = _target.OrderPeriods();
            Assert.AreEqual(_periods.Count, resultList.Count);
            Assert.AreEqual(resultList[0], p1995_1_1_1995_5_31);
            Assert.AreEqual(resultList[1], p1995_1_1_1995_12_31);
            Assert.AreEqual(resultList[2], p1996_6_1_1996_12_31);
            Assert.AreEqual(resultList[3], p1997_1_1_1997_12_31);
        }



    }
}
