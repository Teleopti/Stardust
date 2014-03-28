using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Secret;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class DayOffOptimizerTest
    {
        private DayOffOptimizer _target;
        private MockRepository _mocks;
        private IScheduleMatrixLockableBitArrayConverter _converter;
        private IDayOffDecisionMaker _decisionMaker;
        private IScheduleResultDataExtractor _dataExtractor;
        private IDaysOffPreferences _daysOffPreferences;
        private IScheduleMatrixPro _scheduleMatrix;
        private IScheduleMatrixOriginalStateContainer _originalStateContainer;
        private IDayOffDecisionMakerExecuter _dayOffDecisionMakerExecuter;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _converter = _mocks.StrictMock<IScheduleMatrixLockableBitArrayConverter>();
            _decisionMaker = _mocks.StrictMock<IDayOffDecisionMaker>();
            _dataExtractor = _mocks.StrictMock<IScheduleResultDataExtractor>();
            _daysOffPreferences = new DaysOffPreferences();
            _scheduleMatrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _originalStateContainer = _mocks.StrictMock<IScheduleMatrixOriginalStateContainer>();
            _dayOffDecisionMakerExecuter = _mocks.StrictMock<IDayOffDecisionMakerExecuter>();
        }

        [Test]
        public void VerifyExecuteReturnsTrue()
        {
            ILockableBitArray bitArray = new LockableBitArray(2, false, false, null);
            bitArray.PeriodArea = new MinMax<int>(0, 1);
            bitArray.Set(0, true);

            using (_mocks.Record())
            {
                Expect.Call(_decisionMaker.Execute(null, null)).IgnoreArguments().Return(true).Repeat.AtLeastOnce();
                Expect.Call(_dayOffDecisionMakerExecuter.Execute(null, null, _scheduleMatrix, _originalStateContainer, true, true, true)).IgnoreArguments().Return(true).Repeat.Once();

                // not essetial to the test logic
                Expect.Call(_scheduleMatrix.Person).Return(new Person()).Repeat.AtLeastOnce();
                Expect.Call(_converter.Convert(false, false)).Return(bitArray).Repeat.AtLeastOnce();
                Expect.Call(_dataExtractor.Values()).Return(new List<double?> { 1, 2 }).Repeat.Once();
            }
            using (_mocks.Playback())
            {
                _target = createTarget();
                _target.Execute(_scheduleMatrix, _originalStateContainer);
            }
        }

        private DayOffOptimizer createTarget()
        {
            return new DayOffOptimizer(_converter,
                                      _decisionMaker,
                                      _dataExtractor,
                                      _daysOffPreferences,
                                      _dayOffDecisionMakerExecuter);
        }
    }


}
