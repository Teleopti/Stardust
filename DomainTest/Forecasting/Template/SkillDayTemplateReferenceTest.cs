using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting.Template
{
    [TestFixture, SetUICulture("en-US")]
    public class SkillDayTemplateReferenceTest
    {
        private SkillDay _skillDay;
        private ISkill _skill;
        private DateTime _dt;
        private IScenario _scenario;
        private IList<ISkillDataPeriod> _skillDataPeriods;
        private SkillDayCalculator _calculator;

        [SetUp]
        public void Setup()
        {
            _dt = new DateTime(2007, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            _skill = new Skill("skill1", "skill1", Color.FromArgb(255), 15, SkillTypeFactory.CreateSkillTypePhone());
            _scenario = ScenarioFactory.CreateScenarioAggregate();
            _skillDataPeriods = new List<ISkillDataPeriod>();
			
            _skillDataPeriods.Add(
                new SkillDataPeriod(
                    new ServiceAgreement(
                        new ServiceLevel(
                            new Percent(0.8), 20),
                                new Percent(0.5),
                                new Percent(0.7)),
                                new SkillPersonData(),
                                new DateTimePeriod(_dt.Add(TimeSpan.FromHours(4)), _dt.Add(TimeSpan.FromHours(19)))));

            _skill.SetId(Guid.NewGuid());

            _skillDay = new SkillDay(new DateOnly(_dt), _skill, _scenario, WorkloadDayFactory.GetWorkloadDaysForTest(_dt, _skill), _skillDataPeriods);
            _skillDay.SetupSkillDay();
            _calculator = new SkillDayCalculator(_skill, new List<ISkillDay> {_skillDay},
                                                 new DateOnlyPeriod(_skillDay.CurrentDate,
                                                                    _skillDay.CurrentDate.AddDays(1)));
            Assert.IsNotNull(_calculator);
        }
        [Test]
        public void VerifyCanGetProperties()
        {
            Assert.AreEqual(_skill, ((SkillDayTemplateReference)_skillDay.TemplateReference).Skill);
        }
        [Test]
        public void VerifyCanSetAndGetName()
        {
            _skillDay.TemplateReference.TemplateName = TemplateReference.LongtermTemplateKey;
            Assert.AreEqual(TemplateReference.LongtermTemplateKey, _skillDay.TemplateReference.TemplateName);
        }
        [Test]
        public void CanReturnNameWithOldTime()
        {
            const string baseTemplateName = "GROUNDHOGDAY";
            const string templateName = "<" + baseTemplateName + ">";
            SkillDayTemplate skillDayTemplate;

            skillDayTemplate = CreateAndAddSkillDayTemplate(templateName);

            // change template, should make reference "OLD"
            skillDayTemplate.TemplateSkillDataPeriodCollection[0].ServiceAgreement =
                new ServiceAgreement(
                        new ServiceLevel(
                            new Percent(0.88), 19),
                                new Percent(0.3),
                                new Percent(0.88));
			var dateTime = new DateTime(2008, 12, 9, 0, 0, 0, DateTimeKind.Utc);
			var localDateTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime, _skill.TimeZone);
			_skillDay.TemplateReference.UpdatedDate = localDateTime;
			var expectedTemplateName = string.Format(CultureInfo.CurrentUICulture, "<{0} {1} {2}>", baseTemplateName, localDateTime.ToShortDateString(), localDateTime.ToShortTimeString());

			Assert.AreEqual(expectedTemplateName, _skillDay.TemplateReference.TemplateName);
        }

        [Test]
        public void CanReturnNameWithDeleted()
        {
            const string baseTemplateName = "NEWYEARSDAY";
            const string templateName = "<" + baseTemplateName + ">";
            
            CreateAndAddSkillDayTemplate(templateName);

            _skill.RemoveTemplate(templateName);
            Assert.AreEqual("<DELETED>", _skillDay.TemplateReference.TemplateName);
        }

        [Test]
        public void CanReturnNameWithRenamedAndDeleted()
        {
            const string baseTemplateName = "NEWYEARSDAY";
            const string templateName = "<" + baseTemplateName + ">";

            SkillDayTemplate skillDayTemplate = CreateAndAddSkillDayTemplate(templateName);

            const string newBaseTemplateName = "NYÅRSDAGEN";
            const string newTemplateName = "<" + newBaseTemplateName + ">";
            skillDayTemplate.Name = newTemplateName;

            _skill.RemoveTemplate(newTemplateName);
            Assert.AreEqual("<DELETED>", _skillDay.TemplateReference.TemplateName);
        }

        private SkillDayTemplate CreateAndAddSkillDayTemplate(string templateName)
        {
            SkillDayTemplate skillDayTemplate;
            IList<ITemplateSkillDataPeriod> skillDataPeriods = new List<ITemplateSkillDataPeriod>();
            SkillPersonData skillPersonData = new SkillPersonData(11, 27);

            DateTimePeriod timePeriod = new DateTimePeriod(
                TimeZoneHelper.ConvertToUtc(SkillDayTemplate.BaseDate.Date, _skill.TimeZone).Add(new TimeSpan(4, 0, 0)),
                TimeZoneHelper.ConvertToUtc(SkillDayTemplate.BaseDate.Date.Add(new TimeSpan(19, 0, 0)), _skill.TimeZone));

            TemplateSkillDataPeriod templateSkilldataPeriod = new TemplateSkillDataPeriod(
                new ServiceAgreement(
                    new ServiceLevel(
                        new Percent(0.22), 19),
                    new Percent(0.3),
                    new Percent(0.88)),
                skillPersonData,
                timePeriod);
            templateSkilldataPeriod.Shrinkage = new Percent(0.2);

            skillDataPeriods.Add(templateSkilldataPeriod);

            skillDayTemplate = new SkillDayTemplate(templateName, skillDataPeriods);
            // set id to simulate nhibernate persistance behaviour
            ((IEntity)skillDayTemplate).SetId(Guid.NewGuid());
            _skill.AddTemplate(skillDayTemplate);

            //Sholud not be equal
            Assert.AreNotEqual(_skillDay.SkillDataPeriodCollection[0].SkillPersonData, skillDayTemplate.TemplateSkillDataPeriodCollection[0].SkillPersonData);
            Assert.AreNotEqual(_skillDay.SkillDataPeriodCollection[0].ServiceAgreement, skillDayTemplate.TemplateSkillDataPeriodCollection[0].ServiceAgreement);
            Assert.AreNotEqual(templateName, _skillDay.TemplateReference.TemplateName);
            //Apply the template
            _skillDay.ApplyTemplate(skillDayTemplate);

            //Should be equal
            Assert.AreEqual(_skillDay.SkillDataPeriodCollection[0].SkillPersonData, skillDayTemplate.TemplateSkillDataPeriodCollection[0].SkillPersonData);
            Assert.AreEqual(_skillDay.SkillDataPeriodCollection[0].ServiceAgreement, skillDayTemplate.TemplateSkillDataPeriodCollection[0].ServiceAgreement);
            Assert.AreEqual(templateName, _skillDay.TemplateReference.TemplateName);
            Assert.AreEqual(_skillDay.SkillDataPeriodCollection[0].Shrinkage, skillDayTemplate.TemplateSkillDataPeriodCollection[0].Shrinkage);
            return skillDayTemplate;
        }
    }
}
