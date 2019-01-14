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
    }
}