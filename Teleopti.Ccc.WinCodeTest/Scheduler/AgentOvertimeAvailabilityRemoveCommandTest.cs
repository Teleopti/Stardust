using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.WinCode.Scheduling;
using NUnit.Framework;
using Teleopti.Interfaces.Domain;
using Rhino.Mocks;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class AgentOvertimeAvailabilityRemoveCommandTest
	{
		private AgentOvertimeAvailabilityRemoveCommand _target;
		private IScheduleDay _scheduleDay;
		private MockRepository _mock;
		private IOvertimeAvailability _overtimeAvailabilityDay;
	    private IScheduleDictionary _scheduleDictionary;

	    [SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			_scheduleDictionary = _mock.DynamicMock<IScheduleDictionary>();
			_target = new AgentOvertimeAvailabilityRemoveCommand(_scheduleDay,_scheduleDictionary);
			_overtimeAvailabilityDay = _mock.StrictMock<IOvertimeAvailability>();
		}

		[Test]
		public void ShouldRemove()
		{
			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<INonversionedPersistableScheduleData>(new List<INonversionedPersistableScheduleData> { _overtimeAvailabilityDay }));
				Expect.Call(() => _scheduleDay.DeleteOvertimeAvailability());
			}

			using (_mock.Playback())
			{
				_target.Execute();	
			}
		}

		[Test]
		public void ShouldNotRemoveWhenNoDay()
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
