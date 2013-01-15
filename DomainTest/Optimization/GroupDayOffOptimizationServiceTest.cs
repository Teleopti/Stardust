using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class GroupDayOffOptimizationServiceTest
    {
        private IGroupDayOffOptimizationService _target;
        private MockRepository _mocks;
        private IPeriodValueCalculator _periodValueCalculator;
        private IEnumerable<IGroupDayOffOptimizerContainer> _optimizers;
        private IGroupDayOffOptimizerContainer _container1;
        private IGroupDayOffOptimizerContainer _container2;
        private ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
        private IScheduleDay _scheduleDay;
        private IDateOnlyAsDateTimePeriod _dateOnlyAsDateTimePeriod;
    	private IGroupOptimizerFindMatrixesForGroup _groupOptimizerFindMatrixesForGroup;
    	private IScheduleMatrixPro _matrix1;
		private IScheduleMatrixPro _matrix2;
    	private IVirtualSchedulePeriod _virtualSchedulePeriod1;
		private IVirtualSchedulePeriod _virtualSchedulePeriod2;
    	private IScheduleDayPro _scheduleDayPro;
		private IDaysOffPreferences _daysOffPreferences;
    	private IGroupDayOffOptimizationResourceHelper _groupDayOffOptimizationResourceHelper;
        
		[SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _periodValueCalculator = _mocks.StrictMock<IPeriodValueCalculator>();
            _container1 = _mocks.StrictMock<IGroupDayOffOptimizerContainer>();
            _container2 = _mocks.StrictMock<IGroupDayOffOptimizerContainer>();
            _schedulePartModifyAndRollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
            _scheduleDay = _mocks.StrictMock<IScheduleDay>();
            _dateOnlyAsDateTimePeriod = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();
        	_groupOptimizerFindMatrixesForGroup = _mocks.StrictMock<IGroupOptimizerFindMatrixesForGroup>();
        	_matrix1 = _mocks.StrictMock<IScheduleMatrixPro>();
			_matrix2 = _mocks.StrictMock<IScheduleMatrixPro>();
        	_virtualSchedulePeriod1 = _mocks.StrictMock<IVirtualSchedulePeriod>();
			_virtualSchedulePeriod2 = _mocks.StrictMock<IVirtualSchedulePeriod>();
 			_daysOffPreferences = _mocks.StrictMock<IDaysOffPreferences>();
			_groupDayOffOptimizationResourceHelper = _mocks.StrictMock<IGroupDayOffOptimizationResourceHelper>();
			_target = new GroupDayOffOptimizationService(_periodValueCalculator, _schedulePartModifyAndRollbackService, _groupOptimizerFindMatrixesForGroup, _daysOffPreferences, _groupDayOffOptimizationResourceHelper);
        	_scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
        }

		//[Test]
		//public void VerifySuccessfulOptimization()
		//{
		//    //_target.ReportProgress += _target_ReportProgress;
		//    _optimizers = new List<IGroupDayOffOptimizerContainer> { _container1, _container2 };
		//    IPerson owner1 = PersonFactory.CreatePerson("p1");
		//    IPerson owner2 = PersonFactory.CreatePerson("p2");
            
		//    using (_mocks.Record())
		//    {
		//        Expect.Call(() => _schedulePartModifyAndRollbackService.ClearModificationCollection()).Repeat.Times(4);
		//        // first round 
		//        Expect.Call(_container1.Execute())
		//            .Return(true),RepeatAttribute;
		//        Expect.Call(_container2.Execute())
		//            .Return(true).Repeat.Any();
		//        Expect.Call(_container1.Execute())
		//            .Return(false).Repeat.Any();
		//        Expect.Call(_container2.Execute())
		//            .Return(false).Repeat.Any();

		//        // second round 
		//        //Expect.Call(_container1.Execute())
		//        //    .Return(true);
		//        //Expect.Call(_container2.Execute())
		//        //    .Return(true);
		//        //Expect.Call(_container1.Execute())
		//        //    .Return(false);
		//        //Expect.Call(_container2.Execute())
		//        //    .Return(false);

		//        Expect.Call(_schedulePartModifyAndRollbackService.ModificationCollection).Return(
		//        new ReadOnlyCollection<IScheduleDay>(new List<IScheduleDay> { _scheduleDay })).Repeat.AtLeastOnce();
		//        Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod).Repeat.Times(4);
		//        Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(new DateOnly(2011, 1, 1)).Repeat.Times(4);
		//        Expect.Call(() => _schedulePartModifyAndRollbackService.Rollback()).Repeat.Times(4);
		//        Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2011, 1, 1), false, false)).Repeat.AtLeastOnce();
                
		//        Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization))
		//            .Return(10).Repeat.AtLeastOnce();
		//        Expect.Call(_container1.Owner)
		//            .Return(owner1).Repeat.AtLeastOnce();
		//        Expect.Call(_container2.Owner)
		//            .Return(owner2).Repeat.AtLeastOnce();
		//        Expect.Call(() => _schedulePartModifyAndRollbackService.ClearModificationCollection()).Repeat.Times(4);
		//        Expect.Call(_groupOptimizerFindMatrixesForGroup.Find(owner1, new DateOnly(2011, 1, 1))).Return(
		//            new List<IScheduleMatrixPro> {_matrix1, _matrix2}).Repeat.Any();
		//        Expect.Call(_groupOptimizerFindMatrixesForGroup.Find(owner2, new DateOnly(2011, 1, 1))).Return(
		//            new List<IScheduleMatrixPro> { _matrix1, _matrix2 }).Repeat.Any();
		//        Expect.Call(_matrix1.SchedulePeriod).Return(_virtualSchedulePeriod1).Repeat.Any();
		//        Expect.Call(_matrix2.SchedulePeriod).Return(_virtualSchedulePeriod2).Repeat.Any();
		//        Expect.Call(_container1.Matrix).Return(_matrix1).Repeat.Any();
		//        Expect.Call(_container2.Matrix).Return(_matrix2).Repeat.Any();
		//        //Expect.Call(_groupOptimizerFindMatrixesForGroup.Find(owner, new DateOnly(2011, 1, 1))).Return(
		//        //    new List<IScheduleMatrixPro> { _container2.Matrix });
		//    }

		//    using (_mocks.Playback())
		//    {
		//        _target.Execute(_optimizers, true);
		//    }
		//    //_target.ReportProgress -= _target_ReportProgress;
		//}

        [Test]
        public void VerifyCancel()
        {
            _target.ReportProgress += _target_ReportProgress;
  			_optimizers = new List<IGroupDayOffOptimizerContainer> { _container1 };
            IPerson owner = PersonFactory.CreatePerson();
            var scheduleDay1 = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDay2 = _mocks.StrictMock<IScheduleDayPro>();
            IList<IScheduleDayPro> scheduleDayProList = new List<IScheduleDayPro>
                                                       {
                                                           scheduleDay1,
                                                           scheduleDay2
                                                       };
            var dateOnly = new DateOnly(new DateTime(2012, 11, 22));
            var part = _mocks.StrictMock<IScheduleDay>();
            ILockableBitArray bitArrayBeforeMove = new LockableBitArray(2, false, false, null) { PeriodArea = new MinMax<int>(0, 1) };
            bitArrayBeforeMove.Set(0, true);
            ILockableBitArray bitArrayAfterMove = new LockableBitArray(2, false, false, null) { PeriodArea = new MinMax<int>(0, 1) };
            bitArrayAfterMove.Set(1, true);

            using (_mocks.Record())
            {
                Expect.Call(() => _schedulePartModifyAndRollbackService.ClearModificationCollection());
                // only one round 
                Expect.Call(_container1.Execute())
                    .Return(true);

                Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization))
                    .Return(10).Repeat.AtLeastOnce();
                Expect.Call(_container1.Owner)
                    .Return(owner).Repeat.AtLeastOnce();
            	Expect.Call(_container1.Matrix).Return(_matrix1).Repeat.AtLeastOnce();
            	Expect.Call(_matrix1.EffectivePeriodDays).Return(
					new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro })).Repeat.AtLeastOnce();
            	Expect.Call(_scheduleDayPro.Day).Return(DateOnly.MinValue).Repeat.AtLeastOnce();
            	Expect.Call(_groupOptimizerFindMatrixesForGroup.Find(owner, DateOnly.MinValue)).Return(new List<IScheduleMatrixPro>{_matrix1});
            	Expect.Call(_matrix1.SchedulePeriod).Return(_virtualSchedulePeriod1).Repeat.Twice();
            	Expect.Call(_schedulePartModifyAndRollbackService.ModificationCollection).Return(new List<IScheduleDay>());
                Expect.Call(() => _schedulePartModifyAndRollbackService.ClearModificationCollection());
 				
				Expect.Call(scheduleDay1.DaySchedulePart())
                    .Return(part).Repeat.AtLeastOnce();
                Expect.Call(scheduleDay1.Day).Return(dateOnly);
                Expect.Call(scheduleDay2.DaySchedulePart())
                    .Return(part).Repeat.AtLeastOnce();
                Expect.Call(_matrix1.OuterWeeksPeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(scheduleDayProList)).Repeat.AtLeastOnce();
                Expect.Call(part.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.AtLeastOnce();
                Expect.Call(_matrix1.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>())).Repeat.AtLeastOnce();
                Expect.Call(_container1.WorkingBitArray).Return(bitArrayAfterMove);
                Expect.Call(() => _matrix1.LockPeriod(new DateOnlyPeriod(dateOnly, dateOnly))).Repeat.AtLeastOnce();
                Expect.Call(_daysOffPreferences.ConsiderWeekBefore).Return(true).Repeat.AtLeastOnce();
                Expect.Call(_daysOffPreferences.ConsiderWeekAfter).Return(true).Repeat.AtLeastOnce();
            }

            using (_mocks.Playback())
            {
                _target.Execute(_optimizers, true);
            }
            _target.ReportProgress -= _target_ReportProgress;
        }

        [Test]
        public void ShouldRollbackAndAddToListIfPeriodValueHigher()
        {
            _optimizers = new List<IGroupDayOffOptimizerContainer> { _container1 };
            IPerson owner = PersonFactory.CreatePerson();
          	var scheduleDay1 = _mocks.StrictMock<IScheduleDayPro>();
            var scheduleDay2 = _mocks.StrictMock<IScheduleDayPro>();
            IList<IScheduleDayPro> scheduleDayProList = new List<IScheduleDayPro>
                                                       {
                                                           scheduleDay1,
                                                           scheduleDay2
                                                       };
            var part = _mocks.StrictMock<IScheduleDay>();
            ILockableBitArray bitArrayBeforeMove = new LockableBitArray(2, false, false, null) { PeriodArea = new MinMax<int>(0, 1) };
            bitArrayBeforeMove.Set(0, true);
            ILockableBitArray bitArrayAfterMove = new LockableBitArray(2, false, false, null) { PeriodArea = new MinMax<int>(0, 1) };
            bitArrayAfterMove.Set(1, true);
            Expect.Call(() => _schedulePartModifyAndRollbackService.ClearModificationCollection()).Repeat.Times(2);
            Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(10);
            Expect.Call(_container1.Execute()).Return(true);
            //worse
            Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(11);
            Expect.Call(_schedulePartModifyAndRollbackService.ModificationCollection).Return(
                new ReadOnlyCollection<IScheduleDay>(new List<IScheduleDay> {_scheduleDay}));
            Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod).Repeat.AtLeastOnce();
            Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(new DateOnly(2011, 1, 1)).Repeat.AtLeastOnce();
            Expect.Call(() => _schedulePartModifyAndRollbackService.Rollback());
        	Expect.Call(_matrix1.GetScheduleDayByKey(new DateOnly(2011, 1, 1))).Return(_scheduleDayPro);
        	Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
        	Expect.Call(() =>_groupDayOffOptimizationResourceHelper.ResourceCalculateContainersToRemove(new List<IScheduleDay>{_scheduleDay}, new List<IScheduleDay>{_scheduleDay}));
            //Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2011, 1, 1), false, false));

            Expect.Call(scheduleDay1.DaySchedulePart())
                .Return(part).Repeat.AtLeastOnce();
            Expect.Call(scheduleDay2.DaySchedulePart())
                .Return(part).Repeat.AtLeastOnce();
            Expect.Call(_matrix1.OuterWeeksPeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(scheduleDayProList)).Repeat.AtLeastOnce();
            Expect.Call(_matrix1.EffectivePeriodDays).Return(
                new ReadOnlyCollection<IScheduleDayPro>(scheduleDayProList)).Repeat.AtLeastOnce();
            Expect.Call(part.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.AtLeastOnce();
            Expect.Call(_matrix1.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>())).Repeat.AtLeastOnce();
            Expect.Call(_daysOffPreferences.ConsiderWeekBefore).Return(true).Repeat.AtLeastOnce();
            Expect.Call(_daysOffPreferences.ConsiderWeekAfter).Return(true).Repeat.AtLeastOnce();

			Expect.Call(_container1.Owner).Return(owner).Repeat.AtLeastOnce();
			//Expect.Call(() => _schedulePartModifyAndRollbackService.ClearModificationCollection()).Repeat.Times(2);
        	Expect.Call(_groupOptimizerFindMatrixesForGroup.Find(owner, new DateOnly(2011, 1, 1))).Return(
        		new List<IScheduleMatrixPro> {_matrix1, _matrix2}).Repeat.Any();
        	Expect.Call(_matrix1.SchedulePeriod).Return(_virtualSchedulePeriod1).Repeat.Any();
			Expect.Call(_matrix2.SchedulePeriod).Return(_virtualSchedulePeriod2).Repeat.Any();
			Expect.Call(_container1.Matrix).Return(_matrix1).Repeat.Any();
			Expect.Call(_container2.Matrix).Return(_matrix2).Repeat.Any();

            _mocks.ReplayAll();

            _target.Execute(_optimizers, true);
            _mocks.VerifyAll();
        }

        static void _target_ReportProgress(object sender, ResourceOptimizerProgressEventArgs e)
        {
            e.Cancel = true;
        }

    }
}