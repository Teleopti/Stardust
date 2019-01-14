using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    /// <summary>
    /// Tests for the MultisiteHelper class
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-05-08
    /// </remarks>
    [TestFixture]
    public class MultisiteHelperTest
    {
        private IList<IMultisiteDay> _multisiteDays;
        private IList<ISkillDay> _multisiteSkillDays;
        private DateOnly _dt;
        private IMultisiteSkill _multisiteSkill;

        [SetUp]
        public void Setup()
        {
            var skillType = SkillTypeFactory.CreateSkillTypePhone();
            _multisiteSkill = SkillFactory.CreateMultisiteSkill("parent", skillType, 15);
            var childSkill1 = SkillFactory.CreateChildSkill("child1", _multisiteSkill);
            var childSkill2 = SkillFactory.CreateChildSkill("child2", _multisiteSkill);
            
            _dt = new DateOnly(2008, 7, 16);
            _multisiteSkillDays = new List<ISkillDay>
                                      {
                                          SkillDayFactory.CreateSkillDay(_multisiteSkill,_dt),
                                          SkillDayFactory.CreateSkillDay(_multisiteSkill,_dt.AddDays(1)),
                                          SkillDayFactory.CreateSkillDay(_multisiteSkill,_dt.AddDays(2)),
                                          SkillDayFactory.CreateSkillDay(_multisiteSkill,_dt.AddDays(3))
                                      };

            _multisiteDays = new List<IMultisiteDay>
                                 {
                                     new MultisiteDay(_dt,_multisiteSkill,_multisiteSkillDays[0].Scenario),
                                     new MultisiteDay(_dt.AddDays(1),_multisiteSkill,_multisiteSkillDays[0].Scenario),
                                     new MultisiteDay(_dt.AddDays(2),_multisiteSkill,_multisiteSkillDays[0].Scenario)
                                 };

            IDictionary<IChildSkill, Percent> distribution = new Dictionary<IChildSkill, Percent>();
            distribution.Add(childSkill1, new Percent(0.3));
            distribution.Add(childSkill2, new Percent(0.7));
            SetDistribution(_multisiteDays[0], distribution);
            SetDistribution(_multisiteDays[1], distribution);
            SetDistribution(_multisiteDays[2], distribution);
        }

        /// <summary>
        /// Sets the distribution.
        /// </summary>
        /// <param name="multisiteDay">The multisite day.</param>
        /// <param name="distribution">The distribution.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-08
        /// </remarks>
        private static void SetDistribution(
            IMultisiteDay multisiteDay,
            IDictionary<IChildSkill, Percent> distribution)
        {
            multisiteDay.SetMultisitePeriodCollection(
                new List<IMultisitePeriod>
                    {
                        new MultisitePeriod(
                            new DateOnlyPeriod(multisiteDay.MultisiteDayDate,multisiteDay.MultisiteDayDate).ToDateTimePeriod(multisiteDay.Skill.TimeZone),
                            distribution)
                    });
        }

        [Test]
        public void VerifyEvenDistributionOfPercentage()
        {
            Random random = new Random();
            for (int i = 0; i < 10; i++)
            {
                int count = random.Next(1, 100);
                double[] percent = MultisiteHelper.CalculateLowVarianceDistribution(100, count, 2);
                decimal sum = 0m;
                for (int j = 0; j < percent.Length; j++)
                    sum += (decimal)percent[j];
                Assert.AreEqual(1m, sum);
                int decimalNumber = random.Next(0, 10);
                percent = MultisiteHelper.CalculateLowVarianceDistribution(100, count, decimalNumber);
                sum = 0m;
                for (int j = 0; j < percent.Length; j++)
                    sum += (decimal)percent[j];
                Assert.AreEqual(1m, sum);
            }
        }
    }
}