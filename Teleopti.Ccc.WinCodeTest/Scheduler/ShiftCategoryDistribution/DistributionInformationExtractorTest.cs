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
            _scheduleDays = new List<IScheduleDay>{_scheduleDay1,_scheduleDay2,_scheduleDay3};
        }

        [Test]
        public void TestGetShiftFairness()
        {
            var person1 = PersonFactory.CreatePerson("person1");
            var person2 = PersonFactory.CreatePerson("person2");
            using (_mock.Record())
            {
                scheduleDayExpect(_scheduleDay1, _morning, person1);
                scheduleDayExpect(_scheduleDay2, _day, person1);
                scheduleDayExpect(_scheduleDay3, _morning, person2);
            }
            using (_mock.Playback())
            {
                _target = new DistributionInformationExtractor();
            }
            var result = _target.ShiftFairness ;
            Assert.AreEqual(result.Count(), 2);
        }

        [Test]
        public void TestGetShiftCategoryPerAgent()
        {
            var person1 = PersonFactory.CreatePerson("person1");
            var person2 = PersonFactory.CreatePerson("person2");
            using (_mock.Record())
            {
                scheduleDayExpect(_scheduleDay1, _morning, person1);
                scheduleDayExpect(_scheduleDay2, _day, person1);
                scheduleDayExpect(_scheduleDay3, _morning, person2);
            }
            using (_mock.Playback())
            {
                _target = new DistributionInformationExtractor();
            }
            var result = _target.ShiftCategoryPerAgents  ;
            Assert.AreEqual(result.Count(), 3);
        }
        
        [Test]
        public void TestGetShiftDistribution()
        {
            var person1 = PersonFactory.CreatePerson("person1");
            var person2 = PersonFactory.CreatePerson("person2");
            using (_mock.Record())
            {
                scheduleDayExpect(_scheduleDay1, _morning, person1);
                scheduleDayExpect(_scheduleDay2, _day, person1);
                scheduleDayExpect(_scheduleDay3, _morning, person2);
            }
            using (_mock.Playback())
            {
                _target = new DistributionInformationExtractor();
            }
            var result = _target.ShiftDistributions ;
            Assert.AreEqual(result.Count(), 2);
        }

        [Test]
        public void TestPersonInvolvedList()
        {
            var person1 = PersonFactory.CreatePerson("person1");
            var person2 = PersonFactory.CreatePerson("person2");
            using (_mock.Record())
            {
                scheduleDayExpect(_scheduleDay1, _morning, person1);
                scheduleDayExpect(_scheduleDay2, _day, person1);
                scheduleDayExpect(_scheduleDay3, _morning, person2);
            }
            using (_mock.Playback())
            {
                _target = new DistributionInformationExtractor();
            }
            var result = _target.PersonInvolved.OrderBy(s=>s.Name.FirstName).ToArray();
            Assert.AreEqual(result.Count(), 2);
            Assert.AreEqual(result[0] ,person1 );
            Assert.AreEqual(result[1] ,person2 );
        }

        [Test]
        public void TestDatesList()
        {
            var person1 = PersonFactory.CreatePerson("person1");
            using (_mock.Record())
            {
                scheduleDayExpect(_scheduleDay1, _morning, person1);
                scheduleDayExpect(_scheduleDay2, _day, person1);
                scheduleDayExpect(_scheduleDay3, _morning, person1);
            }
            using (_mock.Playback())
            {
                _target = new DistributionInformationExtractor();
            }
            var result = _target.Dates.OrderBy(s=>s.Date ).ToArray();
            Assert.AreEqual(result.Count(), 1);
        }

        [Test]
        public void TestShiftCategoryList()
        {
            var person1 = PersonFactory.CreatePerson("person1");
            using (_mock.Record())
            {
                scheduleDayExpect(_scheduleDay1, _morning, person1);
                scheduleDayExpect(_scheduleDay2, _day, person1);
                scheduleDayExpect(_scheduleDay3, _morning, person1);
            }
            using (_mock.Playback())
            {
                _target = new DistributionInformationExtractor();
            }
            var result = _target.ShiftCategories.OrderBy(s=>s.Description.Name ).ToArray() ;
            Assert.AreEqual(result.Count(), 2);
            Assert.AreEqual(result[0], _day );
            Assert.AreEqual(result[1], _morning );
        }

        [Test]
        public void TestShiftCategoryFrequency()
        {
            var person1 = PersonFactory.CreatePerson("person1");
            using (_mock.Record())
            {
                scheduleDayExpect(_scheduleDay1, _morning, person1);
                scheduleDayExpect(_scheduleDay2,_day,person1);
                scheduleDayExpect(_scheduleDay3,_morning,person1 );
            }
            using (_mock.Playback())
            {
                _target = new DistributionInformationExtractor();
            }
            var result = _target.GetShiftCategoryFrequency(_morning );
            Assert.AreEqual(result.Count(), 1);
            Assert.AreEqual(result.Keys.First( ), 2);
            Assert.AreEqual(result.Values.First( ), 1);
        }
        
        private void scheduleDayExpect(IScheduleDay scheduleDay,IShiftCategory shiftCategory,IPerson person  )
        {
            var personAssignment = _mock.StrictMock<IPersonAssignment>();
            IDateOnlyAsDateTimePeriod dateOnlyPeriod = new DateOnlyAsDateTimePeriod(DateOnly.Today ,TimeZoneInfo.Utc );

            Expect.Call(scheduleDay.PersonAssignment()).Return(personAssignment);
            Expect.Call(personAssignment.ShiftCategory).Return(shiftCategory).Repeat.Twice();
            Expect.Call(scheduleDay.Person).Return(person);
            Expect.Call(scheduleDay.DateOnlyAsPeriod).Return(dateOnlyPeriod);

        }
    }

    
}
