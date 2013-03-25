using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
    [TestFixture]
    public class TeamSchedulingTest
    {

        private MockRepository _mock;
        private ITeamScheduling _target;
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
        private ITeamBlockInfo _teamBlockInfo;
        private ITeamInfo _teaminfo;
        private IBlockInfo _blockInfo;
        private List<IScheduleMatrixPro> _matrixList;
        private IDateOnlyAsDateTimePeriod _dateOnlyAsDateTimePeriod;
        private DateTimePeriod _dateTimePeriod;
        private DateOnlyPeriod _dateOnlyPeriod;
        private DateOnly _startDateOfBlock;
	    private List<IList<IScheduleMatrixPro>> _groupMatrixList;

	    [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _baseLineData = new BaseLineData();
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

            _matrixList = new List<IScheduleMatrixPro> {_scheduleMatrixPro };
		    _groupMatrixList = new List<IList<IScheduleMatrixPro>>();
			_groupMatrixList.Add(_matrixList);
			_teaminfo = new TeamInfo(_groupPerson, _groupMatrixList);
            _blockInfo = new BlockInfo(new DateOnlyPeriod(DateOnly.Today,DateOnly.Today.AddDays(1)));
            _teamBlockInfo = new TeamBlockInfo(_teaminfo,_blockInfo);
            _dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(DateOnly.Today , TimeZoneInfo.Local);
            _startDateOfBlock = DateOnly.Today;
            _dateOnlyPeriod = new DateOnlyPeriod(_startDateOfBlock, _startDateOfBlock.AddDays(1));
            _dateTimePeriod = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
            
        }

        [Test]
        public void ShouldExecuteWithMainShift()
        {

            using (_mock.Record())
            {
                expectCalls(SchedulePartView.MainShift, false);

                Expect.Call(_shiftProjectionCache.TheMainShift).Return(_mainShift);
                Expect.Call(() => _scheduleDay.AddMainShift(_mainShift)).IgnoreArguments();
                Expect.Call(_mainShift.EntityClone()).Return(_mainShift);
                Expect.Call(() => _schedulePartModifyAndRollbackService.Modify(_scheduleDay)).IgnoreArguments();
                Expect.Call(() => _scheduleDay.Merge(_scheduleDay, false));
                Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
                Expect.Call(_shiftProjectionCache.WorkShiftProjectionPeriod).Return(_dateTimePeriod);
                Expect.Call(_resourceCalculateDelayer.CalculateIfNeeded(_dateOnlyAsDateTimePeriod.DateOnly, _dateTimePeriod,
                                                                    new List<IScheduleDay> { _scheduleDay })).
                IgnoreArguments().Return(true);
            }

            using (_mock.Playback())
            {

                _target.Execute(_teamBlockInfo,_shiftProjectionCache, false);
            }
        }

        [Test]
        public void ShouldNotContinueWithDayOff()
        {

            using (_mock.Record())
            {
                expectCalls(SchedulePartView.DayOff, false);
                Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
                Expect.Call(_shiftProjectionCache.WorkShiftProjectionPeriod).Return(_dateTimePeriod);
                Expect.Call(_resourceCalculateDelayer.CalculateIfNeeded(_dateOnlyAsDateTimePeriod.DateOnly, _dateTimePeriod,
                                                                    new List<IScheduleDay> { _scheduleDay })).
                IgnoreArguments().Return(true);

            }
            using (_mock.Playback())
            {
				_target.Execute(_teamBlockInfo, _shiftProjectionCache, false);
            }
        }

        [Test]
        public void ShouldNotContinueWithContractDayOff()
        {
            using (_mock.Record())
            {
                expectCalls(SchedulePartView.ContractDayOff, false);
                Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
                Expect.Call(_shiftProjectionCache.WorkShiftProjectionPeriod).Return(_dateTimePeriod);
                Expect.Call(_resourceCalculateDelayer.CalculateIfNeeded(_dateOnlyAsDateTimePeriod.DateOnly, _dateTimePeriod,
                                                                    new List<IScheduleDay> { _scheduleDay })).
                IgnoreArguments().Return(true);


            }
            using (_mock.Playback())
            {
				_target.Execute(_teamBlockInfo, _shiftProjectionCache, false);
            }
        }

        [Test]
        public void ShouldNotContinueWithFullDayAbsence()
        {
            using (_mock.Record())
            {
                expectCalls(SchedulePartView.FullDayAbsence,false );

                Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
                Expect.Call(_shiftProjectionCache.WorkShiftProjectionPeriod).Return(_dateTimePeriod);
                Expect.Call(_resourceCalculateDelayer.CalculateIfNeeded(_dateOnlyAsDateTimePeriod.DateOnly, _dateTimePeriod,
                                                                    new List<IScheduleDay> { _scheduleDay })).
                IgnoreArguments().Return(true);
            }
            using (_mock.Playback())
            {
				_target.Execute(_teamBlockInfo, _shiftProjectionCache, false);
            }
        }


        [Test]
        public void ShouldNotContinueIfTheDayIsScheduled()
        {
            using (_mock.Record())
            {
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(
                     new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro })).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro.Day).Return(_startDateOfBlock).Repeat.AtLeastOnce();
                Expect.Call(_groupPerson.GroupMembers).Return(_baseLineData.ReadOnlyCollectionPersonList).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.Person).Return(_baseLineData.Person1).Repeat.AtLeastOnce();

                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod);
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(new DateOnly())).IgnoreArguments().Return(
                    _scheduleDayPro).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDay.IsScheduled()).Return(true);
            }
            using (_mock.Playback())
            {
				_target.Execute(_teamBlockInfo, _shiftProjectionCache, false);
            }
        }

		[Test]
		public void ShouldRaiseEventForEveryScheduleDayModified()
		{
			using (_mock.Record())
            {
                expectCalls(SchedulePartView.MainShift,false );

                Expect.Call(_shiftProjectionCache.TheMainShift).Return(_mainShift);
                Expect.Call(() => _schedulePartModifyAndRollbackService.Modify(_scheduleDay)).IgnoreArguments();
                Expect.Call(() => _scheduleDay.AddMainShift(_mainShift)).IgnoreArguments();
                Expect.Call(_mainShift.EntityClone()).Return(_mainShift);

                Expect.Call(() => _scheduleDay.Merge(_scheduleDay, false));
                Expect.Call(_resourceCalculateDelayer.CalculateIfNeeded(_dateOnlyAsDateTimePeriod .DateOnly,_dateTimePeriod  ,
                                                                    new List<IScheduleDay> { _scheduleDay })).
                IgnoreArguments().Return(true);
                Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod );
                Expect.Call(_shiftProjectionCache.WorkShiftProjectionPeriod).Return(_dateTimePeriod);
            }

            using (_mock.Playback())
            {
                _target.DayScheduled += targetDayScheduledNotCanceled;
				_target.Execute(_teamBlockInfo, _shiftProjectionCache, false);
                _target.DayScheduled += targetDayScheduledNotCanceled;
            }

            Assert.AreEqual(1, _numberOfEventsFired);
		}

		[Test]
		public void ShouldRespondToCancel()
		{

            using (_mock.Record())
            {
                expectCalls(SchedulePartView.MainShift,false);
                Expect.Call(_shiftProjectionCache.TheMainShift).Return(_mainShift);
                Expect.Call(() => _schedulePartModifyAndRollbackService.Modify(_scheduleDay)).IgnoreArguments();
                Expect.Call(() => _scheduleDay.AddMainShift(_mainShift)).IgnoreArguments();
                Expect.Call(_mainShift.EntityClone()).Return(_mainShift);

                Expect.Call(() => _scheduleDay.Merge(_scheduleDay, false));
            }

            using (_mock.Playback())
            {
                _target.DayScheduled += targetDayScheduledCanceled;
				_target.Execute(_teamBlockInfo, _shiftProjectionCache, false);
                _target.DayScheduled += targetDayScheduledCanceled;
            }

		}

		void targetDayScheduledCanceled(object sender, SchedulingServiceBaseEventArgs e)
		{
			e.Cancel = true;
		}

		void targetDayScheduledNotCanceled(object sender, SchedulingServiceBaseEventArgs e)
		{
			_numberOfEventsFired++;
		}

        private void expectCalls(SchedulePartView sigPart,bool isScheduled)
		{
            Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(
                    new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro })).Repeat.AtLeastOnce();
            Expect.Call(_scheduleDayPro.Day).Return(_startDateOfBlock).Repeat.AtLeastOnce();
            Expect.Call(_groupPerson.GroupMembers).Return(_baseLineData.ReadOnlyCollectionPersonList).Repeat.AtLeastOnce();
            Expect.Call(_scheduleMatrixPro.Person).Return(_baseLineData.Person1).Repeat.AtLeastOnce();

            Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod);
            Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
            Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(new DateOnly())).IgnoreArguments().Return(
                _scheduleDayPro).Repeat.AtLeastOnce();
            Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
            Expect.Call(_scheduleDay.IsScheduled()).Return(isScheduled);

            Expect.Call(_scheduleDay.SignificantPart()).Return(sigPart);
            
            //
            
            

		}


        [Test]
        public void ShouldSchedulePerDayWithOffSet()
        {
            DateOnly dateOnly = DateOnly.Today;
            using (_mock.Record())
            {
                Expect.Call(_scheduleMatrixPro.Person).Return(_baseLineData.Person1).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod);
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(new DateOnly())).IgnoreArguments().Return(
                _scheduleDayPro).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDay.IsScheduled()).Return(false);
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(
                new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);

                Expect.Call(_shiftProjectionCache.TheMainShift).Return(_mainShift);
                Expect.Call(() => _schedulePartModifyAndRollbackService.Modify(_scheduleDay)).IgnoreArguments();
                Expect.Call(() => _scheduleDay.AddMainShift(_mainShift)).IgnoreArguments();
                Expect.Call(_mainShift.EntityClone()).Return(_mainShift);
                Expect.Call(() => _scheduleDay.Merge(_scheduleDay, false));
                
                Expect.Call(_resourceCalculateDelayer.CalculateIfNeeded(_dateOnlyAsDateTimePeriod.DateOnly, _dateTimePeriod,
                                                                    new List<IScheduleDay> { _scheduleDay })).
                IgnoreArguments().Return(true);
                Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
                Expect.Call(_shiftProjectionCache.WorkShiftProjectionPeriod).Return(_dateTimePeriod);
            }

            using (_mock.Playback())
            {
                _target.DayScheduled += targetDayScheduledCanceled;
                _target.ExecutePerDayPerPerson(_baseLineData.Person1,dateOnly, _teamBlockInfo, _shiftProjectionCache, true, TODO);
                _target.DayScheduled += targetDayScheduledCanceled;
            }

        }

        [Test]
        public void ShouldSchedulePerDayWithoutOffSet()
        {
            DateOnly dateOnly = DateOnly.Today;
            using (_mock.Record())
            {
                Expect.Call(_scheduleMatrixPro.Person).Return(_baseLineData.Person1).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod);
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(new DateOnly())).IgnoreArguments().Return(
                _scheduleDayPro).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDay.IsScheduled()).Return(false);
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(
                new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);

                Expect.Call(_shiftProjectionCache.TheMainShift).Return(_mainShift);
                Expect.Call(() => _schedulePartModifyAndRollbackService.Modify(_scheduleDay)).IgnoreArguments();
                Expect.Call(() => _scheduleDay.AddMainShift(_mainShift)).IgnoreArguments();
                Expect.Call(_mainShift.EntityClone()).Return(_mainShift);
                Expect.Call(() => _scheduleDay.Merge(_scheduleDay, false));

                Expect.Call(_resourceCalculateDelayer.CalculateIfNeeded(_dateOnlyAsDateTimePeriod.DateOnly, _dateTimePeriod,
                                                                    new List<IScheduleDay> { _scheduleDay })).
                IgnoreArguments().Return(true);
                Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
                Expect.Call(_shiftProjectionCache.WorkShiftProjectionPeriod).Return(_dateTimePeriod);
            }

            using (_mock.Playback())
            {
                _target.DayScheduled += targetDayScheduledCanceled;
                _target.ExecutePerDayPerPerson(_baseLineData.Person1, dateOnly, _teamBlockInfo, _shiftProjectionCache, false, TODO);
                _target.DayScheduled += targetDayScheduledCanceled;
            }

        }

        [Test]
        public void ShouldNotSchedulePerDayIfScheduledDay()
        {
            DateOnly dateOnly = DateOnly.Today;
            using (_mock.Record())
            {
                Expect.Call(_scheduleMatrixPro.Person).Return(_baseLineData.Person1).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod);
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(new DateOnly())).IgnoreArguments().Return(
                _scheduleDayPro).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDay.IsScheduled()).Return(true);

            }

            using (_mock.Playback())
            {
                _target.DayScheduled += targetDayScheduledCanceled;
                _target.ExecutePerDayPerPerson(_baseLineData.Person1, dateOnly, _teamBlockInfo, _shiftProjectionCache, false, TODO);
                _target.DayScheduled += targetDayScheduledCanceled;
            }

        }

        [Test]
        public void ShouldNotSchedulePerDayWithFullDayAbsence()
        {
            DateOnly dateOnly = DateOnly.Today;
            using (_mock.Record())
            {
                Expect.Call(_scheduleMatrixPro.Person).Return(_baseLineData.Person1).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod);
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(new DateOnly())).IgnoreArguments().Return(
                _scheduleDayPro).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDay.IsScheduled()).Return(false);
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(
                new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.FullDayAbsence);

                Expect.Call(_resourceCalculateDelayer.CalculateIfNeeded(_dateOnlyAsDateTimePeriod.DateOnly, _dateTimePeriod,
                                                                    new List<IScheduleDay> { _scheduleDay })).
                IgnoreArguments().Return(true);
                Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
                Expect.Call(_shiftProjectionCache.WorkShiftProjectionPeriod).Return(_dateTimePeriod);
            }

            using (_mock.Playback())
            {
                _target.DayScheduled += targetDayScheduledCanceled;
                _target.ExecutePerDayPerPerson(_baseLineData.Person1, dateOnly, _teamBlockInfo, _shiftProjectionCache, false, TODO);
                _target.DayScheduled += targetDayScheduledCanceled;
            }

        }

        [Test]
        public void ShouldNotSchedulePerDayWithDayOff()
        {
            DateOnly dateOnly = DateOnly.Today;
            using (_mock.Record())
            {
                Expect.Call(_scheduleMatrixPro.Person).Return(_baseLineData.Person1).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod);
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(new DateOnly())).IgnoreArguments().Return(
                _scheduleDayPro).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDay.IsScheduled()).Return(false);
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(
                new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.DayOff );
                Expect.Call(_resourceCalculateDelayer.CalculateIfNeeded(_dateOnlyAsDateTimePeriod.DateOnly, _dateTimePeriod,
                                                                    new List<IScheduleDay> { _scheduleDay })).
                IgnoreArguments().Return(true);
                Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
                Expect.Call(_shiftProjectionCache.WorkShiftProjectionPeriod).Return(_dateTimePeriod);
            }

            using (_mock.Playback())
            {
                _target.DayScheduled += targetDayScheduledCanceled;
                _target.ExecutePerDayPerPerson(_baseLineData.Person1, dateOnly, _teamBlockInfo, _shiftProjectionCache, false, TODO);
                _target.DayScheduled += targetDayScheduledCanceled;
            }

        }

        [Test]
        public void ShouldNotSchedulePerDayWithContractDayOff()
        {
            DateOnly dateOnly = DateOnly.Today;
            using (_mock.Record())
            {
                Expect.Call(_scheduleMatrixPro.Person).Return(_baseLineData.Person1).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod);
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(new DateOnly())).IgnoreArguments().Return(
                _scheduleDayPro).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDay.IsScheduled()).Return(false);
                Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(
                new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.ContractDayOff );

                Expect.Call(_resourceCalculateDelayer.CalculateIfNeeded(_dateOnlyAsDateTimePeriod.DateOnly, _dateTimePeriod,
                                                                    new List<IScheduleDay> { _scheduleDay })).
                IgnoreArguments().Return(true);
                Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
                Expect.Call(_shiftProjectionCache.WorkShiftProjectionPeriod).Return(_dateTimePeriod);
            }

            using (_mock.Playback())
            {
                _target.DayScheduled += targetDayScheduledCanceled;
                _target.ExecutePerDayPerPerson(_baseLineData.Person1, dateOnly, _teamBlockInfo, _shiftProjectionCache, false, TODO);
                _target.DayScheduled += targetDayScheduledCanceled;
            }

        }
    }

   
}
