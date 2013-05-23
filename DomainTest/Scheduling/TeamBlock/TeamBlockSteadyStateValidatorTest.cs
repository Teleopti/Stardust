using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
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

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _target = new BlockSteadyStateValidator();
            _baseLineData = new BaseLineData();
            _matrixList = new List<IScheduleMatrixPro>();
            _scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();
            _virtualSchedulePeriod = _mock.StrictMock<IVirtualSchedulePeriod>();
            _todayScheduleDayPro = _mock.StrictMock<IScheduleDayPro>();
            _tomorrowScheduleDayPro = _mock.StrictMock<IScheduleDayPro>();
            _todayScheduleDay = _mock.StrictMock<IScheduleDay>();
            _tomorrowScheduleDay = _mock.StrictMock<IScheduleDay>();
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
            var schedulingOptions =new SchedulingOptions();
            schedulingOptions.UseTeamBlockSameShift = true;
            using (_mock.Record())
            {
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod)
                      .Return(new DateOnlyPeriod(_today, _today.AddDays(2))).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_today)).Return(_todayScheduleDayPro);
                Expect.Call(_todayScheduleDayPro.DaySchedulePart()).Return(_todayScheduleDay);

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
                expectCallForDayForSameStartTime(_todayScheduleDay, _dayAfterTomorrow, now, true);

            }
            Assert.False(_target.IsBlockInSteadyState(teamBlockInfo,schedulingOptions));
        }

        [Test]
        public void ShouldReturnTrueIfAllAreSameShiftForSameShift()
        {
            _matrixList = new List<IScheduleMatrixPro>();
            _matrixList.Add(_scheduleMatrixPro);
            _teamInfo = new TeamInfo(_baseLineData.GroupPerson, new List<IList<IScheduleMatrixPro>> { _matrixList });
            _blockInfo = new BlockInfo(new DateOnlyPeriod(_today, _today.AddDays(2)));
            var teamBlockInfo = new TeamBlockInfo(_teamInfo, _blockInfo);
            var schedulingOptions = new SchedulingOptions();
            schedulingOptions.UseTeamBlockSameShift = true;
            using (_mock.Record())
            {
                Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod).Repeat.AtLeastOnce();
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod)
                      .Return(new DateOnlyPeriod(_today, _today.AddDays(2))).Repeat.AtLeastOnce();
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_today)).Return(_todayScheduleDayPro);
                Expect.Call(_todayScheduleDayPro.DaySchedulePart()).Return(_todayScheduleDay);
                //day1
                expectCallForDayForSameShift(_todayScheduleDay, _today);

                //day2
                expectCallForDayForSameShift(_todayScheduleDay, _tomorrow);

                //day3
                expectCallForDayForSameShift(_todayScheduleDay, _dayAfterTomorrow);

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
                expectCallForDayForSameStartTime(_todayScheduleDay, _tomorrow, now, true);

                //day3
                expectCallForDayForSameStartTime(_todayScheduleDay, _dayAfterTomorrow, now, true);

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

                Expect.Call(_todayScheduleDay.AssignmentHighZOrder()).Return(null).Repeat.AtLeastOnce();
                

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

                //day2
                //expectCallForDayForSameStartTime(_todayScheduleDay, _tomorrow, now, true);

                var mainShift = _mock.StrictMock<IMainShift>();
                IProjectionService projectionService = _mock.StrictMock<IProjectionService>();
                var personAssignment = _mock.StrictMock<IPersonAssignment>();
                IVisualLayerCollection visualLayerCollection = _mock.StrictMock<IVisualLayerCollection>();

                Expect.Call(_tomorrowScheduleDay  .AssignmentHighZOrder()).Return(null);
                Expect.Call(personAssignment.ToMainShift()).Return(mainShift).Repeat.AtLeastOnce();
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
                expectCallForDayForSameStartTime(_todayScheduleDay, _tomorrow, now,false);

                //day3
                expectCallForDayForSameStartTime(_todayScheduleDay, _dayAfterTomorrow, now, true);

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
            IList<IPersonDayOff> list = new List<IPersonDayOff>();
            ReadOnlyCollection<IPersonDayOff> personDayOffCollection = new ReadOnlyCollection<IPersonDayOff>(list);
            var pA = _mock.StrictMock<IPersonAssignment>();
            IList<IPersonAssignment> personAssignment = new List<IPersonAssignment>();
            ReadOnlyCollection<IPersonAssignment> personAssignmentList = new ReadOnlyCollection<IPersonAssignment>(personAssignment);
            IMainShift mainShift = _mock.StrictMock<IMainShift>();
            IShiftCategory shiftCategory = _mock.StrictMock<IShiftCategory>();

            Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(dateOnly)).Return(_todayScheduleDayPro);
            Expect.Call(_todayScheduleDayPro.DaySchedulePart()).Return(scheduleDay);
            Expect.Call(scheduleDay.IsScheduled()).Return(true);

            Expect.Call(scheduleDay.PersonDayOffCollection()).Return(personDayOffCollection).Repeat.AtLeastOnce();
            Expect.Call(scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
            Expect.Call(scheduleDay.PersonAssignmentCollection()).Return(personAssignmentList);
            Expect.Call(scheduleDay.AssignmentHighZOrder()).Return(pA).Repeat.AtLeastOnce();
            
            Expect.Call(pA.ToMainShift()).Return(mainShift).Repeat.AtLeastOnce();
            Expect.Call(mainShift.ShiftCategory).Return(shiftCategory).Repeat.AtLeastOnce();
            Expect.Call(shiftCategory.Id).Return(new Guid()).Repeat.AtLeastOnce();
            Expect.Call(mainShift.LayerCollection).Return(new LayerCollection<IActivity>()).Repeat.AtLeastOnce();
        }

        private void expectCallForDayForSameStartTime(IScheduleDay scheduleDay, DateOnly dateOnly, DateTime startTime, bool isScheduled)
        {
            var mainShift = _mock.StrictMock<IMainShift>();
            IProjectionService projectionService = _mock.StrictMock<IProjectionService>();
            var personAssignment = _mock.StrictMock<IPersonAssignment>();
            IVisualLayerCollection visualLayerCollection = _mock.StrictMock<IVisualLayerCollection>();

            Expect.Call(scheduleDay.AssignmentHighZOrder()).Return(personAssignment).Repeat.AtLeastOnce();
            Expect.Call(personAssignment.ToMainShift()).Return(mainShift).Repeat.AtLeastOnce();
            Expect.Call(mainShift.ProjectionService()).Return(projectionService).Repeat.AtLeastOnce();
            Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection).Repeat.AtLeastOnce();
            Expect.Call(visualLayerCollection.Period()).Return(new DateTimePeriod(startTime, startTime.AddHours(1))).Repeat.AtLeastOnce();
            Expect.Call(scheduleDay.TimeZone).Return(TimeZoneInfo.Local).Repeat.AtLeastOnce();
            Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(dateOnly)).Return(_todayScheduleDayPro);
            Expect.Call(_todayScheduleDayPro.DaySchedulePart()).Return(scheduleDay);
            Expect.Call(scheduleDay.IsScheduled()).Return(isScheduled);
            Expect.Call(scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();

        }
    }

}
