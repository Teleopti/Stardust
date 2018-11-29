using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Logic.Restrictions;


namespace Teleopti.Ccc.Sdk.LogicTest.Restrictions
{
    [TestFixture]
    public class PlanningTimeBankCalculatorTest
    {
        private PlanningTimeBankCalculator _target;
        private PlanningTimeBackCalculatorParameters _calculatorParameters;

        bool _adjustTimeBankWithSeasonality;
        bool _adjustTimeBankWithPartTimePercentage;
        int _balanceOutMinMinutes;
        int _balanceOutMaxMinutes;
        int _targetTimeMinutes;
        double _partTimePercentage;
        double _seasonality;


        [SetUp]
        public void Setup()
        {
            _adjustTimeBankWithSeasonality = false;
            _adjustTimeBankWithPartTimePercentage = false;
            _balanceOutMinMinutes = -20 * 60;
            _balanceOutMaxMinutes = 40 * 60;
            _targetTimeMinutes = 160 * 60;
            _partTimePercentage = 0;
            _seasonality = 0;

            _target = new PlanningTimeBankCalculator();
        }

        [Test]
        public void VerifyNotZeroSeasonalityWithNoAdjustSetting()
        {

            _seasonality = 0.5;
            _calculatorParameters = new PlanningTimeBackCalculatorParameters(
                _balanceOutMinMinutes,
                _balanceOutMaxMinutes,
                _seasonality,
                _targetTimeMinutes,
                _adjustTimeBankWithSeasonality,
                _partTimePercentage,
                _adjustTimeBankWithPartTimePercentage);


            MinMax<int> result = _target.CalculateAdjustedTimeBank(_calculatorParameters);
            Assert.AreEqual(result.Minimum, _balanceOutMinMinutes);
            Assert.AreEqual(result.Maximum, _balanceOutMaxMinutes);
        }

        [Test]
        public void VerifyZeroSeasonality()
        {
            _adjustTimeBankWithSeasonality = true;
            _seasonality = 0;

            _calculatorParameters = new PlanningTimeBackCalculatorParameters(
                _balanceOutMinMinutes,
                _balanceOutMaxMinutes,
                _seasonality,
                _targetTimeMinutes,
                _adjustTimeBankWithSeasonality,
                _partTimePercentage,
                _adjustTimeBankWithPartTimePercentage);

            MinMax<int> result = _target.CalculateAdjustedTimeBank(_calculatorParameters);
            Assert.AreEqual(result.Minimum, _balanceOutMinMinutes);
            Assert.AreEqual(result.Maximum, _balanceOutMaxMinutes);
        }

        [Test]
        public void VerifyPositiveSeasonality()
        {
            _adjustTimeBankWithSeasonality = true;
            _seasonality = 0.1d;

            _calculatorParameters = new PlanningTimeBackCalculatorParameters(
                _balanceOutMinMinutes,
                _balanceOutMaxMinutes,
                _seasonality,
                _targetTimeMinutes,
                _adjustTimeBankWithSeasonality,
                _partTimePercentage,
                _adjustTimeBankWithPartTimePercentage);

            MinMax<int> result = _target.CalculateAdjustedTimeBank(_calculatorParameters);
            Assert.AreEqual(result.Minimum, _balanceOutMinMinutes);
            Assert.AreEqual(result.Maximum, 24 * 60);
        }

        [Test]
        public void VerifyNegativeSeasonality()
        {
            _adjustTimeBankWithSeasonality = true;
            _seasonality = -0.1d;

            _calculatorParameters = new PlanningTimeBackCalculatorParameters(
                _balanceOutMinMinutes,
                _balanceOutMaxMinutes,
                _seasonality,
                _targetTimeMinutes,
                _adjustTimeBankWithSeasonality,
                _partTimePercentage,
                _adjustTimeBankWithPartTimePercentage);


            MinMax<int> result = _target.CalculateAdjustedTimeBank(_calculatorParameters);
            Assert.AreEqual(result.Minimum, -4 * 60);
            Assert.AreEqual(result.Maximum, _balanceOutMaxMinutes);
        }

        [Test]
        public void VerifyNotPartTimeWithNoAdjustSetting()
        {
            _adjustTimeBankWithPartTimePercentage = false;
            _partTimePercentage = 0.5;

            _calculatorParameters = new PlanningTimeBackCalculatorParameters(
                _balanceOutMinMinutes,
                _balanceOutMaxMinutes,
                _seasonality,
                _targetTimeMinutes,
                _adjustTimeBankWithSeasonality,
                _partTimePercentage,
                _adjustTimeBankWithPartTimePercentage);

            MinMax<int> result = _target.CalculateAdjustedTimeBank(_calculatorParameters);
            Assert.AreEqual(result.Minimum, _balanceOutMinMinutes);
            Assert.AreEqual(result.Maximum, _balanceOutMaxMinutes);
        }

        [Test]
        public void VerifyNotZeroPartTime()
        {
            _adjustTimeBankWithPartTimePercentage = true;
            _partTimePercentage = 0;

            _calculatorParameters = new PlanningTimeBackCalculatorParameters(
                _balanceOutMinMinutes,
                _balanceOutMaxMinutes,
                _seasonality,
                _targetTimeMinutes,
                _adjustTimeBankWithSeasonality,
                _partTimePercentage,
                _adjustTimeBankWithPartTimePercentage);

            MinMax<int> result = _target.CalculateAdjustedTimeBank(_calculatorParameters);
            Assert.AreEqual(result.Minimum, _balanceOutMinMinutes);
            Assert.AreEqual(result.Maximum, _balanceOutMaxMinutes);
        }

        [Test]
        public void VerifyPartTime()
        {
            _adjustTimeBankWithPartTimePercentage = true;
            _partTimePercentage = 0.5;

            _calculatorParameters = new PlanningTimeBackCalculatorParameters(
                _balanceOutMinMinutes,
                _balanceOutMaxMinutes,
                _seasonality,
                _targetTimeMinutes,
                _adjustTimeBankWithSeasonality,
                _partTimePercentage,
                _adjustTimeBankWithPartTimePercentage);

            MinMax<int> result = _target.CalculateAdjustedTimeBank(_calculatorParameters);
            Assert.AreEqual(result.Minimum, -10 * 60);
            Assert.AreEqual(result.Maximum, 20 * 60);
        }

        [Test]
        public void VerifyCombinedCalculation()
        {
            _adjustTimeBankWithPartTimePercentage = true;
            _partTimePercentage = 0.5;
            _adjustTimeBankWithSeasonality = true;
            _seasonality = -0.1d;
            _targetTimeMinutes = 80 * 60;

            _calculatorParameters = new PlanningTimeBackCalculatorParameters(
                _balanceOutMinMinutes,
                _balanceOutMaxMinutes,
                _seasonality,
                _targetTimeMinutes,
                _adjustTimeBankWithSeasonality,
                _partTimePercentage,
                _adjustTimeBankWithPartTimePercentage);

            MinMax<int> result = _target.CalculateAdjustedTimeBank(_calculatorParameters);
            Assert.AreEqual(-2 * 60, result.Minimum);
            Assert.AreEqual(20 * 60, result.Maximum);
        }


    }
}
