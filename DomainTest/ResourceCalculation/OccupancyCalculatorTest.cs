using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Obfuscated.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    /// <summary>
    /// Unit tests for <see cref="OccupancyCalculator"/>
    /// </summary>
    [TestFixture]
    public class OccupancyCalculatorTest
    {
        private OccupancyCalculator _target;
        private IDictionary<ISkill, ISkillStaffPeriod> _relevantSkillStaffPeriods;
        private KeyedSkillResourceDictionary _relativeKeyedSkillResourceResources;
        private ISkill _skill1;
        private ISkill _skill2;
        private ISkill _skill3;

        [SetUp]
        public void Setup()
        {
            _skill1 = SkillFactory.CreateSkill("s1");
            _skill2 = SkillFactory.CreateSkill("s2");
            _skill3 = SkillFactory.CreateSkill("s3");
            _relevantSkillStaffPeriods = createSkillStaffPeriodsForTest();
            _relativeKeyedSkillResourceResources = CreatePersonSkillResourcesForTest();

            _target = new OccupancyCalculator(_relevantSkillStaffPeriods, _relativeKeyedSkillResourceResources);
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyCalculationGivesCorrectNewOccupancy()
        {
            Assert.AreEqual(0.68, _relevantSkillStaffPeriods[_skill1].Payload.CalculatedOccupancy, 0.01);
            Assert.AreEqual(0.67, _relevantSkillStaffPeriods[_skill2].Payload.CalculatedOccupancy, 0.01);
            Assert.AreEqual(0.42, _relevantSkillStaffPeriods[_skill3].Payload.CalculatedOccupancy, 0.01);

            Assert.AreEqual(7.24, _relevantSkillStaffPeriods[_skill1].Payload.ForecastedIncomingDemand, 0.01);
            Assert.AreEqual(7.42, _relevantSkillStaffPeriods[_skill2].Payload.ForecastedIncomingDemand, 0.01);
            Assert.AreEqual(1.95, _relevantSkillStaffPeriods[_skill3].Payload.ForecastedIncomingDemand, 0.01);

            _target.AdjustOccupancy();
            Assert.AreEqual(3, _relevantSkillStaffPeriods.Count);
            Assert.AreEqual(0.73, _relevantSkillStaffPeriods[_skill1].Payload.CalculatedOccupancy, 0.01);
            Assert.AreEqual(0.73, _relevantSkillStaffPeriods[_skill2].Payload.CalculatedOccupancy, 0.01);
            Assert.AreEqual(0.66, _relevantSkillStaffPeriods[_skill3].Payload.CalculatedOccupancy, 0.01);

            Assert.AreEqual(6.76, _relevantSkillStaffPeriods[_skill1].Payload.ForecastedIncomingDemand, 0.01);
            Assert.AreEqual(6.81, _relevantSkillStaffPeriods[_skill2].Payload.ForecastedIncomingDemand, 0.01);
			Assert.AreEqual(1.25, _relevantSkillStaffPeriods[_skill3].Payload.ForecastedIncomingDemand, 0.01);
        }

        [Test]
        public void VerifyResetToOriginal()
        {
            Assert.AreEqual(7.246, _relevantSkillStaffPeriods[_skill1].Payload.ForecastedIncomingDemand, 0.001);
            _target.AdjustOccupancy();
            Assert.AreEqual(6.767, _relevantSkillStaffPeriods[_skill1].Payload.ForecastedIncomingDemand, 0.001);
            _relevantSkillStaffPeriods[_skill1].Payload.MultiskillMinOccupancy = null;
            _relevantSkillStaffPeriods[_skill1].CalculateStaff();
            Assert.AreEqual(7.246, _relevantSkillStaffPeriods[_skill1].Payload.ForecastedIncomingDemand, 0.001);
        }


        /// <summary>
        /// Creates the skill staff periods for test.
        /// </summary>
        /// <remarks>
        /// NOTE: See the OccupancyCalculatorTestSet.xls excel sheet for source of data.
        /// </remarks>
        private IDictionary<ISkill, ISkillStaffPeriod> createSkillStaffPeriodsForTest()
        {
            var skillPeriodDictionary = new Dictionary<ISkill, ISkillStaffPeriod>();
            var dateTimePeriod = new DateTimePeriod(new DateTime(2007, 1, 1, 11, 0, 0, DateTimeKind.Utc), new DateTime(2007, 1, 1, 12, 0, 0, DateTimeKind.Utc));

            var task1 = new Task(150,TimeSpan.FromSeconds(110), TimeSpan.FromSeconds(10));
            var serviceAgreement = new ServiceAgreement(new ServiceLevel(new Percent(0.8), 20), new Percent(0), new Percent(1));
            var skillStaffPeriod1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(dateTimePeriod, task1, serviceAgreement);
			ssp1.SetSkillDay(SkillDayFactory.CreateSkillDay(SkillFactory.CreateSkill("skill"),dtp.StartDateTime));
            skillPeriodDictionary.Add(_skill1, skillStaffPeriod1);

            var task2 = new Task(100, TimeSpan.FromSeconds(170), TimeSpan.FromSeconds(10));
            var skillStaffPeriod2 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(dateTimePeriod, task2, serviceAgreement);
			ssp2.SetSkillDay(SkillDayFactory.CreateSkillDay(SkillFactory.CreateSkill("skill"), dtp.StartDateTime));
            skillPeriodDictionary.Add(_skill2, skillStaffPeriod2);

            var task3 = new Task(50, TimeSpan.FromSeconds(50), TimeSpan.FromSeconds(10));
            var skillStaffPeriod3 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(dateTimePeriod, task3, serviceAgreement);
			ssp3.SetSkillDay(SkillDayFactory.CreateSkillDay(SkillFactory.CreateSkill("skill"), dtp.StartDateTime));
            skillPeriodDictionary.Add(_skill3, skillStaffPeriod3);

            return skillPeriodDictionary;
        }

        /// <summary>
        /// Creates the person skill resources for test.
        /// </summary>
        /// <remarks>
        /// NOTE: See the OccupancyCalculatorTestSet.xls excel sheet for source of data.
        /// The test data constructed here is the same as has been defined on the excel sheet.
        /// </remarks>
        private KeyedSkillResourceDictionary CreatePersonSkillResourcesForTest()
        {
            KeyedSkillResourceDictionary ret = new KeyedSkillResourceDictionary();

            var p1 = new Person();
            var d1 = new Dictionary<ISkill, double> {{_skill1, 0.33}};
	        personSkillDictionary.Add(p1, d1);

            var p2 = new Person();
            var d2 = new Dictionary<ISkill, double> {{_skill2, 0.66}, {_skill3, 0.66}};
	        personSkillDictionary.Add(p2, d2);

            var p3 = new Person();
            var d3 = new Dictionary<ISkill, double> {{_skill2, 1}};
	        personSkillDictionary.Add(p3, d3);

            var p4 = new Person();
            var d4 = new Dictionary<ISkill, double> {{_skill1, 1}, {_skill2, 1}, {_skill3, 1}};
	        personSkillDictionary.Add(p4, d4);

			var p5 = new Person();
            var d5 = new Dictionary<ISkill, double> {{_skill1, 0.33}, {_skill2, 0.33}, {_skill3, 0.33}};
	        personSkillDictionary.Add(p5, d5);

			var p6 = new Person();
            var d6 = new Dictionary<ISkill, double> {{_skill1, 1}, {_skill2, 1}};
	        personSkillDictionary.Add(p6, d6);

			var p7 = new Person();
            var d7 = new Dictionary<ISkill, double> {{_skill1, 0.66}};
	        personSkillDictionary.Add(p7, d7);

			var p8 = new Person();
            var d8 = new Dictionary<ISkill, double> {{_skill3, 1}};
	        personSkillDictionary.Add(p8, d8);

			var p9 = new Person();
            var d9 = new Dictionary<ISkill, double> {{_skill2, 1}, {_skill3, 1}};
	        personSkillDictionary.Add(p9, d9);

			var p10 = new Person();
            var d10 = new Dictionary<ISkill, double> {{_skill1, 1}};
	        personSkillDictionary.Add(p10, d10);

            return personSkillDictionary;
        }
    }
}
