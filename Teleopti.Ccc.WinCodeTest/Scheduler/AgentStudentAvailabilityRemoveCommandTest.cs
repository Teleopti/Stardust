﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.WinCode.Scheduling;
using NUnit.Framework;
using Teleopti.Interfaces.Domain;
using Rhino.Mocks;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class AgentStudentAvailabilityRemoveCommandTest
	{
		private AgentStudentAvailabilityRemoveCommand _target;
		private IScheduleDay _scheduleDay;
		private MockRepository _mock;
		private IStudentAvailabilityDay _studentAvailabilityDay;
	    private IScheduleDictionary _scheduleDictionary;

	    [SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			_scheduleDictionary = _mock.DynamicMock<IScheduleDictionary>();
			_target = new AgentStudentAvailabilityRemoveCommand(_scheduleDay,_scheduleDictionary);
			_studentAvailabilityDay = _mock.StrictMock<IStudentAvailabilityDay>();
		}

		[Test]
		public void ShouldRemove()
		{
			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(new ReadOnlyCollection<IPersistableScheduleData>(new List<IPersistableScheduleData>{_studentAvailabilityDay}));
				Expect.Call(() => _scheduleDay.DeleteStudentAvailabilityRestriction());
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
