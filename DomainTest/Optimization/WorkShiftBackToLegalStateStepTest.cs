using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

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
            _target = new WorkShiftBackToLegalStateStep(_bitArrayCreator, _decisionMaker, _deleteService, _modifyAndRollbackService);

        }

        [Test]
        public void VerifyExecuteWeekStep()
        {
            _weekIndex = 0;
            using(_mockRepository.Record())
            {
                Expect.Call(_bitArrayCreator.CreateWeeklyBitArray(_weekIndex, _scheduleMatrix)).Return(_lockableBitArray);
                Expect.Call(_decisionMaker.Execute(_lockableBitArray, false)).Return(0);
                Expect.Call(_scheduleMatrix.FullWeeksPeriodDays).Return(
                    new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> {_day1}));
                Expect.Call(_day1.DaySchedulePart()).Return(_scheduleDay);
                Expect.Call(_day1.Day).Return(DateOnly.MaxValue);
                _deleteService.Delete(_scheduleDayList, _modifyAndRollbackService);
            }
            using(_mockRepository.Playback())
            {
                DateOnly? result = _target.ExecuteWeekStep(_weekIndex, _scheduleMatrix);
                Assert.AreEqual(DateOnly.MaxValue, result.Value);
            }
        }

        [Test]
        public void VerifyExecutePeriodStepWithRaise()
        {
            const bool raise = true;
            using (_mockRepository.Record())
            {
                Expect.Call(_bitArrayCreator.CreatePeriodBitArray(raise, _scheduleMatrix)).Return(_lockableBitArray);
                Expect.Call(_decisionMaker.Execute(_lockableBitArray, raise)).Return(0);
                Expect.Call(_scheduleMatrix.FullWeeksPeriodDays).Return(
                    new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _day1 }));
                Expect.Call(_day1.DaySchedulePart()).Return(_scheduleDay);
                Expect.Call(_day1.Day).Return(DateOnly.MaxValue);
                _deleteService.Delete(_scheduleDayList, _modifyAndRollbackService);
            }
            using (_mockRepository.Playback())
            {
                DateOnly? result = _target.ExecutePeriodStep(raise, _scheduleMatrix);
                Assert.AreEqual(DateOnly.MaxValue, result.Value);
            }
        }

        [Test]
        public void VerifyExecutePeriodStepWithReduce()
        {
            const bool raise = false;
            using (_mockRepository.Record())
            {
                Expect.Call(_bitArrayCreator.CreatePeriodBitArray(raise, _scheduleMatrix)).Return(_lockableBitArray);
                Expect.Call(_decisionMaker.Execute(_lockableBitArray, raise)).Return(0);
                Expect.Call(_scheduleMatrix.FullWeeksPeriodDays).Return(
                    new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _day1 }));
                Expect.Call(_day1.DaySchedulePart()).Return(_scheduleDay);
                Expect.Call(_day1.Day).Return(DateOnly.MaxValue);
                _deleteService.Delete(_scheduleDayList, _modifyAndRollbackService);
            }
            using (_mockRepository.Playback())
            {
                DateOnly? result = _target.ExecutePeriodStep(raise, _scheduleMatrix);
                Assert.AreEqual(DateOnly.MaxValue, result.Value);
            }
        }
    }
}
