using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Restriction;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Scheduler.Restriction
{
    [TestFixture]
    public class OvertimeAvailabilityPersonFilterTestOld
    {
        private MockRepository _mock;
        private IList<IScheduleDay > _scheduleDaysList;
        private IScheduleDay _scheduleDay1;
        private IScheduleDay _scheduleDay2;
        private IScheduleDay _scheduleDay3;
		private IEnumerable<IPersistableScheduleData> _persistableScheduleData1;
		private IEnumerable<IPersistableScheduleData> _persistableScheduleData2;
		private IEnumerable<IPersistableScheduleData> _persistableScheduleData3;
        private IOvertimeAvailability _overtimeAvailability1;
        private IOvertimeAvailability _overtimeAvailability2;
        private IOvertimeAvailability _overtimeAvailability3;
        private TimeSpan _filterStartTime;
        private TimeSpan _filterEndTime;
        private IPerson _person1;
        private IPerson _person2;
        private IPerson _person3;
	    private DateOnly _dateOnly;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
	        _dateOnly = new DateOnly(2016, 7, 22);
            _scheduleDay1 = _mock.Stub<IScheduleDay>();
            _scheduleDay2 = _mock.Stub<IScheduleDay>();
            _scheduleDay3 = _mock.Stub<IScheduleDay>();
            _scheduleDaysList = new List<IScheduleDay>{_scheduleDay1,_scheduleDay2,_scheduleDay3 };
            _filterStartTime = TimeSpan.FromHours(8);
            _filterEndTime = TimeSpan.FromHours(16);
            _person1 = PersonFactory.CreatePerson("p1");
			_person1.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.StockholmTimeZoneInfo());
            _person2 = PersonFactory.CreatePerson("p2");
			_person2.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.StockholmTimeZoneInfo());
			_person3 = PersonFactory.CreatePerson("p3");
			_person3.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.StockholmTimeZoneInfo());

			_overtimeAvailability1 = new OvertimeAvailability(_person1, _dateOnly, TimeSpan.FromHours(5),
                                                              TimeSpan.FromHours(7));
            _overtimeAvailability2 = new OvertimeAvailability(_person2, _dateOnly, TimeSpan.FromHours(4),
                                                              TimeSpan.FromHours(7));
            _overtimeAvailability3 = new OvertimeAvailability(_person3, _dateOnly, TimeSpan.FromHours(4),
                                                              TimeSpan.FromHours(6));

			_persistableScheduleData1 = new List<IPersistableScheduleData> { _overtimeAvailability1 };
			_persistableScheduleData2 = new List<IPersistableScheduleData> { _overtimeAvailability2 };
			_persistableScheduleData3 = new List<IPersistableScheduleData> { _overtimeAvailability3 };
        }

        [Test]
        public void NoPersonFoundInFilterWithoutOvertimeAvailability()
        {
            
            using (_mock.Record())
            {
                commonPersistableScheduleDataCollectionMocks();
            }
            var personList = new OvertimeAvailabilityPersonFilter(new FakeTimeZoneGuard(TimeZoneInfoFactory.StockholmTimeZoneInfo()))
				.GetFilteredPerson(_scheduleDaysList, new TimePeriod(_filterStartTime, _filterEndTime), false);
            Assert.AreEqual(personList.Count(),0);

        }
        
        [Test]
        public void NoPersonFoundInFilterWithOvertimeAvailability()
        {
            

            using (_mock.Record())
            {
                commonPersistableScheduleDataCollectionMocks();
            }
            var personList = new OvertimeAvailabilityPersonFilter(new FakeTimeZoneGuard(TimeZoneInfoFactory.StockholmTimeZoneInfo()))
				.GetFilteredPerson(_scheduleDaysList, new TimePeriod(_filterStartTime, _filterEndTime), false);
            Assert.AreEqual(personList.Count(),0);

        }
        
        [Test]
        public void OnePersonFoundInFilterWithOvertimeAvailability()
        {
            _overtimeAvailability3 = new OvertimeAvailability(_person3, _dateOnly, TimeSpan.FromHours(8),
                                                              TimeSpan.FromHours(17));
			_persistableScheduleData3 = new List<IPersistableScheduleData> { _overtimeAvailability3 };

            using (_mock.Record())
            {
                commonPersistableScheduleDataCollectionMocks();
            }
			var personList = new OvertimeAvailabilityPersonFilter(new FakeTimeZoneGuard(TimeZoneInfoFactory.StockholmTimeZoneInfo()))
							.GetFilteredPerson(_scheduleDaysList, new TimePeriod(_filterStartTime, _filterEndTime), false);
            Assert.AreEqual(personList.Count(),1);
            Assert.AreEqual(personList.First(),_person3);

        }
        
       [Test]
        public void AllPersonFoundInFilterWithOvertimeAvailability()
        {
            _overtimeAvailability1 = new OvertimeAvailability(_person1, _dateOnly, TimeSpan.FromHours(7),
                                                              TimeSpan.FromHours(19));
            _overtimeAvailability2 = new OvertimeAvailability(_person2, _dateOnly, TimeSpan.FromHours(7),
                                                              TimeSpan.FromHours(17));
            _overtimeAvailability3 = new OvertimeAvailability(_person3, _dateOnly, TimeSpan.FromHours(8),
                                                              TimeSpan.FromHours(16));
			_persistableScheduleData1 = new List<IPersistableScheduleData> { _overtimeAvailability1 };
			_persistableScheduleData2 = new List<IPersistableScheduleData> { _overtimeAvailability2 };
			_persistableScheduleData3 = new List<IPersistableScheduleData> { _overtimeAvailability3 };

            using (_mock.Record())
            {
                commonPersistableScheduleDataCollectionMocks();
            }
			var personList = new OvertimeAvailabilityPersonFilter(new FakeTimeZoneGuard(TimeZoneInfoFactory.StockholmTimeZoneInfo()))
				.GetFilteredPerson(_scheduleDaysList, new TimePeriod(_filterStartTime, _filterEndTime), false);
            Assert.AreEqual(3, personList.Count());
            
        }

       [Test]
       public void ShouldReturnEmptyListWhenShiftsAreOnNextDay()
       {
           var time1 = TimeSpan.FromHours(8).Add(TimeSpan.FromDays(1));
           var time2 = TimeSpan.FromHours(5).Add(TimeSpan.FromDays(1));
           var time3 = TimeSpan.FromHours(15).Add(TimeSpan.FromDays(1));
           _overtimeAvailability1 = new OvertimeAvailability(_person1, _dateOnly, TimeSpan.FromHours(22),
                                                             time1);
           _overtimeAvailability2 = new OvertimeAvailability(_person2, _dateOnly, TimeSpan.FromHours(18),
                                                             time2);
           _overtimeAvailability3 = new OvertimeAvailability(_person3, _dateOnly, TimeSpan.FromHours(18),
                                                             time3);
		   _persistableScheduleData1 = new List<IPersistableScheduleData> { _overtimeAvailability1 };
		   _persistableScheduleData2 = new List<IPersistableScheduleData> { _overtimeAvailability2 };
		   _persistableScheduleData3 = new List<IPersistableScheduleData> { _overtimeAvailability3 };

           using (_mock.Record())
           {
               commonPersistableScheduleDataCollectionMocks();
           }
			var personList = new OvertimeAvailabilityPersonFilter(new FakeTimeZoneGuard(TimeZoneInfoFactory.StockholmTimeZoneInfo()))
				.GetFilteredPerson(_scheduleDaysList, new TimePeriod(TimeSpan.FromHours(14), TimeSpan.FromHours(16)), false);
           Assert.AreEqual(0, personList.Count());

       }

       [Test]
       public void ShouldReturnOnePersonListWhenTwoOfTheThreeShiftsAreOnNextDay()
       {
           var time1 = TimeSpan.FromHours(8).Add(TimeSpan.FromDays(1));
           _overtimeAvailability1 = new OvertimeAvailability(_person1, _dateOnly, TimeSpan.FromHours(22),
                                                             time1);
           _overtimeAvailability2 = new OvertimeAvailability(_person2, _dateOnly, TimeSpan.FromHours(18),
                                                             TimeSpan.FromHours(5));
           _overtimeAvailability3 = new OvertimeAvailability(_person3, _dateOnly, TimeSpan.FromHours(8),
                                                             TimeSpan.FromHours(16));
		   _persistableScheduleData1 = new List<IPersistableScheduleData> { _overtimeAvailability1 };
		   _persistableScheduleData2 = new List<IPersistableScheduleData> { _overtimeAvailability2 };
		   _persistableScheduleData3 = new List<IPersistableScheduleData> { _overtimeAvailability3 };

           using (_mock.Record())
           {
               commonPersistableScheduleDataCollectionMocks();
           }
			var personList = new OvertimeAvailabilityPersonFilter(new FakeTimeZoneGuard(TimeZoneInfoFactory.StockholmTimeZoneInfo()))
				.GetFilteredPerson(_scheduleDaysList, new TimePeriod(TimeSpan.FromHours(14), TimeSpan.FromHours(16)), false);
           Assert.AreEqual(personList.Count(), 1);

       }

       [Test]
       public void ShouldReturnTwoPersonListWhenTwoOfTheThreeShiftsAreOnNextDay()
       {
           var time1 = TimeSpan.FromHours(8).Add(TimeSpan.FromDays(1));
           var time2 = TimeSpan.FromHours(5).Add(TimeSpan.FromDays(1));
           var filterEndDate = TimeSpan.FromHours(1).Add(TimeSpan.FromDays(1));
           _overtimeAvailability1 = new OvertimeAvailability(_person1, _dateOnly, TimeSpan.FromHours(22),
                                                             time1);
           _overtimeAvailability2 = new OvertimeAvailability(_person2, _dateOnly, TimeSpan.FromHours(18),
                                                             time2);
           _overtimeAvailability3 = new OvertimeAvailability(_person3, _dateOnly, TimeSpan.FromHours(8),
                                                             TimeSpan.FromHours(16));
		   _persistableScheduleData1 = new List<IPersistableScheduleData> { _overtimeAvailability1 };
		   _persistableScheduleData2 = new List<IPersistableScheduleData> { _overtimeAvailability2 };
		   _persistableScheduleData3 = new List<IPersistableScheduleData> { _overtimeAvailability3 };

           using (_mock.Record())
           {
               commonPersistableScheduleDataCollectionMocks();
           }
			var personList = new OvertimeAvailabilityPersonFilter(new FakeTimeZoneGuard(TimeZoneInfoFactory.StockholmTimeZoneInfo()))
				.GetFilteredPerson(_scheduleDaysList, new TimePeriod(TimeSpan.FromHours(23), filterEndDate), false);
           Assert.AreEqual(personList.Count(), 2);

       }

		[Test]
		public void ShouldHandleAgentsInOtherTimeZonThanMyViewPoint()
		{		
			_person1.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.GmtTimeZoneInfo());
			_overtimeAvailability1 = new OvertimeAvailability(_person1, _dateOnly, TimeSpan.FromHours(12),
															  TimeSpan.FromHours(13));
			
			_persistableScheduleData1 = new List<IPersistableScheduleData> { _overtimeAvailability1 };
			_persistableScheduleData2 = new List<IPersistableScheduleData>();
			_persistableScheduleData3 = new List<IPersistableScheduleData>();

			using (_mock.Record())
			{
				commonPersistableScheduleDataCollectionMocks();
			}
			var personList = new OvertimeAvailabilityPersonFilter(new FakeTimeZoneGuard(TimeZoneInfoFactory.StockholmTimeZoneInfo()))
				.GetFilteredPerson(_scheduleDaysList, new TimePeriod(TimeSpan.FromHours(12), TimeSpan.FromHours(13)), false);
			personList.Count().Should().Be.EqualTo(0); //the agent is in another time zone and I only want OT on 12-13 my time zone

		}


		private void commonPersistableScheduleDataCollectionMocks()
        {
            Expect.Call(_scheduleDay1.PersistableScheduleDataCollection()).Return(_persistableScheduleData1);
            Expect.Call(_scheduleDay2.PersistableScheduleDataCollection()).Return(_persistableScheduleData2);
            Expect.Call(_scheduleDay3.PersistableScheduleDataCollection()).Return(_persistableScheduleData3);
	        Expect.Call(_scheduleDay1.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(_dateOnly, TimeZoneInfoFactory.StockholmTimeZoneInfo()));
	        Expect.Call(_scheduleDay2.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(_dateOnly, TimeZoneInfoFactory.StockholmTimeZoneInfo()));
	        Expect.Call(_scheduleDay3.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(_dateOnly, TimeZoneInfoFactory.StockholmTimeZoneInfo()));
			Expect.Call(_scheduleDay1.Person).Return(_person1);
			Expect.Call(_scheduleDay2.Person).Return(_person2);
			Expect.Call(_scheduleDay3.Person).Return(_person3);
		}

    }

   
}
