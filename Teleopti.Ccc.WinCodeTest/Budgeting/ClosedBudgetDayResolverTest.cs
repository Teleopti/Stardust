using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Budgeting
{
    [TestFixture]
    public class ClosedBudgetDayResolverTest
    {
        private ClosedBudgetDayResolver target;
        private List<IBudgetGroupDayDetailModel> selectedBudgetDays;

        [SetUp]
        public void Setup()
        {
            IBudgetGroup group = new BudgetGroup();
	        var scenario = new Scenario("Default");
	        IBudgetDay day1 = new BudgetDay(group, scenario, new DateOnly(2010,12,1));
            IBudgetDay day2 = new BudgetDay(group, scenario, new DateOnly(2010,12,2));
            IBudgetDay day3 = new BudgetDay(group, scenario, new DateOnly(2010,12,3));

	        selectedBudgetDays = new List<IBudgetGroupDayDetailModel>
	        {
		        new BudgetGroupDayDetailModel(day1),
		        new BudgetGroupDayDetailModel(day2),
		        new BudgetGroupDayDetailModel(day3)
	        };

	        IDictionary<ISkill, IEnumerable<ISkillDay>> skillDaysForSkills = new Dictionary<ISkill, IEnumerable<ISkillDay>>();
            
            ISkill skill = new Skill("skill","",Color.Blue,15, new SkillTypePhone(new Description("d"), ForecastSource.InboundTelephony));
            ISkill skill2 = new Skill("anotherSkill","",Color.Blue,15, new SkillTypePhone(new Description("d"), ForecastSource.InboundTelephony));

            var date = new DateOnly(2010, 12, 1);
            
            var skillDay1 =  SkillDayFactory.CreateSkillDay(skill,date); //Open
            var skillDay2 = SkillDayFactory.CreateSkillDay(skill,date.AddDays(1)); //Closed
            var skillDay3 = SkillDayFactory.CreateSkillDay(skill,date.AddDays(2)); //Open
            var skillDay4 = SkillDayFactory.CreateSkillDay(skill, date.AddDays(3)); //Closed but Outside period of selected days

            var skillDay5 = SkillDayFactory.CreateSkillDay(skill2, date.AddDays(2));


            var skillDays = new List<ISkillDay> { skillDay1, skillDay2, skillDay3, skillDay4 };
            var skillDays2 = new List<ISkillDay> { skillDay5 };

            //Close day2
            skillDay2.SkillDayCalculator = new SkillDayCalculator(skill,skillDays,new DateOnlyPeriod(date,date.AddDays(2)));
            skillDay2.WorkloadDayCollection[0].Close();
            skillDay2.WorkloadDayCollection[1].Close();

            //Close day4 just for checking "not crash"
            skillDay4.SkillDayCalculator = new SkillDayCalculator(skill, skillDays, new DateOnlyPeriod(date, date.AddDays(3)));
            skillDay4.WorkloadDayCollection[0].Close();
            skillDay4.WorkloadDayCollection[1].Close();

            //close skillday 5 with another skill
            skillDay5.SkillDayCalculator = new SkillDayCalculator(skill, skillDays2, new DateOnlyPeriod(date, date.AddDays(3)));
            skillDay5.WorkloadDayCollection[0].Close();
            skillDay5.WorkloadDayCollection[1].Close();

            skillDaysForSkills.Add(skill, skillDays);
            skillDaysForSkills.Add(skill2, skillDays2);

            target = new ClosedBudgetDayResolver(selectedBudgetDays, skillDaysForSkills);
        }

        [Test]
        public void ShouldSetClosedOnClosedSkillDays()
        {
            selectedBudgetDays[0].IsClosed.Should().Be.EqualTo(false);
            selectedBudgetDays[1].IsClosed.Should().Be.EqualTo(false);
            selectedBudgetDays[2].IsClosed.Should().Be.EqualTo(false);
            target.InjectClosedDaysFromSkillDays();

            selectedBudgetDays[0].IsClosed.Should().Be.EqualTo(false);
            selectedBudgetDays[1].IsClosed.Should().Be.EqualTo(true);
            selectedBudgetDays[2].IsClosed.Should().Be.EqualTo(false);
        }
    }
}
