using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Rules;


namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
    [TestFixture]
    public class VirtualSchedulePeriodExtractorTest
    {
        private IVirtualSchedulePeriodExtractor _target;
        private MockRepository _mocks;
        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _target = new VirtualSchedulePeriodExtractor();
        }

        [Test]
        public void CanGetUniqueSchedulePeriods()
        {
            DateOnly dateOnly1 = new DateOnly(2009,2,2);
            DateOnly dateOnly2 = new DateOnly(2009,3,1);
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.Utc;

            IVirtualSchedulePeriod vPeriod1 = _mocks.StrictMock<IVirtualSchedulePeriod>();
            IVirtualSchedulePeriod vPeriod2 = _mocks.StrictMock<IVirtualSchedulePeriod>();
            IVirtualSchedulePeriod vPeriod3 = _mocks.StrictMock<IVirtualSchedulePeriod>();

            IScheduleDay day0 = _mocks.StrictMock<IScheduleDay>();
            IScheduleDay day1 = _mocks.StrictMock<IScheduleDay>();
            IScheduleDay day2 = _mocks.StrictMock<IScheduleDay>();
            IScheduleDay day3 = _mocks.StrictMock<IScheduleDay>();

            IList<IScheduleDay> days = new List<IScheduleDay>{day0,day1,day2,day3};
            IPerson person1 = _mocks.StrictMock<IPerson>();
            IPerson person2 = _mocks.StrictMock<IPerson>();
            IPerson person3 = _mocks.StrictMock<IPerson>();

            using (_mocks.Record())
            {
                Expect.Call(day0.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(dateOnly1,timeZoneInfo));
				Expect.Call(day1.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(dateOnly1, timeZoneInfo));
				Expect.Call(day2.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(dateOnly1, timeZoneInfo));
				Expect.Call(day3.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(dateOnly2, timeZoneInfo));

                Expect.Call(day0.Person).Return(person1).Repeat.AtLeastOnce();
                Expect.Call(day1.Person).Return(person1).Repeat.AtLeastOnce();
                Expect.Call(day2.Person).Return(person2).Repeat.AtLeastOnce();
                Expect.Call(day3.Person).Return(person3).Repeat.AtLeastOnce();

                Expect.Call(person1.VirtualSchedulePeriod(dateOnly1)).Return(vPeriod1).Repeat.Twice();
                Expect.Call(person2.VirtualSchedulePeriod(dateOnly1)).Return(vPeriod2);
                Expect.Call(person3.VirtualSchedulePeriod(dateOnly2)).Return(vPeriod3);
            }

            using (_mocks.Playback())
            {
                var lst = _target.CreateVirtualSchedulePeriodsFromScheduleDays(days);
                Assert.AreEqual(3, lst.Count());
            }
        }
    }
}
