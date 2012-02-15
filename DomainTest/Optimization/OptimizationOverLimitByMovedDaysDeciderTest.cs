using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class OptimizationOverLimitByMovedDaysDeciderTest
    {
        private OptimizationOverLimitByMovedDaysDecider _target;
        private MockRepository _mocks;
        private IScheduleMatrixOriginalStateContainer _originalStateContainer;
        private int _moveMaxDayOff;
        int _moveMaxWorkShift;
        private ILogWriter _logWriter;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _originalStateContainer = _mocks.StrictMock<IScheduleMatrixOriginalStateContainer>();
            _logWriter = new LogWriter<OptimizationOverLimitByMovedDaysDeciderTest>();
        }

        [Test]
        public void TestConst()
        {
            _target = new OptimizationOverLimitByMovedDaysDecider(_originalStateContainer, _moveMaxDayOff, _moveMaxWorkShift);
            Assert.IsNotNull(_target);
        }

        [Test]
        public void TestUnderLimit()
        {
            _moveMaxDayOff = 1;
            _moveMaxWorkShift = 1;

            using (_mocks.Record())
            {
                Expect.Call(_originalStateContainer.CountChangedDayOffs())
                    .Return(0);
                Expect.Call(_originalStateContainer.CountChangedWorkShifts())
                    .Return(0);

            }
            using (_mocks.Playback())
            {
                _target = new OptimizationOverLimitByMovedDaysDecider(_originalStateContainer, _moveMaxDayOff, _moveMaxWorkShift);
                bool result = _target.OverLimit(_logWriter);
                Assert.IsFalse(result);
            }
        }

        [Test]
        public void TestEqualLimit()
        {
            _moveMaxDayOff = 1;
            _moveMaxWorkShift = 1;

            using (_mocks.Record())
            {
                Expect.Call(_originalStateContainer.CountChangedDayOffs())
                    .Return(1);
                Expect.Call(_originalStateContainer.CountChangedWorkShifts())
                    .Return(1);

            }
            using (_mocks.Playback())
            {
                _target = new OptimizationOverLimitByMovedDaysDecider(_originalStateContainer, _moveMaxDayOff, _moveMaxWorkShift);
                bool result = _target.OverLimit(_logWriter);
                Assert.IsFalse(result);
            }
        }

        [Test]
        public void TestDayOffOverLimit()
        {
            IScheduleMatrixPro matrix = _mocks.StrictMock<IScheduleMatrixPro>();
            IPerson person = PersonFactory.CreatePerson("Anonym");

            _moveMaxDayOff = 1;
            _moveMaxWorkShift = 1;

            using (_mocks.Record())
            {
                Expect.Call(_originalStateContainer.CountChangedDayOffs())
                    .Return(2);

                Expect.Call(_originalStateContainer.ScheduleMatrix)
                    .Return(matrix);
                Expect.Call(matrix.Person)
                    .Return(person);

            }
            using (_mocks.Playback())
            {
                _target = new OptimizationOverLimitByMovedDaysDecider(_originalStateContainer, _moveMaxDayOff, _moveMaxWorkShift);
                bool result = _target.OverLimit(_logWriter);
                Assert.IsTrue(result);
            }
        }

        [Test]
        public void TestWorkShiftOverLimit()
        {
            IScheduleMatrixPro matrix = _mocks.StrictMock<IScheduleMatrixPro>();
            IPerson person = PersonFactory.CreatePerson("Anonym");

            _moveMaxDayOff = 1;
            _moveMaxWorkShift = 1;

            using (_mocks.Record())
            {
                Expect.Call(_originalStateContainer.CountChangedDayOffs())
                    .Return(1);
                Expect.Call(_originalStateContainer.CountChangedWorkShifts())
                    .Return(2);
                Expect.Call(_originalStateContainer.ScheduleMatrix)
                    .Return(matrix);
                Expect.Call(matrix.Person)
                    .Return(person);

            }
            using (_mocks.Playback())
            {
                _target = new OptimizationOverLimitByMovedDaysDecider(_originalStateContainer, _moveMaxDayOff, _moveMaxWorkShift);
                bool result = _target.OverLimit(_logWriter);
                Assert.IsTrue(result);
            }
        }

        [Test]
        public void TestOverLimit()
        {
            IScheduleMatrixPro matrix = _mocks.StrictMock<IScheduleMatrixPro>();
            IPerson person = PersonFactory.CreatePerson("Anonym");

            _moveMaxDayOff = 1;
            _moveMaxWorkShift = 1;

            using (_mocks.Record())
            {
                Expect.Call(_originalStateContainer.CountChangedDayOffs())
                    .Return(2);

                Expect.Call(_originalStateContainer.ScheduleMatrix)
                    .Return(matrix);
                Expect.Call(matrix.Person)
                    .Return(person);

            }
            using (_mocks.Playback())
            {
                _target = new OptimizationOverLimitByMovedDaysDecider(_originalStateContainer, _moveMaxDayOff, _moveMaxWorkShift);
                bool result = _target.OverLimit(_logWriter);
                Assert.IsTrue(result);
            }
        }
    }
}
