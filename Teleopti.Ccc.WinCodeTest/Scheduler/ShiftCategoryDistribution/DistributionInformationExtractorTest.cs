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
        private DateTimePeriod _todayDateTimePeriod;
        private ShiftCategory _late;
        private ShiftCategory _night;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _morning = new ShiftCategory("Morning");
            _day = new ShiftCategory("Day");
            _late = new ShiftCategory("Late");
            _night = new ShiftCategory("night");
            _scheduleDay1 = _mock.StrictMock<IScheduleDay>();
            _scheduleDay2 = _mock.StrictMock<IScheduleDay>();
            _scheduleDay3 = _mock.StrictMock<IScheduleDay>();
            _scheduleDays = new List<IScheduleDay>{_scheduleDay1,_scheduleDay2,_scheduleDay3};
            _target = new DistributionInformationExtractor();
            _todayDateTimePeriod = new DateTimePeriod(DateTime.UtcNow ,DateTime.UtcNow );
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
                _target.ExtractDistributionInfo(_scheduleDays,null,TimeZoneInfo.Utc );
            }
            var result = _target.ShiftFairness;
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
                _target.ExtractDistributionInfo(_scheduleDays, null, TimeZoneInfo.Utc);
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
                _target.ExtractDistributionInfo(_scheduleDays, null, TimeZoneInfo.Utc);
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
                _target.ExtractDistributionInfo(_scheduleDays, null, TimeZoneInfo.Utc);
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
                _target.ExtractDistributionInfo(_scheduleDays, null, TimeZoneInfo.Utc);
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
                _target.ExtractDistributionInfo(_scheduleDays, null, TimeZoneInfo.Utc);
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
                _target.ExtractDistributionInfo(_scheduleDays, null, TimeZoneInfo.Utc);
            }
            var result = _target.GetShiftCategoryFrequency(_morning );
            Assert.AreEqual(result.Count(), 1);
            Assert.AreEqual(result.Keys.First( ), 2);
            Assert.AreEqual(result.Values.First( ), 1);
        }

        [Test]
        public void TestPeopleCacheBeingPopulatedWithDefaultValues()
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
                _target.ExtractDistributionInfo(_scheduleDays, null, TimeZoneInfo.Utc);
            }
            var result = _target.PersonCache ;
            Assert.AreEqual(result.Count, 1);
            Assert.AreEqual(result.Keys.First(), person1 );
        }

        [Test]
        public void TestPeopleCacheBeingUpdated()
        {
            var person1 = PersonFactory.CreatePerson("person1");
            var person2 = PersonFactory.CreatePerson("person2");
            using (_mock.Record())
            {
                scheduleDayExpect(_scheduleDay1, _morning, person1);
                scheduleDayExpect(_scheduleDay2, _day, person1);
                scheduleDayExpect(_scheduleDay3, _morning, person2);

                Expect.Call(_scheduleDay1.Person).Return(person1);
                Expect.Call(_scheduleDay2.Person).Return(person2);
                Expect.Call(_scheduleDay3.Person).Return(person2);

                scheduleDayExpect(_scheduleDay1, _late, person1);

                Expect.Call(_scheduleDay1.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(DateOnly.Today,TimeZoneInfo.Utc ));
                Expect.Call(_scheduleDay2.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(DateOnly.Today,TimeZoneInfo.Utc ));
                Expect.Call(_scheduleDay3.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(DateOnly.Today,TimeZoneInfo.Utc ));

                scheduleDayExpect(_scheduleDay1, _day, person1);
                scheduleDayExpect(_scheduleDay2, _day, person2);
                scheduleDayExpect(_scheduleDay3, _morning, person2);

            }
            using (_mock.Playback())
            {
                _target.ExtractDistributionInfo(_scheduleDays, null, TimeZoneInfo.Utc);
                _target.ExtractDistributionInfo(_scheduleDays, new ModifyEventArgs(new ScheduleModifier(), person1, _todayDateTimePeriod), TimeZoneInfo.Utc);
            }
            
               

            var result = _target.PersonCache ;
            Assert.AreEqual(result.Count(), 2);
            Assert.AreEqual(result[person1 ].Count ,1 );
            Assert.AreEqual(result[person1 ][0].ShiftCategory  ,_late );
        }

        [Test]
        public void TestDateCacheBeingPopulatedWithDefaultValues()
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
                _target.ExtractDistributionInfo(_scheduleDays, null, TimeZoneInfo.Utc);
            }
            var result = _target.DateCache ;
            Assert.AreEqual(result.Count, 1);
            Assert.AreEqual(result.Keys.First(), DateOnly.Today);
        }

        [Test]
        public void TestDateCacheBeingUpdated()
        {
            var person1 = PersonFactory.CreatePerson("person1");
            var person2 = PersonFactory.CreatePerson("person2");
            using (_mock.Record())
            {
                scheduleDayExpect(_scheduleDay1, _morning, person1);
                scheduleDayExpect(_scheduleDay2, _day, person1);
                scheduleDayExpect(_scheduleDay3, _morning, person2);

                Expect.Call(_scheduleDay1.Person).Return(person1);
                Expect.Call(_scheduleDay2.Person).Return(person2);
                Expect.Call(_scheduleDay3.Person).Return(person2);

                scheduleDayExpect(_scheduleDay1, _late, person1);

                Expect.Call(_scheduleDay1.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc));
                Expect.Call(_scheduleDay2.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc));
                Expect.Call(_scheduleDay3.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc));

                scheduleDayExpect(_scheduleDay1, _late , person1);
                scheduleDayExpect(_scheduleDay2, _late , person2);
                scheduleDayExpect(_scheduleDay3, _night, person2);

            }
            using (_mock.Playback())
            {
                _target.ExtractDistributionInfo(_scheduleDays, null, TimeZoneInfo.Utc);
                _target.ExtractDistributionInfo(_scheduleDays, new ModifyEventArgs(new ScheduleModifier(), person1, _todayDateTimePeriod), TimeZoneInfo.Utc);
            }



            var result = _target.DateCache ;
            Assert.AreEqual(result.Count(), 1);
            Assert.AreEqual(result[DateOnly.Today ].Count, 2);
            Assert.AreEqual(result[DateOnly.Today][0].ShiftCategory , _late);
            Assert.AreEqual(result[DateOnly.Today][1].ShiftCategory, _night);
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
