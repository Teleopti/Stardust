﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.Restriction;
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
		private IProjectionService _projectionService;
		private IVisualLayerCollection _visualLayerCollection;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_overtimeAvailabilityDayCreator = _mock.StrictMock<IOvertimeAvailabilityCreator>();
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			_startTime = TimeSpan.FromHours(8);
			_endTime = TimeSpan.FromHours(10);
			_target = new AgentOvertimeAvailabilityEditCommand(_scheduleDay, _startTime, _endTime, _overtimeAvailabilityDayCreator);
			_overtimeAvailabilityDay = _mock.StrictMock<IOvertimeAvailability>();
			_projectionService = _mock.StrictMock<IProjectionService>();
			_visualLayerCollection = _mock.StrictMock<IVisualLayerCollection>();
		}

		[Test]
		public void ShouldEdit()
		{
			using (_mock.Record())
			{
				bool startTimeError;
				bool endTimeError;
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData>{_overtimeAvailabilityDay}));
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
		public void ShouldEditForExistingShift()
		{
			using (_mock.Record())
			{
				bool startTimeError;
				bool endTimeError;
				Expect.Call(_overtimeAvailabilityDayCreator.CanCreate(_startTime, _endTime,TimeSpan.Zero, TimeSpan.Zero, out startTimeError, out endTimeError)).IgnoreArguments().Return(true);
				Expect.Call(_overtimeAvailabilityDayCreator.Create(_scheduleDay, _startTime, _endTime, TimeSpan.Zero, TimeSpan.Zero)).IgnoreArguments().Return(new List<IOvertimeAvailability>{_overtimeAvailabilityDay, _overtimeAvailabilityDay});
				Expect.Call(() => _scheduleDay.DeleteOvertimeAvailability());
				Expect.Call(() => _scheduleDay.Add(_overtimeAvailabilityDay)).Repeat.Twice();

				Expect.Call(_scheduleDay.ProjectionService()).Return(_projectionService);
				Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection);
				Expect.Call(_visualLayerCollection.Period()).Return(new DateTimePeriod());
			}

			using (_mock.Playback())
			{
				_target.Initialize();
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
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> {_overtimeAvailabilityDay }));
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
