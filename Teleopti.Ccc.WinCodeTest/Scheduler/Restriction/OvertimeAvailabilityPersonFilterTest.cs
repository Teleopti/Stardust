using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.Restriction
{
    [TestFixture]
    public class OvertimeAvailabilityPersonFilterTest
    {
        private MockRepository _mock;
        private IOvertimeAvailabilityPersonFilter _target;
        private IList<IScheduleDay > _scheduleDaysList;
        private IScheduleDay _scheduleDay1;
        private IScheduleDay _scheduleDay2;
        private IScheduleDay _scheduleDay3;
		private IEnumerable<INonversionedPersistableScheduleData> _persistableScheduleData1;
		private IEnumerable<INonversionedPersistableScheduleData> _persistableScheduleData2;
		private IEnumerable<INonversionedPersistableScheduleData> _persistableScheduleData3;
        private IOvertimeAvailability _overtimeAvailability1;
        private IOvertimeAvailability _overtimeAvailability2;
        private IOvertimeAvailability _overtimeAvailability3;
        private TimeSpan _filterStartTime;
        private TimeSpan _filterEndTime;
        private IPerson _person1;
        private IPerson _person2;
        private IPerson _person3;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _target = new OvertimeAvailabilityPersonFilter();
            _scheduleDay1 = _mock.StrictMock<IScheduleDay>();
            _scheduleDay2 = _mock.StrictMock<IScheduleDay>();
            _scheduleDay3 = _mock.StrictMock<IScheduleDay>();
            _scheduleDaysList = new List<IScheduleDay>{_scheduleDay1,_scheduleDay2,_scheduleDay3 };
            _filterStartTime = TimeSpan.FromHours(8);
            _filterEndTime = TimeSpan.FromHours(16);
            _person1 = PersonFactory.CreatePerson("p1");
            _person2 = PersonFactory.CreatePerson("p2");
            _person3 = PersonFactory.CreatePerson("p3");

            _overtimeAvailability1 = new OvertimeAvailability(_person1, DateOnly.Today, TimeSpan.FromHours(5),
                                                              TimeSpan.FromHours(7));
            _overtimeAvailability2 = new OvertimeAvailability(_person2, DateOnly.Today, TimeSpan.FromHours(4),
                                                              TimeSpan.FromHours(7));
            _overtimeAvailability3 = new OvertimeAvailability(_person3, DateOnly.Today, TimeSpan.FromHours(4),
                                                              TimeSpan.FromHours(6));

			_persistableScheduleData1 = new List<INonversionedPersistableScheduleData> { _overtimeAvailability1 };
			_persistableScheduleData2 = new List<INonversionedPersistableScheduleData> { _overtimeAvailability2 };
			_persistableScheduleData3 = new List<INonversionedPersistableScheduleData> { _overtimeAvailability3 };
        }

        [Test]
        public void NoPersonFoundInFilterWithoutOvertimeAvailability()
        {
            
            using (_mock.Record())
            {
                commonPersistableScheduleDataCollectionMocks();
            }
            var personList = _target.GetFilterdPerson(_scheduleDaysList, _filterStartTime, _filterEndTime);
            Assert.AreEqual(personList.Count(),0);

        }
        
        [Test]
        public void NoPersonFoundInFilterWithOvertimeAvailability()
        {
            

            using (_mock.Record())
            {
                commonPersistableScheduleDataCollectionMocks();
            }
            var personList = _target.GetFilterdPerson(_scheduleDaysList, _filterStartTime , _filterEndTime );
            Assert.AreEqual(personList.Count(),0);

        }
        
        [Test]
        public void OnePersonFoundInFilterWithOvertimeAvailability()
        {
            _overtimeAvailability3 = new OvertimeAvailability(_person3, DateOnly.Today, TimeSpan.FromHours(8),
                                                              TimeSpan.FromHours(17));
			_persistableScheduleData3 = new List<INonversionedPersistableScheduleData> { _overtimeAvailability3 };

            using (_mock.Record())
            {
                commonPersistableScheduleDataCollectionMocks();
            }
            var personList = _target.GetFilterdPerson(_scheduleDaysList, _filterStartTime , _filterEndTime );
            Assert.AreEqual(personList.Count(),1);
            Assert.AreEqual(personList[0],_person3);

        }
        
       [Test]
        public void AllPersonFoundInFilterWithOvertimeAvailability()
        {
            _overtimeAvailability1 = new OvertimeAvailability(_person1, DateOnly.Today, TimeSpan.FromHours(7),
                                                              TimeSpan.FromHours(19));
            _overtimeAvailability2 = new OvertimeAvailability(_person2, DateOnly.Today, TimeSpan.FromHours(7),
                                                              TimeSpan.FromHours(17));
            _overtimeAvailability3 = new OvertimeAvailability(_person3, DateOnly.Today, TimeSpan.FromHours(8),
                                                              TimeSpan.FromHours(16));
			_persistableScheduleData1 = new List<INonversionedPersistableScheduleData> { _overtimeAvailability1 };
			_persistableScheduleData2 = new List<INonversionedPersistableScheduleData> { _overtimeAvailability2 };
			_persistableScheduleData3 = new List<INonversionedPersistableScheduleData> { _overtimeAvailability3 };

            using (_mock.Record())
            {
                commonPersistableScheduleDataCollectionMocks();
            }
            var personList = _target.GetFilterdPerson(_scheduleDaysList, _filterStartTime , _filterEndTime );
            Assert.AreEqual(personList.Count(),3);
            
        }

       [Test]
       public void ShouldReturnEmptyListWhenShiftsAreOnNextDay()
       {
           var time1 = TimeSpan.FromHours(8).Add(TimeSpan.FromDays(1));
           var time2 = TimeSpan.FromHours(5).Add(TimeSpan.FromDays(1));
           var time3 = TimeSpan.FromHours(15).Add(TimeSpan.FromDays(1));
           _overtimeAvailability1 = new OvertimeAvailability(_person1, DateOnly.Today, TimeSpan.FromHours(22),
                                                             time1);
           _overtimeAvailability2 = new OvertimeAvailability(_person2, DateOnly.Today, TimeSpan.FromHours(18),
                                                             time2);
           _overtimeAvailability3 = new OvertimeAvailability(_person3, DateOnly.Today, TimeSpan.FromHours(8),
                                                             time3);
		   _persistableScheduleData1 = new List<INonversionedPersistableScheduleData> { _overtimeAvailability1 };
		   _persistableScheduleData2 = new List<INonversionedPersistableScheduleData> { _overtimeAvailability2 };
		   _persistableScheduleData3 = new List<INonversionedPersistableScheduleData> { _overtimeAvailability3 };

           using (_mock.Record())
           {
               commonPersistableScheduleDataCollectionMocks();
           }
           var personList = _target.GetFilterdPerson(_scheduleDaysList, TimeSpan.FromHours(14),TimeSpan.FromHours(16));
           Assert.AreEqual(personList.Count(), 0);

       }

       [Test]
       public void ShouldReturnOnePersonListWhenTwoOfTheThreeShiftsAreOnNextDay()
       {
           var time1 = TimeSpan.FromHours(8).Add(TimeSpan.FromDays(1));
           _overtimeAvailability1 = new OvertimeAvailability(_person1, DateOnly.Today, TimeSpan.FromHours(22),
                                                             time1);
           _overtimeAvailability2 = new OvertimeAvailability(_person2, DateOnly.Today, TimeSpan.FromHours(18),
                                                             TimeSpan.FromHours(5));
           _overtimeAvailability3 = new OvertimeAvailability(_person3, DateOnly.Today, TimeSpan.FromHours(8),
                                                             TimeSpan.FromHours(16));
		   _persistableScheduleData1 = new List<INonversionedPersistableScheduleData> { _overtimeAvailability1 };
		   _persistableScheduleData2 = new List<INonversionedPersistableScheduleData> { _overtimeAvailability2 };
		   _persistableScheduleData3 = new List<INonversionedPersistableScheduleData> { _overtimeAvailability3 };

           using (_mock.Record())
           {
               commonPersistableScheduleDataCollectionMocks();
           }
           var personList = _target.GetFilterdPerson(_scheduleDaysList, TimeSpan.FromHours(14), TimeSpan.FromHours(16));
           Assert.AreEqual(personList.Count(), 1);

       }

       [Test]
       public void ShouldReturnTwoPersonListWhenTwoOfTheThreeShiftsAreOnNextDay()
       {
           var time1 = TimeSpan.FromHours(8).Add(TimeSpan.FromDays(1));
           var time2 = TimeSpan.FromHours(5).Add(TimeSpan.FromDays(1));
           var filterEndDate = TimeSpan.FromHours(1).Add(TimeSpan.FromDays(1));
           _overtimeAvailability1 = new OvertimeAvailability(_person1, DateOnly.Today, TimeSpan.FromHours(22),
                                                             time1);
           _overtimeAvailability2 = new OvertimeAvailability(_person2, DateOnly.Today, TimeSpan.FromHours(18),
                                                             time2);
           _overtimeAvailability3 = new OvertimeAvailability(_person3, DateOnly.Today, TimeSpan.FromHours(8),
                                                             TimeSpan.FromHours(16));
		   _persistableScheduleData1 = new List<INonversionedPersistableScheduleData> { _overtimeAvailability1 };
		   _persistableScheduleData2 = new List<INonversionedPersistableScheduleData> { _overtimeAvailability2 };
		   _persistableScheduleData3 = new List<INonversionedPersistableScheduleData> { _overtimeAvailability3 };

           using (_mock.Record())
           {
               commonPersistableScheduleDataCollectionMocks();
           }
           var personList = _target.GetFilterdPerson(_scheduleDaysList, TimeSpan.FromHours(23), filterEndDate);
           Assert.AreEqual(personList.Count(), 2);

       }

       
        private void commonPersistableScheduleDataCollectionMocks()
        {
            Expect.Call(_scheduleDay1.PersistableScheduleDataCollection()).Return(_persistableScheduleData1);
            Expect.Call(_scheduleDay2.PersistableScheduleDataCollection()).Return(_persistableScheduleData2);
            Expect.Call(_scheduleDay3.PersistableScheduleDataCollection()).Return(_persistableScheduleData3);
        }

    }

   
}
