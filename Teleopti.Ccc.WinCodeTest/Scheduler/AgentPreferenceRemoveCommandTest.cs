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
	public class AgentPreferenceRemoveCommandTest
	{
		private AgentPreferenceRemoveCommand _removeCommand;
		private MockRepository _mock;
		private IScheduleDay _scheduleDay;
		private IPreferenceDay _preferenceDay;
	    private IScheduleDictionary _scheduleDictionary;

	    [SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			_scheduleDictionary = _mock.DynamicMock<IScheduleDictionary>();
			_preferenceDay = _mock.StrictMock<IPreferenceDay>();
			_removeCommand = new AgentPreferenceRemoveCommand(_scheduleDay, _scheduleDictionary, new DoNothingScheduleDayChangeCallBack());
		}

		[Test]
		public void ShouldRemove()
		{
			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData> { _preferenceDay }));
				Expect.Call(() => _scheduleDay.DeletePreferenceRestriction());
			}

			using (_mock.Playback())
			{
				_removeCommand.Execute();		
			}	
		}
	}
}
