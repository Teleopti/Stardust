using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class MultisiteDayCloneIssueTest
    {
        private IMultisiteDay target;
        private IMultisiteSkill _skill;
        private DateOnly _dt;
        private IScenario _scenario;
        private IList<IMultisitePeriod> _multisitePeriods;
        private IList<ISkillDay> _childSkillDays;
        private IChildSkill _childSkill1;
        private IChildSkill _childSkill2;
        private ISkillDay _multisiteSkillDay;
        private MultisiteSkillDayCalculator calculator;

        [SetUp]
        public void Setup()
        {
            _dt = new DateOnly(2007, 1, 1);
            _skill = SkillFactory.CreateMultisiteSkill("skill1");
            _scenario = ScenarioFactory.CreateScenarioAggregate();
            _childSkill1 = SkillFactory.CreateChildSkill("child1", _skill);
            _childSkill2 = SkillFactory.CreateChildSkill("child2", _skill);

            _skill.AddChildSkill(_childSkill1);
            _skill.AddChildSkill(_childSkill2);
            _multisiteSkillDay = SkillDayFactory.CreateSkillDay(_skill, _dt);
            _childSkillDays = new List<ISkillDay> { 
                SkillDayFactory.CreateSkillDay(_childSkill1,_dt),
                SkillDayFactory.CreateSkillDay(_childSkill2,_dt) };

            _childSkillDays[0].SetupSkillDay();
            _childSkillDays[1].SetupSkillDay();
            _multisiteSkillDay.SetupSkillDay();

            IDictionary<IChildSkill, Percent> distribution = new Dictionary<IChildSkill, Percent>();
            distribution.Add(_childSkill1, new Percent(0.6));
            distribution.Add(_childSkill2, new Percent(0.4));
            MultisitePeriod multisitePeriod = new MultisitePeriod(
                new DateTimePeriod(DateTime.SpecifyKind(_dt.Date, DateTimeKind.Utc).Add(TimeSpan.FromHours(4)),
                                   DateTime.SpecifyKind(_dt.Date, DateTimeKind.Utc).Add(TimeSpan.FromHours(19))),
                distribution);

            _multisitePeriods = new List<IMultisitePeriod> { multisitePeriod };

            _skill.SetId(Guid.NewGuid());

            target = new MultisiteDay(_dt, _skill, _scenario);
            target.SetMultisitePeriodCollection(_multisitePeriods);

            calculator = new MultisiteSkillDayCalculator(_skill, new List<ISkillDay> { _multisiteSkillDay },
                                                         new List<IMultisiteDay> { target }, new DateOnlyPeriod(_dt, _dt.AddDays(1)));
            calculator.SetChildSkillDays(_childSkill1, new List<ISkillDay> { _childSkillDays[0] });
            calculator.SetChildSkillDays(_childSkill2, new List<ISkillDay> { _childSkillDays[1] });
            calculator.InitializeChildSkills();
        }

        [Test]
        public void ShouldSetValuesAfterClone()
        {
            TaskOwnerPeriod month = new TaskOwnerPeriod(_dt, _childSkillDays.OfType<ITaskOwner>(), TaskOwnerPeriodType.Month);
            _childSkillDays[0].AddParent(month);
            _childSkillDays[1].AddParent(month);

           
            var newCalculator = (MultisiteSkillDayCalculator)calculator.CloneToScenario(ScenarioFactory.CreateScenarioAggregate());
            newCalculator.InitializeChildSkills();
            newCalculator.SkillDays.First().WorkloadDayCollection.First().Tasks = 10000;

            TaskOwnerPeriod newMonth = new TaskOwnerPeriod(_dt,
                                                       newCalculator.SkillDays.OfType<ITaskOwner>(), TaskOwnerPeriodType.Month);
            newCalculator.SkillDays.First().AddParent(newMonth);

            newMonth.Lock();
            newCalculator.SkillDays.First().WorkloadDayCollection[0].Tasks = 12451;
            newMonth.Release();
        }
    }
}
