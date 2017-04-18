using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class AgentOvertimeAvailabilityEditCommandTest
	{
		private AgentOvertimeAvailabilityEditCommand _target;

		private MockRepository _mock;
		private IScheduleDay _scheduleDay;
		private TimeSpan _startTime;
		private TimeSpan _endTime;
		private IOvertimeAvailability _overtimeAvailabilityDay;
		private IOvertimeAvailabilityCreator _overtimeAvailabilityDayCreator;
	    private IScheduleDictionary _scheduleDictionary;

	    [SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_overtimeAvailabilityDayCreator = _mock.StrictMock<IOvertimeAvailabilityCreator>();
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
            _scheduleDictionary = _mock.DynamicMock<IScheduleDictionary>();
			_startTime = TimeSpan.FromHours(8);
			_endTime = TimeSpan.FromHours(10);
			_target = new AgentOvertimeAvailabilityEditCommand(_scheduleDay, _startTime, _endTime, _overtimeAvailabilityDayCreator,_scheduleDictionary, new DoNothingScheduleDayChangeCallBack());
			_overtimeAvailabilityDay = _mock.StrictMock<IOvertimeAvailability>();
		}

		[Test]
		public void ShouldEdit()
		{
			using (_mock.Record())
			{
				bool startTimeError;
				bool endTimeError;
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> { _overtimeAvailabilityDay }));
				Expect.Call(_overtimeAvailabilityDayCreator.CanCreate(_startTime, _endTime, out startTimeError, out endTimeError)).Return(true);
				Expect.Call(_overtimeAvailabilityDayCreator.Create(_scheduleDay, _startTime, _endTime)).Return(_overtimeAvailabilityDay);
				Expect.Call(() => _scheduleDay.DeleteOvertimeAvailability());
				Expect.Call(() => _scheduleDay.Add(_overtimeAvailabilityDay));
			}

			using (_mock.Playback())
			{
				_target.Execute();
			}
		}

		[Test]
		public void ShouldNotEditWhenCannotCreateDay()
		{
			using (_mock.Record())
			{
				bool startTimeError;
				bool endTimeError;
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> { _overtimeAvailabilityDay }));
				Expect.Call(_overtimeAvailabilityDayCreator.CanCreate(_startTime, _endTime, out startTimeError, out  endTimeError)).Return(false);
			}

			using (_mock.Playback())
			{
				_target.Execute();
			}			
		}

		[Test]
		public void ShouldNotEditWhenNoDay()
		{
			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData>()));
			}

			using (_mock.Playback())
			{
				_target.Execute();
			}	
		}
	}
}
