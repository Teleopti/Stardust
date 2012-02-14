using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class StdDevPeriodValueCalculatorTest
    {
        private StdDevPeriodValueCalculator _target;
        private MockRepository _mockRepository;
        private IScheduleResultDataExtractor _scheduleResultDataExtractor;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _scheduleResultDataExtractor = _mockRepository.StrictMock<IScheduleResultDataExtractor>();
            _target = new StdDevPeriodValueCalculator(_scheduleResultDataExtractor);
        }

        [Test]
        public void VerifyPeriodValue()
        {
            IList<double?> valueList = new List<double?>{ 1,2,3,null,4,5 };
            using (_mockRepository.Record())
            {
                Expect.Call(_scheduleResultDataExtractor.Values()).Return(valueList).Repeat.Any();
            }
            using(_mockRepository.Playback())
            {
                Assert.AreEqual(1.41, _target.PeriodValue(IterationOperationOption.WorkShiftOptimization), 0.1);
            }

        }



    }
}
