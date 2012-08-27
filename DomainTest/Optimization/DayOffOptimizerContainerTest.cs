using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class DayOffOptimizerContainerTest
    {
        private DayOffOptimizerContainer _target;
        private MockRepository _mocks;
        private IScheduleMatrixLockableBitArrayConverter _converter;
        private IDayOffDecisionMaker _decisionMaker;
        private IScheduleResultDataExtractor _dataExtractor;
        private IDaysOffPreferences _daysOffPreferences;
        private IScheduleMatrixPro _matrix;
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
            _matrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _originalStateContainer = _mocks.StrictMock<IScheduleMatrixOriginalStateContainer>();
            _dayOffDecisionMakerExecuter = _mocks.StrictMock<IDayOffDecisionMakerExecuter>();
        }

        [Test]
        public void VerifyOwner()
        {

            IPerson owner = PersonFactory.CreatePerson();
            using (_mocks.Record())
            {
                Expect.Call(_matrix.Person).Return(owner);

            }
            using (_mocks.Playback())
            {
                _target = createTarget();
                Assert.AreEqual(owner, _target.Owner);
            }

        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [Test]
        public void VerifyExecuteFirstGroupSuccessful()
        {
            ILockableBitArray bitArrayBeforeMove = new LockableBitArray(2, false, false, null);
            bitArrayBeforeMove.PeriodArea = new MinMax<int>(0, 1);
            bitArrayBeforeMove.Set(0, true);
            ILockableBitArray bitArrayAfterMove = new LockableBitArray(2, false, false, null);
            bitArrayAfterMove.PeriodArea = new MinMax<int>(0, 1);
            bitArrayAfterMove.Set(1, true);

            using (_mocks.Record())
            {
                Expect.Call(_converter.Convert(false, false)).Return(bitArrayBeforeMove).Repeat.Times(2);
                Expect.Call(_dataExtractor.Values()).Return(new List<double?> { 1, 2 }).Repeat.Times(1);
                Expect.Call(_decisionMaker.Execute(bitArrayBeforeMove, new List<double?> { 1, 2 })).IgnoreArguments().Return(true).Repeat.Any();
                Expect.Call(_matrix.Person).Return(new Person()).Repeat.Any();
                Expect.Call(_dayOffDecisionMakerExecuter.Execute(bitArrayBeforeMove, bitArrayBeforeMove, _matrix, _originalStateContainer, true, true, true))
                    .Return(true)
                    .Repeat.AtLeastOnce();
            }

            bool result;

            using (_mocks.Playback())
            {
                _target = createTarget();
                result = _target.Execute();
            }

            Assert.IsTrue(result);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [Test]
        public void VerifyExecuteFirstTwoGroupUnsuccessfulThirdGroupSuccessful()
        {
            ILockableBitArray bitArrayBeforeMove = new LockableBitArray(2, false, false, null);
            bitArrayBeforeMove.PeriodArea = new MinMax<int>(0, 1);
            bitArrayBeforeMove.Set(0, true);
            ILockableBitArray bitArrayAfterMove = new LockableBitArray(2, false, false, null);
            bitArrayAfterMove.PeriodArea = new MinMax<int>(0, 1);
            bitArrayAfterMove.Set(1, true);

            using (_mocks.Record())
            {
                Expect.Call(_converter.Convert(false, false)).Return(bitArrayBeforeMove).Repeat.Times(6);
                Expect.Call(_dataExtractor.Values()).Return(new List<double?> { 1, 2 }).Repeat.Times(3);
                Expect.Call(_decisionMaker.Execute(bitArrayBeforeMove, new List<double?> { 1, 2 })).IgnoreArguments().Return(false).Repeat.Once();
                Expect.Call(_decisionMaker.Execute(bitArrayBeforeMove, new List<double?> { 1, 2 })).IgnoreArguments().Return(false).Repeat.Once();
                Expect.Call(_decisionMaker.Execute(bitArrayBeforeMove, new List<double?> { 1, 2 })).IgnoreArguments().Return(true).Repeat.Once();
                Expect.Call(_matrix.Person).Return(new Person()).Repeat.Any();
                Expect.Call(_dayOffDecisionMakerExecuter.Execute(bitArrayBeforeMove, bitArrayBeforeMove, _matrix, _originalStateContainer, true, true, true))
                    .Return(true)
                    .Repeat.AtLeastOnce();
            }

            bool result;

            using (_mocks.Playback())
            {
                _target = createTarget();
                result = _target.Execute();
            }

            Assert.IsTrue(result);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [Test]
        public void VerifyExecuteAllGroupUnsuccessful()
        {
            ILockableBitArray bitArrayBeforeMove = new LockableBitArray(2, false, false, null);
            bitArrayBeforeMove.PeriodArea = new MinMax<int>(0, 1);
            bitArrayBeforeMove.Set(0, true);
            ILockableBitArray bitArrayAfterMove = new LockableBitArray(2, false, false, null);
            bitArrayAfterMove.PeriodArea = new MinMax<int>(0, 1);
            bitArrayAfterMove.Set(1, true);

            using (_mocks.Record())
            {
                Expect.Call(_converter.Convert(false, false)).Return(bitArrayBeforeMove).Repeat.Times(6);
                Expect.Call(_dataExtractor.Values()).Return(new List<double?> { 1, 2 }).Repeat.Times(3);
                Expect.Call(_decisionMaker.Execute(bitArrayBeforeMove, new List<double?> { 1, 2 })).IgnoreArguments().Return(false).Repeat.Once();
                Expect.Call(_decisionMaker.Execute(bitArrayBeforeMove, new List<double?> { 1, 2 })).IgnoreArguments().Return(false).Repeat.Once();
                Expect.Call(_decisionMaker.Execute(bitArrayBeforeMove, new List<double?> { 1, 2 })).IgnoreArguments().Return(false).Repeat.Once();
                Expect.Call(_matrix.Person).Return(new Person()).Repeat.Any();
            }

            bool result;

            using (_mocks.Playback())
            {
                _target = createTarget();
                result = _target.Execute();
            }

            Assert.IsFalse(result);
        }

        private DayOffOptimizerContainer createTarget()
        {
            return new DayOffOptimizerContainer(_converter,
                                                new List<IDayOffDecisionMaker> { _decisionMaker, _decisionMaker, _decisionMaker },
                                                _dataExtractor,
                                                _daysOffPreferences,
                                                _matrix,
                                                _dayOffDecisionMakerExecuter,
                                                _originalStateContainer
                                                );
        }
    }
}
