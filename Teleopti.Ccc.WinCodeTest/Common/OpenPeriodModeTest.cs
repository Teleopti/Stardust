using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;


namespace Teleopti.Ccc.WinCodeTest.Common
{
    [TestFixture]
    public class OpenPeriodModeTest
    {
        private IOpenPeriodMode budgetMode;
        private IOpenPeriodMode forecasterMode;
        private IOpenPeriodMode schedulerMode;
        private IOpenPeriodMode intradayMode;

        [SetUp]
        public void Setup()
        {
            budgetMode = new OpenPeriodBudgetsMode();
            forecasterMode = new OpenPeriodForecasterMode();
            schedulerMode = new OpenPeriodSchedulerMode();
            intradayMode = new OpenPeriodIntradayMode();
        }

        [Test]
        public void ShouldHaveRightSpecification()
        {
            Assert.IsTrue(budgetMode.Specification.IsSatisfiedBy(new DateOnlyPeriod(2010, 1, 1, 2012, 1, 1)));
            Assert.IsTrue(forecasterMode.Specification.IsSatisfiedBy(new DateOnlyPeriod(2010, 1, 1, 2011, 1, 1)));
            Assert.IsTrue(schedulerMode.Specification.IsSatisfiedBy(new DateOnlyPeriod(2010, 1, 1, 2011, 1, 1)));
            Assert.IsTrue(intradayMode.Specification.IsSatisfiedBy(new DateOnlyPeriod(2010, 1, 1, 2010, 1, 14)));
        }

        [Test]
        public void ShouldHaveRightSettingName()
        {
            Assert.AreEqual("OpenBudgetGroup", budgetMode.SettingName);
            Assert.AreEqual("OpenForecast", forecasterMode.SettingName);
            Assert.AreEqual("OpenScheduler", schedulerMode.SettingName);
            Assert.AreEqual("Intraday", intradayMode.SettingName);
        }

        [Test]
        public void ShouldHaveRightSettingForConsiderRestrictedScenarios()
        {
            Assert.IsFalse(budgetMode.ConsiderRestrictedScenarios);
            Assert.IsFalse(forecasterMode.ConsiderRestrictedScenarios);
            Assert.IsTrue(schedulerMode.ConsiderRestrictedScenarios);
            Assert.IsTrue(intradayMode.ConsiderRestrictedScenarios);
        }

        [Test]
        public void ShouldHaveFriendlyAliasName()
        {
            Assert.IsNotEmpty(budgetMode.AliasOfMaxNumberOfDays);
            Assert.IsNotEmpty(forecasterMode.AliasOfMaxNumberOfDays);
            Assert.IsNotEmpty(schedulerMode.AliasOfMaxNumberOfDays);
            Assert.IsNotEmpty(intradayMode.AliasOfMaxNumberOfDays);
        }

        [Test]
        public void ShouldHaveRightStyle()
        {
            Assert.IsTrue(budgetMode.ForecasterStyle);
            Assert.IsTrue(forecasterMode.ForecasterStyle);
            Assert.IsFalse(schedulerMode.ForecasterStyle);
            Assert.IsTrue(intradayMode.ForecasterStyle);
        }
    }
}
