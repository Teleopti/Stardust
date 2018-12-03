using System;
using System.Reflection;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class SkillStaffTest
    {
        private SkillStaff target;
        private Task task;
        private ServiceAgreement sa;
        private readonly Percent _shrinkage = new Percent(0.5);
        private readonly Percent _efficiency = new Percent(0);
        private SkillPersonData skillPersonData;

        [SetUp]
        public void Setup()
        {
            task = new Task(100, TimeSpan.FromSeconds(120), TimeSpan.FromSeconds(20));
            sa = new ServiceAgreement(new ServiceLevel(new Percent(0.8), 20), new Percent(0), new Percent(1));
            skillPersonData = new SkillPersonData(0, 50);
            target = new SkillStaff(task, sa);
            target.CalculatedLoggedOn = 321;
            target.Shrinkage = _shrinkage;
            target.Efficiency = _efficiency;
            target.SkillPersonData = skillPersonData;
            target.UseShrinkage = true;
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(task, target.TaskData);
            Assert.AreEqual(sa, target.ServiceAgreementData);
            Assert.AreEqual(0, target.ForecastedIncomingDemand);
            Assert.AreEqual(0, target.CalculatedOccupancy);
            Assert.AreEqual(new Percent(target.CalculatedOccupancy), target.CalculatedOccupancyPercent);
            Assert.IsNull(target.MultiskillMinOccupancy);
            Assert.AreEqual(0, target.CalculatedResource);
            Assert.AreEqual(321, target.CalculatedLoggedOn);
            Assert.AreEqual(_shrinkage, target.Shrinkage);
            Assert.AreEqual(new Percent(0), target.Efficiency);
            Assert.AreEqual(target.ForecastedIncomingDemand, target.CalculatedTrafficIntensityWithShrinkage);
            Assert.AreEqual(skillPersonData, target.SkillPersonData);
            Assert.IsFalse(target.IsCalculated);
            Assert.IsTrue(target.UseShrinkage);
			Assert.IsNull(target.ManualAgents);
        }

        [Test]
        public void VerifyCanSetAgreementData()
        {
            ServiceAgreement agreementToSet = ServiceAgreement.DefaultValues();
            target.ServiceAgreementData = agreementToSet;
            Assert.AreEqual(agreementToSet, target.ServiceAgreementData);
            Assert.IsFalse(target.IsCalculated);
        }

        [Test]
        public void VerifyCanSetTaskData()
        {
            ITask taskToSet = new Task(100d, TimeSpan.FromSeconds(120d), TimeSpan.Zero);
            target.TaskData = taskToSet;
            Assert.AreEqual(taskToSet, target.TaskData);
            Assert.IsFalse(target.IsCalculated);
        }
		
		[Test]
        public void VerifyCanSetManualAgents()
        {
            target.ManualAgents = 168d;
			Assert.AreEqual(168d, target.ManualAgents);
            Assert.IsFalse(target.IsCalculated);
        }

        [Test]
        public void VerifyCanReset()
        {
        	target.ManualAgents = 150d;
            target.Reset();
            Assert.AreEqual(0,target.CalculatedLoggedOn);
            Assert.AreEqual(0, target.CalculatedOccupancy);
            Assert.AreEqual(0, target.CalculatedResource);
            Assert.AreEqual(0, target.CalculatedTrafficIntensityWithShrinkage);
            Assert.AreEqual(0, target.ForecastedIncomingDemand);
            Assert.AreEqual(0, target.Shrinkage.Value);
            Assert.AreEqual(0, target.Efficiency.Value);
			Assert.IsNull(target.ManualAgents);
            Assert.IsNull(target.MultiskillMinOccupancy);
            Assert.IsFalse(target.IsCalculated);
            Assert.AreEqual(new SkillPersonData(), target.SkillPersonData);
            Assert.AreEqual(new Task(), target.TaskData);
            Assert.AreEqual(new ServiceAgreement(),target.ServiceAgreementData);
        }

        [Test]
        public void VerifyCanReturnForecastedIncomingDemandWithAndWithoutShrinkage()
        {
            double demand = 10.2;
            typeof(SkillStaff).GetField("_forecastedIncomingDemand", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, demand);
            double shrinkage = target.Shrinkage.Value;
            double expectedDemandWithShrinkage = demand / (1 - shrinkage);
            target.UseShrinkage = true;
			Assert.AreEqual(demand, Math.Round(target.ForecastedIncomingDemandWithoutShrinkage, 2));
            Assert.AreEqual(Math.Round(expectedDemandWithShrinkage, 2), Math.Round(target.ForecastedIncomingDemand, 2));

            target.UseShrinkage = false;
            Assert.AreEqual(demand, Math.Round(target.ForecastedIncomingDemand, 2));
        }

        [Test]
        public void VerifyStaffDemandIfShrinkageIs50Percent()
        {
            double currentStaff = 10.0;
            typeof(SkillStaff).GetField("_forecastedIncomingDemand", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, currentStaff);
            double shrinkage = target.Shrinkage.Value;
            double expectedDemandWithShrinkage = currentStaff / (1 - shrinkage);
            target.UseShrinkage = true;

            Assert.AreEqual(Math.Round(expectedDemandWithShrinkage,2), Math.Round(target.ForecastedIncomingDemand,2));
        }

        [Test]
        public void VerifyStaffDemandIfShrinkageIs100Percent()
        {
            double currentStaff = 10.0;
            target.Shrinkage = new Percent(1.0);
            typeof(SkillStaff).GetField("_forecastedIncomingDemand", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, currentStaff);
            Assert.AreEqual(0, Math.Round(target.ForecastedIncomingDemand, 2));
        }
    }
}
