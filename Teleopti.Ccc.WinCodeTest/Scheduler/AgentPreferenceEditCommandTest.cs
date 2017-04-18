using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
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
		private IAgentPreferenceData _data;
	    private IScheduleDictionary _scheduleDictionary;

	    [SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
            _scheduleDictionary = _mock.DynamicMock<IScheduleDictionary>();
			_preferenceDay = _mock.StrictMock<IPreferenceDay>();
			_agentPreferenceDayCreator = _mock.StrictMock<IAgentPreferenceDayCreator>();
			
			_data = new AgentPreferenceData
			{
				MinStart = TimeSpan.FromHours(1),
				MaxStart = TimeSpan.FromHours(2),
				MinEnd = TimeSpan.FromHours(3),
				MaxEnd = TimeSpan.FromHours(4),
				MinLength = TimeSpan.FromHours(2),
				MaxLength = TimeSpan.FromHours(3)
			};

			_target = new AgentPreferenceEditCommand(_scheduleDay, _data, _agentPreferenceDayCreator, _scheduleDictionary, new DoNothingScheduleDayChangeCallBack());
		}

		[Test]
		public void ShouldEdit()
		{
			using (_mock.Record())
			{
				var result = new AgentPreferenceCanCreateResult();
				result.Result = true;
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> { _preferenceDay }));
				Expect.Call(_agentPreferenceDayCreator.CanCreate(_data)).Return(result);
				Expect.Call(_agentPreferenceDayCreator.Create(_scheduleDay, _data)).Return(_preferenceDay);
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
				Expect.Call(_agentPreferenceDayCreator.CanCreate(_data)).Return(result);
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
