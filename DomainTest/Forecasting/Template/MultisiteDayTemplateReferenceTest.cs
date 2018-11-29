using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting.Template
{
    [TestFixture, SetUICulture("en-US")]
    public class MultisiteDayTemplateReferenceTest
    {
        private MultisiteDay target;
        private IMultisiteSkill _skill;
        private DateOnly _dt;
        private IScenario _scenario;
        private IList<IMultisitePeriod> _multisitePeriods;
        private IList<ISkillDay> _childSkillDays;
        private IChildSkill _childSkill1;
        private IChildSkill _childSkill2;
        private ISkillDay _multisiteSkillDay;
        private MultisiteSkillDayCalculator calculator;

		private void setup()
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

            IDictionary<IChildSkill, Percent> distribution = new Dictionary<IChildSkill, Percent>();
            distribution.Add(_childSkill1, new Percent(0.6));
            distribution.Add(_childSkill2, new Percent(0.4));
            MultisitePeriod multisitePeriod = new MultisitePeriod(
                TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(_dt.Date.Add(TimeSpan.FromHours(4)), _dt.Date.Add(TimeSpan.FromHours(19)), TimeZoneInfoFactory.StockholmTimeZoneInfo()),
                distribution);

            _multisitePeriods = new List<IMultisitePeriod> { multisitePeriod };

            _skill.SetId(Guid.NewGuid());

            target = new MultisiteDay(_dt, _skill, _scenario);
            target.SetMultisitePeriodCollection(_multisitePeriods);

            calculator = new MultisiteSkillDayCalculator(_skill, new List<ISkillDay> { _multisiteSkillDay },
                                                         new List<IMultisiteDay> { target }, new DateOnlyPeriod(_dt, _dt.AddDays(1)));
            calculator.SetChildSkillDays(_childSkill1, new List<ISkillDay> { _childSkillDays[0] });
            calculator.SetChildSkillDays(_childSkill2, new List<ISkillDay> { _childSkillDays[1] });
        }
        [Test]
        public void VerifyCanGetProperties()
        {
	        setup();
            Assert.AreEqual(_skill,((MultisiteDayTemplateReference) target.TemplateReference).MultisiteSkill);
        }
        [Test]
        public void VerifyCanSetAndGetName()
        {
			setup();
			target.TemplateReference.TemplateName = TemplateReference.LongtermTemplateKey;
            Assert.AreEqual(TemplateReference.LongtermTemplateKey, target.TemplateReference.TemplateName);
        }

        [Test]
        public void CanApplyTemplate()
        {
			setup();
			IList<ITemplateMultisitePeriod> multisitePeriods = new List<ITemplateMultisitePeriod>();
            //SkillPersonData skillPersonData = new SkillPersonData(11, 27);

            DateTimePeriod timePeriod = new DateTimePeriod(
            TimeZoneInfo.ConvertTimeToUtc(SkillDayTemplate.BaseDate.Date, _skill.TimeZone).Add(new TimeSpan(4, 0, 0)),
            TimeZoneInfo.ConvertTimeToUtc(SkillDayTemplate.BaseDate.Date.Add(new TimeSpan(19, 0, 0)), _skill.TimeZone));

            multisitePeriods.Add(
                new TemplateMultisitePeriod(timePeriod,
                    new Dictionary<IChildSkill, Percent>()));

            const string baseTemplateName = "GROUNDHOGDAY";
            const string templateName = "<" + baseTemplateName + ">";
            MultisiteDayTemplate multisiteDayTemplate = CreateAndAddMultisiteDayTemplate(templateName, multisitePeriods);

            //Sholud not be equal
            Assert.AreNotEqual(templateName, target.TemplateReference.TemplateName);

            //Apply the template
            target.ApplyTemplate(multisiteDayTemplate);

            //Should be equal
            Assert.AreEqual(multisiteDayTemplate.VersionNumber, target.TemplateReference.VersionNumber);
            Assert.AreEqual(templateName, target.TemplateReference.TemplateName);

            IList<ITemplateMultisitePeriod> multisitePeriods2 = new List<ITemplateMultisitePeriod>();

            DateTimePeriod timePeriod2 = new DateTimePeriod(
            TimeZoneInfo.ConvertTimeToUtc(SkillDayTemplate.BaseDate.Date, _skill.TimeZone).Add(new TimeSpan(1, 0, 0)),
            TimeZoneInfo.ConvertTimeToUtc(SkillDayTemplate.BaseDate.Date.Add(new TimeSpan(2, 0, 0)), _skill.TimeZone));
            multisitePeriods2.Add(
                new TemplateMultisitePeriod(timePeriod2,
                    new Dictionary<IChildSkill, Percent>()));

            ChildSkill childSkill = new ChildSkill("hej", "kom och hjälp", Color.Black, _skill);

            ((TemplateMultisitePeriod)multisiteDayTemplate.TemplateMultisitePeriodCollection[0]).SetPercentage(childSkill, new Percent(0.1));
            Assert.Greater(multisiteDayTemplate.VersionNumber, target.TemplateReference.VersionNumber);
			Assert.AreNotEqual(templateName, target.TemplateReference.TemplateName);

            target.ApplyTemplate(multisiteDayTemplate);
            Assert.AreEqual(multisiteDayTemplate.VersionNumber, target.TemplateReference.VersionNumber);
            Assert.AreEqual(templateName, target.TemplateReference.TemplateName);

            _skill.RemoveTemplate(TemplateTarget.Multisite, templateName);
            Assert.AreEqual("<DELETED>", target.TemplateReference.TemplateName);
        }

        private MultisiteDayTemplate CreateAndAddMultisiteDayTemplate(string templateName, IList<ITemplateMultisitePeriod> multisitePeriods)
        {
            MultisiteDayTemplate multisiteDayTemplate = new MultisiteDayTemplate(templateName, multisitePeriods);
            ((IEntity)multisiteDayTemplate).SetId(Guid.NewGuid());
            _skill.AddTemplate(multisiteDayTemplate);
            return multisiteDayTemplate;
        }

    }
}
