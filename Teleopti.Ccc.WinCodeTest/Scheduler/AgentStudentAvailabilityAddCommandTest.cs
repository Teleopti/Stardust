using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Teleopti.Interfaces.Domain;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class AgentStudentAvailabilityAddCommandTest
	{
		private AgentStudentAvailabilityAddCommand _target;
		private MockRepository _mock;
		private IScheduleDay _scheduleDay;
		private TimeSpan _startTime;
		private TimeSpan _endTime;
		private IStudentAvailabilityDay _studentAvailabilityDay;
		private IAgentStudentAvailabilityDayCreator _studentAvailabilityDayCreator;
	    private IScheduleDictionary _scheduleDictionary;

	    [SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_studentAvailabilityDayCreator = _mock.StrictMock<IAgentStudentAvailabilityDayCreator>();
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
            _scheduleDictionary = _mock.DynamicMock<IScheduleDictionary>();
			_startTime = TimeSpan.FromHours(8);
			_endTime = TimeSpan.FromHours(10);
			_target = new AgentStudentAvailabilityAddCommand(_scheduleDay, _startTime, _endTime, _studentAvailabilityDayCreator,_scheduleDictionary, new DoNothingScheduleDayChangeCallBack());
			_studentAvailabilityDay = _mock.StrictMock<IStudentAvailabilityDay>();
		}

		[Test]
		public void ShouldAdd()
		{
			
			using (_mock.Record())
			{
				bool startTimeError;
				bool endTimeError;
				Expect.Call(_scheduleDay.FullAccess).Return(true);
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData>()));
				Expect.Call(_studentAvailabilityDayCreator.CanCreate(_startTime, _endTime, out startTimeError, out endTimeError)).Return(true);
				Expect.Call(_studentAvailabilityDayCreator.Create(_scheduleDay, _startTime, _endTime)).Return(_studentAvailabilityDay);
				Expect.Call(() => _scheduleDay.Add(_studentAvailabilityDay));
			}

			using (_mock.Playback())
			{
				_target.Execute();
			}
		}

		[Test]
		public void ShouldNotAddWhenCannotCreateDay()
		{
			using (_mock.Record())
			{
				bool startTimeError;
				bool endTimeError;
				Expect.Call(_scheduleDay.FullAccess).Return(true);
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData>()));
				Expect.Call(_studentAvailabilityDayCreator.CanCreate(_startTime, _endTime, out startTimeError, out endTimeError)).Return(false);
			}

			using (_mock.Playback())
			{
				_target.Execute();
			}		
		}

		[Test]
		public void ShouldNotAddWhenDayExists()
		{
			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.FullAccess).Return(true);
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData>{_studentAvailabilityDay}));
			}

			using (_mock.Playback())
			{
				_target.Execute();
			}		
		}

		[Test]
		public void ShouldNotBeAbleToExecuteWhenNoFullAccess()
		{
			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.FullAccess).Return(false);
			}

			using (_mock.Playback())
			{
				Assert.IsFalse(_target.CanExecute());
			}		
		}
	}
}
