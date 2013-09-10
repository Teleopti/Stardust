using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Scheduling;
using NUnit.Framework;
using Teleopti.Interfaces.Domain;
using Rhino.Mocks;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class AbsenceMergerTest
	{
		private AbsenceMerger _target;
		private IList<IScheduleDay> _days;
		private IScheduleDay _scheduleDay;
		private IScheduleDay _scheduleDayBefore;
		private MockRepository _mock;
		private IPerson _person;
		private IList<IPersistableScheduleData> _persistableScheduleData1;
		private IList<IPersistableScheduleData> _persistableScheduleData2;
		private IPersonAbsence _personAbsence1;
		private IPersonAbsence _personAbsence2;
		private IScenario _scenario;
		private DateTimePeriod _period1;
		private DateTimePeriod _period2;
		private IAbsence _absence;
			
		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			_scheduleDayBefore = _mock.StrictMock<IScheduleDay>();
			_days = new List<IScheduleDay>{_scheduleDayBefore, _scheduleDay};
			_target = new AbsenceMerger(_days);
			_person = PersonFactory.CreatePerson("person");
			_scenario = new Scenario("scenario");
			_period1 = new DateTimePeriod(new DateTime(2000,1,1,17,0,0,DateTimeKind.Utc), new DateTime(2000,1,2,2,0,0,DateTimeKind.Utc));
			_period2 = new DateTimePeriod(new DateTime(2000,1,2,2,0,0,DateTimeKind.Utc), new DateTime(2000,1,2,6,0,0,DateTimeKind.Utc));
			_absence = AbsenceFactory.CreateAbsenceWithId();
			_personAbsence1 = PersonAbsenceFactory.CreatePersonAbsence(_person, _scenario, _period1, _absence);
			_personAbsence2 = PersonAbsenceFactory.CreatePersonAbsence(_person, _scenario, _period2, _absence);
			_persistableScheduleData1 = new List<IPersistableScheduleData>{_personAbsence1};
			_persistableScheduleData2 = new List<IPersistableScheduleData> { _personAbsence2 };

		}

		[Test]
		public void ShouldMergeBetweenDays()
		{
			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.FullDayAbsence);
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_scheduleDayBefore.Person).Return(_person);
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(_persistableScheduleData2);
				Expect.Call(_scheduleDayBefore.PersistableScheduleDataCollection()).Return(_persistableScheduleData1);
				Expect.Call(() => _scheduleDayBefore.Remove(_personAbsence1));
				Expect.Call(() => _scheduleDayBefore.Add(null)).IgnoreArguments();
			}
			using (_mock.Playback())
			{
				_target.MergeWithDayBefore();
				Assert.AreEqual(1, _days.Count);
				Assert.AreEqual(_days[0], _scheduleDayBefore);
			}
		}

		[Test]
		public void ShouldMergeOnDayStart()
		{
			_days = new List<IScheduleDay>{_scheduleDay};
			_target = new AbsenceMerger(_days);
			_period1 = new DateTimePeriod(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 20, 0, 0, DateTimeKind.Utc));
			_period2 = new DateTimePeriod(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 19, 0, 0, DateTimeKind.Utc));
			_personAbsence1 = PersonAbsenceFactory.CreatePersonAbsence(_person, _scenario, _period1, _absence);
			_personAbsence2 = PersonAbsenceFactory.CreatePersonAbsence(_person, _scenario, _period2, _absence);
			_persistableScheduleData1 = new List<IPersistableScheduleData> { _personAbsence1, _personAbsence2 };

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.Period).Return(_period1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(_persistableScheduleData1);
				Expect.Call(() => _scheduleDay.Clear<IPersonAbsence>());
				Expect.Call(() => _scheduleDay.Add(null)).IgnoreArguments();
			}
			using (_mock.Playback())
			{
				_target.MergeOnDayStart();
			}
		}

		[Test]
		public void ShouldNotMergeWhenNoAbsenceContainsDayStart()
		{
			_days = new List<IScheduleDay> { _scheduleDay };
			_target = new AbsenceMerger(_days);
			var periodDay = new DateTimePeriod(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 20, 0, 0, DateTimeKind.Utc));
			_period1 = new DateTimePeriod(new DateTime(2000, 1, 1, 5, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 20, 0, 0, DateTimeKind.Utc));
			_period2 = new DateTimePeriod(new DateTime(2000, 1, 1, 5, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 19, 0, 0, DateTimeKind.Utc));
			_personAbsence1 = PersonAbsenceFactory.CreatePersonAbsence(_person, _scenario, _period1, _absence);
			_personAbsence2 = PersonAbsenceFactory.CreatePersonAbsence(_person, _scenario, _period2, _absence);
			_persistableScheduleData1 = new List<IPersistableScheduleData> { _personAbsence1, _personAbsence2 };

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(_persistableScheduleData1);
				Expect.Call(_scheduleDay.Period).Return(periodDay).Repeat.AtLeastOnce();
			}
			using (_mock.Playback())
			{
				_target.MergeOnDayStart();
			}	
		}
	}
}
