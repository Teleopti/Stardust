using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;


namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class WorkShiftBackToLegalStateStepTest
    {
        private IWorkShiftBackToLegalStateStep _target;
        private int _weekIndex;
        private MockRepository _mockRepository;
        private IWorkShiftBackToLegalStateBitArrayCreator _bitArrayCreator;
        private IScheduleMatrixPro _scheduleMatrix;
        private ILockableBitArray _lockableBitArray;
        private IWorkShiftBackToLegalStateDecisionMaker _decisionMaker;
        private IScheduleDayPro _day1;
        private IDeleteSchedulePartService _deleteService;
        private IScheduleDay _scheduleDay;
        private IList<IScheduleDay> _scheduleDayList;
        private ISchedulePartModifyAndRollbackService _modifyAndRollbackService;
    	private DateOnlyPeriod _period;
    	private IDateOnlyAsDateTimePeriod _dateOnlyAsDateTimePeriod;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();

            _scheduleMatrix = _mockRepository.StrictMock<IScheduleMatrixPro>();
            _lockableBitArray = _mockRepository.StrictMock<ILockableBitArray>();
            _bitArrayCreator = _mockRepository.StrictMock<IWorkShiftBackToLegalStateBitArrayCreator>();
            _decisionMaker = _mockRepository.StrictMock<IWorkShiftBackToLegalStateDecisionMaker>();
            _day1 = _mockRepository.StrictMock<IScheduleDayPro>();
            _deleteService = _mockRepository.StrictMock<IDeleteSchedulePartService>();
            _scheduleDay = _mockRepository.StrictMock<IScheduleDay>();
            _scheduleDayList = new List<IScheduleDay>{_scheduleDay};
            _modifyAndRollbackService = _mockRepository.StrictMock<ISchedulePartModifyAndRollbackService>();
			_period = new DateOnlyPeriod(DateOnly.MaxValue, DateOnly.MaxValue);
            _target = new WorkShiftBackToLegalStateStep(_bitArrayCreator, _decisionMaker, _deleteService);
        	_dateOnlyAsDateTimePeriod = _mockRepository.StrictMock<IDateOnlyAsDateTimePeriod>();
        }

        [Test]
        public void VerifyExecuteWeekStep()
        {
            _weekIndex = 0;
            using(_mockRepository.Record())
            {
                Expect.Call(_bitArrayCreator.CreateWeeklyBitArray(_weekIndex, _scheduleMatrix)).Return(_lockableBitArray);
				Expect.Call(_decisionMaker.Execute(_lockableBitArray, false, _period)).Return(0);
                Expect.Call(_scheduleMatrix.FullWeeksPeriodDays).Return(
                    new [] {_day1}).Repeat.AtLeastOnce();
                Expect.Call(_day1.DaySchedulePart()).Return(_scheduleDay);
                Expect.Call(_day1.Day).Return(DateOnly.MaxValue).Repeat.AtLeastOnce();
            	Expect.Call(_scheduleDay.Clone()).Return(_scheduleDay);
                _deleteService.Delete(_scheduleDayList, _modifyAndRollbackService);
            	Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
            	Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(DateOnly.MaxValue);
            }
            using(_mockRepository.Playback())
            {
				IScheduleDay result = _target.ExecuteWeekStep(_weekIndex, _scheduleMatrix, _modifyAndRollbackService);
                Assert.AreEqual(DateOnly.MaxValue, result.DateOnlyAsPeriod.DateOnly);
            }
        }

        [Test]
        public void VerifyExecutePeriodStepWithRaise()
        {
            const bool raise = true;
            using (_mockRepository.Record())
            {
                Expect.Call(_bitArrayCreator.CreatePeriodBitArray(_scheduleMatrix)).Return(_lockableBitArray);
				Expect.Call(_decisionMaker.Execute(_lockableBitArray, raise, _period)).Return(0);
                Expect.Call(_scheduleMatrix.FullWeeksPeriodDays).Return(
                    new [] { _day1 }).Repeat.AtLeastOnce();
                Expect.Call(_day1.DaySchedulePart()).Return(_scheduleDay);
                Expect.Call(_day1.Day).Return(DateOnly.MaxValue).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay.Clone()).Return(_scheduleDay);
                _deleteService.Delete(_scheduleDayList, _modifyAndRollbackService);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(DateOnly.MaxValue);
            }
            using (_mockRepository.Playback())
            {
				IScheduleDay result = _target.ExecutePeriodStep(raise, _scheduleMatrix, _modifyAndRollbackService);
                Assert.AreEqual(DateOnly.MaxValue, result.DateOnlyAsPeriod.DateOnly);
            }
        }

        [Test]
        public void VerifyExecutePeriodStepWithReduce()
        {
            const bool raise = false;
            using (_mockRepository.Record())
            {
                Expect.Call(_bitArrayCreator.CreatePeriodBitArray(_scheduleMatrix)).Return(_lockableBitArray);
				Expect.Call(_decisionMaker.Execute(_lockableBitArray, raise, _period)).Return(0);
                Expect.Call(_scheduleMatrix.FullWeeksPeriodDays).Return(
                    new [] { _day1 }).Repeat.AtLeastOnce();
                Expect.Call(_day1.DaySchedulePart()).Return(_scheduleDay);
            	Expect.Call(_day1.Day).Return(DateOnly.MaxValue).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay.Clone()).Return(_scheduleDay);
                _deleteService.Delete(_scheduleDayList, _modifyAndRollbackService);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(DateOnly.MaxValue);
            }
            using (_mockRepository.Playback())
            {
				IScheduleDay result = _target.ExecutePeriodStep(raise, _scheduleMatrix, _modifyAndRollbackService);
                Assert.AreEqual(DateOnly.MaxValue, result.DateOnlyAsPeriod.DateOnly);
            }
        }
    }
}
