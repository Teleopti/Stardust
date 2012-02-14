using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class TeleoptiPeriodValueCalculatorTest
    {
        private IPeriodValueCalculator _target;
        private MockRepository _mocks;
        private IScheduleResultDataExtractor _extractor;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _extractor = _mocks.StrictMock<IScheduleResultDataExtractor>();
            _target = new TeleoptiPeriodValueCalculator(_extractor);
        }

        [Test]
        public void ValidateMixedPositiveAndNegativeValues()
        {
            using (_mocks.Record())
            {
                Expect.Call(_extractor.Values()).Return(new List<double?> {0, -10, 10, 5, -5, 1, -1, 100, -100});
            }
            using (_mocks.Playback())
            {
                double result = _target.PeriodValue(IterationOperationOption.DayOffOptimization);
                Assert.AreEqual(279.4365073, result, 0.000001);
            }
        }

        [Test]
        public void ValidateMixedPositiveValues()
        {
            using (_mocks.Record())
            {
                Expect.Call(_extractor.Values()).Return(new List<double?> {5, 3, 100, 50, 6, 1, 0, 100, 20});
            }
            using (_mocks.Playback())
            {
                double result = _target.PeriodValue(IterationOperationOption.DayOffOptimization);
                Assert.AreEqual(324.364394515292, result, 0.000001);
            }
        }

        [Test]
        public void ValidateMixedNegativeValues()
        {
            using (_mocks.Record())
            {
                Expect.Call(_extractor.Values()).Return(new List<double?> { -5, -3, -100, -50, -6, -1, -0, -100, -20 });
            }
            using (_mocks.Playback())
            {
                double result = _target.PeriodValue(IterationOperationOption.DayOffOptimization);
                Assert.AreEqual(324.364394515292, result, 0.000001);
            }
        }

        [Test]
        public void ValidateAllZeroValues()
        {
            using (_mocks.Record())
            {
                Expect.Call(_extractor.Values()).Return(new List<double?> { 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            }
            using (_mocks.Playback())
            {
                double result = _target.PeriodValue(IterationOperationOption.DayOffOptimization);
                Assert.AreEqual(0, result, 0.000001);
            }
        }
    }
}