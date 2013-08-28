using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
    [TestFixture]
    public class BlockSteadyStateValidatorTest
    {
        private MockRepository _mock;
        private IBlockSteadyStateValidator _target;
        private BaseLineData _baseLineData;
        private ITeamInfo _teamInfo;
        private IBlockInfo _blockInfo;
        private IList<IScheduleMatrixPro> _matrixList;
        private IScheduleMatrixPro _scheduleMatrixPro;
        private IVirtualSchedulePeriod _virtualSchedulePeriod;
        private DateOnly _today;
        private IScheduleDayPro _todayScheduleDayPro;
        private IScheduleDayPro _tomorrowScheduleDayPro;
        private IScheduleDay _todayScheduleDay;
        private IScheduleDay _tomorrowScheduleDay;
        private DateOnly _tomorrow;
        private DateOnly _dayAfterTomorrow;
	    private IScheduleDayEquator _scheduleDayEquator;
        private IScheduleDay _dayAfterTomorrowDay;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
	        _scheduleDayEquator = _mock.StrictMock<IScheduleDayEquator>();
			_target = new BlockSteadyStateValidator(_scheduleDayEquator);
            _baseLineData = new BaseLineData();
            _matrixList = new List<IScheduleMatrixPro>();
            _scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();
            _virtualSchedulePeriod = _mock.StrictMock<IVirtualSchedulePeriod>();
            _todayScheduleDayPro = _mock.StrictMock<IScheduleDayPro>();
            _tomorrowScheduleDayPro = _mock.StrictMock<IScheduleDayPro>();
            _todayScheduleDay = _mock.StrictMock<IScheduleDay>();
            _tomorrowScheduleDay = _mock.StrictMock<IScheduleDay>();
            _dayAfterTomorrowDay = _mock.StrictMock<IScheduleDay>();
            _today = new DateOnly();
            _tomorrow = _today.AddDays(1);
            _dayAfterTomorrow = _tomorrow.AddDays(1);
        }

        [Test]
        public void ShouldReturnTrueForNonscheduledDays()
        {
            _matrixList = new List<IScheduleMatrixPro>();
            _matrixList.Add(_scheduleMatrixPro);
            _teamInfo = new TeamInfo(_baseLineData.GroupPerson, new List<IList<IScheduleMatrixPro>> { _matrixList });
            _blockInfo = new BlockInfo(new DateOnlyPeriod(_today, _today.AddDays(2)));
            var teamBlockInfo = new TeamBlockInfo(_teamInfo, _blockInfo);
            var schedulingOptions = new SchedulingOptions {UseTeamBlockSameShift = true};
	        using (_mock.Record())
            {
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod)
                      .Return(new DateOnlyPeriod(_today, _today.AddDays(2))).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_today)).Return(_todayScheduleDayPro);
                Expect.Call(_todayScheduleDayPro.DaySchedulePart()).Return(_todayScheduleDay);

                IEditableShift editableShift = _mock.StrictMock<IEditableShift>();
                Expect.Call(_todayScheduleDay.GetEditorShift()).Return(editableShift);

                //day1
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_today)).Return(_todayScheduleDayPro);
                Expect.Call(_todayScheduleDayPro.DaySchedulePart()).Return(_todayScheduleDay);
                Expect.Call(_todayScheduleDay.IsScheduled()).Return(false);

                //day2
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_tomorrow)).Return(_todayScheduleDayPro);
                Expect.Call(_todayScheduleDayPro.DaySchedulePart()).Return(_todayScheduleDay);
                Expect.Call(_todayScheduleDay.IsScheduled()).Return(false);

                //day3
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dayAfterTomorrow)).Return(_todayScheduleDayPro);
                Expect.Call(_todayScheduleDayPro.DaySchedulePart()).Return(_todayScheduleDay);
                Expect.Call(_todayScheduleDay.IsScheduled()).Return(false);

            }
            Assert.IsTrue(_target.IsBlockInSteadyState(teamBlockInfo,schedulingOptions));
        }


        [Test]
        public void ShouldReturnFalseIfAllAreNotSameStartTimeForSameStartTime()
        {
            _matrixList = new List<IScheduleMatrixPro>();
            _matrixList.Add(_scheduleMatrixPro);
            _teamInfo = new TeamInfo(_baseLineData.GroupPerson, new List<IList<IScheduleMatrixPro>> { _matrixList });
            _blockInfo = new BlockInfo(new DateOnlyPeriod(_today, _today.AddDays(2)));
            var teamBlockInfo = new TeamBlockInfo(_teamInfo, _blockInfo);
            var schedulingOptions = new SchedulingOptions();
            schedulingOptions.UseTeamBlockSameStartTime = true;
            var now = DateTime.UtcNow;
            using (_mock.Record())
            {
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod)
                      .Return(new DateOnlyPeriod(_today, _today.AddDays(2))).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_today)).Return(_todayScheduleDayPro);
                Expect.Call(_todayScheduleDayPro.DaySchedulePart()).Return(_todayScheduleDay);

                //day1
                expectCallForDayForSameStartTime(_todayScheduleDay, _today, now, true);

                //day2
                expectCallForDayForSameStartTime(_tomorrowScheduleDay, _tomorrow, now.AddMinutes(30), true);

                //day3
                expectCallForDayForSameStartTime(_dayAfterTomorrowDay, _dayAfterTomorrow, now, true);

            }
            Assert.False(_target.IsBlockInSteadyState(teamBlockInfo,schedulingOptions));
        }

        [Test]
        public void ShouldReturnTrueIfAllAreSameShiftForSameShift()
        {
            _matrixList = new List<IScheduleMatrixPro>();
            _matrixList.Add(_scheduleMatrixPro);
            _teamInfo = new TeamInfo(_baseLineData.GroupPerson, new List<IList<IScheduleMatrixPro>> { _matrixList });
            _blockInfo = new BlockInfo(new DateOnlyPeriod(_today, _today.AddDays(1)));
            var teamBlockInfo = new TeamBlockInfo(_teamInfo, _blockInfo);
            var schedulingOptions = new SchedulingOptions();
            schedulingOptions.UseTeamBlockSameShift = true;
            using (_mock.Record())
            {
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod).Repeat.AtLeastOnce() ;
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod)
                      .Return(new DateOnlyPeriod(_today, _today.AddDays(2))).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_today)).Return(_todayScheduleDayPro) ;
                Expect.Call(_todayScheduleDayPro.DaySchedulePart()).Return(_todayScheduleDay) ;
                
                //day1
                expectCallForDayForSameShift(_todayScheduleDay, _today);

                //day2
                expectCallForDayForSameShift(_tomorrowScheduleDay , _tomorrow);
                
            }
            Assert.IsTrue(_target.IsBlockInSteadyState(teamBlockInfo, schedulingOptions));
        }
        
        [Test]
        public void ShouldReturnTrueIfAllAreSameShiftForSameStartTime()
        {
            _matrixList = new List<IScheduleMatrixPro>();
            _matrixList.Add(_scheduleMatrixPro);
            _teamInfo = new TeamInfo(_baseLineData.GroupPerson, new List<IList<IScheduleMatrixPro>> { _matrixList });
            _blockInfo = new BlockInfo(new DateOnlyPeriod(_today, _today.AddDays(2)));
            var teamBlockInfo = new TeamBlockInfo(_teamInfo, _blockInfo);
            var schedulingOptions = new SchedulingOptions();
            schedulingOptions.UseTeamBlockSameStartTime  = true;
            var now = DateTime.UtcNow;
            using (_mock.Record())
            {
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod)
                      .Return(new DateOnlyPeriod(_today, _today.AddDays(2))).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_today)).Return(_todayScheduleDayPro);
                Expect.Call(_todayScheduleDayPro.DaySchedulePart()).Return(_todayScheduleDay);

                //day1
                expectCallForDayForSameStartTime(_todayScheduleDay, _today, now, true);

                //day2
                expectCallForDayForSameStartTime(_tomorrowScheduleDay, _tomorrow, now, true);

                //day3
                expectCallForDayForSameStartTime(_dayAfterTomorrowDay , _dayAfterTomorrow, now, true);

            }
            Assert.IsTrue(_target.IsBlockInSteadyState(teamBlockInfo, schedulingOptions));
        }

        [Test]
        public void ShouldReturnTrueWhenPersonAssignmentIsNullIfAllAreSameShiftForSameStartTime()
        {
            _matrixList = new List<IScheduleMatrixPro>();
            _matrixList.Add(_scheduleMatrixPro);
            _teamInfo = new TeamInfo(_baseLineData.GroupPerson, new List<IList<IScheduleMatrixPro>> { _matrixList });
            _blockInfo = new BlockInfo(new DateOnlyPeriod(_today, _today.AddDays(2)));
            var teamBlockInfo = new TeamBlockInfo(_teamInfo, _blockInfo);
            var schedulingOptions = new SchedulingOptions();
            schedulingOptions.UseTeamBlockSameStartTime = true;
            using (_mock.Record())
            {
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod)
                      .Return(new DateOnlyPeriod(_today, _today.AddDays(2))).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_today)).Return(_todayScheduleDayPro);
                Expect.Call(_todayScheduleDayPro.DaySchedulePart()).Return(_todayScheduleDay);
                Expect.Call(_todayScheduleDay.IsScheduled()).Return(true);
                Expect.Call(_todayScheduleDay.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce() ;
                Expect.Call(_todayScheduleDay.GetEditorShift()).Return(null);
                

            }
            Assert.IsTrue(_target.IsBlockInSteadyState(teamBlockInfo, schedulingOptions));
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldReturnTrueWhenShiftPeriodIsNullIfAllAreSameShiftForSameStartTime()
        {
            _matrixList = new List<IScheduleMatrixPro>();
            _matrixList.Add(_scheduleMatrixPro);
            _teamInfo = new TeamInfo(_baseLineData.GroupPerson, new List<IList<IScheduleMatrixPro>> { _matrixList });
            _blockInfo = new BlockInfo(new DateOnlyPeriod(_today, _today.AddDays(2)));
            var teamBlockInfo = new TeamBlockInfo(_teamInfo, _blockInfo);
            var schedulingOptions = new SchedulingOptions();
            schedulingOptions.UseTeamBlockSameStartTime = true;
            var now = DateTime.UtcNow;
            using (_mock.Record())
            {
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod)
                      .Return(new DateOnlyPeriod(_today, _today.AddDays(1))).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_today)).Return(_todayScheduleDayPro);
                Expect.Call(_todayScheduleDayPro.DaySchedulePart()).Return(_todayScheduleDay);

                //day1
                expectCallForDayForSameStartTime(_todayScheduleDay, _today, now, true);

                var mainShift = _mock.StrictMock<IEditableShift>();
                IProjectionService projectionService = _mock.StrictMock<IProjectionService>();
                IVisualLayerCollection visualLayerCollection = _mock.StrictMock<IVisualLayerCollection>();
                Expect.Call(_tomorrowScheduleDay.GetEditorShift()).Return(mainShift).Repeat.AtLeastOnce();
                Expect.Call(mainShift.ProjectionService()).Return(projectionService).Repeat.AtLeastOnce();
                Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection).Repeat.AtLeastOnce();
                Expect.Call(visualLayerCollection.Period()).Return(new DateTimePeriod(now, now.AddHours(1))).Repeat.AtLeastOnce();
                Expect.Call(_tomorrowScheduleDay.TimeZone).Return(TimeZoneInfo.Local).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_tomorrow  )).Return(_tomorrowScheduleDayPro );
                Expect.Call(_tomorrowScheduleDayPro.DaySchedulePart()).Return(_tomorrowScheduleDay );
                Expect.Call(_tomorrowScheduleDay.IsScheduled()).Return(true);
                Expect.Call(_tomorrowScheduleDay.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();


            }
            Assert.IsTrue(_target.IsBlockInSteadyState(teamBlockInfo, schedulingOptions));
        }

        [Test]
        public void ShouldReturnTrueIfSomeAreSameShiftForSameStartTime()
        {
            _matrixList = new List<IScheduleMatrixPro>();
            _matrixList.Add(_scheduleMatrixPro);
            _teamInfo = new TeamInfo(_baseLineData.GroupPerson, new List<IList<IScheduleMatrixPro>> { _matrixList });
            _blockInfo = new BlockInfo(new DateOnlyPeriod(_today, _today.AddDays(2)));
            var teamBlockInfo = new TeamBlockInfo(_teamInfo, _blockInfo);
            var schedulingOptions = new SchedulingOptions();
            schedulingOptions.UseTeamBlockSameStartTime = true;
            var now = DateTime.UtcNow;
            using (_mock.Record())
            {
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod)
                      .Return(new DateOnlyPeriod(_today, _today.AddDays(2))).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_today)).Return(_todayScheduleDayPro);
                Expect.Call(_todayScheduleDayPro.DaySchedulePart()).Return(_todayScheduleDay);

                //day1
                expectCallForDayForSameStartTime(_todayScheduleDay, _today, now, true);

                //day2
                expectCallForDayForSameStartTime(_tomorrowScheduleDay , _tomorrow, now,false);

                //day3
                expectCallForDayForSameStartTime(_dayAfterTomorrowDay, _dayAfterTomorrow, now, true);

            }
            Assert.IsTrue(_target.IsBlockInSteadyState(teamBlockInfo, schedulingOptions));
        }

        [Test]
        public void ReturnTrueForScheduledBlock()
        {
            _matrixList = new List<IScheduleMatrixPro>();
            _matrixList.Add(_scheduleMatrixPro);
            _teamInfo = new TeamInfo(_baseLineData.GroupPerson, new List<IList<IScheduleMatrixPro>> { _matrixList });
            _blockInfo = new BlockInfo(new DateOnlyPeriod(_today, _today.AddDays(2)));
            var teamBlockInfo = new TeamBlockInfo(_teamInfo, _blockInfo);
            using (_mock.Record())
            {
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod)
                      .Return(new DateOnlyPeriod(_today, _today.AddDays(2))).Repeat.AtLeastOnce().Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_today)).Return(_todayScheduleDayPro);
                Expect.Call(_todayScheduleDayPro.DaySchedulePart()).Return(_todayScheduleDay).Repeat.AtLeastOnce();
                Expect.Call(_todayScheduleDay.IsScheduled()).Return(true).Repeat.AtLeastOnce();
                
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_tomorrow )).Return(_todayScheduleDayPro);
                Expect.Call(_todayScheduleDayPro.DaySchedulePart()).Return(_todayScheduleDay).Repeat.AtLeastOnce();
                Expect.Call(_todayScheduleDay.IsScheduled()).Return(true).Repeat.AtLeastOnce();
                
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dayAfterTomorrow )).Return(_todayScheduleDayPro);
                Expect.Call(_todayScheduleDayPro.DaySchedulePart()).Return(_todayScheduleDay).Repeat.AtLeastOnce();
                Expect.Call(_todayScheduleDay.IsScheduled()).Return(true).Repeat.AtLeastOnce();
            }
            Assert.IsTrue(_target.IsBlockScheduled(teamBlockInfo));
        }

        [Test]
        public void ReturnFalseForSomeScheduledBlock()
        {
            _matrixList = new List<IScheduleMatrixPro>();
            _matrixList.Add(_scheduleMatrixPro);
            _teamInfo = new TeamInfo(_baseLineData.GroupPerson, new List<IList<IScheduleMatrixPro>> { _matrixList });
            _blockInfo = new BlockInfo(new DateOnlyPeriod(_today, _today.AddDays(2)));
            var teamBlockInfo = new TeamBlockInfo(_teamInfo, _blockInfo);
            using (_mock.Record())
            {
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod)
                      .Return(new DateOnlyPeriod(_today, _today.AddDays(2))).Repeat.AtLeastOnce().Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_today)).Return(_todayScheduleDayPro);
                Expect.Call(_todayScheduleDayPro.DaySchedulePart()).Return(_todayScheduleDay).Repeat.AtLeastOnce();
                Expect.Call(_todayScheduleDay.IsScheduled()).Return(true).Repeat.AtLeastOnce();

                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_tomorrow)).Return(_tomorrowScheduleDayPro);
                Expect.Call(_tomorrowScheduleDayPro.DaySchedulePart()).Return(_tomorrowScheduleDay).Repeat.AtLeastOnce();
                Expect.Call(_tomorrowScheduleDay.IsScheduled()).Return(false).Repeat.AtLeastOnce();

                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dayAfterTomorrow)).Return(_todayScheduleDayPro);
                Expect.Call(_todayScheduleDayPro.DaySchedulePart()).Return(_todayScheduleDay).Repeat.AtLeastOnce();
                Expect.Call(_todayScheduleDay.IsScheduled()).Return(true).Repeat.AtLeastOnce();
            }
            Assert.IsFalse(_target.IsBlockScheduled(teamBlockInfo));
        }

        [Test]
        public void ReturnFalseIfTeamBlockIsNull()
        {
            Assert.IsFalse(_target.IsBlockInSteadyState(null,new SchedulingOptions()));
        }

        [Test]
        public void ReturnFalseIfSchedulingOptionsAreNull()
        {
            Assert.IsFalse(_target.IsBlockInSteadyState(new TeamBlockInfo(_teamInfo,_blockInfo ), null));
        }

      
        private void expectCallForDayForSameShift(IScheduleDay scheduleDay, DateOnly dateOnly)
        {
            var mainShift = _mock.StrictMock<IEditableShift>();
            IScheduleDayPro scheduleDayPro = _mock.StrictMock<IScheduleDayPro>();

            Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(dateOnly)).Return(scheduleDayPro);
            Expect.Call(scheduleDayPro.DaySchedulePart()).Return(scheduleDay);
            Expect.Call(scheduleDay.IsScheduled()).Return(true).Repeat.AtLeastOnce();
            Expect.Call(scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
            Expect.Call(scheduleDay.GetEditorShift()).Return(mainShift).Repeat.AtLeastOnce();
            Expect.Call(_scheduleDayEquator.MainShiftEqualsWithoutPeriod(mainShift, mainShift)).IgnoreArguments().Return(true).Repeat.AtLeastOnce();
        }

        private void expectCallForDayForSameStartTime(IScheduleDay scheduleDay, DateOnly dateOnly, DateTime startTime, bool isScheduled)
        {
            var mainShift = _mock.StrictMock<IEditableShift>();
            IProjectionService projectionService = _mock.StrictMock<IProjectionService>();
            IVisualLayerCollection visualLayerCollection = _mock.StrictMock<IVisualLayerCollection>();

			Expect.Call(scheduleDay.GetEditorShift()).Return(mainShift).Repeat.AtLeastOnce();
            Expect.Call(mainShift.ProjectionService()).Return(projectionService).Repeat.AtLeastOnce();
            Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection).Repeat.AtLeastOnce();
            Expect.Call(visualLayerCollection.Period()).Return(new DateTimePeriod(startTime, startTime.AddHours(1))).Repeat.AtLeastOnce();
            Expect.Call(scheduleDay.TimeZone).Return(TimeZoneInfo.Local).Repeat.AtLeastOnce();
            Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(dateOnly)).Return(_todayScheduleDayPro);
            Expect.Call(_todayScheduleDayPro.DaySchedulePart()).Return(scheduleDay);
            Expect.Call(scheduleDay.IsScheduled()).Return(isScheduled).Repeat.AtLeastOnce() ;
            Expect.Call(scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();

        }

        private void expectCallForDayForSameEndTime(IScheduleDay scheduleDay, DateOnly dateOnly, DateTime endTime, bool isScheduled)
        {
            var mainShift = _mock.StrictMock<IEditableShift>();
            IProjectionService projectionService = _mock.StrictMock<IProjectionService>();
            IVisualLayerCollection visualLayerCollection = _mock.StrictMock<IVisualLayerCollection>();

            Expect.Call(scheduleDay.GetEditorShift()).Return(mainShift).Repeat.AtLeastOnce();
            Expect.Call(mainShift.ProjectionService()).Return(projectionService).Repeat.AtLeastOnce();
            Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection).Repeat.AtLeastOnce();
            Expect.Call(visualLayerCollection.Period()).Return(new DateTimePeriod(DateTime.UtcNow.AddHours(-2), endTime)).Repeat.AtLeastOnce();
            Expect.Call(scheduleDay.TimeZone).Return(TimeZoneInfo.Local).Repeat.AtLeastOnce();
            Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(dateOnly)).Return(_todayScheduleDayPro);
            Expect.Call(_todayScheduleDayPro.DaySchedulePart()).Return(scheduleDay);
            Expect.Call(scheduleDay.IsScheduled()).Return(isScheduled).Repeat.AtLeastOnce() ;
            Expect.Call(scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();

        }

        [Test]
        public void ShouldReturnTrueIfAllAreSameShiftForSameEndTime()
        {
            _matrixList = new List<IScheduleMatrixPro>();
            _matrixList.Add(_scheduleMatrixPro);
            _teamInfo = new TeamInfo(_baseLineData.GroupPerson, new List<IList<IScheduleMatrixPro>> { _matrixList });
            _blockInfo = new BlockInfo(new DateOnlyPeriod(_today, _today.AddDays(2)));
            var teamBlockInfo = new TeamBlockInfo(_teamInfo, _blockInfo);
            var schedulingOptions = new SchedulingOptions();
            schedulingOptions.UseTeamBlockSameEndTime = true;
            var now = DateTime.UtcNow;
            using (_mock.Record())
            {
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod)
                      .Return(new DateOnlyPeriod(_today, _today.AddDays(2))).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_today)).Return(_todayScheduleDayPro);
                Expect.Call(_todayScheduleDayPro.DaySchedulePart()).Return(_todayScheduleDay);

                //day1
                expectCallForDayForSameEndTime(_todayScheduleDay, _today, now, true);

                //day2
                expectCallForDayForSameEndTime(_todayScheduleDay, _tomorrow, now, false);

                //day3
                expectCallForDayForSameEndTime(_todayScheduleDay, _dayAfterTomorrow, now, true);

            }
            Assert.IsTrue(_target.IsBlockInSteadyState(teamBlockInfo, schedulingOptions));
        }

        [Test]
        public void ShouldReturnFalseIfAllAreNotSameShiftForSameEndTime()
        {
            _matrixList = new List<IScheduleMatrixPro>();
            _matrixList.Add(_scheduleMatrixPro);
            _teamInfo = new TeamInfo(_baseLineData.GroupPerson, new List<IList<IScheduleMatrixPro>> { _matrixList });
            _blockInfo = new BlockInfo(new DateOnlyPeriod(_today, _today.AddDays(2)));
            var teamBlockInfo = new TeamBlockInfo(_teamInfo, _blockInfo);
            var schedulingOptions = new SchedulingOptions();
            schedulingOptions.UseTeamBlockSameEndTime = true;
            var now = DateTime.UtcNow;
            using (_mock.Record())
            {
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod)
                      .Return(new DateOnlyPeriod(_today, _today.AddDays(2))).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_today)).Return(_todayScheduleDayPro);
                Expect.Call(_todayScheduleDayPro.DaySchedulePart()).Return(_todayScheduleDay);
                Expect.Call(_todayScheduleDay.IsScheduled()).Return(true);
                //day1
                expectCallForDayForSameEndTime(_todayScheduleDay, _today, now, true);

                //day2
                expectCallForDayForSameEndTime(_tomorrowScheduleDay, _tomorrow, now, false);

                //day3
                expectCallForDayForSameEndTime(_dayAfterTomorrowDay , _dayAfterTomorrow, now.AddMinutes(30), true);

            }
            Assert.IsFalse(_target.IsBlockInSteadyState(teamBlockInfo, schedulingOptions));
        }


        private void expectCallForDayForSameShiftCategory(IScheduleDay scheduleDay, DateOnly dateOnly, IShiftCategory shiftCategory , bool isScheduled)
        {
            IPersonAssignment personAssignment = _mock.StrictMock<IPersonAssignment>();

            Expect.Call(scheduleDay.PersonAssignment()).Return(personAssignment).Repeat.AtLeastOnce() ;
            Expect.Call(personAssignment.ShiftCategory).Return(shiftCategory).Repeat.AtLeastOnce()  ;
            Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(dateOnly)).Return(_todayScheduleDayPro);
            Expect.Call(_todayScheduleDayPro.DaySchedulePart()).Return(scheduleDay);
            Expect.Call(scheduleDay.IsScheduled()).Return(isScheduled).Repeat.AtLeastOnce();
            Expect.Call(scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();

        }

        [Test]
        public void ShouldReturnTrueIfAllAreSameShiftForSameShiftCategory()
        {
            _matrixList = new List<IScheduleMatrixPro>();
            _matrixList.Add(_scheduleMatrixPro);
            _teamInfo = new TeamInfo(_baseLineData.GroupPerson, new List<IList<IScheduleMatrixPro>> { _matrixList });
            _blockInfo = new BlockInfo(new DateOnlyPeriod(_today, _today.AddDays(2)));
            var teamBlockInfo = new TeamBlockInfo(_teamInfo, _blockInfo);
            var schedulingOptions = new SchedulingOptions();
            schedulingOptions.UseTeamBlockSameShiftCategory  = true;
            var sc = new ShiftCategory("Morning");
            using (_mock.Record())
            {
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod)
                      .Return(new DateOnlyPeriod(_today, _today.AddDays(2))).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_today)).Return(_todayScheduleDayPro);
                Expect.Call(_todayScheduleDayPro.DaySchedulePart()).Return(_todayScheduleDay);

                IEditableShift editableShift = _mock.StrictMock<IEditableShift>();
                Expect.Call(_todayScheduleDay.GetEditorShift()).Return(editableShift);

                //day1
                expectCallForDayForSameShiftCategory(_todayScheduleDay, _today,sc , true);

                //day2
                expectCallForDayForSameShiftCategory(_todayScheduleDay, _tomorrow, sc, false);

                //day3
                expectCallForDayForSameShiftCategory(_tomorrowScheduleDay, _dayAfterTomorrow, sc, true);

            }
            Assert.IsTrue(_target.IsBlockInSteadyState(teamBlockInfo, schedulingOptions));
        }

        [Test]
        public void ShouldReturnFalseIfAllAreNotSameShiftForSameShiftCategory()
        {
            _matrixList = new List<IScheduleMatrixPro>();
            _matrixList.Add(_scheduleMatrixPro);
            _teamInfo = new TeamInfo(_baseLineData.GroupPerson, new List<IList<IScheduleMatrixPro>> { _matrixList });
            _blockInfo = new BlockInfo(new DateOnlyPeriod(_today, _today.AddDays(2)));
            var teamBlockInfo = new TeamBlockInfo(_teamInfo, _blockInfo);
            var schedulingOptions = new SchedulingOptions();
            schedulingOptions.UseTeamBlockSameShiftCategory = true;
            var sc = new ShiftCategory("Morning");
            using (_mock.Record())
            {
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod)
                      .Return(new DateOnlyPeriod(_today, _today.AddDays(2))).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_today)).Return(_todayScheduleDayPro);
                Expect.Call(_todayScheduleDayPro.DaySchedulePart()).Return(_todayScheduleDay);
                Expect.Call(_tomorrowScheduleDay.IsScheduled()).Return(true);

                //day1
                expectCallForDayForSameShiftCategory(_todayScheduleDay, _today, sc, true);

                //day2
                expectCallForDayForSameShiftCategory(_tomorrowScheduleDay , _tomorrow, sc, false);

                //day3
                expectCallForDayForSameShiftCategory(_dayAfterTomorrowDay , _dayAfterTomorrow, new ShiftCategory("Late"), true);

            }
            Assert.IsFalse(_target.IsBlockInSteadyState(teamBlockInfo, schedulingOptions));
        }

        [Test]
        public void ShouldReturnFalseIfAllAreNotSameShiftForSameShiftCategoryWithFirstDayNotScheduled()
        {
            _matrixList = new List<IScheduleMatrixPro>();
            _matrixList.Add(_scheduleMatrixPro);
            _teamInfo = new TeamInfo(_baseLineData.GroupPerson, new List<IList<IScheduleMatrixPro>> { _matrixList });
            _blockInfo = new BlockInfo(new DateOnlyPeriod(_today, _today.AddDays(2)));
            var teamBlockInfo = new TeamBlockInfo(_teamInfo, _blockInfo);
            var schedulingOptions = new SchedulingOptions();
            schedulingOptions.UseTeamBlockSameShiftCategory = true;
            var sc = new ShiftCategory("Morning");
            using (_mock.Record())
            {
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod)
                      .Return(new DateOnlyPeriod(_today, _today.AddDays(2))).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_today)).Return(_todayScheduleDayPro);
                Expect.Call(_todayScheduleDayPro.DaySchedulePart()).Return(_todayScheduleDay);
                Expect.Call(_todayScheduleDay.IsScheduled() ).Return(false);
                
                //day1
                expectCallForDayForSameShiftCategory(_todayScheduleDay, _today, sc, false);

                //day2
                expectCallForDayForSameShiftCategory(_tomorrowScheduleDay, _tomorrow, sc, true);

                //day3
                expectCallForDayForSameShiftCategory(_dayAfterTomorrowDay, _dayAfterTomorrow, new ShiftCategory("Late"), true);

            }
            Assert.IsFalse(_target.IsBlockInSteadyState(teamBlockInfo, schedulingOptions));
        }
    }

}
