using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job.Steps
{
    [TestFixture]
    public class ScheduleDayRestrictorTest
    {
        [Test]
        public void ShouldExcludePartsEndingAfterGivenDate()
        {
            var target = new ScheduleDayRestrictor();
            var scheduleDayEndsInsidePeriod = MockRepository.GenerateMock<IScheduleDay>();
            var scheduleDayEndsOutsidePeriod = MockRepository.GenerateMock<IScheduleDay>();
            
            IList<IScheduleDay> scheduleDayList = new List<IScheduleDay>{scheduleDayEndsInsidePeriod, scheduleDayEndsOutsidePeriod};
            var givenDate = new DateTime(2011, 1, 1, 23, 59, 59, DateTimeKind.Utc);

            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            var insideDateOnlyAsPeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2011, 1, 1), timeZoneInfo);
            var outsideDateOnlyAsPeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2011, 1, 2), timeZoneInfo);

			scheduleDayEndsInsidePeriod.Stub(x => x.DateOnlyAsPeriod).Return(insideDateOnlyAsPeriod);
			scheduleDayEndsOutsidePeriod.Stub(x => x.DateOnlyAsPeriod).Return(outsideDateOnlyAsPeriod);

			scheduleDayList = target.RemoveScheduleDayEndingTooLate(scheduleDayList, givenDate);
            
            Assert.AreEqual(1, scheduleDayList.Count);
            Assert.IsTrue(scheduleDayList.Contains(scheduleDayEndsInsidePeriod));
        }
    }
}