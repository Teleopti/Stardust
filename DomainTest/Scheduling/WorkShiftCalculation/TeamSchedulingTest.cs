using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.WorkShiftCalculation
{
    [TestFixture]
    public class TeamSchedulingTest
    {

        private MockRepository _mock;
        private ITeamScheduling _target;
        private IEffectiveRestriction _effectiveRestriction;
        private IShiftProjectionCache _shiftProjectionCache;
        private IScheduleMatrixPro _scheduleMatrixPro;
        private IGroupPerson _groupPerson;
        private IResourceCalculateDelayer _resourceCalculateDelayer;
        private IScheduleDayPro _scheduleDayPro;
        private IScheduleDay _scheduleDay;
        private IMainShift _mainShift;
        private ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
        private IVirtualSchedulePeriod _virtualSchedulePeriod;
        private BaseLineData _baseLineData;
	    private int _numberOfEventsFired;

	    [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _baseLineData = new BaseLineData();
            _effectiveRestriction = _mock.StrictMock<IEffectiveRestriction>();
            _shiftProjectionCache = _mock.StrictMock<IShiftProjectionCache>();
            _virtualSchedulePeriod = _mock.StrictMock<IVirtualSchedulePeriod>();
            _scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();
            _groupPerson = _mock.StrictMock<IGroupPerson>();
            _resourceCalculateDelayer = _mock.StrictMock<IResourceCalculateDelayer>();
            _schedulePartModifyAndRollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
            _target = new TeamScheduling(_resourceCalculateDelayer,_schedulePartModifyAndRollbackService);
            _scheduleDayPro = _mock.StrictMock<IScheduleDayPro>();
            _scheduleDay = _mock.StrictMock<IScheduleDay>();
            _mainShift = _mock.StrictMock<IMainShift>();
        }

        [Test]
        public void ShouldExecuteWithMainShift()
        {
			//DateOnly startDateOfBlock = DateOnly.Today;
			//IList<DateOnly> selectedDays = new List<DateOnly> { startDateOfBlock };
			//IList<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> {_scheduleMatrixPro };
			//var dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(startDateOfBlock, TimeZoneInfo.Local);
			//var dateTime = new DateTimePeriod();
			//var dateOnlyPeriod = new DateOnlyPeriod(startDateOfBlock,startDateOfBlock.AddDays(2));
            
			//using (_mock.Record())
			//{
			//    expectCalls(dateOnlyAsDateTimePeriod, dateOnlyPeriod, dateTime);

			//    Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
			//    Expect.Call(_shiftProjectionCache.TheMainShift).Return(_mainShift);
			//    Expect.Call(() => _scheduleDay.AddMainShift(_mainShift)).IgnoreArguments();
			//    Expect.Call(_mainShift.EntityClone()).Return(_mainShift);
			//    Expect.Call(() => _schedulePartModifyAndRollbackService.Modify(_scheduleDay)).IgnoreArguments();
			//    Expect.Call(() => _scheduleDay.Merge(_scheduleDay, false));
			//}

			//using(_mock.Playback()   )
			//{

			//    _target.Execute(startDateOfBlock, selectedDays, matrixList, _groupPerson,
			//                    _shiftProjectionCache, new List<DateOnly> {startDateOfBlock}, _baseLineData.PersonList);
			//}
        }

        [Test]
        public void ShouldNotContinueWithDayOff()
        {
			//DateOnly startDateOfBlock = DateOnly.Today;
			//IList<DateOnly> selectedDays = new List<DateOnly> { startDateOfBlock };
			//IList<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			//var dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(startDateOfBlock, TimeZoneInfo.Local);

			//var dateTime = new DateTimePeriod();
			//var dateOnlyPeriod = new DateOnlyPeriod(startDateOfBlock, startDateOfBlock.AddDays(2));
			//using (_mock.Record())
			//{
			//    expectCalls(dateOnlyAsDateTimePeriod, dateOnlyPeriod, dateTime);
			//    Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.DayOff );
                
			//}
			//using (_mock.Playback())
			//{
			//    _target.Execute(startDateOfBlock, selectedDays, matrixList, _groupPerson,
			//                    _shiftProjectionCache, new List<DateOnly> {startDateOfBlock}, _baseLineData.PersonList);
			//}
        }

        [Test]
        public void ShouldNotContinueWithContractDayOff()
        {
			//DateOnly startDateOfBlock = DateOnly.Today;
			//IList<DateOnly> selectedDays = new List<DateOnly> { startDateOfBlock };
			//IList<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			//var dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(startDateOfBlock, TimeZoneInfo.Local);

			//var dateTime = new DateTimePeriod();
			//var dateOnlyPeriod = new DateOnlyPeriod(startDateOfBlock, startDateOfBlock.AddDays(2));
			//using (_mock.Record())
			//{
			//    expectCalls(dateOnlyAsDateTimePeriod, dateOnlyPeriod, dateTime);
			//    Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.ContractDayOff );
                

			//}
			//using (_mock.Playback())
			//{
			//    _target.Execute(startDateOfBlock, selectedDays, matrixList, _groupPerson,
			//                    _shiftProjectionCache, new List<DateOnly> {startDateOfBlock}, _baseLineData.PersonList);
			//}
        }

        [Test]
        public void ShouldNotContinueWithFullDayAbsence()
        {
			//DateOnly startDateOfBlock = DateOnly.Today;
			//IList<DateOnly> selectedDays = new List<DateOnly> { startDateOfBlock };
			//IList<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			//var dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(startDateOfBlock, TimeZoneInfo.Local);

			//var dateTime = new DateTimePeriod();
			//var dateOnlyPeriod = new DateOnlyPeriod(startDateOfBlock, startDateOfBlock.AddDays(2));
			//using (_mock.Record())
			//{
			//    expectCalls(dateOnlyAsDateTimePeriod, dateOnlyPeriod, dateTime);

			//    Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.FullDayAbsence );

			//}
			//using (_mock.Playback())
			//{
			//    _target.Execute(startDateOfBlock, selectedDays, matrixList, _groupPerson,
			//                    _shiftProjectionCache, new List<DateOnly> {startDateOfBlock}, _baseLineData.PersonList);
			//}
        }

		[Test]
		public void ShouldRaiseEventForEveryScheduleDayModified()
		{
			//DateOnly startDateOfBlock = DateOnly.Today;
			//IList<DateOnly> selectedDays = new List<DateOnly> { startDateOfBlock };
			//IList<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			//var dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(startDateOfBlock, TimeZoneInfo.Local);
			//var dateTime = new DateTimePeriod();
			//var dateOnlyPeriod = new DateOnlyPeriod(startDateOfBlock, startDateOfBlock.AddDays(2));

			//using (_mock.Record())
			//{
			//    expectCalls(dateOnlyAsDateTimePeriod, dateOnlyPeriod, dateTime);

			//    Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
			//    Expect.Call(_shiftProjectionCache.TheMainShift).Return(_mainShift);
			//    Expect.Call(() => _scheduleDay.AddMainShift(_mainShift)).IgnoreArguments();
			//    Expect.Call(_mainShift.EntityClone()).Return(_mainShift);
			//    Expect.Call(() => _schedulePartModifyAndRollbackService.Modify(_scheduleDay)).IgnoreArguments();
			//    Expect.Call(() => _scheduleDay.Merge(_scheduleDay, false));
			//}

			//using (_mock.Playback())
			//{
			//    _target.DayScheduled += targetDayScheduledNotCanceled;
			//    _target.Execute(startDateOfBlock, selectedDays, matrixList, _groupPerson,
			//                    _shiftProjectionCache, new List<DateOnly> { startDateOfBlock }, _baseLineData.PersonList);
			//    _target.DayScheduled += targetDayScheduledNotCanceled;
			//}

			//Assert.AreEqual(1, _numberOfEventsFired);
		}

		[Test]
		public void ShouldRespondToCancel()
		{
			//DateOnly startDateOfBlock = DateOnly.Today;
			//IList<DateOnly> selectedDays = new List<DateOnly> { startDateOfBlock };
			//IList<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			//var dateOnlyPeriod = new DateOnlyPeriod(startDateOfBlock, startDateOfBlock.AddDays(2));

			//using (_mock.Record())
			//{
			//    expectCallsForCanceled(dateOnlyPeriod);

			//    Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
			//    Expect.Call(_shiftProjectionCache.TheMainShift).Return(_mainShift);
			//    Expect.Call(() => _scheduleDay.AddMainShift(_mainShift)).IgnoreArguments();
			//    Expect.Call(_mainShift.EntityClone()).Return(_mainShift);
			//    Expect.Call(() => _schedulePartModifyAndRollbackService.Modify(_scheduleDay)).IgnoreArguments();
			//    Expect.Call(() => _scheduleDay.Merge(_scheduleDay, false));
			//}

			//using (_mock.Playback())
			//{
			//    _target.DayScheduled += targetDayScheduledCanceled;
			//    _target.Execute(startDateOfBlock, selectedDays, matrixList, _groupPerson,
			//                    _shiftProjectionCache, new List<DateOnly> { startDateOfBlock }, _baseLineData.PersonList);
			//    _target.DayScheduled += targetDayScheduledCanceled;
			//}

		}

		void targetDayScheduledCanceled(object sender, SchedulingServiceBaseEventArgs e)
		{
			e.Cancel = true;
		}

		void targetDayScheduledNotCanceled(object sender, SchedulingServiceBaseEventArgs e)
		{
			_numberOfEventsFired++;
		}

		private void expectCalls(DateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod, DateOnlyPeriod dateOnlyPeriod,
								 DateTimePeriod dateTime)
		{
			Expect.Call(_groupPerson.GroupMembers).Return(_baseLineData.ReadOnlyCollectionPersonList).Repeat.AtLeastOnce();
			Expect.Call(_scheduleMatrixPro.Person).Return(_baseLineData.Person1).Repeat.AtLeastOnce();
			Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(new DateOnly())).IgnoreArguments().Return(
				_scheduleDayPro).Repeat.AtLeastOnce();
			Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
			Expect.Call(_shiftProjectionCache.WorkShiftProjectionPeriod).Return(dateTime);
			Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod);
			Expect.Call(_resourceCalculateDelayer.CalculateIfNeeded(dateOnlyAsDateTimePeriod.DateOnly,
																	dateTime,
																	new List<IScheduleDay> { _scheduleDay })).
				IgnoreArguments().Return(true);
			Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod);
			Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(dateOnlyPeriod);
			Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(
				new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));

		}

		private void expectCallsForCanceled(DateOnlyPeriod dateOnlyPeriod)
		{
			Expect.Call(_groupPerson.GroupMembers).Return(_baseLineData.ReadOnlyCollectionPersonList).Repeat.AtLeastOnce();
			Expect.Call(_scheduleMatrixPro.Person).Return(_baseLineData.Person1).Repeat.AtLeastOnce();
			Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(new DateOnly())).IgnoreArguments().Return(
				_scheduleDayPro).Repeat.AtLeastOnce();
			Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();

			Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod);
			Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(dateOnlyPeriod);
			Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(
				new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));

		}
    }

   
}
