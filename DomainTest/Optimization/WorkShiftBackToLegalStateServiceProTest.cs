using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ProTest")]
    public class WorkShiftBackToLegalStateServiceProTest
    {
        private WorkShiftBackToLegalStateServicePro _target;
        private MockRepository _mocks;
        private IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
        private IWorkShiftBackToLegalStateStep _workShiftBackToLegalStateStep;
        private IScheduleMatrixPro _matrix;
        private ISchedulingOptions _schedulingOptions;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _workShiftMinMaxCalculator = _mocks.StrictMock<IWorkShiftMinMaxCalculator>();
            _workShiftBackToLegalStateStep = _mocks.StrictMock<IWorkShiftBackToLegalStateStep>();
            _target = new WorkShiftBackToLegalStateServicePro(_workShiftBackToLegalStateStep, _workShiftMinMaxCalculator);
            _matrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _schedulingOptions = _mocks.StrictMock<ISchedulingOptions>();
        }

        [Test]
        public void VerifyExecuteLegalStateNotPossible()
        {
            using (_mocks.Record())
            {
                Expect.Call(() => _workShiftMinMaxCalculator.ResetCache());
                Expect.Call(_workShiftMinMaxCalculator.WeekCount(_matrix))
                    .Return(1);
                Expect.Call(_workShiftMinMaxCalculator.IsWeekInLegalState(0, _matrix, _schedulingOptions))
                    .Return(false);
                Expect.Call(_workShiftBackToLegalStateStep.ExecuteWeekStep(0, _matrix))
                    .Return(null);
            }
            using (_mocks.Playback())
            {
                Assert.IsFalse(_target.Execute(_matrix, _schedulingOptions));
                Assert.IsNotNull(_target.RemovedDays);
                Assert.AreEqual(0, _target.RemovedDays.Count);
            }
        }

        [Test]
        public void VerifyExecuteInLegalState()
        {
            using(_mocks.Record())
            {
                Expect.Call(() => _workShiftMinMaxCalculator.ResetCache());
                Expect.Call(_workShiftMinMaxCalculator.WeekCount(_matrix))
                    .Return(1);
                Expect.Call(_workShiftMinMaxCalculator.IsWeekInLegalState(0, _matrix, _schedulingOptions))
                    .Return(true);
                Expect.Call(_workShiftMinMaxCalculator.PeriodLegalStateStatus(_matrix, _schedulingOptions))
                    .Return(0);
            }
            using(_mocks.Playback())
            {
                Assert.IsTrue(_target.Execute(_matrix, _schedulingOptions));
                Assert.IsNotNull(_target.RemovedDays);
                Assert.AreEqual(0, _target.RemovedDays.Count);
            }
        }

        [Test]
        public void VerifyExecutePeriodNotWeekInLegalState()
        {
            using (_mocks.Record())
            {
                Expect.Call(() => _workShiftMinMaxCalculator.ResetCache());
                Expect.Call(_workShiftMinMaxCalculator.WeekCount(_matrix))
                    .Return(1);
                Expect.Call(_workShiftMinMaxCalculator.IsWeekInLegalState(0, _matrix, _schedulingOptions))
                    .Return(true);
                Expect.Call(_workShiftMinMaxCalculator.PeriodLegalStateStatus(_matrix, _schedulingOptions))
                    .Return(-1);
                Expect.Call(_workShiftMinMaxCalculator.PeriodLegalStateStatus(_matrix, _schedulingOptions))
                    .Return(0);

                Expect.Call(_workShiftBackToLegalStateStep.ExecutePeriodStep(true, _matrix))
                    .Return(DateOnly.MaxValue);

            }
            using (_mocks.Playback())
            {
                Assert.IsTrue(_target.Execute(_matrix, _schedulingOptions));
                Assert.IsNotNull(_target.RemovedDays);
                Assert.AreEqual(1, _target.RemovedDays.Count);
                Assert.AreEqual(DateOnly.MaxValue, _target.RemovedDays[0]);
            }
        }

        [Test]
        public void VerifyExecuteWeekNotPeriodInLegalState()
        {
            using (_mocks.Record())
            {
                Expect.Call(() => _workShiftMinMaxCalculator.ResetCache());
                Expect.Call(_workShiftMinMaxCalculator.WeekCount(_matrix))
                    .Return(1);
                Expect.Call(_workShiftMinMaxCalculator.IsWeekInLegalState(0, _matrix, _schedulingOptions))
                    .Return(false);
                Expect.Call(_workShiftMinMaxCalculator.IsWeekInLegalState(0, _matrix, _schedulingOptions))
                    .Return(true);
                Expect.Call(_workShiftMinMaxCalculator.PeriodLegalStateStatus(_matrix, _schedulingOptions))
                    .Return(0);

                Expect.Call(_workShiftBackToLegalStateStep.ExecuteWeekStep(0, _matrix))
                    .Return(DateOnly.MaxValue);

            }
            using (_mocks.Playback())
            {
                Assert.IsTrue(_target.Execute(_matrix, _schedulingOptions));
                Assert.IsNotNull(_target.RemovedDays);
                Assert.AreEqual(1, _target.RemovedDays.Count);
                Assert.AreEqual(DateOnly.MaxValue, _target.RemovedDays[0]);
            }
        }

    }
}
