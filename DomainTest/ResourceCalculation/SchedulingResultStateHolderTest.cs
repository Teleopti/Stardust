using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class SchedulingResultStateHolderTest
    {
        private DateTimePeriod _period;

        [SetUp]
        public void Setup()
        {
            _period = new DateTimePeriod(new DateTime(2008, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                                       new DateTime(2008, 1, 2, 0, 0, 0, DateTimeKind.Utc));
        }

        [Test]
        public void VerifyProperties()
        {
            Skill skill = new Skill("skilling me softly", "with his song", Color.Red, 50, new SkillTypePhone(new Description("amfal"), ForecastSource.InboundTelephony));
            SchedulingResultStateHolder target = SchedulingResultStateHolderFactory.Create(_period, skill);
			target.SeniorityWorkDayRanks = new SeniorityWorkDayRanks();
            IList<ISkill> list = target.VisibleSkills;
            Assert.IsNotNull(list);
            Assert.AreEqual(1, target.Skills.Length);
            Assert.IsNotNull(target.LoadedAgents);
            Assert.IsNotNull(target.Schedules);
            Assert.IsNotNull(target.SkillDays);
            Assert.IsNotNull(target.SkillStaffPeriodHolder);
			Assert.IsNotNull(target.SeniorityWorkDayRanks);
			Assert.IsFalse(target.UseMaximumWorkday);
        }


        [Test]
        public void VerifySkillDayOnDate()
        {
            MockRepository mocks = new MockRepository();
            DateOnly date1 = new DateOnly(2008, 2, 2);
            DateOnly date2 = new DateOnly(2008, 2, 3);
            DateOnly date3 = new DateOnly(2008, 2, 4);
            IList<DateOnly> dateOnlys = new List<DateOnly> { date1, date2 };
            ISkill skill = mocks.StrictMock<ISkill>();

            ISkillDay skillDay1 = mocks.StrictMock<ISkillDay>();
            ISkillDay skillDay2 = mocks.StrictMock<ISkillDay>();
            ISkillDay skillDay3 = mocks.StrictMock<ISkillDay>();

            IList<ISkillDay> lst = new List<ISkillDay> { skillDay1, skillDay2, skillDay3 };

            SchedulingResultStateHolder target = SchedulingResultStateHolderFactory.Create(_period, skill, lst);

            using (mocks.Record())
            {
                Expect.Call(skillDay1.CurrentDate).Return(date1);
                Expect.Call(skillDay2.CurrentDate).Return(date2);
                Expect.Call(skillDay3.CurrentDate).Return(date3);

            }
            using (mocks.Playback())
            {
                var ret = target.SkillDaysOnDateOnly(dateOnlys);
                Assert.IsNotNull(ret);
                Assert.AreEqual(2, ret.Count());
            }
        }

        [Test]
        public void VerifyNewSkillStaffPeriodHolderIsCreatedWhenSettingSkillDays()
        {
            Skill skill = new Skill("skilling me softly", "with his song", Color.Red, 50, new SkillTypePhone(new Description("amfal"), ForecastSource.InboundTelephony));
            SchedulingResultStateHolder target = SchedulingResultStateHolderFactory.Create(_period, skill);
            var previousHolder = target.SkillStaffPeriodHolder;
            target.SkillDays = new Dictionary<ISkill, IEnumerable<ISkillDay>>();
            Assert.AreNotSame(previousHolder, target.SkillStaffPeriodHolder);
        }

        [Test]
		public void VisibleSkillsShouldContainMaxSeatSkillsAndNonBlendSkills()
		{
			ISkill normal = SkillFactory.CreateSiteSkill("normal");
			ISkill maxSeat = SkillFactory.CreateSiteSkill("maxseat");
			ISkill nonBlend = SkillFactory.CreateNonBlendSkill("nonBlend");
			SchedulingResultStateHolder target = SchedulingResultStateHolderFactory.Create(_period);
			target.AddSkills(normal,maxSeat,nonBlend);
			IList<ISkill> result = target.VisibleSkills;
			Assert.AreEqual(3, result.Count);
		}

        [Test]
        public void ShouldGetRulesWithMinWeekWorkTimeActivated()
        {
            var target = new SchedulingResultStateHolder {UseValidation = true};
            var rules = target.GetRulesToRun();
            var rule = (MinWeekWorkTimeRule)rules.Item(typeof(MinWeekWorkTimeRule));
            Assert.IsNotNull(rule);
        }

		[Test]
		public void ShouldGetRulesWithMaximumWorkTimeActivated()
		{
			var target = new SchedulingResultStateHolder { UseMaximumWorkday = true, UseValidation = true };
			var rules = target.GetRulesToRun();
			var rule = (MaximumWorkdayRule)rules.Item(typeof(MaximumWorkdayRule));
			Assert.IsNotNull(rule);
		}

		[Test]
		public void ShouldGetRulesWithMaximumWorktimeDeActivated()
		{
			var target = new SchedulingResultStateHolder { UseValidation = true };
			var rules = target.GetRulesToRun();
			var rule = (MaximumWorkdayRule)rules.Item(typeof(MaximumWorkdayRule));
			Assert.IsNull(rule);
		}
	}
}
