using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Scheduling;
using Rhino.Mocks;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class AgentPreferenceEditCommandTest
	{
		private AgentPreferenceEditCommand _target;
		private MockRepository _mock;
		private IScheduleDay _scheduleDay;
		private IPreferenceDay _preferenceDay;
		private IAgentPreferenceDayCreator _agentPreferenceDayCreator;
		private TimeSpan? minStart;
		private TimeSpan? maxStart;
		private TimeSpan? minEnd;
		private TimeSpan? maxEnd;
		private TimeSpan? minLength;
		private TimeSpan? maxLength;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			_preferenceDay = _mock.StrictMock<IPreferenceDay>();
			_agentPreferenceDayCreator = _mock.StrictMock<IAgentPreferenceDayCreator>();
			minStart = TimeSpan.FromHours(1);
			maxStart = TimeSpan.FromHours(2);
			minEnd = TimeSpan.FromHours(3);
			maxEnd = TimeSpan.FromHours(4);
			minLength = TimeSpan.FromHours(2);
			maxLength = TimeSpan.FromHours(3);
			_target = new AgentPreferenceEditCommand(_scheduleDay, minStart, maxStart, minEnd, maxEnd, minLength, maxLength, false, _agentPreferenceDayCreator);
		}

		[Test]
		public void ShouldEdit()
		{
			using (_mock.Record())
			{
				var result = new AgentPreferenceCanCreateResult();
				result.Result = true;
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> { _preferenceDay }));
				Expect.Call(_agentPreferenceDayCreator.CanCreate(minStart, maxStart, minEnd, maxEnd, minLength, maxLength, false)).Return(result);
				Expect.Call(_agentPreferenceDayCreator.Create(_scheduleDay, minStart, maxStart, minEnd, maxEnd, minLength, maxLength, false)).Return(_preferenceDay);
				Expect.Call(() => _scheduleDay.DeletePreferenceRestriction());
				Expect.Call(() => _scheduleDay.Add(_preferenceDay));
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
				var result = new AgentPreferenceCanCreateResult();
				result.Result = false;
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> { _preferenceDay }));
				Expect.Call(_agentPreferenceDayCreator.CanCreate(minStart, maxStart, minEnd, maxEnd, minLength, maxLength, false)).Return(result);
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
