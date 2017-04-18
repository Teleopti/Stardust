using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.AgentRestrictions;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.AgentRestrictions
{
	[TestFixture]
	public class AgentRestrictionsDisplayRowCreatorTest
	{
		private AgentRestrictionsDisplayRowCreator _agentRestrictionsDisplayRowCreator;
		private ISchedulerStateHolder _stateHolder;
		private IList<IPerson> _persons;
		private IMatrixListFactory _matrixListFactory;
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
		private IMatrixUserLockLocker _matrixUserLockLocker;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_stateHolder = _mocks.StrictMock<ISchedulerStateHolder>();
			_person = _mocks.StrictMock<IPerson>();
			_matrixUserLockLocker = _mocks.StrictMock<IMatrixUserLockLocker>();
			_persons = new List<IPerson> { _person };
			_matrixListFactory = _mocks.StrictMock<IMatrixListFactory>();
			_agentRestrictionsDisplayRowCreator = new AgentRestrictionsDisplayRowCreator(_stateHolder, _matrixListFactory, _matrixUserLockLocker);
			_dateOnlyPeriodAsDateTimePeriod = _mocks.StrictMock<IDateOnlyPeriodAsDateTimePeriod>();
			_virtualSchedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			_scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			_scheduleRange = _mocks.StrictMock<IScheduleRange>();
			_scheduleDay = _mocks.StrictMock<IScheduleDay>();
			_scheduleDays = new List<IScheduleDay> { _scheduleDay };
			_scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
			_scheduleMatrixPros = new List<IScheduleMatrixPro>{_scheduleMatrixPro};
		}

		[Test]
		public void ShouldThrowExceptionOnNullPersons()
		{
			Assert.Throws<ArgumentNullException>(() => _agentRestrictionsDisplayRowCreator.Create(null));		
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
				Expect.Call(_stateHolder.Schedules).Return(_scheduleDictionary).Repeat.Any();
				Expect.Call(_scheduleDictionary[_person]).Return(_scheduleRange);
				Expect.Call(_scheduleRange.ScheduledDay(startDate)).Return(_scheduleDay);
				Expect.Call(_matrixListFactory.CreateMatrixListForSelection(_scheduleDictionary, _scheduleDays)).Return(_scheduleMatrixPros);
				Expect.Call(() => _matrixUserLockLocker.Execute(null, dateOnlyPeriod)).IgnoreArguments();
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
