﻿using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class BlockDayOffOptimizerContainerTest
    {
        private BlockDayOffOptimizerContainer _target;
        private MockRepository _mocks;
        private IDayOffDecisionMaker _decisionMaker;
        private IScheduleMatrixPro _matrix;
        private IScheduleMatrixOriginalStateContainer _originalStateContainer;
        private IBlockDayOffOptimizer _blockDayOffOptimizer;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _decisionMaker = _mocks.StrictMock<IDayOffDecisionMaker>();
            _matrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _originalStateContainer = _mocks.StrictMock<IScheduleMatrixOriginalStateContainer>();
            _blockDayOffOptimizer = _mocks.StrictMock<IBlockDayOffOptimizer>();
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
            ILockableBitArray bitArrayBeforeMove = new LockableBitArray(2, false, false, null) { PeriodArea = new MinMax<int>(0, 1) };
            bitArrayBeforeMove.Set(0, true);
            ILockableBitArray bitArrayAfterMove = new LockableBitArray(2, false, false, null) { PeriodArea = new MinMax<int>(0, 1) };
            bitArrayAfterMove.Set(1, true);

            using (_mocks.Record())
            {

                Expect.Call(_matrix.Person).Return(new Person()).Repeat.Any();
                Expect.Call(_blockDayOffOptimizer.Execute(_matrix, _originalStateContainer, _decisionMaker)).Return(true);
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
            ILockableBitArray bitArrayBeforeMove = new LockableBitArray(2, false, false, null) { PeriodArea = new MinMax<int>(0, 1) };
            bitArrayBeforeMove.Set(0, true);
            ILockableBitArray bitArrayAfterMove = new LockableBitArray(2, false, false, null) { PeriodArea = new MinMax<int>(0, 1) };
            bitArrayAfterMove.Set(1, true);

            using (_mocks.Record())
            {

                Expect.Call(_matrix.Person).Return(new Person()).Repeat.Any();
                Expect.Call(_blockDayOffOptimizer.Execute(_matrix, _originalStateContainer, _decisionMaker)).Return(false);
                Expect.Call(_blockDayOffOptimizer.Execute(_matrix, _originalStateContainer, _decisionMaker)).Return(false);
                Expect.Call(_blockDayOffOptimizer.Execute(_matrix, _originalStateContainer, _decisionMaker)).Return(true);
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
            ILockableBitArray bitArrayBeforeMove = new LockableBitArray(2, false, false, null) { PeriodArea = new MinMax<int>(0, 1) };
            bitArrayBeforeMove.Set(0, true);
            ILockableBitArray bitArrayAfterMove = new LockableBitArray(2, false, false, null) { PeriodArea = new MinMax<int>(0, 1) };
            bitArrayAfterMove.Set(1, true);

            using (_mocks.Record())
            {

                Expect.Call(_matrix.Person).Return(new Person()).Repeat.Any();
                Expect.Call(_blockDayOffOptimizer.Execute(_matrix, _originalStateContainer, _decisionMaker)).Return(false);
                Expect.Call(_blockDayOffOptimizer.Execute(_matrix, _originalStateContainer, _decisionMaker)).Return(false);
                Expect.Call(_blockDayOffOptimizer.Execute(_matrix, _originalStateContainer, _decisionMaker)).Return(false);
            }

            bool result;

            using (_mocks.Playback())
            {
                _target = createTarget();
                result = _target.Execute();
            }

            Assert.IsFalse(result);
        }

        private BlockDayOffOptimizerContainer createTarget()
        {
            return new BlockDayOffOptimizerContainer(
                                                new List<IDayOffDecisionMaker> { _decisionMaker, _decisionMaker, _decisionMaker },
                                                _matrix,
                                                _originalStateContainer,
                                                _blockDayOffOptimizer);
        }
    }
}
