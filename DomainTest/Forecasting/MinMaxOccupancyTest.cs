using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.DomainTest.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class MinMaxOccupancySkillTest : RestrictionTest<ISkill>
    {
        ISkill _testSkill;

        protected override void ConcreteSetup()
        {
            Target = new MinMaxOccupancy();

            _testSkill = SkillFactory.CreateSkill("skill");
        }

        protected override ISkill CreateInvalidEntityToVerify()
        {
            DateTime utcDateTime = TimeZoneInfo.ConvertTimeToUtc(SkillDayTemplate.BaseDate.Date, _testSkill.TimeZone);
            TemplateSkillDataPeriod invalidPeriod = new TemplateSkillDataPeriod(
                new ServiceAgreement(ServiceAgreement.DefaultValues().ServiceLevel, new Percent(0.4), new Percent(0.3)),
                new SkillPersonData(8,7), new DateTimePeriod(utcDateTime, utcDateTime.AddDays(1)));
            SkillDayTemplate template = new SkillDayTemplate("invalid", new List<ITemplateSkillDataPeriod> { invalidPeriod });
            _testSkill.SetTemplateAt((int)DayOfWeek.Monday, template);
            return _testSkill;
        }

        protected override ISkill CreateValidEntityToVerify()
        {
            DateTime utcDateTime = TimeZoneInfo.ConvertTimeToUtc(SkillDayTemplate.BaseDate.Date, _testSkill.TimeZone);
            TemplateSkillDataPeriod validPeriod = new TemplateSkillDataPeriod(ServiceAgreement.DefaultValues(),
                new SkillPersonData(2, 50), new DateTimePeriod(utcDateTime, utcDateTime.AddDays(1)));
            SkillDayTemplate template = new SkillDayTemplate("valid", new List<ITemplateSkillDataPeriod> { validPeriod });
            _testSkill.SetTemplateAt((int)DayOfWeek.Monday, template);
            return _testSkill;
        }
    }

    [TestFixture]
    public class MinMaxOccupancySkillDayTest : RestrictionTest<ISkillDay>
    {
        ISkill _skill;
        IScenario _scenario;
        DateOnly _dateTime;

        protected override void ConcreteSetup()
        {
            Target = new MinMaxOccupancy();

            _dateTime = new DateOnly(2008, 3, 17);
            _scenario = ScenarioFactory.CreateScenarioAggregate();
            _skill = SkillFactory.CreateSkill("skill");
        }

        protected override ISkillDay CreateInvalidEntityToVerify()
        {
            SkillDataPeriod invalidPeriod = new SkillDataPeriod(
                new ServiceAgreement(ServiceAgreement.DefaultValues().ServiceLevel, new Percent(0.4), new Percent(0.3)),
                new SkillPersonData(8, 7), new DateOnlyPeriod(_dateTime, _dateTime).ToDateTimePeriod(_skill.TimeZone));
            SkillDay skillDay = new SkillDay(_dateTime, _skill, _scenario, new List<IWorkloadDay>(), new List<ISkillDataPeriod> { invalidPeriod });
            return skillDay;
        }

        protected override ISkillDay CreateValidEntityToVerify()
        {
            SkillDataPeriod validPeriod = new SkillDataPeriod(ServiceAgreement.DefaultValues(),
                new SkillPersonData(2, 50), new DateOnlyPeriod(_dateTime, _dateTime).ToDateTimePeriod(_skill.TimeZone));
            SkillDay skillDay = new SkillDay(_dateTime, _skill, _scenario, new List<IWorkloadDay>(), new List<ISkillDataPeriod> { validPeriod });
            return skillDay;
        }
    }
}
