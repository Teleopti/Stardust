﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;
using Rhino.Mocks;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class AgentPreferenceAddCommandTest
	{
		private MockRepository _mock;
		private AgentPreferenceAddCommand _target;
		private IScheduleDay _scheduleDay;
		private IPreferenceDay _preferenceDay;
		private IAgentPreferenceDayCreator _preferenceDayCreator;
		private IAgentPreferenceData _data;
		
		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			
			_data = new AgentPreferenceData
				{
					MinStart = TimeSpan.FromHours(1),
					MaxStart = TimeSpan.FromHours(2),
					MinEnd = TimeSpan.FromHours(3),
					MaxEnd = TimeSpan.FromHours(4),
					MinLength = TimeSpan.FromHours(2),
					MaxLength = TimeSpan.FromHours(3)
				};

			_preferenceDayCreator = _mock.StrictMock<IAgentPreferenceDayCreator>();
			_target = new AgentPreferenceAddCommand(_scheduleDay,_data, _preferenceDayCreator);
			_preferenceDay = _mock.StrictMock<IPreferenceDay>();	
		}

		[Test]
		public void ShouldAdd()
		{
			using (_mock.Record())
			{
				var result = new AgentPreferenceCanCreateResult {Result = true};
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData>()));
				Expect.Call(_preferenceDayCreator.CanCreate(_data)).Return(result);
				Expect.Call(_preferenceDayCreator.Create(_scheduleDay, _data)).Return(_preferenceDay);
				Expect.Call(() => _scheduleDay.Add(_preferenceDay));
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
				var result = new AgentPreferenceCanCreateResult {Result = false};
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData>()));
				Expect.Call(_preferenceDayCreator.CanCreate(_data)).Return(result);
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
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> { _preferenceDay }));
			}

			using (_mock.Playback())
			{
				_target.Execute();
			}
		}
	}
}
