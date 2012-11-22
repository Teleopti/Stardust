using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class BlockDayOffOptimizationServiceTest
    {
        private BlockDayOffOptimizationService _target;
        private MockRepository _mocks;
        private IPeriodValueCalculator _periodValueCalculator;
        private IEnumerable<IBlockDayOffOptimizerContainer> _optimizers;
        private IBlockDayOffOptimizerContainer _container1;
        private IBlockDayOffOptimizerContainer _container2;
        private ISchedulePartModifyAndRollbackService _rollbackService;
    	private ISchedulingOptions _schedulingOptions;
        private IDaysOffPreferences _daysOffPreferences;
        private IScheduleMatrixPro _matrix;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _periodValueCalculator = _mocks.StrictMock<IPeriodValueCalculator>();
            _container1 = _mocks.StrictMock<IBlockDayOffOptimizerContainer>();
            _container2 = _mocks.StrictMock<IBlockDayOffOptimizerContainer>();
            _rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_schedulingOptions = new SchedulingOptions();
            _daysOffPreferences = _mocks.StrictMock<IDaysOffPreferences>();
            _matrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _target = new BlockDayOffOptimizationService(_periodValueCalculator, _rollbackService, _daysOffPreferences);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), Test]
        public void VerifySuccessfulOptimization()
        {
            //_target.ReportProgress += _target_ReportProgress;
            _optimizers = new List<IBlockDayOffOptimizerContainer> { _container1, _container2 };
            IPerson owner = PersonFactory.CreatePerson();
            var scheduleDay1 = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDay2 = _mocks.StrictMock<IScheduleDayPro>();
            var part = _mocks.StrictMock<IScheduleDay>();
            IList<IScheduleDayPro> scheduleDayProList = new List<IScheduleDayPro>
                                                       {
                                                           scheduleDay1,
                                                           scheduleDay2
                                                       };
            ILockableBitArray bitArrayBeforeMove = new LockableBitArray(2, false, false, null) { PeriodArea = new MinMax<int>(0, 1) };
            bitArrayBeforeMove.Set(0, true);
            ILockableBitArray bitArrayAfterMove = new LockableBitArray(2, false, false, null) { PeriodArea = new MinMax<int>(0, 1) };
            bitArrayAfterMove.Set(1, true);
            var dateOnly = new DateOnly(new DateTime(2012, 11, 22));
            using (_mocks.Record())
            {
                // first round
                Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_container1.Execute(_schedulingOptions))
                    .Return(true);
                Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_container2.Execute(_schedulingOptions))
                    .Return(true);
                Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_container1.Execute(_schedulingOptions))
                    .Return(false);
                Expect.Call(() => _rollbackService.Rollback());
                Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_container2.Execute(_schedulingOptions))
                    .Return(false);
                Expect.Call(() => _rollbackService.Rollback());

                // second round
                Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_container1.Execute(_schedulingOptions))
                    .Return(true);
                Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_container2.Execute(_schedulingOptions))
                    .Return(true);
                Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_container1.Execute(_schedulingOptions))
                    .Return(false);
                Expect.Call(() => _rollbackService.Rollback());
                Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_container2.Execute(_schedulingOptions))
                    .Return(false);
                Expect.Call(() => _rollbackService.Rollback());


                Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization))
                    .Return(10).Repeat.AtLeastOnce();
                Expect.Call(_container1.Owner)
                    .Return(owner).Repeat.AtLeastOnce();
                Expect.Call(_container2.Owner)
                    .Return(owner).Repeat.AtLeastOnce();

                Expect.Call(_daysOffPreferences.ConsiderWeekBefore).Return(true).Repeat.AtLeastOnce();
                Expect.Call(_daysOffPreferences.ConsiderWeekAfter).Return(true).Repeat.AtLeastOnce();
                Expect.Call(_container1.Matrix).Return(_matrix).Repeat.AtLeastOnce();
                Expect.Call(_container2.Matrix).Return(_matrix).Repeat.AtLeastOnce();
                Expect.Call(_matrix.OuterWeeksPeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(scheduleDayProList)).Repeat.AtLeastOnce();
                Expect.Call(scheduleDay1.Day).Return(dateOnly).Repeat.AtLeastOnce();
                Expect.Call(scheduleDay1.DaySchedulePart())
                    .Return(part).Repeat.AtLeastOnce();
                Expect.Call(scheduleDay2.DaySchedulePart())
                    .Return(part).Repeat.AtLeastOnce();
                Expect.Call(_matrix.EffectivePeriodDays).Return(
                    new ReadOnlyCollection<IScheduleDayPro>(scheduleDayProList)).Repeat.AtLeastOnce();
                Expect.Call(part.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.AtLeastOnce();
                Expect.Call(_matrix.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>())).Repeat.AtLeastOnce();
                Expect.Call(_container1.WorkingBitArray).Return(bitArrayAfterMove).Repeat.AtLeastOnce();
                Expect.Call(_container2.WorkingBitArray).Return(bitArrayAfterMove).Repeat.AtLeastOnce();
                Expect.Call(() => _matrix.LockPeriod(new DateOnlyPeriod(dateOnly, dateOnly))).Repeat.AtLeastOnce();
            }

            using (_mocks.Playback())
            {
				_target.Execute(_optimizers, _schedulingOptions);
            }
        }

        [Test]
        public void VerifyCancel()
        {
            _target.ReportProgress += _target_ReportProgress;
            _optimizers = new List<IBlockDayOffOptimizerContainer> { _container1 };
            IPerson owner = PersonFactory.CreatePerson();

            var scheduleDay1 = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDay2 = _mocks.StrictMock<IScheduleDayPro>();
            var part = _mocks.StrictMock<IScheduleDay>();
            IList<IScheduleDayPro> scheduleDayProList = new List<IScheduleDayPro>
                                                       {
                                                           scheduleDay1,
                                                           scheduleDay2
                                                       };
            ILockableBitArray bitArrayBeforeMove = new LockableBitArray(2, false, false, null) { PeriodArea = new MinMax<int>(0, 1) };
            bitArrayBeforeMove.Set(0, true);
            ILockableBitArray bitArrayAfterMove = new LockableBitArray(2, false, false, null) { PeriodArea = new MinMax<int>(0, 1) };
            bitArrayAfterMove.Set(1, true);
            var dateOnly = new DateOnly(new DateTime(2012, 11, 22));
            using (_mocks.Record())
            {
                // only one round 
                Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_container1.Execute(_schedulingOptions))
                    .Return(true);

                Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization))
                    .Return(10).Repeat.AtLeastOnce();
                Expect.Call(_container1.Owner)
                    .Return(owner).Repeat.AtLeastOnce();

                Expect.Call(_daysOffPreferences.ConsiderWeekBefore).Return(true).Repeat.AtLeastOnce();
                Expect.Call(_daysOffPreferences.ConsiderWeekAfter).Return(true).Repeat.AtLeastOnce();
                Expect.Call(_container1.Matrix).Return(_matrix).Repeat.AtLeastOnce();
                Expect.Call(_matrix.OuterWeeksPeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(scheduleDayProList)).Repeat.AtLeastOnce();
                Expect.Call(scheduleDay1.Day).Return(dateOnly).Repeat.AtLeastOnce();
                Expect.Call(scheduleDay1.DaySchedulePart())
                    .Return(part).Repeat.AtLeastOnce();
                Expect.Call(scheduleDay2.DaySchedulePart())
                    .Return(part).Repeat.AtLeastOnce();
                Expect.Call(_matrix.EffectivePeriodDays).Return(
                    new ReadOnlyCollection<IScheduleDayPro>(scheduleDayProList)).Repeat.AtLeastOnce();
                Expect.Call(part.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.AtLeastOnce();
                Expect.Call(_matrix.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>())).Repeat.AtLeastOnce();
                Expect.Call(_container1.WorkingBitArray).Return(bitArrayAfterMove).Repeat.AtLeastOnce();
                Expect.Call(() => _matrix.LockPeriod(new DateOnlyPeriod(dateOnly, dateOnly))).Repeat.AtLeastOnce();
            }

            using (_mocks.Playback())
            {
				_target.Execute(_optimizers, _schedulingOptions);
            }
            _target.ReportProgress -= _target_ReportProgress;
        }

        static void _target_ReportProgress(object sender, ResourceOptimizerProgressEventArgs e)
        {
            e.Cancel = true;
        }

    }
}