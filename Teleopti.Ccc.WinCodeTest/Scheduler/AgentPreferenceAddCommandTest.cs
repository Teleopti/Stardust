using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
		private TimeSpan? minStart;
		private TimeSpan? maxStart;
		private TimeSpan? minEnd;
		private TimeSpan? maxEnd;
		private TimeSpan? minLength;
		private TimeSpan? maxLength;
		private IPreferenceDay _preferenceDay;
		private IAgentPreferenceDayCreator _preferenceDayCreator;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			minStart = TimeSpan.FromHours(2);
			maxStart = TimeSpan.FromHours(1);
			minEnd = TimeSpan.FromHours(3);
			maxEnd = TimeSpan.FromHours(4);
			minLength = TimeSpan.FromHours(2);
			maxLength = TimeSpan.FromHours(3);
			_preferenceDayCreator = _mock.StrictMock<IAgentPreferenceDayCreator>();
			_target = new AgentPreferenceAddCommand(_scheduleDay, minStart, maxStart, minEnd, maxEnd, minLength, maxLength, false, _preferenceDayCreator);
			_preferenceDay = _mock.StrictMock<IPreferenceDay>();
			
		}

		[Test]
		public void ShouldAdd()
		{
			using (_mock.Record())
			{
				var result = new AgentPreferenceCanCreateResult();
				result.Result = true;
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData>()));
				Expect.Call(_preferenceDayCreator.CanCreate(minStart, maxStart, minEnd, maxEnd, minLength, maxLength, false)).Return(result);
				Expect.Call(_preferenceDayCreator.Create(_scheduleDay, minStart, maxStart, minEnd, maxEnd, minLength, maxLength, false)).Return(_preferenceDay);
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
				var result = new AgentPreferenceCanCreateResult();
				result.Result = false;
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData>()));
				Expect.Call(_preferenceDayCreator.CanCreate(minStart, maxStart, minEnd, maxEnd, minLength, maxLength, false)).Return(result);
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
