using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.MoveTimeOptimization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.MoveTimeOptimization
{
    [TestFixture]
    class ValidateFoundMovedDaysSpecificationTest
    {
        private ValidateFoundMovedDaysSpecification _target;
        private MockRepository _mocks;
        private IScheduleMatrixPro _scheduleMatrix1;
        private IList<IScheduleDayPro> _scheduleDayProList;
        private IScheduleMatrixPro _scheduleMatrix2;
        private IScheduleMatrixPro _scheduleMatrixPro;
        private IProjectionService _projectionService;
        private IScheduleDayPro _scheduleDayProMoveFrom;
        private IScheduleDayPro _scheduleDayProMoveTo;
        private IProjectionService _projectionService2;
        private IScheduleDay _scheduleDayMoveFrom;
        private IScheduleDay _scheduleDayMoveTo;
        private IVisualLayerCollection _visualLayerProjection1;
        private IVisualLayerCollection _visualLayerProjection2;


        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
            _scheduleMatrix1 = _mocks.StrictMock<IScheduleMatrixPro>();
            _scheduleMatrix2 = _mocks.StrictMock<IScheduleMatrixPro>();
            _projectionService = _mocks.StrictMock<IProjectionService>();
            _projectionService2 = _mocks.StrictMock<IProjectionService>();
            _scheduleDayProMoveFrom = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayProMoveTo = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDayMoveFrom = _mocks.StrictMock<IScheduleDay>();
            _scheduleDayMoveTo = _mocks.StrictMock<IScheduleDay>();
            _visualLayerProjection1 = _mocks.StrictMock<IVisualLayerCollection>();
            _visualLayerProjection2 = _mocks.StrictMock<IVisualLayerCollection>();
            _target = new ValidateFoundMovedDaysSpecification();
        }

        [Test]
        public void ValidatorReturnFalseIfMoveFromDayIsDayOff()
        {
            _scheduleDayProList = new List<IScheduleDayPro>();
            _scheduleDayProList.Add(_scheduleDayProMoveFrom);
            _scheduleDayProList.Add(_scheduleDayProMoveTo);
            SchedulePartView shiftType1 = SchedulePartView.DayOff;
            using (_mocks.Record())
            {
                ReadOnlyCollection<IScheduleDayPro> listOfScheduleDays = new ReadOnlyCollection<IScheduleDayPro>(_scheduleDayProList);
                Expect.Call(_scheduleMatrixPro.FullWeeksPeriodDays).Return(listOfScheduleDays).Repeat.Times(2);
                Expect.Call(_scheduleDayProMoveFrom.DaySchedulePart()).Return(_scheduleDayMoveFrom);
                Expect.Call(_scheduleDayProMoveTo.DaySchedulePart()).Return(_scheduleDayMoveTo);
                Expect.Call(_scheduleDayMoveFrom.SignificantPart()).Return(shiftType1);
            }
            using (_mocks.Playback())
            {
                var result = _target.AreFoundDaysValid(0, 1, _scheduleMatrixPro);
                Assert.IsFalse(result);
            }
        }

        [Test]
        public void ValidatorReturnFalseIfMoveToDayIsDayOff()
        {
            _scheduleDayProList = new List<IScheduleDayPro>();
            _scheduleDayProList.Add(_scheduleDayProMoveFrom);
            _scheduleDayProList.Add(_scheduleDayProMoveTo);
            SchedulePartView shiftType1 = SchedulePartView.MainShift;
            SchedulePartView shiftType2 = SchedulePartView.DayOff;
            using (_mocks.Record())
            {
                ReadOnlyCollection<IScheduleDayPro> listOfScheduleDays = new ReadOnlyCollection<IScheduleDayPro>(_scheduleDayProList);
                Expect.Call(_scheduleMatrixPro.FullWeeksPeriodDays).Return(listOfScheduleDays).Repeat.Times(2);
                Expect.Call(_scheduleDayProMoveFrom.DaySchedulePart()).Return(_scheduleDayMoveFrom);
                Expect.Call(_scheduleDayProMoveTo.DaySchedulePart()).Return(_scheduleDayMoveTo);
                Expect.Call(_scheduleDayMoveFrom.SignificantPart()).Return(shiftType1);
                Expect.Call(_scheduleDayMoveTo.SignificantPart()).Return(shiftType2);
            }
            using (_mocks.Playback())
            {
                var result = _target.AreFoundDaysValid(0, 1, _scheduleMatrixPro);
                Assert.IsFalse(result);
            }
        }

        [Test]
        public void ValidatorReturnFalseIfUnderStaffedDayHasLongerShiftTime()
        {
            _scheduleDayProList = new List<IScheduleDayPro>();
            _scheduleDayProList.Add(_scheduleDayProMoveFrom);
            _scheduleDayProList.Add(_scheduleDayProMoveTo);
            SchedulePartView shiftType1 = SchedulePartView.MainShift;
            SchedulePartView shiftType2 = SchedulePartView.MainShift;
            using (_mocks.Record())
            {
                ReadOnlyCollection<IScheduleDayPro> listOfScheduleDays = new ReadOnlyCollection<IScheduleDayPro>(_scheduleDayProList);
                Expect.Call(_scheduleMatrixPro.FullWeeksPeriodDays).Return(listOfScheduleDays).Repeat.Times(2);
                Expect.Call(_scheduleDayProMoveFrom.DaySchedulePart()).Return(_scheduleDayMoveFrom);
                Expect.Call(_scheduleDayProMoveTo.DaySchedulePart()).Return(_scheduleDayMoveTo);
                Expect.Call(_scheduleDayMoveFrom.SignificantPart()).Return(shiftType1);
                Expect.Call(_scheduleDayMoveTo.SignificantPart()).Return(shiftType2);
                Expect.Call(_scheduleDayMoveFrom.ProjectionService()).Return(_projectionService).Repeat.Times(2);
                TimeSpan moveTimeFromTimeSpan = new TimeSpan(8, 0, 0);
                Expect.Call(
                    _projectionService.CreateProjection())
                    .Return(_visualLayerProjection1);
                Expect.Call(_visualLayerProjection1.ContractTime()).Return(moveTimeFromTimeSpan);

                Expect.Call(_scheduleDayMoveTo.ProjectionService()).Return(_projectionService2).Repeat.Times(2);
                TimeSpan moveTimeToTimeSpan = new TimeSpan(7, 30, 0);
                Expect.Call(
                    _projectionService2.CreateProjection())
                    .Return(_visualLayerProjection2);
                Expect.Call(_visualLayerProjection2.ContractTime()).Return(moveTimeToTimeSpan);
            }
            using (_mocks.Playback())
            {
                var result = _target.AreFoundDaysValid(0, 1, _scheduleMatrixPro);
                Assert.IsFalse(result);
            }
        }

        [Test]
        public void ValidatorReturnTrueIfUnderStaffedDayHasShorterShiftTime()
        {
            _scheduleDayProList = new List<IScheduleDayPro>();
            _scheduleDayProList.Add(_scheduleDayProMoveFrom);
            _scheduleDayProList.Add(_scheduleDayProMoveTo);
            SchedulePartView shiftType1 = SchedulePartView.MainShift;
            SchedulePartView shiftType2 = SchedulePartView.MainShift;
            using (_mocks.Record())
            {
                ReadOnlyCollection<IScheduleDayPro> listOfScheduleDays = new ReadOnlyCollection<IScheduleDayPro>(_scheduleDayProList);
                Expect.Call(_scheduleMatrixPro.FullWeeksPeriodDays).Return(listOfScheduleDays).Repeat.Times(2);
                Expect.Call(_scheduleDayProMoveFrom.DaySchedulePart()).Return(_scheduleDayMoveFrom);
                Expect.Call(_scheduleDayProMoveTo.DaySchedulePart()).Return(_scheduleDayMoveTo);
                Expect.Call(_scheduleDayMoveFrom.SignificantPart()).Return(shiftType1);
                Expect.Call(_scheduleDayMoveTo.SignificantPart()).Return(shiftType2);
                Expect.Call(_scheduleDayMoveFrom.ProjectionService()).Return(_projectionService).Repeat.Times(2);
                TimeSpan moveTimeFromTimeSpan = new TimeSpan(8, 0, 0);
                Expect.Call(
                    _projectionService.CreateProjection())
                    .Return(_visualLayerProjection1);
                Expect.Call(_visualLayerProjection1.ContractTime()).Return(moveTimeFromTimeSpan);

                Expect.Call(_scheduleDayMoveTo.ProjectionService()).Return(_projectionService2).Repeat.Times(2);
                TimeSpan moveTimeToTimeSpan = new TimeSpan(8, 30, 0);
                Expect.Call(
                    _projectionService2.CreateProjection())
                    .Return(_visualLayerProjection2);
                Expect.Call(_visualLayerProjection2.ContractTime()).Return(moveTimeToTimeSpan);
            }
            using (_mocks.Playback())
            {
                var result = _target.AreFoundDaysValid(0, 1, _scheduleMatrixPro);
                Assert.IsTrue(result);
            }
        }
    }
}
