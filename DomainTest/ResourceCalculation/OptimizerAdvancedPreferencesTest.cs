using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class OptimizerAdvancedPreferencesTest
    {
        private OptimizerAdvancedPreferences _target;

        [SetUp]
        public void Setup()
        {
            _target = new OptimizerAdvancedPreferences();
        }

        [Test]
        public void VerifyDefaultValues()
        {
            Assert.AreEqual(1d, _target.MaximumMovableWorkShiftPercentagePerPerson);
            Assert.IsTrue(_target.AllowWorkShiftOptimization);
            Assert.IsTrue(_target.AllowDayOffOptimization);
            Assert.IsTrue(_target.AllowIntradayOptimization);
            Assert.IsTrue(_target.AllowExtendReduceTimeOptimization);
            Assert.IsFalse(_target.ConsiderMaximumIntraIntervalStandardDeviation);
            Assert.IsTrue(_target.UseTweakedValues);
        }

        [Test]
        public void VerifyProperties()
        {

            double setMaximumMoveDayOffPerPerson = 0.8;
            _target.MaximumMovableDayOffPercentagePerPerson = setMaximumMoveDayOffPerPerson;
            double getMaximumMoveDayOffPerPerson = _target.MaximumMovableDayOffPercentagePerPerson;
            Assert.AreEqual(setMaximumMoveDayOffPerPerson, getMaximumMoveDayOffPerPerson);

            double setMaximumMoveShiftPerPerson = 0.9;
            _target.MaximumMovableWorkShiftPercentagePerPerson = setMaximumMoveShiftPerPerson;
            double getMaximumMoveShiftPerPerson = _target.MaximumMovableWorkShiftPercentagePerPerson;
            Assert.AreEqual(setMaximumMoveShiftPerPerson, getMaximumMoveShiftPerPerson);


            bool setAllowMoveTime = true;
            _target.AllowWorkShiftOptimization = setAllowMoveTime;
            bool getAllowMoveTime = _target.AllowWorkShiftOptimization;
            Assert.AreEqual(setAllowMoveTime, getAllowMoveTime);
            setAllowMoveTime = false;
            _target.AllowWorkShiftOptimization = setAllowMoveTime;
            getAllowMoveTime = _target.AllowWorkShiftOptimization;
            Assert.AreEqual(setAllowMoveTime, getAllowMoveTime);

            bool setAllowDayOffOptimization = true;
            _target.AllowDayOffOptimization = setAllowDayOffOptimization;
            bool getAllowDayOffOptimization = _target.AllowDayOffOptimization;
            Assert.AreEqual(setAllowDayOffOptimization, getAllowDayOffOptimization);
            setAllowDayOffOptimization = false;
            _target.AllowDayOffOptimization = setAllowDayOffOptimization;
            getAllowDayOffOptimization = _target.AllowDayOffOptimization;
            Assert.AreEqual(setAllowDayOffOptimization, getAllowDayOffOptimization);

            bool setAllowIntradayOptimization = true;
            _target.AllowIntradayOptimization = setAllowIntradayOptimization;
            bool getAllowIntradayOptimization = _target.AllowIntradayOptimization;
            Assert.AreEqual(setAllowIntradayOptimization, getAllowIntradayOptimization);
            setAllowIntradayOptimization = false;
            _target.AllowIntradayOptimization = setAllowIntradayOptimization;
            getAllowIntradayOptimization = _target.AllowIntradayOptimization;
            Assert.AreEqual(setAllowIntradayOptimization, getAllowIntradayOptimization);

            bool setUseMaximumStandardDeviation = false;
            _target.ConsiderMaximumIntraIntervalStandardDeviation = setUseMaximumStandardDeviation;
            Assert.IsFalse(_target.ConsiderMaximumIntraIntervalStandardDeviation);
            setUseMaximumStandardDeviation = true;
            _target.ConsiderMaximumIntraIntervalStandardDeviation = setUseMaximumStandardDeviation;
            Assert.IsTrue(_target.ConsiderMaximumIntraIntervalStandardDeviation);

            bool setUseTweakedValues = true;
            _target.UseTweakedValues = setUseTweakedValues;
            bool getUseTweakedValues = _target.UseTweakedValues;
            Assert.AreEqual(setUseTweakedValues, getUseTweakedValues);
            setUseTweakedValues = false;
            _target.UseTweakedValues = setUseTweakedValues;
            getUseTweakedValues = _target.UseTweakedValues;
            Assert.AreEqual(setUseTweakedValues, getUseTweakedValues);


            bool setUseStandardDeviation = true;
            _target.UseStandardDeviationCalculation = setUseStandardDeviation;
            bool getUseStandardDeviation = _target.UseStandardDeviationCalculation;
            Assert.AreEqual(setUseStandardDeviation, getUseStandardDeviation);
            setUseStandardDeviation = false;
            _target.UseStandardDeviationCalculation = setUseStandardDeviation;
            getUseStandardDeviation = _target.UseStandardDeviationCalculation;
            Assert.AreEqual(setUseStandardDeviation, getUseStandardDeviation);

            Assert.IsFalse(_target.UseTeleoptiCalculation);
            _target.UseTeleoptiCalculation = true;
            Assert.IsTrue(_target.UseTeleoptiCalculation);

            _target.AllowExtendReduceTimeOptimization = false;
            Assert.IsFalse(_target.AllowExtendReduceTimeOptimization);
        }

        [Test]
        public void VerifyClone()
        {
            IOptimizerAdvancedPreferences cloned = _target.Clone() as IOptimizerAdvancedPreferences;
            Assert.IsNotNull(cloned);
            Assert.AreNotSame(_target, cloned);
        }
    }
}
