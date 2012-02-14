﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

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
            DateTimePeriod period = new DateTimePeriod(2009,2,2,2009,2,3);
            DateTimePeriod period2 = new DateTimePeriod(2009, 3, 1, 2009, 3, 2);
            DateOnly dateOnly1 = new DateOnly(2009,2,2);
            DateOnly dateOnly2 = new DateOnly(2009,3,1);
            IPermissionInformation permissionInformation = _mocks.StrictMock<IPermissionInformation>();
            ICccTimeZoneInfo timeZoneInfo = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("UTC"));

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
                Expect.Call(day0.Period).Return(period);
                Expect.Call(day1.Period).Return(period);
                Expect.Call(day2.Period).Return(period);
                Expect.Call(day3.Period).Return(period2);

                Expect.Call(day0.Person).Return(person1).Repeat.AtLeastOnce();
                Expect.Call(day1.Person).Return(person1).Repeat.AtLeastOnce();
                Expect.Call(day2.Person).Return(person2).Repeat.AtLeastOnce();
                Expect.Call(day3.Person).Return(person3).Repeat.AtLeastOnce();

                Expect.Call(person1.PermissionInformation).Return(permissionInformation).Repeat.Twice();
                Expect.Call(person2.PermissionInformation).Return(permissionInformation);
                Expect.Call(person3.PermissionInformation).Return(permissionInformation);

                Expect.Call(permissionInformation.DefaultTimeZone()).Return(timeZoneInfo).Repeat.AtLeastOnce();

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
