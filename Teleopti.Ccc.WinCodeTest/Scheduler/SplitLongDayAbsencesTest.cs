﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

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
		private ICccTimeZoneInfo _originalTimeZone;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_target = new SplitLongDayAbsences();
			_scheduleDay = _mocks.StrictMock<IScheduleDay>();
			_person = PersonFactory.CreatePerson();
			_scenario = new Scenario("blää");
			_absence = new Absence();
			_originalTimeZone = StateHolderReader.Instance.StateReader.SessionScopeData.TimeZone;
		}

		[TearDown]
		public void Teardown()
		{
			StateHolderReader.Instance.StateReader.SessionScopeData.TimeZone = _originalTimeZone;
		}

		[Test]
		public void ShouldNotAddLayerIfStarTimeIsLessThanEndTime()
		{
			StateHolderReader.Instance.StateReader.SessionScopeData.TimeZone = new CccTimeZoneInfo(TimeZoneInfo.Utc);
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
				Expect.Call(_scheduleDay.PersonAbsenceCollection()).Return(
					new ReadOnlyCollection<IPersonAbsence>(new List<IPersonAbsence> {absenceCrossMidNight}));
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