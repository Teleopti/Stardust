using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Analytics.Etl.Transformer.Job.Steps;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerTest.Job.Steps
{
    [TestFixture]
    public class ScheduleDayRestrictorTest
    {
        private ScheduleDayRestrictor _target;

        [Test]
        public void ShouldExcludePartsEndingAfterGivenDate()
        {
            _target = new ScheduleDayRestrictor();
            var mock = new MockRepository();
            var scheduleDayEndsInsidePeriod = mock.StrictMock<IScheduleDay>();
            var scheduleDayEndsOutsidePeriod = mock.StrictMock<IScheduleDay>();
            
            IList<IScheduleDay> scheduleDayList = new List<IScheduleDay>{scheduleDayEndsInsidePeriod, scheduleDayEndsOutsidePeriod};
            var givenDate = new DateTime(2011, 1, 1, 23, 59, 59, DateTimeKind.Utc);

            ICccTimeZoneInfo cccTimeZoneInfo = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            var insideDateOnlyAsPeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2011, 1, 1), cccTimeZoneInfo);
            var outsideDateOnlyAsPeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2011, 1, 2), cccTimeZoneInfo);

            using (mock.Record())
            {
                Expect.Call(scheduleDayEndsInsidePeriod.DateOnlyAsPeriod).Return(insideDateOnlyAsPeriod);
                Expect.Call(scheduleDayEndsOutsidePeriod.DateOnlyAsPeriod).Return(outsideDateOnlyAsPeriod);
            }

            using (mock.Playback())
            {
                scheduleDayList = _target.RemoveScheduleDayEndingTooLate(scheduleDayList, givenDate);
            }

            Assert.AreEqual(1, scheduleDayList.Count);
            Assert.IsTrue(scheduleDayList.Contains(scheduleDayEndsInsidePeriod));
        }
    }
}