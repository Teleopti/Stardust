using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Restriction;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Scheduler.Restriction
{
	[TestFixture]
	public class HourlyAvailabilityPersonFilterTest
	{
		private MockRepository _mock;
		private HourlyAvailabilityPersonFilter _target;
		private IList<IScheduleDay> _scheduleDaysList;
		private IScheduleDay _scheduleDay1;
		private IScheduleDay _scheduleDay2;
		private IScheduleDay _scheduleDay3;
		private IEnumerable<IPersistableScheduleData> _persistableScheduleData1;
		private IEnumerable<IPersistableScheduleData> _persistableScheduleData2;
		private IEnumerable<IPersistableScheduleData> _persistableScheduleData3;
		private IStudentAvailabilityDay _studentAvailabilityDay1;
		private IStudentAvailabilityDay _studentAvailabilityDay2;
		private IStudentAvailabilityDay _studentAvailabilityDay3;
		private IStudentAvailabilityRestriction _studentAvailabilityRestriction1;
		private IStudentAvailabilityRestriction _studentAvailabilityRestriction2;
		private IStudentAvailabilityRestriction _studentAvailabilityRestriction3;
		private TimeSpan _filterStartTime;
		private TimeSpan _filterEndTime;
		private IPerson _person1;
		private IPerson _person2;
		private IPerson _person3;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_target = new HourlyAvailabilityPersonFilter();
			_scheduleDay1 = _mock.StrictMock<IScheduleDay>();
			_scheduleDay2 = _mock.StrictMock<IScheduleDay>();
			_scheduleDay3 = _mock.StrictMock<IScheduleDay>();
			_scheduleDaysList = new List<IScheduleDay> { _scheduleDay1, _scheduleDay2, _scheduleDay3 };
			_persistableScheduleData1 = new List<IPersistableScheduleData>();
			_persistableScheduleData2 = new List<IPersistableScheduleData>();
			_persistableScheduleData3 = new List<IPersistableScheduleData>();
			_filterStartTime = TimeSpan.FromHours(8);
			_filterEndTime = TimeSpan.FromHours(16);
			_person1 = PersonFactory.CreatePerson("p1");
			_person2 = PersonFactory.CreatePerson("p2");
			_person3 = PersonFactory.CreatePerson("p3");

			_studentAvailabilityRestriction1 = new StudentAvailabilityRestriction();
			_studentAvailabilityRestriction1.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(5), null);
			_studentAvailabilityRestriction1.EndTimeLimitation = new EndTimeLimitation(null, TimeSpan.FromHours(7));

			_studentAvailabilityRestriction2 = new StudentAvailabilityRestriction();
			_studentAvailabilityRestriction2.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(4), null);
			_studentAvailabilityRestriction2.EndTimeLimitation = new EndTimeLimitation(null, TimeSpan.FromHours(7));

			_studentAvailabilityRestriction3 = new StudentAvailabilityRestriction();
			_studentAvailabilityRestriction3.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(4), null);
			_studentAvailabilityRestriction3.EndTimeLimitation = new EndTimeLimitation(null, TimeSpan.FromHours(6));

			_studentAvailabilityDay1 = new StudentAvailabilityDay(_person1, DateOnly.Today, new List<IStudentAvailabilityRestriction>{_studentAvailabilityRestriction1});
			_studentAvailabilityDay2 = new StudentAvailabilityDay(_person2, DateOnly.Today, new List<IStudentAvailabilityRestriction> { _studentAvailabilityRestriction2 });
			_studentAvailabilityDay3 = new StudentAvailabilityDay(_person3, DateOnly.Today, new List<IStudentAvailabilityRestriction> { _studentAvailabilityRestriction3 });

			_persistableScheduleData1 = new List<IPersistableScheduleData> { _studentAvailabilityDay1 };
			_persistableScheduleData2 = new List<IPersistableScheduleData> { _studentAvailabilityDay2 };
			_persistableScheduleData3 = new List<IPersistableScheduleData> { _studentAvailabilityDay3 };
		}

		[Test]
		public void NoPersonFoundInFilterWithoutHourlyAvailability()
		{

			using (_mock.Record())
			{
				commonPersistableScheduleDataCollectionMocks();
			}
			var personList = _target.GetFilterdPerson(_scheduleDaysList, _filterStartTime, _filterEndTime);
			Assert.AreEqual(personList.Count(), 0);

		}

		[Test]
		public void NoPersonFoundInFilterWithHourlyAvailability()
		{


			using (_mock.Record())
			{
				commonPersistableScheduleDataCollectionMocks();
			}
			var personList = _target.GetFilterdPerson(_scheduleDaysList, _filterStartTime, _filterEndTime);
			Assert.AreEqual(personList.Count(), 0);

		}

		[Test]
		public void OnePersonFoundInFilterWithOvertimeAvailability()
		{
			_studentAvailabilityRestriction3 = new StudentAvailabilityRestriction();
			_studentAvailabilityRestriction3.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(8), null);
			_studentAvailabilityRestriction3.EndTimeLimitation = new EndTimeLimitation(null, TimeSpan.FromHours(17));
			_studentAvailabilityDay3 = new StudentAvailabilityDay(_person3, DateOnly.Today, new List<IStudentAvailabilityRestriction> { _studentAvailabilityRestriction3 });

			_persistableScheduleData3 = new List<IPersistableScheduleData> { _studentAvailabilityDay3 };

			using (_mock.Record())
			{
				commonPersistableScheduleDataCollectionMocks();
			}
			var personList = _target.GetFilterdPerson(_scheduleDaysList, _filterStartTime, _filterEndTime);
			Assert.AreEqual(personList.Count(), 1);
			Assert.AreEqual(personList[0], _person3);

		}

		[Test]
		public void AllPersonFoundInFilterWithOvertimeAvailability()
		{
			_studentAvailabilityRestriction1 = new StudentAvailabilityRestriction();
			_studentAvailabilityRestriction1.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(7), null);
			_studentAvailabilityRestriction1.EndTimeLimitation = new EndTimeLimitation(null, TimeSpan.FromHours(19));

			_studentAvailabilityRestriction2 = new StudentAvailabilityRestriction();
			_studentAvailabilityRestriction2.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(7), null);
			_studentAvailabilityRestriction2.EndTimeLimitation = new EndTimeLimitation(null, TimeSpan.FromHours(17));

			_studentAvailabilityRestriction3 = new StudentAvailabilityRestriction();
			_studentAvailabilityRestriction3.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(8), null);
			_studentAvailabilityRestriction3.EndTimeLimitation = new EndTimeLimitation(null, TimeSpan.FromHours(16));

			_studentAvailabilityDay1 = new StudentAvailabilityDay(_person1, DateOnly.Today, new List<IStudentAvailabilityRestriction> { _studentAvailabilityRestriction1 });
			_studentAvailabilityDay2 = new StudentAvailabilityDay(_person2, DateOnly.Today, new List<IStudentAvailabilityRestriction> { _studentAvailabilityRestriction2 });
			_studentAvailabilityDay3 = new StudentAvailabilityDay(_person3, DateOnly.Today, new List<IStudentAvailabilityRestriction> { _studentAvailabilityRestriction3 });

			_persistableScheduleData1 = new List<IPersistableScheduleData> { _studentAvailabilityDay1 };
			_persistableScheduleData2 = new List<IPersistableScheduleData> { _studentAvailabilityDay2 };
			_persistableScheduleData3 = new List<IPersistableScheduleData> { _studentAvailabilityDay3 };

			using (_mock.Record())
			{
				commonPersistableScheduleDataCollectionMocks();
			}
			var personList = _target.GetFilterdPerson(_scheduleDaysList, _filterStartTime, _filterEndTime);
			Assert.AreEqual(personList.Count(), 3);

		}

		[Test]
		public void ShouldReturnEmptyListWhenShiftsAreOnNextDay()
		{
			var time1 = TimeSpan.FromHours(8).Add(TimeSpan.FromDays(1));
			var time2 = TimeSpan.FromHours(5).Add(TimeSpan.FromDays(1));
			var time3 = TimeSpan.FromHours(15).Add(TimeSpan.FromDays(1));

			_studentAvailabilityRestriction1 = new StudentAvailabilityRestriction();
			_studentAvailabilityRestriction1.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(22), null);
			_studentAvailabilityRestriction1.EndTimeLimitation = new EndTimeLimitation(null, time1);

			_studentAvailabilityRestriction2 = new StudentAvailabilityRestriction();
			_studentAvailabilityRestriction2.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(18), null);
			_studentAvailabilityRestriction2.EndTimeLimitation = new EndTimeLimitation(null, time2);

			_studentAvailabilityRestriction3 = new StudentAvailabilityRestriction();
			_studentAvailabilityRestriction3.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(8), null);
			_studentAvailabilityRestriction3.EndTimeLimitation = new EndTimeLimitation(null, time3);

			_studentAvailabilityDay1 = new StudentAvailabilityDay(_person1, DateOnly.Today, new List<IStudentAvailabilityRestriction> { _studentAvailabilityRestriction1 });
			_studentAvailabilityDay2 = new StudentAvailabilityDay(_person2, DateOnly.Today, new List<IStudentAvailabilityRestriction> { _studentAvailabilityRestriction2 });
			_studentAvailabilityDay3 = new StudentAvailabilityDay(_person3, DateOnly.Today, new List<IStudentAvailabilityRestriction> { _studentAvailabilityRestriction3 });

			_persistableScheduleData1 = new List<IPersistableScheduleData> { _studentAvailabilityDay1 };
			_persistableScheduleData2 = new List<IPersistableScheduleData> { _studentAvailabilityDay2 };
			_persistableScheduleData3 = new List<IPersistableScheduleData> { _studentAvailabilityDay3 };

			using (_mock.Record())
			{
				commonPersistableScheduleDataCollectionMocks();
			}
			var personList = _target.GetFilterdPerson(_scheduleDaysList, TimeSpan.FromHours(14), TimeSpan.FromHours(16));
			Assert.AreEqual(personList.Count(), 0);

		}

		[Test]
		public void ShouldReturnOnePersonListWhenTwoOfTheThreeShiftsAreOnNextDay()
		{
			var time1 = TimeSpan.FromHours(8).Add(TimeSpan.FromDays(1));

			_studentAvailabilityRestriction1 = new StudentAvailabilityRestriction();
			_studentAvailabilityRestriction1.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(22), null);
			_studentAvailabilityRestriction1.EndTimeLimitation = new EndTimeLimitation(null, time1);

			_studentAvailabilityRestriction2 = new StudentAvailabilityRestriction();
			_studentAvailabilityRestriction2.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(18), null);
			_studentAvailabilityRestriction2.EndTimeLimitation = new EndTimeLimitation(null, TimeSpan.FromHours(5));

			_studentAvailabilityRestriction3 = new StudentAvailabilityRestriction();
			_studentAvailabilityRestriction3.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(8), null);
			_studentAvailabilityRestriction3.EndTimeLimitation = new EndTimeLimitation(null, TimeSpan.FromHours(16));

			_studentAvailabilityDay1 = new StudentAvailabilityDay(_person1, DateOnly.Today, new List<IStudentAvailabilityRestriction> { _studentAvailabilityRestriction1 });
			_studentAvailabilityDay2 = new StudentAvailabilityDay(_person2, DateOnly.Today, new List<IStudentAvailabilityRestriction> { _studentAvailabilityRestriction2 });
			_studentAvailabilityDay3 = new StudentAvailabilityDay(_person3, DateOnly.Today, new List<IStudentAvailabilityRestriction> { _studentAvailabilityRestriction3 });

			_persistableScheduleData1 = new List<IPersistableScheduleData> { _studentAvailabilityDay1 };
			_persistableScheduleData2 = new List<IPersistableScheduleData> { _studentAvailabilityDay2 };
			_persistableScheduleData3 = new List<IPersistableScheduleData> { _studentAvailabilityDay3 };

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


			_studentAvailabilityRestriction1 = new StudentAvailabilityRestriction();
			_studentAvailabilityRestriction1.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(22), null);
			_studentAvailabilityRestriction1.EndTimeLimitation = new EndTimeLimitation(null, time1);

			_studentAvailabilityRestriction2 = new StudentAvailabilityRestriction();
			_studentAvailabilityRestriction2.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(18), null);
			_studentAvailabilityRestriction2.EndTimeLimitation = new EndTimeLimitation(null, time2);

			_studentAvailabilityRestriction3 = new StudentAvailabilityRestriction();
			_studentAvailabilityRestriction3.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(8), null);
			_studentAvailabilityRestriction3.EndTimeLimitation = new EndTimeLimitation(null, TimeSpan.FromHours(16));

			_studentAvailabilityDay1 = new StudentAvailabilityDay(_person1, DateOnly.Today, new List<IStudentAvailabilityRestriction> { _studentAvailabilityRestriction1 });
			_studentAvailabilityDay2 = new StudentAvailabilityDay(_person2, DateOnly.Today, new List<IStudentAvailabilityRestriction> { _studentAvailabilityRestriction2 });
			_studentAvailabilityDay3 = new StudentAvailabilityDay(_person3, DateOnly.Today, new List<IStudentAvailabilityRestriction> { _studentAvailabilityRestriction3 });

			_persistableScheduleData1 = new List<IPersistableScheduleData> { _studentAvailabilityDay1 };
			_persistableScheduleData2 = new List<IPersistableScheduleData> { _studentAvailabilityDay2 };
			_persistableScheduleData3 = new List<IPersistableScheduleData> { _studentAvailabilityDay3 };

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
