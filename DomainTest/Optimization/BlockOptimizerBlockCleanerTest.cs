using System.Collections.ObjectModel;
using NUnit.Framework;
using System.Collections.Generic;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class BlockOptimizerBlockCleanerTest
    {
        private MockRepository _mock;
        private ISchedulePartModifyAndRollbackService _modifyAndRollbackService;
        private BlockOptimizerBlockCleaner _target;
        private IScheduleMatrixPro _matrix;
        private ISchedulingOptions _schedulingOptions;
        private IBlockFinderFactory _blockFinderFactory;
        private IDeleteSchedulePartService _deleteSchedulePartService;
        private IBetweenDayOffBlockFinder _betweenDayOffBlockFinder;
        private IBlockFinderResult _blockFinderResult;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _modifyAndRollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
            _schedulingOptions = new SchedulingOptions
                                     {
                                         UseBlockOptimizing = BlockFinderType.BetweenDayOff
                                     };
            _blockFinderFactory = _mock.StrictMock<IBlockFinderFactory>();
            _deleteSchedulePartService = _mock.StrictMock<IDeleteSchedulePartService>();
            _target = new BlockOptimizerBlockCleaner(_modifyAndRollbackService, _blockFinderFactory, _deleteSchedulePartService);
            _matrix = _mock.StrictMock<IScheduleMatrixPro>();
            _betweenDayOffBlockFinder = _mock.StrictMock<IBetweenDayOffBlockFinder>();
            _blockFinderResult = _mock.StrictMock<IBlockFinderResult>();
        }

        [Test]
        public void ShouldRemoveAllSchedulesBetweenDayOffs()
        {
            var date1 = new DateOnly(2011, 10, 25);
            var date2 = new DateOnly(2011, 10, 26);
            var date3 = new DateOnly(2011, 10, 27);
            var date4 = new DateOnly(2011, 10, 28);

            var scheduleDayPro = _mock.StrictMock<IScheduleDayPro>();
            var scheduleDay = _mock.StrictMock<IScheduleDay>();
            var dateOnlyAsPeriod = _mock.StrictMock<IDateOnlyAsDateTimePeriod>();
            
            Expect.Call(_blockFinderFactory.CreateBetweenDayOffBlockFinder(_matrix)).Return(_betweenDayOffBlockFinder);
            Expect.Call(_betweenDayOffBlockFinder.FindValidBlockForDate(date3)).Return(_blockFinderResult);
            Expect.Call(_blockFinderResult.BlockDays).Return(new List<DateOnly> {date1, date2, date3, date4});
            Expect.Call(_matrix.GetScheduleDayByKey(date1)).Return(scheduleDayPro);
            Expect.Call(_matrix.GetScheduleDayByKey(date2)).Return(scheduleDayPro);
            Expect.Call(_matrix.GetScheduleDayByKey(date3)).Return(scheduleDayPro);
            Expect.Call(_matrix.GetScheduleDayByKey(date4)).Return(scheduleDayPro);

            Expect.Call(scheduleDayPro.DaySchedulePart()).Return(scheduleDay).Repeat.Times(4);
            Expect.Call(
                _deleteSchedulePartService.Delete(
                    new List<IScheduleDay> {scheduleDay, scheduleDay, scheduleDay, scheduleDay},
                    new DeleteOption {MainShift = true}, _modifyAndRollbackService,null)).IgnoreArguments();
            Expect.Call(_matrix.UnlockedDays).Return(
                new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> {scheduleDayPro}));
            Expect.Call(scheduleDay.DateOnlyAsPeriod).Return(dateOnlyAsPeriod).Repeat.Times(4);
            Expect.Call(dateOnlyAsPeriod.DateOnly).Return(date1).Repeat.Times(4);

            _mock.ReplayAll();

            _target.ClearSchedules(_matrix, new List<DateOnly> {date3}, _schedulingOptions);

            _mock.VerifyAll();

        }

        [Test]
        public void ShouldRemoveAllSchedulesInSchedulePeriod()
        {
            _schedulingOptions.UseBlockOptimizing = BlockFinderType.SchedulePeriod;
            var date1 = new DateOnly(2011, 10, 25);
            var date3 = new DateOnly(2011, 10, 27);

            var scheduleDayPro1 = _mock.StrictMock<IScheduleDayPro>();
            var scheduleDayPro2 = _mock.StrictMock<IScheduleDayPro>();
            var scheduleDayPro3 = _mock.StrictMock<IScheduleDayPro>();
            var scheduleDayPro4 = _mock.StrictMock<IScheduleDayPro>();

            var scheduleDay = _mock.StrictMock<IScheduleDay>();
            var dateOnlyAsPeriod = _mock.StrictMock<IDateOnlyAsDateTimePeriod>();

            Expect.Call(_matrix.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { scheduleDayPro1, scheduleDayPro2, scheduleDayPro3, scheduleDayPro4 }));
            Expect.Call(scheduleDayPro1.DaySchedulePart()).Return(scheduleDay);
            Expect.Call(scheduleDayPro2.DaySchedulePart()).Return(scheduleDay);
            Expect.Call(scheduleDayPro3.DaySchedulePart()).Return(scheduleDay);
            Expect.Call(scheduleDayPro4.DaySchedulePart()).Return(scheduleDay);
            Expect.Call(
                _deleteSchedulePartService.Delete(
                    new List<IScheduleDay> { scheduleDay, scheduleDay, scheduleDay, scheduleDay },
                    new DeleteOption { MainShift = true }, _modifyAndRollbackService, null)).IgnoreArguments();
            
            Expect.Call(scheduleDay.DateOnlyAsPeriod).Return(dateOnlyAsPeriod).Repeat.Times(4);
            Expect.Call(dateOnlyAsPeriod.DateOnly).Return(date1).Repeat.Times(4);

            _mock.ReplayAll();

            _target.ClearSchedules(_matrix, new List<DateOnly> { date3 }, _schedulingOptions);

            _mock.VerifyAll();
        }
    }

    
}