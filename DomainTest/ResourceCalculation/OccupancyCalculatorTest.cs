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
        private PersonSkillDictionary _relativePersonSkillResources;
        private ISkill _s1;
        private ISkill _s2;
        private ISkill _s3;

        [SetUp]
        public void Setup()
        {
            _s1 = SkillFactory.CreateSkill("s1");
            _s2 = SkillFactory.CreateSkill("s2");
            _s3 = SkillFactory.CreateSkill("s3");
            _relevantSkillStaffPeriods = CreateSkillStaffPeriodsForTest();
            _relativePersonSkillResources = CreatePersonSkillResourcesForTest();

            _target = new OccupancyCalculator(_relevantSkillStaffPeriods, _relativePersonSkillResources);
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyCalculationGivesCorrectNewOccupancy()
        {
            Assert.AreEqual(0.68, _relevantSkillStaffPeriods[_s1].Payload.CalculatedOccupancy, 0.01);
            Assert.AreEqual(0.67, _relevantSkillStaffPeriods[_s2].Payload.CalculatedOccupancy, 0.01);
            Assert.AreEqual(0.42, _relevantSkillStaffPeriods[_s3].Payload.CalculatedOccupancy, 0.01);

            Assert.AreEqual(7.24, _relevantSkillStaffPeriods[_s1].Payload.ForecastedIncomingDemand, 0.01);
            Assert.AreEqual(7.42, _relevantSkillStaffPeriods[_s2].Payload.ForecastedIncomingDemand, 0.01);
            Assert.AreEqual(1.95, _relevantSkillStaffPeriods[_s3].Payload.ForecastedIncomingDemand, 0.01);

            _target.AdjustOccupancy();
            Assert.AreEqual(3, _relevantSkillStaffPeriods.Count);
            Assert.AreEqual(0.73, _relevantSkillStaffPeriods[_s1].Payload.CalculatedOccupancy, 0.01);
            Assert.AreEqual(0.73, _relevantSkillStaffPeriods[_s2].Payload.CalculatedOccupancy, 0.01);
            Assert.AreEqual(0.66, _relevantSkillStaffPeriods[_s3].Payload.CalculatedOccupancy, 0.01);

            Assert.AreEqual(6.76, _relevantSkillStaffPeriods[_s1].Payload.ForecastedIncomingDemand, 0.01);
            Assert.AreEqual(6.81, _relevantSkillStaffPeriods[_s2].Payload.ForecastedIncomingDemand, 0.01);
			Assert.AreEqual(1.25, _relevantSkillStaffPeriods[_s3].Payload.ForecastedIncomingDemand, 0.01);
        }

        [Test]
        public void VerifyResetToOriginal()
        {
            Assert.AreEqual(7.246, _relevantSkillStaffPeriods[_s1].Payload.ForecastedIncomingDemand, 0.001);
            _target.AdjustOccupancy();
            Assert.AreEqual(6.767, _relevantSkillStaffPeriods[_s1].Payload.ForecastedIncomingDemand, 0.001);
            _relevantSkillStaffPeriods[_s1].Payload.MultiskillMinOccupancy = null;
            _relevantSkillStaffPeriods[_s1].CalculateStaff();
            Assert.AreEqual(7.246, _relevantSkillStaffPeriods[_s1].Payload.ForecastedIncomingDemand, 0.001);
        }


        /// <summary>
        /// Creates the skill staff periods for test.
        /// </summary>
        /// <remarks>
        /// NOTE: See the OccupancyCalculatorTestSet.xls excel sheet for source of data.
        /// </remarks>
        private IDictionary<ISkill, ISkillStaffPeriod> CreateSkillStaffPeriodsForTest()
        {
            IDictionary<ISkill, ISkillStaffPeriod> ret = new Dictionary<ISkill, ISkillStaffPeriod>();
            DateTimePeriod dtp = new DateTimePeriod(new DateTime(2007, 1, 1, 11, 0, 0, DateTimeKind.Utc), new DateTime(2007, 1, 1, 12, 0, 0, DateTimeKind.Utc));

            ITask t1 = new Task(150,TimeSpan.FromSeconds(110), TimeSpan.FromSeconds(10));
            ServiceAgreement sa1 = new ServiceAgreement(new ServiceLevel(new Percent(0.8), 20), new Percent(0), new Percent(1));
            ISkillStaffPeriod ssp1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(dtp, t1, sa1);
            ssp1.CalculateStaff();
            ret.Add(_s1, ssp1);

            ITask t2 = new Task(100, TimeSpan.FromSeconds(170), TimeSpan.FromSeconds(10));
            ISkillStaffPeriod ssp2 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(dtp, t2, sa1);
            ssp2.CalculateStaff();
            ret.Add(_s2, ssp2);

            ITask t3 = new Task(50, TimeSpan.FromSeconds(50), TimeSpan.FromSeconds(10));
            ISkillStaffPeriod ssp3 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(dtp, t3, sa1);
            ssp3.CalculateStaff();
            ret.Add(_s3, ssp3);

            return ret;
        }

        /// <summary>
        /// Creates the person skill resources for test.
        /// </summary>
        /// <remarks>
        /// NOTE: See the OccupancyCalculatorTestSet.xls excel sheet for source of data.
        /// The test data constructed here is the same as has been defined on the excel sheet.
        /// </remarks>
        private PersonSkillDictionary CreatePersonSkillResourcesForTest()
        {
            PersonSkillDictionary ret = new PersonSkillDictionary();

            IPerson p1 = new Person();
            Dictionary<ISkill,double> d1 = new Dictionary<ISkill, double>();
            d1.Add(_s1, 0.33);
            ret.Add(p1, d1);

            IPerson p2 = new Person();
            Dictionary<ISkill, double> d2 = new Dictionary<ISkill, double>();
            d2.Add(_s2, 0.66);
            d2.Add(_s3, 0.66);
            ret.Add(p2, d2);

            IPerson p3 = new Person();
            Dictionary<ISkill, double> d3 = new Dictionary<ISkill, double>();
            d3.Add(_s2, 1);
            ret.Add(p3, d3);

            IPerson p4 = new Person();
            Dictionary<ISkill, double> d4 = new Dictionary<ISkill, double>();
            d4.Add(_s1, 1);
            d4.Add(_s2, 1);
            d4.Add(_s3, 1);
            ret.Add(p4, d4);

            IPerson p5 = new Person();
            Dictionary<ISkill, double> d5 = new Dictionary<ISkill, double>();
            d5.Add(_s1, 0.33);
            d5.Add(_s2, 0.33);
            d5.Add(_s3, 0.33);
            ret.Add(p5, d5);

            IPerson p6 = new Person();
            Dictionary<ISkill, double> d6 = new Dictionary<ISkill, double>();
            d6.Add(_s1, 1);
            d6.Add(_s2, 1);
            ret.Add(p6, d6);

            IPerson p7 = new Person();
            Dictionary<ISkill, double> d7 = new Dictionary<ISkill, double>();
            d7.Add(_s1, 0.66);
            ret.Add(p7, d7);

            IPerson p8 = new Person();
            Dictionary<ISkill, double> d8 = new Dictionary<ISkill, double>();
            d8.Add(_s3, 1);
            ret.Add(p8, d8);

            IPerson p9 = new Person();
            Dictionary<ISkill, double> d9 = new Dictionary<ISkill, double>();
            d9.Add(_s2, 1);
            d9.Add(_s3, 1);
            ret.Add(p9, d9);

            IPerson p10 = new Person();
            Dictionary<ISkill, double> d10 = new Dictionary<ISkill, double>();
            d10.Add(_s1, 1);
            ret.Add(p10, d10);

            return ret;
        }
    }
}
