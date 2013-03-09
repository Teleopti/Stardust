using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
    [TestFixture]
    public class TeamScheduleSameStartTimeTest
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
            _target = new TeamSchedulingSameStartTime(_resourceCalculateDelayer,_schedulePartModifyAndRollbackService);
            _scheduleDayPro = _mock.StrictMock<IScheduleDayPro>();
            _scheduleDay = _mock.StrictMock<IScheduleDay>();
            _mainShift = _mock.StrictMock<IMainShift>();
	        _groupMatrixList = new List<IList<IScheduleMatrixPro>>();
            _matrixList = new List<IScheduleMatrixPro> {_scheduleMatrixPro };
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

                _target.Execute(_teamBlockInfo,_shiftProjectionCache );
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
                _target.Execute(_teamBlockInfo, _shiftProjectionCache);
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
                _target.Execute(_teamBlockInfo, _shiftProjectionCache);
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
                _target.Execute(_teamBlockInfo,_shiftProjectionCache );
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
                _target.Execute(_teamBlockInfo, _shiftProjectionCache);
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
                _target.Execute(_teamBlockInfo,_shiftProjectionCache );
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
                _target.Execute(_teamBlockInfo,_shiftProjectionCache );
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

    public class TeamSchedulingSameStartTime : ITeamScheduling
    {
        private readonly IResourceCalculateDelayer _resourceCalculateDelayer;
        private readonly ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
        private bool _cancelMe;
        public TeamSchedulingSameStartTime(IResourceCalculateDelayer resourceCalculateDelayer, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
        {
            _resourceCalculateDelayer = resourceCalculateDelayer;
            _schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
        }

        public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;
        public void Execute(IList<DateOnly> daysInBlock, IList<IScheduleMatrixPro> matrixList, IGroupPerson groupPerson, IShiftProjectionCache shiftProjectionCache, IList<DateOnly> unlockedDays, IList<IPerson> selectedPersons)
        {
            throw new NotImplementedException();
        }

        public void Execute(ITeamBlockInfo teamBlockInfo, IShiftProjectionCache shiftProjectionCache)
        {
            //getting the start time of the block
            var startTime = shiftProjectionCache.WorkShiftStartTime;
            var startDateOfBlock = teamBlockInfo.BlockInfo.BlockPeriod.StartDate;
            foreach (var day in teamBlockInfo.BlockInfo.BlockPeriod.DayCollection())
            {
                if (teamBlockInfo.TeamInfo.MatrixesForGroup().Any(singleMatrix => singleMatrix.UnlockedDays.Any(schedulePro => schedulePro.Day == day)))
                {
                    IScheduleDay destinationScheduleDay = null;
                    var listOfDestinationScheduleDays = new List<IScheduleDay>();
                    foreach (var person in teamBlockInfo.TeamInfo.GroupPerson.GroupMembers)
                    {
                        if (_cancelMe)
                            continue;

                        //if (!selectedPersons.Contains(person)) continue;
                        IPerson tmpPerson = person;
                        var tempMatrixList = teamBlockInfo.TeamInfo.MatrixesForGroup().Where(scheduleMatrixPro => scheduleMatrixPro.Person == tmpPerson).ToList();
                        if (tempMatrixList.Any())
                        {
                            IScheduleMatrixPro matrix = null;
                            foreach (var scheduleMatrixPro in tempMatrixList)
                            {
                                if (scheduleMatrixPro.SchedulePeriod.DateOnlyPeriod.Contains(startDateOfBlock))
                                    matrix = scheduleMatrixPro;
                            }
                            if (matrix == null) continue;
                            if (matrix.GetScheduleDayByKey(day).DaySchedulePart().IsScheduled()) continue;
                            destinationScheduleDay = assignShiftProjection(startDateOfBlock, shiftProjectionCache, listOfDestinationScheduleDays, matrix, day);
                            OnDayScheduled(new SchedulingServiceBaseEventArgs(destinationScheduleDay));
                        }

                    }

                    if (_cancelMe)
                        return;
                    if (destinationScheduleDay != null)
                        _resourceCalculateDelayer.CalculateIfNeeded(destinationScheduleDay.DateOnlyAsPeriod.DateOnly,
                                                                    shiftProjectionCache.WorkShiftProjectionPeriod, listOfDestinationScheduleDays);
                }

            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected virtual void OnDayScheduled(SchedulingServiceBaseEventArgs scheduleServiceBaseEventArgs)
        {
            EventHandler<SchedulingServiceBaseEventArgs> temp = DayScheduled;
            if (temp != null)
            {
                temp(this, scheduleServiceBaseEventArgs);
                _cancelMe = scheduleServiceBaseEventArgs.Cancel;
            }
        }

        private IScheduleDay assignShiftProjection(DateOnly startDateOfBlock, IShiftProjectionCache shiftProjectionCache,
                                                    List<IScheduleDay> listOfDestinationScheduleDays, IScheduleMatrixPro matrix, DateOnly day)
        {
            var scheduleDayPro = matrix.GetScheduleDayByKey(day);
            if (!matrix.UnlockedDays.Contains(scheduleDayPro)) return null;
            //does that day count as is_scheduled??
            IScheduleDay destinationScheduleDay;
            destinationScheduleDay = matrix.GetScheduleDayByKey(day).DaySchedulePart();
            var destinationSignificanceType = destinationScheduleDay.SignificantPart();
            if (destinationSignificanceType == SchedulePartView.DayOff ||
                destinationSignificanceType == SchedulePartView.ContractDayOff ||
                destinationSignificanceType == SchedulePartView.FullDayAbsence)
                return destinationScheduleDay;
            listOfDestinationScheduleDays.Add(destinationScheduleDay);
            var sourceScheduleDay = matrix.GetScheduleDayByKey(startDateOfBlock).DaySchedulePart();
            sourceScheduleDay.AddMainShift((IMainShift)shiftProjectionCache.TheMainShift.EntityClone());
            destinationScheduleDay.Merge(sourceScheduleDay, false);

            _schedulePartModifyAndRollbackService.Modify(destinationScheduleDay);
            return destinationScheduleDay;
        }
    }
}

