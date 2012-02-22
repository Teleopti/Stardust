using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class BlockDayOffOptimizerTest
    {
        private IBlockDayOffOptimizer _target;
        private MockRepository _mocks;
        private IScheduleMatrixLockableBitArrayConverter _converter;
        private IDayOffDecisionMaker _decisionMaker;
        private IScheduleResultDataExtractor _scheduleResultDataExtractor;
        private IDayOffDecisionMakerExecuter _dayOffDecisionMakerExecuter;
        private IBlockSchedulingService _blockSchedulingService;
        private IScheduleMatrixPro _matrix;
        private IScheduleMatrixOriginalStateContainer _originalStateContainer;
        private IDaysOffPreferences _daysOffPreferences;
        private ILockableBitArray _originalArray;
        private ILockableBitArray _workingArray;
        private List<double?> _values;
        private IBlockOptimizerBlockCleaner _blockCleaner;
        private ILockableBitArrayChangesTracker _changesTracker;
        private IResourceOptimizationHelper _resourceOptimizationHelper;


        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _converter = _mocks.StrictMock<IScheduleMatrixLockableBitArrayConverter>();
            _decisionMaker  = _mocks.StrictMock<IDayOffDecisionMaker>();
            _scheduleResultDataExtractor = _mocks.StrictMock<IScheduleResultDataExtractor>();
            _dayOffDecisionMakerExecuter = _mocks.StrictMock<IDayOffDecisionMakerExecuter>();
            _blockSchedulingService = _mocks.StrictMock<IBlockSchedulingService>();
            _daysOffPreferences = new DaysOffPreferences();
            _matrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _changesTracker = _mocks.StrictMock<ILockableBitArrayChangesTracker>();
            _blockCleaner = _mocks.StrictMock<IBlockOptimizerBlockCleaner>();
            _resourceOptimizationHelper = _mocks.StrictMock<IResourceOptimizationHelper>();
            _target = new BlockDayOffOptimizer(_converter, _scheduleResultDataExtractor, _daysOffPreferences, _dayOffDecisionMakerExecuter, _blockSchedulingService, _blockCleaner, _changesTracker, _resourceOptimizationHelper);
            
            _originalStateContainer = _mocks.StrictMock<IScheduleMatrixOriginalStateContainer>();
            _originalArray = new LockableBitArray(14, _daysOffPreferences.ConsiderWeekBefore, _daysOffPreferences.ConsiderWeekAfter, null);
            _workingArray = new LockableBitArray(14, _daysOffPreferences.ConsiderWeekBefore, _daysOffPreferences.ConsiderWeekAfter, null);
            _values = new List<double?>();
        }

        [Test]
        public void ShouldReturnFalseIfDecisionMakerFails()
        {
            using(_mocks.Record())
            {
                Expect.Call(_matrix.Person).Return(PersonFactory.CreatePerson());
                Expect.Call(_converter.Convert(_daysOffPreferences.ConsiderWeekBefore, _daysOffPreferences.ConsiderWeekAfter)).Return(
                    _originalArray);
                Expect.Call(_converter.Convert(_daysOffPreferences.ConsiderWeekBefore, _daysOffPreferences.ConsiderWeekAfter)).Return(
                    _workingArray);
                Expect.Call(_scheduleResultDataExtractor.Values()).Return(_values);
                Expect.Call(_decisionMaker.Execute(_workingArray, _values)).Return(false);
            }

            using(_mocks.Playback())
            {
                Assert.IsFalse(_target.Execute(_matrix, _originalStateContainer, _decisionMaker));
            }
        }

        [Test]
        public void ShouldReturnFalseIfMoveDaysOffFails()
        {
            using (_mocks.Record())
            {
                Expect.Call(_matrix.Person).Return(PersonFactory.CreatePerson());
                Expect.Call(_converter.Convert(_daysOffPreferences.ConsiderWeekBefore, _daysOffPreferences.ConsiderWeekAfter)).Return(
                    _originalArray);
                Expect.Call(_converter.Convert(_daysOffPreferences.ConsiderWeekBefore, _daysOffPreferences.ConsiderWeekAfter)).Return(
                    _workingArray);
                Expect.Call(_scheduleResultDataExtractor.Values()).Return(_values);
                Expect.Call(_decisionMaker.Execute(_workingArray, _values)).Return(true);
                Expect.Call(_dayOffDecisionMakerExecuter.Execute(_workingArray, _originalArray, _matrix,
                                                                 _originalStateContainer, false, false)).Return(false);
            }

            using (_mocks.Playback())
            {
                Assert.IsFalse(_target.Execute(_matrix, _originalStateContainer, _decisionMaker));
            }
        }

        [Test]
        public void ShouldReturnFalseIfBlockSchedulingAfterCleaningBlockFails()
        {
            IList<DateOnly> dates = new List<DateOnly>{new DateOnly(2000, 1, 1)};
            using (_mocks.Record())
            {
                Expect.Call(_matrix.Person).Return(PersonFactory.CreatePerson());
                Expect.Call(_converter.Convert(_daysOffPreferences.ConsiderWeekBefore, _daysOffPreferences.ConsiderWeekAfter)).Return(
                    _originalArray);
                Expect.Call(_converter.Convert(_daysOffPreferences.ConsiderWeekBefore, _daysOffPreferences.ConsiderWeekAfter)).Return(
                    _workingArray);
                Expect.Call(_scheduleResultDataExtractor.Values()).Return(_values);
                Expect.Call(_decisionMaker.Execute(_workingArray, _values)).Return(true);
                
                Expect.Call(_dayOffDecisionMakerExecuter.Execute(_workingArray, _originalArray, _matrix,
                                                                 _originalStateContainer, false, false)).Return(true);
                Expect.Call(_blockSchedulingService.Execute(new List<IScheduleMatrixPro> {_matrix})).Return(false);
                Expect.Call(_changesTracker.DaysOffRemoved(_workingArray, _originalArray, _matrix,
                                                           _daysOffPreferences.ConsiderWeekBefore)).Return(dates);
                Expect.Call(_blockCleaner.ClearSchedules(_matrix, dates)).Return(dates);
                Expect.Call(() =>_resourceOptimizationHelper.ResourceCalculateDate(dates[0], true, true));
                Expect.Call(_blockSchedulingService.Execute(new List<IScheduleMatrixPro> { _matrix })).Return(false);
            }

            using (_mocks.Playback())
            {
                Assert.IsFalse(_target.Execute(_matrix, _originalStateContainer, _decisionMaker));
            }
        }

        [Test]
        public void ShouldReturnTrueIfEverythingIsOk()
        {
            using (_mocks.Record())
            {
                Expect.Call(_matrix.Person).Return(PersonFactory.CreatePerson());
                Expect.Call(_converter.Convert(_daysOffPreferences.ConsiderWeekBefore, _daysOffPreferences.ConsiderWeekAfter)).Return(
                    _originalArray);
                Expect.Call(_converter.Convert(_daysOffPreferences.ConsiderWeekBefore, _daysOffPreferences.ConsiderWeekAfter)).Return(
                    _workingArray);
                Expect.Call(_scheduleResultDataExtractor.Values()).Return(_values);
                Expect.Call(_decisionMaker.Execute(_workingArray, _values)).Return(true);
                Expect.Call(_dayOffDecisionMakerExecuter.Execute(_workingArray, _originalArray, _matrix,
                                                                 _originalStateContainer, false, false)).Return(true);
                Expect.Call(_blockSchedulingService.Execute(new List<IScheduleMatrixPro> { _matrix })).Return(true);
            }

            using (_mocks.Playback())
            {
                Assert.IsTrue(_target.Execute(_matrix, _originalStateContainer, _decisionMaker));
            }
        }
    }

    

}