using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.ShiftCategoryDistribution
{
    
    [TestFixture]
    public class DistributionInformationExtractorTest
    {
        private MockRepository _mock;
        private List<IScheduleDay> _scheduleDays;
        private IDistributionInformationExtractor _target;
        private IShiftCategory _morning;
        private IShiftCategory _day;
        private IScheduleDay _scheduleDay1;
        private IScheduleDay _scheduleDay2;
        private IScheduleDay _scheduleDay3;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _morning = new ShiftCategory("Morning");
            _day = new ShiftCategory("Day");
            _scheduleDay1 = _mock.StrictMock<IScheduleDay>();
            _scheduleDay2 = _mock.StrictMock<IScheduleDay>();
            _scheduleDay3 = _mock.StrictMock<IScheduleDay>();
            _scheduleDays = new List<IScheduleDay>(){_scheduleDay1,_scheduleDay2,_scheduleDay3};
            
        }

        [Test]
        public void TestShiftCategoryFrequency()
        {
            _target = new DistributionInformationExtractor(_scheduleDays);

            using (_mock.Record())
            {
                scheduleDayExpect(_scheduleDay1);
                scheduleDayExpect(_scheduleDay2);
                scheduleDayExpect(_scheduleDay3);
            }

            //var result = _target.GetShiftCategoryFrequency(_morning );

        }

        private void scheduleDayExpect(IScheduleDay scheduleDay)
        {
            IPersonAssignment personAssignment = _mock.StrictMock<IPersonAssignment>();
            IPerson person = PersonFactory.CreatePerson("person1");
            IDateOnlyAsDateTimePeriod dateOnlyPeriod = new DateOnlyAsDateTimePeriod(DateOnly.Today ,TimeZoneInfo.Utc );

            Expect.Call(scheduleDay.PersonAssignment()).Return(personAssignment);
            Expect.Call(personAssignment.ShiftCategory).Return(_morning);
            Expect.Call(scheduleDay.Person).Return(person);
            Expect.Call(scheduleDay.DateOnlyAsPeriod).Return(dateOnlyPeriod);

        }
    }

    
}
