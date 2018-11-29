using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class SplitLongDayAbsencesTest
	{
		private ISplitLongDayAbsences _target;
		private MockRepository _mocks;
		private IScheduleDay _scheduleDay;
		private IPerson _person;
		private IScenario _scenario;
		private IAbsence _absence;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_target = new SplitLongDayAbsences();
			_scheduleDay = _mocks.StrictMock<IScheduleDay>();
			_person = PersonFactory.CreatePerson();
			_scenario = new Scenario("blää");
			_absence = new Absence();
		}
		
		[Test]
		public void ShouldNotAddLayerIfStarTimeIsLessThanEndTime()
		{
			DateTime absenceStart = new DateTime(2012, 1, 2, 0, 15, 0, DateTimeKind.Utc);
			DateTime absenceEnd = new DateTime(2012, 1, 2, 1, 15, 0, DateTimeKind.Utc);
			DateTimePeriod absencePeriod = new DateTimePeriod(absenceStart, absenceEnd);
			DateTime shiftStart = new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			DateTime shiftEnd = new DateTime(2012, 1, 2, 0, 0, 0, DateTimeKind.Utc);
			DateTimePeriod shiftPeriod = new DateTimePeriod(shiftStart, shiftEnd);
			IAbsenceLayer absenceLayer = new AbsenceLayer(_absence, absencePeriod);
			IPersonAbsence absenceCrossMidNight = new PersonAbsence(_person, _scenario, absenceLayer);
			using (_mocks.Record())
			{
				Expect.Call(_scheduleDay.PersonAbsenceCollection()).Return(new [] {absenceCrossMidNight});
				Expect.Call(_scheduleDay.Period).Return(shiftPeriod);
			}

			IList<IPersonAbsence> result;

			using (_mocks.Playback())
			{
				result = _target.SplitAbsences(_scheduleDay);
			}

			Assert.AreEqual(0, result.Count);
		}
	}
}