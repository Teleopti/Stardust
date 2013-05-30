﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions;
using Teleopti.Interfaces.Domain;
using Rhino.Mocks;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.AgentRestrictions
{
	[TestFixture]
	public class AgentRestrictionsDisplayRowCreatorTest
	{
		private AgentRestrictionsDisplayRowCreator _agentRestrictionsDisplayRowCreator;
		private ISchedulerStateHolder _stateHolder;
		private IList<IPerson> _persons;
		private IScheduleMatrixListCreator _scheduleMatrixListCreator;
		private MockRepository _mocks;
		private IPerson _person;
		private IDateOnlyPeriodAsDateTimePeriod _dateOnlyPeriodAsDateTimePeriod;
		private IVirtualSchedulePeriod _virtualSchedulePeriod;
		private IScheduleDictionary _scheduleDictionary;
		private IScheduleRange _scheduleRange;
		private IScheduleDay _scheduleDay;
		private IList<IScheduleDay> _scheduleDays;
		private IScheduleMatrixPro _scheduleMatrixPro;
		private List<IScheduleMatrixPro> _scheduleMatrixPros;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_stateHolder = _mocks.StrictMock<ISchedulerStateHolder>();
			_person = _mocks.StrictMock<IPerson>();
			_persons = new List<IPerson> { _person };
			_scheduleMatrixListCreator = _mocks.StrictMock<IScheduleMatrixListCreator>();
			_agentRestrictionsDisplayRowCreator = new AgentRestrictionsDisplayRowCreator(_stateHolder, _scheduleMatrixListCreator);
			_dateOnlyPeriodAsDateTimePeriod = _mocks.StrictMock<IDateOnlyPeriodAsDateTimePeriod>();
			_virtualSchedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			_scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			_scheduleRange = _mocks.StrictMock<IScheduleRange>();
			_scheduleDay = _mocks.StrictMock<IScheduleDay>();
			_scheduleDays = new List<IScheduleDay> { _scheduleDay };
			_scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
			_scheduleMatrixPros = new List<IScheduleMatrixPro>{_scheduleMatrixPro};
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionOnNullPersons()
		{
			_agentRestrictionsDisplayRowCreator.Create(null);		
		}

		[Test]
		public void ShouldCreateDisplayRows()
		{
			var startDate = new DateOnly(2011, 1, 1);
			var endDate = new DateOnly(2011, 1, 1);
			var dateOnlyPeriod = new DateOnlyPeriod(startDate, endDate);

			using (_mocks.Record())
			{
				Expect.Call(_stateHolder.RequestedPeriod).Return(_dateOnlyPeriodAsDateTimePeriod);
				Expect.Call(_dateOnlyPeriodAsDateTimePeriod.DateOnlyPeriod).Return(dateOnlyPeriod);
				Expect.Call(_person.VirtualSchedulePeriod(startDate)).Return(_virtualSchedulePeriod);
				Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(dateOnlyPeriod).Repeat.AtLeastOnce();
				Expect.Call(_virtualSchedulePeriod.IsValid).Return(true);
				Expect.Call(_stateHolder.Schedules).Return(_scheduleDictionary);
				Expect.Call(_scheduleDictionary[_person]).Return(_scheduleRange);
				Expect.Call(_scheduleRange.ScheduledDay(startDate)).Return(_scheduleDay);
				Expect.Call(_scheduleMatrixListCreator.CreateMatrixListFromScheduleParts(_scheduleDays)).Return(_scheduleMatrixPros);
				Expect.Call(_stateHolder.CommonAgentName(_person)).Return("Name");
				Expect.Call(_scheduleMatrixPro.Person).Return(_person);
			}

			using (_mocks.Playback())
			{
				var rows = _agentRestrictionsDisplayRowCreator.Create(_persons);	
				Assert.AreEqual(1, rows.Count);
			}
		}
	}
}
