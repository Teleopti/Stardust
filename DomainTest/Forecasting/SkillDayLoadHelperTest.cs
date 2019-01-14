using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [DomainTest]
    public class SkillDayLoadHelperTest
    {
        public FakeMultisiteDayRepository MultisiteDayRep;
        public FakeSkillDayRepository SkillDayRep;
		public IStaffingCalculatorServiceFacade StaffingCalculatorServiceFacade;
		public ISkillDayLoadHelper Target;
		
        [Test]
        public void VerifyLoadWithMultisiteDays()
		{
			var scenario = ScenarioFactory.CreateScenarioAggregate().WithId();
			var dtp = new DateOnlyPeriod(new DateOnly(2008, 7, 16), new DateOnly(2008, 7, 19));
			var skills = new List<ISkill>
			{
				SkillFactory.CreateSkill("skill2").WithId()
			};

			IMultisiteSkill multisiteSkill = SkillFactory.CreateMultisiteSkill("skill1");
            skills.Add(multisiteSkill);
                
            IList<ISkillDay> skillDays1 = new List<ISkillDay>
                                              { 
                                                                 SkillDayFactory.CreateSkillDay(multisiteSkill, dtp.StartDate, scenario),
                                                                 SkillDayFactory.CreateSkillDay(multisiteSkill, dtp.StartDate.AddDays(1), scenario),
                                                                 SkillDayFactory.CreateSkillDay(multisiteSkill, dtp.StartDate.AddDays(2), scenario)
                                                             };

            IList<ISkillDay> skillDays2 = new List<ISkillDay>
                                              { 
                                                                 SkillDayFactory.CreateSkillDay(skills[0], dtp.StartDate, scenario),
                                                                 SkillDayFactory.CreateSkillDay(skills[0], dtp.StartDate.AddDays(1), scenario),
                                                                 SkillDayFactory.CreateSkillDay(skills[0], dtp.StartDate.AddDays(2), scenario)
                                                             };

            SkillDayRep.AddRange(skillDays1);
			SkillDayRep.AddRange(skillDays2);
			var multisiteDays = new[]
			{
				new MultisiteDay(dtp.StartDate, multisiteSkill, scenario),
				new MultisiteDay(dtp.StartDate.AddDays(1), multisiteSkill, scenario),
				new MultisiteDay(dtp.StartDate.AddDays(2), multisiteSkill, scenario)
			};
			MultisiteDayRep.Has(multisiteDays);
				
            var result = Target.LoadSchedulerSkillDays(dtp, skills, scenario);
            var resultSkillDays = result[multisiteSkill];

            Assert.AreEqual(3, resultSkillDays.Count());
            Assert.AreEqual(multisiteDays[0].MultisiteSkillDay,resultSkillDays.ElementAt(0));
            Assert.AreEqual(multisiteDays[1].MultisiteSkillDay, resultSkillDays.ElementAt(1)); 
            Assert.AreEqual(multisiteDays[2].MultisiteSkillDay, resultSkillDays.ElementAt(2));
            Assert.IsNotNull(resultSkillDays.ElementAt(0).SkillDayCalculator);
            Assert.IsNotNull(resultSkillDays.ElementAt(2).SkillDayCalculator);
        }

		[Test]
		public void ShouldSetStaffingCalculatorServiceOnMultisiteSkill()
		{
			var scenario = ScenarioFactory.CreateScenarioAggregate().WithId();
			var dtp = new DateOnlyPeriod(new DateOnly(2008, 7, 16), new DateOnly(2008, 7, 19));
			
			IMultisiteSkill multisiteSkill = SkillFactory.CreateMultisiteSkill("skill1");
			
			IList<ISkillDay> skillDays1 = new List<ISkillDay>
											  {
																 SkillDayFactory.CreateSkillDay(multisiteSkill, dtp.StartDate, scenario)
															 };
			
			SkillDayRep.AddRange(skillDays1);
			var multisiteDays = new[]
			{
				new MultisiteDay(dtp.StartDate, multisiteSkill, scenario)
			};
			MultisiteDayRep.Has(multisiteDays);

			var result = Target.LoadSchedulerSkillDays(dtp, new []{multisiteSkill}, scenario);
			var resultSkillDays = result[multisiteSkill];

			Assert.AreEqual(1, resultSkillDays.Count());
			Assert.AreEqual(resultSkillDays.ElementAt(0).Skill.SkillType.StaffingCalculatorService, StaffingCalculatorServiceFacade);
		}

		[Test]
        public void VerifyLoadWithoutMultisiteDays()
		{
			var scenario = ScenarioFactory.CreateScenarioAggregate().WithId();
			var dtp = new DateOnlyPeriod(new DateOnly(2008, 7, 16), new DateOnly(2008, 7, 19));
			var skills = new List<ISkill>
			{
				SkillFactory.CreateSkill("skill2").WithId()
			};
			IList<ISkillDay> skillDays = new List<ISkillDay>
                                             { 
                                                                 SkillDayFactory.CreateSkillDay(skills[0], dtp.StartDate, scenario).WithId(),
                                                                 SkillDayFactory.CreateSkillDay(skills[0], dtp.StartDate.AddDays(1), scenario).WithId(),
                                                                 SkillDayFactory.CreateSkillDay(skills[0], dtp.StartDate.AddDays(2), scenario).WithId()
                                                             };
			SkillDayRep.AddRange(skillDays);

            var result = Target.LoadSchedulerSkillDays(dtp, skills, scenario);

            var resultSkillDays = result[skills[0]];
            Assert.AreEqual(3, resultSkillDays.Count());
            Assert.AreEqual(skillDays[0], resultSkillDays.ElementAt(0));
            Assert.AreEqual(skillDays[1], resultSkillDays.ElementAt(1));
            Assert.AreEqual(skillDays[2], resultSkillDays.ElementAt(2));
            Assert.IsNotNull(resultSkillDays.ElementAt(0).SkillDayCalculator);
            Assert.IsNotNull(resultSkillDays.ElementAt(2).SkillDayCalculator);
        }

        [Test]
        public void VerifyLoadBudgetSkillDaysWithMultisiteSkill()
        {
			var scenario = ScenarioFactory.CreateScenarioAggregate().WithId();
			var dtp = new DateOnlyPeriod(new DateOnly(2008, 7, 16), new DateOnly(2008, 7, 19));
			var skills = new List<ISkill>
			{
				SkillFactory.CreateSkill("skill2").WithId()
			};

			var multisiteSkill = SkillFactory.CreateMultisiteSkill("skill1");
            skills.Add(multisiteSkill);
            
			Assert.Throws<ArgumentException>(() => Target.LoadBudgetSkillDays(dtp, skills, scenario));
        }

        [Test]
        public void VerifyLoadBudgetSkillDays()
		{
			var scenario = ScenarioFactory.CreateScenarioAggregate().WithId();
			var dtp = new DateOnlyPeriod(new DateOnly(2008, 7, 16), new DateOnly(2008, 7, 19));
			var skills = new List<ISkill>
			{
				SkillFactory.CreateSkill("skill2").WithId()
			};
			var multisiteSkill = SkillFactory.CreateMultisiteSkill("multisite skill");
            var childSkill = SkillFactory.CreateChildSkill("Sub 1", multisiteSkill);
            multisiteSkill.AddChildSkill(childSkill);
            skills.Add(childSkill);

            IList<ISkillDay> skillDays = new List<ISkillDay>
                                             {
                                                 SkillDayFactory.CreateSkillDay(skills[0], dtp.StartDate.AddDays(1), scenario),
                                                 SkillDayFactory.CreateSkillDay(multisiteSkill, dtp.StartDate, scenario),
                                                 SkillDayFactory.CreateSkillDay(childSkill,dtp.StartDate, scenario)
                                             };
			MultisiteDayRep.Has(new MultisiteDay(dtp.StartDate, multisiteSkill, scenario));
			SkillDayRep.AddRange(skillDays);

            var result = Target.LoadBudgetSkillDays(dtp, skills, scenario);
            var resultSkillDays = result[childSkill];
            
            Assert.AreEqual(1, resultSkillDays.Count());
        }
		
        [Test]
        public void VerifyNullValuesGiveEmptyResults()
		{
			var scenario = ScenarioFactory.CreateScenarioAggregate().WithId();
			var dtp = new DateOnlyPeriod(new DateOnly(2008, 7, 16), new DateOnly(2008, 7, 19));
			var skills = new List<ISkill>
			{
				SkillFactory.CreateSkill("skill2").WithId()
			};
			var result = Target.LoadSchedulerSkillDays(dtp, null, scenario);
            Assert.AreEqual(0, result.Count);
            result = Target.LoadSchedulerSkillDays(dtp, skills, null);
            Assert.AreEqual(0, result.Count);
        }

		[Test]
		public void VerifyPeriodInflationForEmailSkills()
		{
			var scenario = ScenarioFactory.CreateScenarioAggregate().WithId();
			var today = new DateOnly(2008, 7, 16);
			var dtp = new DateOnlyPeriod(new DateOnly(2008, 7, 16), new DateOnly(2008, 7, 16));
			var skills = new List<ISkill>
			{
				SkillFactory.CreateSkill("emailSkill", SkillTypeFactory.CreateSkillTypeEmail(), 5).WithId()
			};
			var skillDays = new List<ISkillDay>();
			skillDays.AddRange(generateSkillDays(skills[0], today.AddDays(-10), 13, scenario));
			SkillDayRep.AddRange(skillDays);

			var result = Target.LoadSkillDaysWithFlexablePeriod(dtp, skills, scenario);

			var resultSkillDays = result[skills[0]];
			Assert.AreEqual(11, resultSkillDays.Count());
			Assert.AreEqual(skillDays[2], resultSkillDays.ElementAt(0));
			Assert.AreEqual(skillDays[3], resultSkillDays.ElementAt(1));
			Assert.AreEqual(skillDays[4], resultSkillDays.ElementAt(2));
			Assert.AreEqual(skillDays[5], resultSkillDays.ElementAt(3));
			Assert.AreEqual(skillDays[6], resultSkillDays.ElementAt(4));
			Assert.AreEqual(skillDays[7], resultSkillDays.ElementAt(5));
			Assert.AreEqual(skillDays[8], resultSkillDays.ElementAt(6));
			Assert.AreEqual(skillDays[9], resultSkillDays.ElementAt(7));
			Assert.AreEqual(skillDays[10], resultSkillDays.ElementAt(8));
			Assert.AreEqual(skillDays[11], resultSkillDays.ElementAt(9));
			Assert.AreEqual(skillDays[12], resultSkillDays.ElementAt(10));
		}

		[Test]
		public void VerifyPeriodInflationForBackofficeSkills()
		{
			var scenario = ScenarioFactory.CreateScenarioAggregate().WithId();
			var today = new DateOnly(2008, 7, 16);
			var dtp = new DateOnlyPeriod(new DateOnly(2008, 7, 16), new DateOnly(2008, 7, 16));
			var skills = new List<ISkill>
			{
				SkillFactory.CreateSkill("emailSkill", SkillTypeFactory.CreateSkillTypeBackoffice(), 5).WithId()
			};
			var skillDays = new List<ISkillDay>();
			skillDays.AddRange(generateSkillDays(skills[0], today.AddDays(-10), 13, scenario));
			SkillDayRep.AddRange(skillDays);

			var result = Target.LoadSkillDaysWithFlexablePeriod(dtp, skills, scenario);

			var resultSkillDays = result[skills[0]];
			Assert.AreEqual(11, resultSkillDays.Count());
			Assert.AreEqual(skillDays[2], resultSkillDays.ElementAt(0));
			Assert.AreEqual(skillDays[3], resultSkillDays.ElementAt(1));
			Assert.AreEqual(skillDays[4], resultSkillDays.ElementAt(2));
			Assert.AreEqual(skillDays[5], resultSkillDays.ElementAt(3));
			Assert.AreEqual(skillDays[6], resultSkillDays.ElementAt(4));
			Assert.AreEqual(skillDays[7], resultSkillDays.ElementAt(5));
			Assert.AreEqual(skillDays[8], resultSkillDays.ElementAt(6));
			Assert.AreEqual(skillDays[9], resultSkillDays.ElementAt(7));
			Assert.AreEqual(skillDays[10], resultSkillDays.ElementAt(8));
			Assert.AreEqual(skillDays[11], resultSkillDays.ElementAt(9));
			Assert.AreEqual(skillDays[12], resultSkillDays.ElementAt(10));
		}


		[Test]
		public void VerifyPeriodInflationForMixedSkillsTypes()
		{
			var scenario = ScenarioFactory.CreateScenarioAggregate().WithId();
			var today = new DateOnly(2008, 7, 16);
			var dtp = new DateOnlyPeriod(new DateOnly(2008, 7, 16), new DateOnly(2008, 7, 16));
			var skills = new List<ISkill>
			{
				SkillFactory.CreateSkill("emailSkill", SkillTypeFactory.CreateSkillTypeBackoffice(), 5).WithId(),
				SkillFactory.CreateSkill("phone", SkillTypeFactory.CreateSkillTypePhone(), 5).WithId()

			};
			var emailSkillDays = generateSkillDays(skills[0], today.AddDays(-10), 13, scenario);
			var phoneSkillDays = generateSkillDays(skills[1], today.AddDays(-10), 13, scenario);
			SkillDayRep.AddRange(emailSkillDays);
			SkillDayRep.AddRange(phoneSkillDays);

			var result = Target.LoadSkillDaysWithFlexablePeriod(dtp, skills, scenario);

			var resultSkillDays = result[skills[0]];
			Assert.AreEqual(11, resultSkillDays.Count());
			Assert.AreEqual(emailSkillDays[2], resultSkillDays.ElementAt(0));
			Assert.AreEqual(emailSkillDays[3], resultSkillDays.ElementAt(1));
			Assert.AreEqual(emailSkillDays[4], resultSkillDays.ElementAt(2));
			Assert.AreEqual(emailSkillDays[5], resultSkillDays.ElementAt(3));
			Assert.AreEqual(emailSkillDays[6], resultSkillDays.ElementAt(4));
			Assert.AreEqual(emailSkillDays[7], resultSkillDays.ElementAt(5));
			Assert.AreEqual(emailSkillDays[8], resultSkillDays.ElementAt(6));
			Assert.AreEqual(emailSkillDays[9], resultSkillDays.ElementAt(7));
			Assert.AreEqual(emailSkillDays[10], resultSkillDays.ElementAt(8));
			Assert.AreEqual(emailSkillDays[11], resultSkillDays.ElementAt(9));
			Assert.AreEqual(emailSkillDays[12], resultSkillDays.ElementAt(10));

			resultSkillDays = result[skills[1]];
			Assert.AreEqual(3, resultSkillDays.Count());
			Assert.AreEqual(phoneSkillDays[9], resultSkillDays.ElementAt(0));
			Assert.AreEqual(phoneSkillDays[10], resultSkillDays.ElementAt(1));
			Assert.AreEqual(phoneSkillDays[11], resultSkillDays.ElementAt(2));


		}

		private IList<ISkillDay> generateSkillDays(ISkill skill, DateOnly startDate, int noOfDays, IScenario scenario)
		{
			IList<ISkillDay> skillDays = new List<ISkillDay>();
			for (int i = 0; i <= noOfDays; i++)
			{
				skillDays.Add(SkillDayFactory.CreateSkillDay(skill, startDate.AddDays(i), scenario).WithId());
			}

			return skillDays;
		}
	}
}