using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class SkillStaffPeriodStatisticsForSkillIntradayTest
    {
        #region Variables

        private SkillStaffPeriodStatisticsForSkillIntraday _target;
        private IDeviationStatisticsCalculator _calculator;

        private ISkillStaffPeriod _period;
        private MockRepository _mocks;

        #endregion

        #region Setup

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _calculator = _mocks.StrictMock<IDeviationStatisticsCalculator>();


            _period = _mocks.StrictMock<ISkillStaffPeriod>();
            
        }   

        #endregion

        #region Testcases

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }


        [Test]
        public void VerifyAnalyze()
        {
            _mocks.Record();

            Expect.Call(_period.FStaff).Return(1d);
            Expect.Call(_period.CalculatedResource).Return(11d);
            Expect.Call(_calculator.AbsoluteDeviationSumma).Return(10);
            Expect.Call(_calculator.RelativeDeviationSumma).Return(1);
            

            _mocks.ReplayAll();
            _target = new SkillStaffPeriodStatisticsForSkillIntraday(new List<ISkillStaffPeriod> { _period });
            _target.StatisticsCalculator = _calculator;
            Assert.AreEqual(10d, _calculator.AbsoluteDeviationSumma);
            Assert.AreEqual(1d, _calculator.RelativeDeviationSumma);
            _mocks.VerifyAll();
        }

        #endregion

        #region Property Tests

        [Test]
        public void VerifyStatisticsCalculator()
        {
            // Declare return variable to hold property get method
            IDeviationStatisticsCalculator getValue;

            // Test get method
            getValue = _target.StatisticsCalculator;

            // Perform Assert Tests
            Assert.IsNotNull(getValue);
        }


        #endregion
    }
}
