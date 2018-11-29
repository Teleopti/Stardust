using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class MultisiteSkillDayCalculatorTest
    {
        private MultisiteSkillDayCalculator target;
        private IMultisiteSkill skill;
        private IChildSkill childSkill1;
        private IChildSkill childSkill2;
        private IList<ISkillDay> skillDays;
        private IList<IMultisiteDay> multisiteDays;
        private DateOnlyPeriod _visiblePeriod;

        [SetUp]
        public void Setup()
        {
            skill = SkillFactory.CreateMultisiteSkill("My multisite skill");
            childSkill1 = SkillFactory.CreateChildSkill("Child1", skill);
            childSkill2 = SkillFactory.CreateChildSkill("Child2", skill);
            skill.AddChildSkill(childSkill1);
            skill.AddChildSkill(childSkill2);

            childSkill1.SetId(Guid.NewGuid());
            childSkill2.SetId(Guid.NewGuid());

            skillDays = new List<ISkillDay>
                            {
                                SkillDayFactory.CreateSkillDay(skill, SkillDayTemplate.BaseDate),
                                SkillDayFactory.CreateSkillDay(skill, SkillDayTemplate.BaseDate.AddDays(1)),
                                SkillDayFactory.CreateSkillDay(skill, SkillDayTemplate.BaseDate.AddDays(2))
                            };
            
            multisiteDays = new List<IMultisiteDay>
                                {
                                    new MultisiteDay(skillDays[0].CurrentDate,skill,skillDays[0].Scenario),
                                    new MultisiteDay(skillDays[1].CurrentDate,skill,skillDays[0].Scenario),
                                    new MultisiteDay(skillDays[2].CurrentDate,skill,skillDays[0].Scenario)
                                };

            IDictionary<IChildSkill, Percent> distribution = new Dictionary<IChildSkill, Percent>();
            distribution.Add(childSkill1, new Percent(0.4));
            distribution.Add(childSkill2, new Percent(0.6));

            multisiteDays[0].SetMultisitePeriodCollection(new List<IMultisitePeriod> { new MultisitePeriod(new DateOnlyPeriod(multisiteDays[0].MultisiteDayDate, multisiteDays[0].MultisiteDayDate).ToDateTimePeriod(skill.TimeZone), distribution) });
            multisiteDays[1].SetMultisitePeriodCollection(new List<IMultisitePeriod> { new MultisitePeriod(new DateOnlyPeriod(multisiteDays[0].MultisiteDayDate, multisiteDays[1].MultisiteDayDate).ToDateTimePeriod(skill.TimeZone), distribution) });
            multisiteDays[2].SetMultisitePeriodCollection(new List<IMultisitePeriod> { new MultisitePeriod(new DateOnlyPeriod(multisiteDays[0].MultisiteDayDate, multisiteDays[2].MultisiteDayDate).ToDateTimePeriod(skill.TimeZone), distribution) });

            _visiblePeriod = new DateOnlyPeriod(skillDays[0].CurrentDate, skillDays[0].CurrentDate.AddDays(3));

            target = new MultisiteSkillDayCalculator(skill,skillDays,multisiteDays,_visiblePeriod);
        }

        [Test]
        public void VerifyInstanceCreated()
        {
            Assert.IsNotNull(target);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(multisiteDays.Count,target.MultisiteDays.Length);
            Assert.AreEqual(multisiteDays[0], target.MultisiteDays[0]);
            Assert.AreEqual(skill,target.MultisiteSkill);
        }

        [Test]
        public void VerifyCorrectInheritance()
        {
            Assert.IsInstanceOf<SkillDayCalculator>(target);
        }

        [Test]
        public void VerifyVisibleSkillDaysWorks()
        {
            Assert.AreEqual(3, target.VisibleMultisiteDays.Length);
            var visiblePeriod = new DateOnlyPeriod(multisiteDays[0].MultisiteDayDate, multisiteDays[0].MultisiteDayDate);
            target.VisiblePeriod = visiblePeriod;
            Assert.AreEqual(1, target.VisibleMultisiteDays.Length);
            Assert.AreEqual(multisiteDays[0], target.VisibleMultisiteDays[0]);
        }

        [Test]
        public void VerifyCanSetChildSkillDays()
        {
            var skillDaysChild1 = new List<ISkillDay>
                            {
                                SkillDayFactory.CreateSkillDay(childSkill1, SkillDayTemplate.BaseDate)
                            };

            target.SetChildSkillDays(childSkill1, skillDaysChild1);
            var skillDaysChild1Result = target.GetVisibleChildSkillDays(childSkill1);

            Assert.AreEqual(skillDaysChild1.Count,skillDaysChild1Result.Count);
            Assert.AreEqual(skillDaysChild1[0],skillDaysChild1Result[0]);
            Assert.AreEqual(target,skillDaysChild1Result[0].SkillDayCalculator);
        }

        [Test]
        public void VerifyCanSetChildSkillDaysTwice()
        {
            var skillDaysChild1 = new List<ISkillDay>
                            {
                                SkillDayFactory.CreateSkillDay(childSkill1, SkillDayTemplate.BaseDate)
                            };

            target.SetChildSkillDays(childSkill1, skillDaysChild1);
            target.SetChildSkillDays(childSkill1, skillDaysChild1);
            var skillDaysChild1Result = target.GetVisibleChildSkillDays(childSkill1);

            Assert.AreEqual(skillDaysChild1.Count, skillDaysChild1Result.Count);
            Assert.AreEqual(skillDaysChild1[0], skillDaysChild1Result[0]);
        }

        [Test]
        public void VerifyCanGetOnlyVisibleChildSkillDays()
        {
            var skillDaysChild1 = new List<ISkillDay>
                                      {
                                          SkillDayFactory.CreateSkillDay(childSkill1, SkillDayTemplate.BaseDate),
                                          SkillDayFactory.CreateSkillDay(childSkill1, SkillDayTemplate.BaseDate.AddDays(1))
                                      };

            target.SetChildSkillDays(childSkill1, skillDaysChild1);
            target.VisiblePeriod = new DateOnlyPeriod(skillDaysChild1[0].CurrentDate, skillDaysChild1[0].CurrentDate);
            var skillDaysChild1Result = target.GetVisibleChildSkillDays(childSkill1);

            Assert.AreEqual(1, skillDaysChild1Result.Count);
            Assert.AreEqual(skillDaysChild1[0], skillDaysChild1Result[0]);
        }

        [Test]
        public void VerifyCannotGetChildSkillDaysForChildSkillNotSet()
        {
            var skillDaysChild1 = new List<ISkillDay>();

            target.SetChildSkillDays(childSkill1, skillDaysChild1);
			Assert.Throws<ArgumentException>(() => target.GetVisibleChildSkillDays(childSkill2));
        }

        [Test]
        public void VerifyChildSkillMustBelongToMultisiteSkill()
        {
            var skillDaysChild1 = new List<ISkillDay>();
            IChildSkill childSkill = new ChildSkill("test", "test", Color.Red, new MultisiteSkill("B","",Color.Blue,15,SkillTypeFactory.CreateSkillType()));

			Assert.Throws<ArgumentException>(() => target.SetChildSkillDays(childSkill, skillDaysChild1));
        }

        [Test]
        public void VerifyCanInitializeChildSkillDays()
        {
            var skillDaysChild1 = new List<ISkillDay>
                            {
                                SkillDayFactory.CreateSkillDay(childSkill1, SkillDayTemplate.BaseDate)
                            };
            var skillDaysChild2 = new List<ISkillDay>
                            {
                                SkillDayFactory.CreateSkillDay(childSkill2, SkillDayTemplate.BaseDate)
                            };

            target.SetChildSkillDays(childSkill1, skillDaysChild1);
            target.SetChildSkillDays(childSkill2, skillDaysChild2);

            Assert.IsNull(multisiteDays[0].MultisiteSkillDay);
            Assert.IsFalse(multisiteDays[0].ChildSkillDays.Contains(skillDaysChild1[0]));
            Assert.IsFalse(multisiteDays[0].ChildSkillDays.Contains(skillDaysChild2[0]));

            target.InitializeChildSkills();

            Assert.AreEqual(skillDays[0],multisiteDays[0].MultisiteSkillDay);
            Assert.IsTrue(multisiteDays[0].ChildSkillDays.Contains(skillDaysChild1[0]));
            Assert.IsTrue(multisiteDays[0].ChildSkillDays.Contains(skillDaysChild2[0]));
        }

        [Test]
        public void VerifyAllChildSkillDaysMustBeSet()
        {
            var skillDaysChild1 = new List<ISkillDay>
                            {
                                SkillDayFactory.CreateSkillDay(childSkill1, SkillDayTemplate.BaseDate)
                            };
            target.SetChildSkillDays(childSkill1, skillDaysChild1);
			Assert.Throws<InvalidOperationException>(() => target.InitializeChildSkills());
        }

        [Test]
        public void VerifyCheckRestrictionsForMultisiteDay()
        {
	        var distribution = new Dictionary<IChildSkill, Percent> {{childSkill1, new Percent(1.1d)}};
	        multisiteDays[1].SetMultisitePeriodCollection(new List<IMultisitePeriod> { new MultisitePeriod(new DateOnlyPeriod(multisiteDays[1].MultisiteDayDate, multisiteDays[1].MultisiteDayDate).ToDateTimePeriod(skill.TimeZone), distribution) });
			Assert.Throws<ValidationException>(() => target.CheckRestrictions());
        }

        [Test]
        public void VerifyCheckRestrictionsForChildSkillDay()
        {
            var skillDaysChild1 = new List<ISkillDay>
                            {
                                SkillDayFactory.CreateSkillDay(childSkill1,SkillDayTemplate.BaseDate),
                                SkillDayFactory.CreateSkillDay(childSkill1,SkillDayTemplate.BaseDate.AddDays(1))
                            };

            target.SetChildSkillDays(childSkill1, skillDaysChild1);

            skillDaysChild1[1].SkillDataPeriodCollection[0].MinimumPersons = 5;
            skillDaysChild1[1].SkillDataPeriodCollection[0].MaximumPersons = 4;

			Assert.Throws<ValidationException>(() => target.CheckRestrictions());
        }

        [Test]
        public void VerifyCanGetUpcomingSkillStaffPeriods()
        {
            var skillDaysChild1 = new List<ISkillDay>
                                      {
                                          SkillDayFactory.CreateSkillDay(childSkill1, skillDays[0].CurrentDate),
                                          SkillDayFactory.CreateSkillDay(childSkill1, skillDays[1].CurrentDate)
                                      };

            target.SetChildSkillDays(childSkill1, skillDaysChild1);
            target.SetChildSkillDays(childSkill2, new List<ISkillDay>());
            target.InitializeChildSkills();
            skillDays[1].RecalculateDailyTasks();
            skillDays[0].RecalculateDailyTasks();
            var result = target.GetSkillStaffPeriodsForDayCalculation(skillDaysChild1[0]);
            Assert.AreEqual(1, target.ChildSkillStaffPeriods.Count);
            Assert.AreEqual(120, result.Count()); //Skill staff periods for two days
        }

        [Test]
        public void VerifyCanResetSkillStaffPeriods()
        {
            var skillDaysChild1 = new List<ISkillDay>
                                      {
                                          SkillDayFactory.CreateSkillDay(childSkill1,
                                                                         skillDays[0].CurrentDate),
                                          SkillDayFactory.CreateSkillDay(childSkill1,
                                                                         skillDays[1].CurrentDate)
                                      };

            target.SetChildSkillDays(childSkill1, skillDaysChild1);
            target.SetChildSkillDays(childSkill2, new List<ISkillDay>());
            target.InitializeChildSkills();
            skillDays[0].RecalculateDailyTasks();
            skillDays[1].RecalculateDailyTasks();
            var result = target.GetSkillStaffPeriodsForDayCalculation(skillDaysChild1[0]);
            Assert.AreEqual(1, target.ChildSkillStaffPeriods.Count);
            Assert.AreEqual(120, result.Count());
            target.ClearSkillStaffPeriods();
            Assert.AreEqual(0, target.ChildSkillStaffPeriods.Count);
            result = target.GetSkillStaffPeriodsForDayCalculation(skillDaysChild1[1]);
            Assert.AreEqual(1, target.ChildSkillStaffPeriods.Count);
            Assert.AreEqual(60, result.Count());
        }

        [Test]
        public void VerifyCanFindPercentageForInterval()
        {
            Percent percentage = target.GetPercentageForInterval(childSkill2,
                                                                 new DateTimePeriod(
                                                                     DateTime.SpecifyKind(skillDays[0].CurrentDate.Date,DateTimeKind.Utc).AddMinutes(-15),
                                                                     DateTime.SpecifyKind(skillDays[0].CurrentDate.Date,DateTimeKind.Utc)));
            Assert.AreEqual(0,percentage.Value);
            percentage = target.GetPercentageForInterval(childSkill2,
                                                                 new DateTimePeriod(
                                                                     DateTime.SpecifyKind(skillDays[0].CurrentDate.Date,DateTimeKind.Utc),
                                                                     DateTime.SpecifyKind(skillDays[0].CurrentDate.Date,DateTimeKind.Utc).AddMinutes(15)));
            Assert.AreEqual(0.6,percentage.Value);
            percentage = target.GetPercentageForInterval(childSkill1,
                                                                             new DateTimePeriod(
                                                                                 DateTime.SpecifyKind(skillDays[0].CurrentDate.Date,DateTimeKind.Utc),
                                                                                 DateTime.SpecifyKind(skillDays[0].CurrentDate.Date, DateTimeKind.Utc).AddMinutes(15)));
            Assert.AreEqual(0.4,percentage.Value);
            percentage = target.GetPercentageForInterval(skill, new DateTimePeriod());
            Assert.AreEqual(1,percentage.Value);
        }

        [Test]
        public void VerifyCannotHaveChildSkillFromOtherMultisiteSkill()
        {
            IChildSkill childSkill = new ChildSkill("test", "test", Color.Red, new MultisiteSkill("B","",Color.Blue,15,SkillTypeFactory.CreateSkillType()));
			Assert.Throws<ArgumentException>(() => target.GetPercentageForInterval(childSkill, new DateTimePeriod()));
        }

        [Test]
        public void VerifyCannotHaveSkillNotInThisSkillDayCalculator()
        {
            ISkill dummySkill = new Skill("test", "test", Color.Red, 15, skill.SkillType);
			Assert.Throws<ArgumentException>(() => target.GetPercentageForInterval(dummySkill, new DateTimePeriod()));
        }

        [Test]
        public void VerifyCanSummarizeStaffingFromSubSkills()
        {
            var skillDaysChild1 = new List<ISkillDay>
                                      {
                                          SkillDayFactory.CreateSkillDay(childSkill1,
                                                                         skillDays[0].CurrentDate)
                                      };

            var skillDaysChild2 = new List<ISkillDay>
                                      {
                                          SkillDayFactory.CreateSkillDay(childSkill2,
                                                                         skillDays[0].CurrentDate)
                                      };

            var skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(skillDays[0].CompleteSkillStaffPeriodCollection[10].Period,
                new Task(), ServiceAgreement.DefaultValues());
            var updatedValues = new NewSkillStaffPeriodValues(new List<ISkillStaffPeriod> { skillStaffPeriod });
            skillDays[0].SetCalculatedStaffCollection(updatedValues);
            updatedValues.BatchCompleted();

            target.SetChildSkillDays(childSkill1, skillDaysChild1);
            target.SetChildSkillDays(childSkill2, skillDaysChild2);
            target.InitializeChildSkills();

            var skillStaffPeriodChild1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(skillDaysChild1[0].CompleteSkillStaffPeriodCollection[10].Period,
                new Task(), ServiceAgreement.DefaultValues());
            var updatedValuesChild1 = new NewSkillStaffPeriodValues(new List<ISkillStaffPeriod> { skillStaffPeriodChild1 });
            skillDaysChild1[0].SetCalculatedStaffCollection(updatedValuesChild1);
            updatedValuesChild1.BatchCompleted();
            skillDaysChild1[0].CompleteSkillStaffPeriodCollection[10].SetCalculatedResource65(10d);
            skillDaysChild1[0].CompleteSkillStaffPeriodCollection[10].Payload.CalculatedLoggedOn = 3;

            var skillStaffPeriodChild2 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(skillDaysChild2[0].CompleteSkillStaffPeriodCollection[10].Period,
                new Task(), ServiceAgreement.DefaultValues());
            var updatedValuesChild2 = new NewSkillStaffPeriodValues(new List<ISkillStaffPeriod> { skillStaffPeriodChild2 });
            skillDaysChild2[0].SetCalculatedStaffCollection(updatedValuesChild2);
            updatedValuesChild2.BatchCompleted();
            skillDaysChild2[0].CompleteSkillStaffPeriodCollection[10].SetCalculatedResource65(20d);
            skillDaysChild2[0].CompleteSkillStaffPeriodCollection[10].Payload.CalculatedLoggedOn = 4;

            IDictionary<ISkill, IEnumerable<ISkillDay>> skillDayDictionary = new Dictionary<ISkill, IEnumerable<ISkillDay>>();
            skillDayDictionary.Add(childSkill1,skillDaysChild1);
            skillDayDictionary.Add(childSkill2,skillDaysChild2);
            skillDayDictionary.Add(skill,skillDays);

            MultisiteSkillDayCalculator.SummarizeStaffingForMultisiteSkillDays(skill, skillDayDictionary);

            Assert.AreEqual(20, skillDaysChild2[0].CompleteSkillStaffPeriodCollection[10].Payload.CalculatedResource);
            Assert.AreEqual(4, skillDaysChild2[0].CompleteSkillStaffPeriodCollection[10].Payload.CalculatedLoggedOn);
            Assert.AreEqual(30,skillDays[0].CompleteSkillStaffPeriodCollection[10].Payload.CalculatedResource);
            Assert.AreEqual(7, skillDays[0].CompleteSkillStaffPeriodCollection[10].Payload.CalculatedLoggedOn);
        }

        [Test]
        public void VerifySummarizeWithNullDoesNotThrowException()
        {
            MultisiteSkillDayCalculator.SummarizeStaffingForMultisiteSkillDays(childSkill2 as IMultisiteSkill,new Dictionary<ISkill, IEnumerable<ISkillDay>>());
            Assert.IsNotNull(childSkill2);
        }

        [Test]
        public void VerifyCloneToScenario()
        {
            MockRepository mocks = new MockRepository();
            IScenario scenario = mocks.StrictMock<IScenario>();

            mocks.ReplayAll();
            var skillDaysChild1 = new List<ISkillDay>
                            {
                                SkillDayFactory.CreateSkillDay(childSkill1, SkillDayTemplate.BaseDate)
                            };
            var skillDaysChild2 = new List<ISkillDay>
                            {
                                SkillDayFactory.CreateSkillDay(childSkill2, SkillDayTemplate.BaseDate)
                            };

            target.SetChildSkillDays(childSkill1, skillDaysChild1);
            target.SetChildSkillDays(childSkill2, skillDaysChild2);
            MultisiteSkillDayCalculator skillDayCalculator = (MultisiteSkillDayCalculator) target.CloneToScenario(scenario);
            Assert.AreNotEqual(skillDayCalculator.MultisiteDays[0],target.MultisiteDays[0]);
            Assert.AreNotEqual(skillDayCalculator.SkillDays.First(), target.SkillDays.First());
            Assert.AreEqual(skillDayCalculator.VisiblePeriod, target.VisiblePeriod);
            Assert.AreEqual(skillDayCalculator.MultisiteSkill, target.MultisiteSkill);
            Assert.AreNotEqual(skillDayCalculator.GetVisibleChildSkillDays(childSkill1)[0], target.GetVisibleChildSkillDays(childSkill1)[0]);
            Assert.AreEqual(scenario, skillDayCalculator.GetVisibleChildSkillDays(childSkill1)[0].Scenario);
            Assert.AreEqual(scenario, skillDayCalculator.SkillDays.First().Scenario);
            Assert.AreEqual(scenario, skillDayCalculator.MultisiteDays[0].Scenario);
            mocks.VerifyAll();
        }
    }
}
