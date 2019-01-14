using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    /// <summary>
    /// Tests for MultiSiteSkill
    /// </summary>
    [TestFixture, SetUICulture("en-US")]
    public class MultisiteSkillTest
    {
        private IMultisiteSkill target;
        private IChildSkill _childSkill1;
        private IChildSkill _childSkill2;
        private ISkillType _skillTypePhone;
        private string _name = "Skill - Name";
        private string _description = "Skill - Description";
        private readonly Color _displayColor = Color.FromArgb(234);
        private DateOnly _dateTime;
		
        [SetUp]
        public void Setup()
        {
            _skillTypePhone = SkillTypeFactory.CreateSkillTypePhone();
            target = new MultisiteSkill(_name, _description, _displayColor, 15, _skillTypePhone);
            _childSkill1 = SkillFactory.CreateChildSkill("Child1", target);
            _childSkill2 = SkillFactory.CreateChildSkill("Child2", target);
        }

        /// <summary>
        /// Verifies the empty constructor.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-21
        /// </remarks>
        [Test]
        public void VerifyEmptyConstructor()
        {
            ReflectionHelper.HasDefaultConstructor(target.GetType());
        }

        /// <summary>
        /// Verifies the type of the instance is created of correct.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-18
        /// </remarks>
        [Test]
        public void VerifyInstanceIsCreatedOfCorrectType()
        {
            Assert.IsNotNull(target);
            Assert.IsInstanceOf<Skill>(target);
        }
		
        /// <summary>
        /// Determines whether this instance [can get template weekdays].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-12
        /// </remarks>
        [Test]
        public void CanGetTemplateWeekdays()
        {
            IList<ISkillDayTemplate> existingTemplates = new List<ISkillDayTemplate>();
            foreach (DayOfWeek dayOfWeek in Enum.GetValues(typeof(DayOfWeek)))
            {
                existingTemplates.Add((ISkillDayTemplate)target.GetTemplate(TemplateTarget.Skill, dayOfWeek));
            }
            Assert.AreEqual(7, existingTemplates.Count);
        }

        /// <summary>
        /// Determines whether this instance [can add template weekday].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-23
        /// </remarks>
        [Test]
        public void CanAddTemplateWeekday()
        {
            IList<ITemplateSkillDataPeriod> skillDataPeriods = new List<ITemplateSkillDataPeriod>();
            skillDataPeriods.Add(
                new TemplateSkillDataPeriod(
                    new ServiceAgreement(
                        new ServiceLevel(
                            new Percent(0.8), 20),
                                new Percent(0.5),
                                new Percent(0.7)),
                                new SkillPersonData(),
                                new DateTimePeriod(
                                    DateTime.SpecifyKind(SkillDayTemplate.BaseDate.Date.Add(
                                        TimeSpan.FromHours(4)), DateTimeKind.Utc),
                                    DateTime.SpecifyKind(SkillDayTemplate.BaseDate.Date.Add(
                                        TimeSpan.FromHours(19)), DateTimeKind.Utc))));

            string name = "<MONDAY>";

            SkillDayTemplate newTemplate = new SkillDayTemplate(name, skillDataPeriods);
            Assert.IsFalse(newTemplate.DayOfWeek.HasValue);
            target.SetTemplateAt((int)DayOfWeek.Wednesday, newTemplate);

            Assert.AreEqual(DayOfWeek.Wednesday, newTemplate.DayOfWeek.Value);
            Assert.AreEqual(newTemplate, target.GetTemplateAt(TemplateTarget.Skill, (int)DayOfWeek.Wednesday));
        }

        /// <summary>
        /// Determines whether this instance [can add template].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-23
        /// </remarks>
        [Test]
        public void CanAddTemplate()
        {
            IList<ITemplateMultisitePeriod> multisitePeriods = new List<ITemplateMultisitePeriod>();
            multisitePeriods.Add(
                new TemplateMultisitePeriod(
                    new DateTimePeriod(
                            DateTime.SpecifyKind(SkillDayTemplate.BaseDate.Date.Add(
                                TimeSpan.FromHours(4)), DateTimeKind.Utc),
                            DateTime.SpecifyKind(SkillDayTemplate.BaseDate.Date.Add(
                                TimeSpan.FromHours(19)), DateTimeKind.Utc)),
                                new Dictionary<IChildSkill, Percent>()));

            MultisiteDayTemplate multisiteDayTemplate = new MultisiteDayTemplate("<MIDSOMMARDAGEN>", multisitePeriods);

            int currentHeighestKey = target.TemplateMultisiteWeekCollection.Count - 1;
            int currentKey = target.AddTemplate(multisiteDayTemplate);

            Assert.IsTrue(ContainsTemplate(multisiteDayTemplate));
            Assert.IsFalse(ContainsTemplate(new MultisiteDayTemplate("Other", new List<ITemplateMultisitePeriod>())));
            Assert.AreEqual(8, target.TemplateMultisiteWeekCollection.Count); //0-6 + 1
            Assert.AreEqual("<MIDSOMMARDAGEN>", target.GetTemplateAt(TemplateTarget.Multisite, 7).Name);
            Assert.AreEqual(currentHeighestKey + 1, currentKey);
        }

        [Test]
        public void CanAddSkillDayTemplate()
        {
            SkillDayTemplate skillDayTemplate = new SkillDayTemplate("<MIDSOMMARDAGEN>", new List<ITemplateSkillDataPeriod>());

            int currentHeighestKey = target.TemplateWeekCollection.Count - 1;
            int currentKey = target.AddTemplate(skillDayTemplate);

            Assert.AreEqual(8, target.TemplateWeekCollection.Count); //0-6 + 1
            Assert.AreEqual("<MIDSOMMARDAGEN>", target.GetTemplateAt(TemplateTarget.Skill, currentKey).Name);
            Assert.AreEqual(currentHeighestKey + 1, currentKey);
        }

        [Test]
        public void CanAddTemplateTwiceAndRemoveAndAddAgain()
        {
            IMultisiteDayTemplate firstTemplate = new MultisiteDayTemplate("<JULAFTON>", new List<ITemplateMultisitePeriod>());
            int currentKey = target.AddTemplate(firstTemplate);

            Assert.AreEqual(7, currentKey);

            IMultisiteDayTemplate secondTemplate = new MultisiteDayTemplate("<JULDAGEN>", new List<ITemplateMultisitePeriod>());
            secondTemplate.Name = "<JULDAGEN>";
            currentKey = target.AddTemplate(secondTemplate);

            Assert.AreEqual(8, currentKey);

            target.RemoveTemplate(TemplateTarget.Multisite, "<JULAFTON>");

            currentKey = target.AddTemplate(firstTemplate);
            Assert.AreEqual(9, currentKey);
        }

        [Test]
        public void CanRemoveSkillDayTemplate()
        {
            SkillDayTemplate skillDayTemplate = new SkillDayTemplate("<MIDSOMMARDAGEN>", new List<ITemplateSkillDataPeriod>());
            target.AddTemplate(skillDayTemplate);

            Assert.AreEqual(8, target.TemplateWeekCollection.Count);
            
            target.RemoveTemplate(TemplateTarget.Skill, "<MIDSOMMARDAGEN>");

            Assert.AreEqual(7, target.TemplateWeekCollection.Count);
        }

        /// <summary>
        /// Determines whether this instance [can remove template].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-20
        /// </remarks>
        [Test]
        public void CanRemoveTemplate()
        {
            IList<ITemplateMultisitePeriod> multisitePeriods = new List<ITemplateMultisitePeriod>();
            multisitePeriods.Add(
                new TemplateMultisitePeriod(
                    new DateTimePeriod(
                            DateTime.SpecifyKind(SkillDayTemplate.BaseDate.Date.Add(
                                TimeSpan.FromHours(4)), DateTimeKind.Utc),
                            DateTime.SpecifyKind(SkillDayTemplate.BaseDate.Date.Add(
                                TimeSpan.FromHours(19)), DateTimeKind.Utc)),
                                new Dictionary<IChildSkill, Percent>()));

            MultisiteDayTemplate multisiteDayTemplate = new MultisiteDayTemplate("<MIDSOMMARDAGEN>", multisitePeriods);

            int currentHeighestKey = target.TemplateMultisiteWeekCollection.Count - 1;
            int currentKey = target.AddTemplate(multisiteDayTemplate);

            Assert.IsTrue(ContainsTemplate(multisiteDayTemplate));
            Assert.IsFalse(ContainsTemplate(new MultisiteDayTemplate("Other", new List<ITemplateMultisitePeriod>())));
            Assert.AreEqual(8, target.TemplateMultisiteWeekCollection.Count); //0-6 + 1
            Assert.AreEqual("<MIDSOMMARDAGEN>", target.GetTemplateAt(TemplateTarget.Multisite,7).Name);
            Assert.AreEqual(currentHeighestKey + 1, currentKey);

            target.RemoveTemplate(TemplateTarget.Multisite, multisiteDayTemplate.Name);

            Assert.AreEqual(7, target.TemplateMultisiteWeekCollection.Count);
        }

        /// <summary>
        /// Determines whether the specified day template contains template.
        /// </summary>
        /// <param name="dayTemplate">The day template.</param>
        /// <returns>
        /// 	<c>true</c> if the specified day template contains template; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-23
        /// </remarks>
        private bool ContainsTemplate(MultisiteDayTemplate dayTemplate)
        {
            return target.TemplateMultisiteWeekCollection.Any(mt => mt.Value.Name == dayTemplate.Name);
        }

        /// <summary>
        /// Determines whether this instance [can find template by name].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-23
        /// </remarks>
        [Test]
        public void CanFindTemplateByName()
        {
            IForecastDayTemplate template = target.TryFindTemplateByName(TemplateTarget.Multisite, "<mon>");
            Assert.IsInstanceOf<MultisiteDayTemplate>(template);
            Assert.AreEqual("<MON>", template.Name);

            template = target.TryFindTemplateByName(TemplateTarget.Multisite, "NONAMEDAY");
            Assert.IsNull(template);
        }

        [Test]
        public void CanFindSkillDayTemplateByName()
        {
            IForecastDayTemplate template = target.TryFindTemplateByName(TemplateTarget.Skill, "<mon>");
            Assert.IsInstanceOf<SkillDayTemplate>(template);
            Assert.AreEqual("<MON>", template.Name);

            template = target.TryFindTemplateByName(TemplateTarget.Skill, "NONAMEDAY");
            Assert.IsNull(template);
        }

        /// <summary>
        /// Verifies the can get all multisite templates.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-22
        /// </remarks>
        [Test]
        public void VerifyCanGetAllMultisiteTemplates()
        {
            Assert.AreEqual(7, target.TemplateMultisiteWeekCollection.Count);
        }

        /// <summary>
        /// Verifies the restrictions can be checked.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-17
        /// </remarks>
        [Test]
        public void VerifyRestrictionsCanBeChecked()
        {
            IRestrictionSet<ISkill> restrictions = target.RestrictionSet;
            target.CheckRestrictions();

            Assert.IsNotNull(restrictions);
        }

        /// <summary>
        /// Verifies the can remove child skill.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-25
        /// </remarks>
        [Test]
        public void VerifyCanRemoveChildSkill()
        {
            Assert.AreEqual(2, target.ChildSkills.Count);
            target.RemoveChildSkill(_childSkill1);
            Assert.IsTrue(((IDeleteTag)_childSkill1).IsDeleted);
            Assert.AreEqual(1, target.ChildSkills.Count);
            Assert.AreEqual(_childSkill2, target.ChildSkills[0]);
        }

        /// <summary>
        /// Verifies the cannot remove if child skill not is member.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-25
        /// </remarks>
        [Test]
        public void VerifyCannotRemoveIfChildSkillNotIsMember()
        {
	        target.RemoveChildSkill(_childSkill1);
	        target.RemoveChildSkill(_childSkill2);
			Assert.AreEqual(0, target.ChildSkills.Count);
            target.RemoveChildSkill(_childSkill1);
            Assert.AreEqual(0, target.ChildSkills.Count);
        }

        /// <summary>
        /// Verifies the cannot remove if child skill is in template.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-02
        /// </remarks>
        [Test]
        public void VerifyCannotRemoveIfChildSkillIsInTemplate()
        {
            MultisiteDayTemplate template = (MultisiteDayTemplate) target.GetTemplateAt(TemplateTarget.Multisite, (int)DayOfWeek.Monday);
            template.SetMultisitePeriodCollection(
                new List<ITemplateMultisitePeriod>
                {
                    new TemplateMultisitePeriod(
                        new DateTimePeriod(
                                    DateTime.SpecifyKind(SkillDayTemplate.BaseDate.Date.Add(
                                        TimeSpan.FromHours(4)), DateTimeKind.Utc),
                                    DateTime.SpecifyKind(SkillDayTemplate.BaseDate.Date.Add(
                                        TimeSpan.FromHours(19)), DateTimeKind.Utc)),
                        new Dictionary<IChildSkill, Percent>())
                });
            template.TemplateMultisitePeriodCollection[0].SetPercentage(_childSkill1, new Percent(0.4));
            template.TemplateMultisitePeriodCollection[0].SetPercentage(_childSkill2, new Percent(0.6));

            Assert.IsTrue(template.TemplateMultisitePeriodCollection[0].IsValid);
            Assert.AreEqual(2, target.ChildSkills.Count);
			Assert.Throws<ArgumentException>(() => target.RemoveChildSkill(_childSkill1));
        }

        /// <summary>
        /// Verifies the can add child skill.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-25
        /// </remarks>
        [Test]
        public void VerifyCanAddChildSkill()
        {
			target.RemoveChildSkill(target.ChildSkills[0]);
			target.RemoveChildSkill(target.ChildSkills[0]);
            Assert.AreEqual(0, target.ChildSkills.Count);
	        _childSkill1 = SkillFactory.CreateChildSkill("Child1", target);
			target.AddChildSkill(_childSkill1);
            Assert.AreEqual(1, target.ChildSkills.Count);
            Assert.AreEqual(_childSkill1, target.ChildSkills[0]);
            target.AddChildSkill(_childSkill1);
            Assert.AreEqual(1, target.ChildSkills.Count);
            Assert.AreEqual(_childSkill1, target.ChildSkills[0]);
        }

        /// <summary>
        /// Verifies the cannot add null.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-25
        /// </remarks>
        [Test]
        public void VerifyCannotAddNull()
        {
			Assert.Throws<ArgumentNullException>(() => target.AddChildSkill(null));
        }

        [Test]
        public void CanSetDefaultTemplates()
        {
            IScenario scenario = ScenarioFactory.CreateScenarioAggregate();
            MultisiteDay multisiteDay1 = new MultisiteDay(new DateOnly(2008, 7, 1),target,scenario);
            MultisiteDay multisiteDay2 = new MultisiteDay(new DateOnly(2008, 7, 2), target, scenario);

            target.SetDefaultTemplates(new List<MultisiteDay> { multisiteDay1, multisiteDay2 });

            Assert.AreEqual(DayOfWeek.Tuesday, multisiteDay1.TemplateReference.DayOfWeek.Value);
            Assert.AreEqual(DayOfWeek.Wednesday, multisiteDay2.TemplateReference.DayOfWeek.Value);
        }

        [Test]
        public void CanSetDefaultSkillDayTemplates()
        {
            IScenario scenario = ScenarioFactory.CreateScenarioAggregate();
            var date = new DateOnly(2008, 7, 1);
            SkillDay skillDay1 = new SkillDay(date, target, scenario, new List<IWorkloadDay>(), new List<ISkillDataPeriod>());
            SkillDay skillDay2 = new SkillDay(date.AddDays(1), target, scenario, new List<IWorkloadDay>(), new List<ISkillDataPeriod>());
            SkillDayCalculator calculator = new SkillDayCalculator(target, new List<ISkillDay> { skillDay1, skillDay2 }, new DateOnlyPeriod(date, date.AddDays(2)));

            target.SetDefaultTemplates(calculator.SkillDays);

            Assert.AreEqual(DayOfWeek.Tuesday, skillDay1.TemplateReference.DayOfWeek.Value);
            Assert.AreEqual(DayOfWeek.Wednesday, skillDay2.TemplateReference.DayOfWeek.Value);
        }

        [Test]
        public void CanAddSkillDayTemplateUsingDayOfWeek()
        {
            SkillDayTemplate skillDayTemplate = new SkillDayTemplate("<MIDSOMMARDAGEN>", new List<ITemplateSkillDataPeriod>());

            target.SetTemplate(DayOfWeek.Tuesday, skillDayTemplate);

            Assert.AreEqual(7, target.TemplateWeekCollection.Count); //0-6 + 1
            Assert.AreEqual(skillDayTemplate, target.GetTemplate(TemplateTarget.Skill, DayOfWeek.Tuesday));
        }

        [Test]
        public void CanAddMultisiteDayTemplateUsingDayOfWeek()
        {
            MultisiteDayTemplate multisiteDayTemplate = new MultisiteDayTemplate("<MIDSOMMARDAGEN>", new List<ITemplateMultisitePeriod>());

            target.SetTemplate(DayOfWeek.Tuesday, multisiteDayTemplate);

            Assert.AreEqual(7, target.TemplateMultisiteWeekCollection.Count); //0-6 + 1
            Assert.AreEqual(multisiteDayTemplate, target.GetTemplate(TemplateTarget.Multisite, DayOfWeek.Tuesday));
        }

        [Test]
        public void VerifyCanRefreshTemplatesFromNewlyLoadedInstance()
        {
            target.SetId(Guid.NewGuid());

            ISkillDayTemplate skillDayTemplate1 = new SkillDayTemplate("To be removed",
                                                                       new List<ITemplateSkillDataPeriod>());
            IMultisiteDayTemplate multisiteDayTemplate1 = new MultisiteDayTemplate("To be removed",
                                                                                   new List<ITemplateMultisitePeriod>());
            target.AddTemplate(skillDayTemplate1);
            target.AddTemplate(multisiteDayTemplate1);

            ISkill clonedSkill = target.EntityClone();
            ISkillDayTemplate skillDayTemplate2 = new SkillDayTemplate("To be added",
                                                                       new List<ITemplateSkillDataPeriod>());
            IMultisiteDayTemplate multisiteDayTemplate2 = new MultisiteDayTemplate("To be added",
                                                                                   new List<ITemplateMultisitePeriod>());
            clonedSkill.RemoveTemplate(TemplateTarget.Skill, "To be removed");
            clonedSkill.RemoveTemplate(TemplateTarget.Multisite, "To be removed");
            clonedSkill.AddTemplate(skillDayTemplate2);
            clonedSkill.AddTemplate(multisiteDayTemplate2);

            Assert.AreEqual(8, target.TemplateWeekCollection.Count);
            Assert.IsTrue(target.TemplateWeekCollection.Values.Contains(skillDayTemplate1));
            Assert.IsFalse(target.TemplateWeekCollection.Values.Contains(skillDayTemplate2));
            Assert.AreEqual(8, target.TemplateMultisiteWeekCollection.Count);
            Assert.IsTrue(target.TemplateMultisiteWeekCollection.Values.Contains(multisiteDayTemplate1));
            Assert.IsFalse(target.TemplateMultisiteWeekCollection.Values.Contains(multisiteDayTemplate2));
            target.RefreshTemplates(clonedSkill);
            Assert.AreEqual(8, target.TemplateWeekCollection.Count);
            Assert.IsFalse(target.TemplateWeekCollection.Values.Contains(skillDayTemplate1));
            Assert.IsTrue(target.TemplateWeekCollection.Values.Contains(skillDayTemplate2));
            Assert.AreEqual(8, target.TemplateMultisiteWeekCollection.Count);
            Assert.IsFalse(target.TemplateMultisiteWeekCollection.Values.Contains(multisiteDayTemplate1));
            Assert.IsTrue(target.TemplateMultisiteWeekCollection.Values.Contains(multisiteDayTemplate2));
        }

        [Test]
        public void VerifyCanRefreshTemplatesFromNewlyLoadedInstanceRequiresSkill()
        {
            target.SetId(Guid.NewGuid());
			Assert.Throws<ArgumentException>(() => target.RefreshTemplates(WorkloadFactory.CreateWorkload(target)));
        }

        [Test]
        public void VerifyCanRefreshTemplatesFromNewlyLoadedInstanceRequiresSameSkill()
        {
            target.SetId(Guid.NewGuid());

            ISkill clonedSkill = target.EntityClone();
            clonedSkill.SetId(Guid.NewGuid());

			Assert.Throws<ArgumentException>(() => target.RefreshTemplates(clonedSkill));
        }

        [Test]
        public void CanGetTemplates()
        {
            var result = target.GetTemplates(TemplateTarget.Multisite);
            Assert.AreEqual(7, result.Count);
            Assert.IsInstanceOf<IMultisiteDayTemplate>(result[0]);

            result = target.GetTemplates(TemplateTarget.Skill);
            Assert.AreEqual(7, result.Count);
            Assert.IsInstanceOf<ISkillDayTemplate>(result[0]);
        }

        [Test]
        public void CanClone()
        {
            target.AddChildSkill(_childSkill1);

            target.SetId(Guid.NewGuid());
            IMultisiteSkill skillClone = (IMultisiteSkill)target.Clone();
            Assert.IsFalse(skillClone.Id.HasValue);
            Assert.AreEqual(target.TemplateWeekCollection.Count, skillClone.TemplateWeekCollection.Count);
            Assert.AreSame(target, target.TemplateWeekCollection[0].Parent);
            Assert.AreSame(skillClone, skillClone.TemplateWeekCollection[0].Parent);
            Assert.AreEqual(target.TemplateMultisiteWeekCollection.Count, skillClone.TemplateMultisiteWeekCollection.Count);
            Assert.AreSame(target, target.TemplateMultisiteWeekCollection[0].Parent);
            Assert.AreSame(skillClone, skillClone.TemplateMultisiteWeekCollection[0].Parent);
            Assert.AreEqual(target.Description, skillClone.Description);
            Assert.AreEqual(target.BusinessUnit, skillClone.BusinessUnit);
            Assert.AreEqual(target.Name, skillClone.Name);
            Assert.AreEqual(target.WorkloadCollection.Count(), skillClone.WorkloadCollection.Count());
            Assert.AreEqual(target.ChildSkills.Count, skillClone.ChildSkills.Count);
            Assert.AreEqual(target.SkillType, skillClone.SkillType);

            skillClone = (IMultisiteSkill) target.NoneEntityClone();
            Assert.IsFalse(skillClone.Id.HasValue);
            Assert.AreEqual(target.TemplateWeekCollection.Count, skillClone.TemplateWeekCollection.Count);
            Assert.AreSame(target, target.TemplateWeekCollection[0].Parent);
            Assert.AreSame(skillClone, skillClone.TemplateWeekCollection[0].Parent);
            Assert.AreEqual(target.TemplateMultisiteWeekCollection.Count, skillClone.TemplateMultisiteWeekCollection.Count);
            Assert.AreSame(target, target.TemplateMultisiteWeekCollection[0].Parent);
            Assert.AreSame(skillClone, skillClone.TemplateMultisiteWeekCollection[0].Parent);
            Assert.AreEqual(target.Description, skillClone.Description);
            Assert.AreEqual(target.BusinessUnit, skillClone.BusinessUnit);
            Assert.AreEqual(target.Name, skillClone.Name);
            Assert.AreEqual(target.WorkloadCollection.Count(), skillClone.WorkloadCollection.Count());
            Assert.AreEqual(target.ChildSkills.Count, skillClone.ChildSkills.Count);
            Assert.AreEqual(target.SkillType, skillClone.SkillType);

            skillClone = (IMultisiteSkill) target.EntityClone();
            Assert.AreEqual(target.Id.Value, skillClone.Id.Value);
            Assert.AreEqual(target.TemplateWeekCollection.Count, skillClone.TemplateWeekCollection.Count);
            Assert.AreSame(target, target.TemplateWeekCollection[0].Parent);
            Assert.AreSame(skillClone, skillClone.TemplateWeekCollection[0].Parent);
            Assert.AreEqual(target.TemplateMultisiteWeekCollection.Count, skillClone.TemplateMultisiteWeekCollection.Count);
            Assert.AreSame(target, target.TemplateMultisiteWeekCollection[0].Parent);
            Assert.AreSame(skillClone, skillClone.TemplateMultisiteWeekCollection[0].Parent);
            Assert.AreEqual(target.Description, skillClone.Description);
            Assert.AreEqual(target.BusinessUnit, skillClone.BusinessUnit);
            Assert.AreEqual(target.Name, skillClone.Name);
            Assert.AreEqual(target.WorkloadCollection.Count(), skillClone.WorkloadCollection.Count());
            Assert.AreEqual(target.ChildSkills.Count, skillClone.ChildSkills.Count);
            Assert.AreEqual(target.SkillType, skillClone.SkillType);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void CanApplyTemplateByName()
        {
            IMultisiteDay multisiteDay;
            IMultisiteSkill skill = SkillFactory.CreateMultisiteSkill("skill1");
            _dateTime = new DateOnly(2007, 1, 1);
            IScenario scenario = ScenarioFactory.CreateScenarioAggregate();
            IList<ISkillDay> childSkillDays;
            IChildSkill childSkill1 = SkillFactory.CreateChildSkill("child1", skill);
            IChildSkill childSkill2 = SkillFactory.CreateChildSkill("child2", skill);
            ISkillDay multisiteSkillDay;
            MultisiteSkillDayCalculator calculator;

            skill.AddChildSkill(childSkill1);
            skill.AddChildSkill(childSkill2);
            multisiteSkillDay = SkillDayFactory.CreateSkillDay(skill, _dateTime);
            childSkillDays = new List<ISkillDay> { 
                SkillDayFactory.CreateSkillDay(childSkill1,_dateTime),
                SkillDayFactory.CreateSkillDay(childSkill2,_dateTime) };

            IDictionary<IChildSkill, Percent> distribution = new Dictionary<IChildSkill, Percent>();
            distribution.Add(childSkill1, new Percent(0.6));
            distribution.Add(childSkill2, new Percent(0.4));
            IMultisitePeriod multisitePeriod = new MultisitePeriod(
                new DateTimePeriod(DateTime.SpecifyKind(_dateTime.Date, DateTimeKind.Utc).Add(TimeSpan.FromHours(4)),
                                   DateTime.SpecifyKind(_dateTime.Date, DateTimeKind.Utc).Add(TimeSpan.FromHours(19))),
                distribution);

            IList<IMultisitePeriod> multisitePeriods = new List<IMultisitePeriod> {multisitePeriod};

            skill.SetId(Guid.NewGuid());

            multisiteDay = new MultisiteDay(_dateTime, skill, scenario);
            multisiteDay.SetMultisitePeriodCollection(multisitePeriods);

            calculator = new MultisiteSkillDayCalculator(skill, new List<ISkillDay> { multisiteSkillDay },
                                                         new List<IMultisiteDay> { multisiteDay }, new DateOnlyPeriod(_dateTime, _dateTime.AddDays(1)));
            calculator.SetChildSkillDays(childSkill1, new List<ISkillDay> { childSkillDays[0] });
            calculator.SetChildSkillDays(childSkill2, new List<ISkillDay> { childSkillDays[1] });

            IList<ITemplateMultisitePeriod> multisitePeriods3 = new List<ITemplateMultisitePeriod>();

            DateTimePeriod timePeriod = new DateTimePeriod(
            TimeZoneInfo.ConvertTimeToUtc(SkillDayTemplate.BaseDate.Date, skill.TimeZone).Add(new TimeSpan(4, 0, 0)),
            TimeZoneInfo.ConvertTimeToUtc(SkillDayTemplate.BaseDate.Date.Add(new TimeSpan(19, 0, 0)), skill.TimeZone));

            multisitePeriods3.Add(
                new TemplateMultisitePeriod(timePeriod,
                    new Dictionary<IChildSkill, Percent>()));

            const string baseTemplateName = "Extra";
            const string templateName = "<" + baseTemplateName + ">";
            IMultisiteDayTemplate multisiteDayTemplate = CreateAndAddMultisiteDayTemplate(templateName, multisitePeriods3, skill );

            ////Apply the template
            multisiteDay.ApplyTemplate(multisiteDayTemplate);
            skill.SetTemplatesByName(TemplateTarget.Multisite, templateName, new List<ITemplateDay> { multisiteDay });

            Assert.AreEqual(templateName, multisiteDay.TemplateReference.TemplateName);
        }

        private static IMultisiteDayTemplate CreateAndAddMultisiteDayTemplate(string templateName, IList<ITemplateMultisitePeriod> multisitePeriods, IMultisiteSkill aSkill)
        {
            IMultisiteDayTemplate multisiteDayTemplate = new MultisiteDayTemplate(templateName, multisitePeriods);
            multisiteDayTemplate.SetId(Guid.NewGuid());
            aSkill.AddTemplate(multisiteDayTemplate);
            return multisiteDayTemplate;
        }

    }
}
