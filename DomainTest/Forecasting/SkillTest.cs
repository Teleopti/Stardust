using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.Scheduling;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    /// <summary>
    /// Tests for Skill
    /// </summary>
    [TestFixture, SetUICulture("en-US")]
    public class SkillTest
    {
        private ISkill target;
        private const string _name = "Skill - Name";
        private const string _description = "Skill - Description";
        private readonly Color _displayColor = Color.FromArgb(234);
	    private ISkillPriorityProvider _skillPriorityProvider;

        /// <summary>
        /// Setup
        /// </summary>
        [SetUp]
        public void Setup()
        {
            target = new Skill(_name, _description, _displayColor, 15, SkillTypeFactory.CreateSkillType());
			_skillPriorityProvider = new SkillPriorityProvider();
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
            foreach(DayOfWeek dayOfWeek in Enum.GetValues(typeof(DayOfWeek)))
            {
                existingTemplates.Add((ISkillDayTemplate)target.GetTemplateAt(TemplateTarget.Skill, (int)dayOfWeek));
            }
            Assert.AreEqual(7, existingTemplates.Count);
        }
        [Test]
        public void VerifyTimeZone()
        {
            Skill skill = new Skill(_name, _description, _displayColor, 15, target.SkillType);
            //skill.TimeZone = TimeZoneInfo.Utc;
            //skill.Activity = ActivityFactory.CreateActivity("activity");
            Assert.AreEqual(TimeZoneInfo.Utc,skill.TimeZone);

            TimeZoneInfo inf = TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time");

            skill.TimeZone = (inf);
            Assert.AreSame(inf,skill.TimeZone);
        }
        [Test]
        public void CanGetTemplateWeekdaysByDayOfWeek()
        {
            IList<IForecastDayTemplate> existingTemplates = new List<IForecastDayTemplate>();
            foreach (DayOfWeek dayOfWeek in Enum.GetValues(typeof(DayOfWeek)))
            {
                existingTemplates.Add(target.GetTemplate(TemplateTarget.Skill, dayOfWeek));
            }
            Assert.AreEqual(7, existingTemplates.Count);
        }

        [Test]
        public void VerifyCannotAddIfTemplateIsNull()
        {
            int cnt = target.TemplateWeekCollection.Count;
            const IForecastDayTemplate newTemplate = null;
            target.SetTemplateAt(0,newTemplate);

            Assert.AreEqual(cnt, target.TemplateWeekCollection.Count);
        }

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

            const string name = "<MONDAY>";

            ISkillDayTemplate newTemplate = new SkillDayTemplate(name, new List<ITemplateSkillDataPeriod>());
            Assert.IsFalse(newTemplate.DayOfWeek.HasValue);
            target.SetTemplateAt((int)DayOfWeek.Wednesday, newTemplate);

            Assert.AreEqual(DayOfWeek.Wednesday, newTemplate.DayOfWeek.Value);
            Assert.AreEqual(newTemplate, target.GetTemplateAt(TemplateTarget.Skill, (int)DayOfWeek.Wednesday));
        }

        [Test]
        public void CanAddTemplate()
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

            SkillDayTemplate skillDayTemplate = new SkillDayTemplate("<MIDSOMMARDAGEN>", skillDataPeriods);

            int currentHeighestKey = target.TemplateWeekCollection.Count - 1;
            int currentKey = target.AddTemplate(skillDayTemplate);

            Assert.IsTrue(ContainsTemplate(skillDayTemplate));
            Assert.IsFalse(ContainsTemplate(new SkillDayTemplate("Other", new List<ITemplateSkillDataPeriod>())));
            Assert.AreEqual(8, target.TemplateWeekCollection.Count); //0-6 + 1
            Assert.AreEqual("<MIDSOMMARDAGEN>", target.GetTemplateAt(TemplateTarget.Skill, 7).Name);
            Assert.AreEqual(currentHeighestKey + 1, currentKey);

            target.RemoveTemplate("<MIDSOMMARDAGEN>");
            Assert.AreEqual(7, target.TemplateWeekCollection.Count);
            IForecastDayTemplate forecastDayTemplate = skillDayTemplate;
            target.AddTemplate(forecastDayTemplate);
            Assert.AreEqual(8, target.TemplateWeekCollection.Count);
        }

        [Test]
        public void CanAddTemplateTwiceAndRemoveAndAddAgain()
        {
            ISkillDayTemplate firstTemplate = target.TemplateWeekCollection[0].NoneEntityClone();
            firstTemplate.Name = "<JULAFTON>";
            int currentKey = target.AddTemplate(firstTemplate);

            Assert.AreEqual(7, currentKey);

            ISkillDayTemplate secondTemplate = target.TemplateWeekCollection[0].NoneEntityClone();
            secondTemplate.Name = "<JULDAGEN>";
            currentKey = target.AddTemplate(secondTemplate);

            Assert.AreEqual(8, currentKey);

            target.RemoveTemplate(TemplateTarget.Skill, "<JULAFTON>");

            currentKey = target.AddTemplate(firstTemplate);
            Assert.AreEqual(9, currentKey);
        }

        [Test]
        public void CanAddTemplateUsingDayOfWeek()
        {
            SkillDayTemplate skillDayTemplate = new SkillDayTemplate("<MIDSOMMARDAGEN>", new List<ITemplateSkillDataPeriod>());

            target.SetTemplate(DayOfWeek.Tuesday, skillDayTemplate);

            Assert.AreEqual(7, target.TemplateWeekCollection.Count); //0-6 + 1
            Assert.AreEqual(skillDayTemplate, target.GetTemplate(TemplateTarget.Skill, DayOfWeek.Tuesday));
        }

        [Test]
        public void CanRemoveTemplate()
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

            SkillDayTemplate skillDayTemplate = new SkillDayTemplate("<MIDSOMMARDAGEN>", skillDataPeriods);
            target.AddTemplate(skillDayTemplate);

            Assert.IsTrue(ContainsTemplate(skillDayTemplate));
            Assert.AreEqual(8, target.TemplateWeekCollection.Count); //0-6 + 1
            Assert.AreEqual("<MIDSOMMARDAGEN>", target.GetTemplateAt(TemplateTarget.Skill, 7).Name);

            target.RemoveTemplate(TemplateTarget.Skill, "<MIDSOMMARDAGEN>");

            Assert.AreEqual(7,target.TemplateWeekCollection.Count);

            target.AddTemplate(skillDayTemplate);

            Assert.IsTrue(ContainsTemplate(skillDayTemplate));
            Assert.AreEqual(8, target.TemplateWeekCollection.Count); //0-6 + 1
            Assert.AreEqual("<MIDSOMMARDAGEN>", target.GetTemplateAt(7).Name);
            target.RemoveTemplate("<MIDSOMMARDAGEN>");
            Assert.AreEqual(7, target.TemplateWeekCollection.Count);
        }

        private bool ContainsTemplate(SkillDayTemplate dayTemplate)
        {
            foreach (KeyValuePair<int, ISkillDayTemplate> template in target.TemplateWeekCollection)
            {
                if (dayTemplate == template.Value)
                {
                    return true;
                }
            }
            return false;
        }

        [Test]
        public void CanFindTemplateByName()
        {
            IForecastDayTemplate template = target.TryFindTemplateByName("<mon>");
            Assert.AreEqual("<MON>", template.Name);

            template = target.TryFindTemplateByName(TemplateTarget.Skill, "NONAMEDAY");
            Assert.IsNull(template);
        }
        /// <summary>
        /// Constructor works.
        /// </summary>
        [Test]
        public void CanCreateSkillObject()
        {
            target = new Skill(_name, _description, _displayColor, 15, SkillTypeFactory.CreateSkillType());
            Assert.IsNotNull(target);
        }

      
        [Test]
        public void VerifyTimeZoneCacheWorksWhenReadFromDatabase()
        {
            const string timeZoneName = "W. Europe Standard Time";
            target.GetType().GetField("_timeZone",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(target,timeZoneName);
            Assert.AreEqual(TimeZoneInfo.FindSystemTimeZoneById(timeZoneName).Id,((TimeZoneInfo)target.TimeZone).Id);
        }

        [Test]
        public void CanSetPropertiesAndAddWorkload()
        {
            const int defaultResolution = 13;
            const string skillName = "Tha Skill Name";
            const string skillDescription = "Tha Skill Description";
            Color skillColor = Color.DarkSlateGray;
            SkillType skillType = SkillTypeFactory.CreateSkillType();
            Activity activity = new Activity("Ehh. test activity?");
            TimeSpan midnightBreakOffset = new TimeSpan(2, 0, 0);
            
            target.DefaultResolution = defaultResolution;
			target.ChangeName(skillName);
            target.Description = skillDescription;
            target.DisplayColor = skillColor;
            target.SkillType = skillType;
            target.Activity = activity;
            target.MidnightBreakOffset = midnightBreakOffset;
            target.IsVirtual = true;
            target.AddAggregateSkill(new Skill("EttSkill", "Summa", Color.DodgerBlue, 15, skillType));

            Assert.AreEqual(defaultResolution,target.DefaultResolution);
            Assert.AreEqual(skillName, target.Name);
            Assert.AreEqual(skillDescription, target.Description);
            Assert.AreEqual(skillColor.ToArgb(), target.DisplayColor.ToArgb());
            Assert.AreEqual(skillType, target.SkillType);
            Assert.AreEqual(target.Activity,activity);
            Assert.AreEqual(target.MidnightBreakOffset,midnightBreakOffset);
            Assert.AreEqual(true, target.IsVirtual);
            Assert.AreEqual(1, target.AggregateSkills.Count);
        }
        [Test]
        public void CanAddAndRemoveAggregateSkill()
        {
            ISkillType type = SkillTypeFactory.CreateSkillType();
            ISkill skill = new Skill("EttSkill", "Summa", Color.DodgerBlue, 15, type);
            target.AddAggregateSkill(skill);
            Assert.AreEqual(1, target.AggregateSkills.Count);
            target.RemoveAggregateSkill(skill);
            Assert.AreEqual(0, target.AggregateSkills.Count);
        }
        [Test]
        public void CanClearAggregatedSkills()
        {
            ISkillType type = SkillTypeFactory.CreateSkillType();
            ISkill skill = new Skill("EttSkill", "Summa", Color.DodgerBlue, 15, type);
            ISkill skill2 = new Skill("EttSkillTill", "SummaSumma", Color.DodgerBlue, 15, type);
            target.AddAggregateSkill(skill);
            target.AddAggregateSkill(skill2);
            Assert.AreEqual(2,target.AggregateSkills.Count);
            target.ClearAggregateSkill();
            Assert.AreEqual(0,target.AggregateSkills.Count);
        }

        [Test]
        public void CanAddWorkload()
        {
            //Add a forecast
            ISkill skill = SkillFactory.CreateSkill("SkillName", SkillTypeFactory.CreateSkillType(), 15);
            IWorkload workload = new Workload(skill);
            target.AddWorkload(workload);

            Assert.AreEqual(workload, target.WorkloadCollection.First());
            Assert.AreEqual(1, target.WorkloadCollection.Count());
        }
        /// <summary>
        /// Verifies that default color is set if no color is choosen
        /// </summary>
        [Test]
        public void CanSetDefaultColorWhenNoColor()
        {
            target = new Skill(_name, _description, _displayColor, 15, SkillTypeFactory.CreateSkillType());
            target.DisplayColor = Color.Empty;
            Assert.AreEqual(Color.Red.ToArgb(), target.DisplayColor.ToArgb());
        }

        /// <summary>
        /// Determines whether this instance [can set time zone].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-21
        /// </remarks>
        [Test]
        public void CanSetTimeZone()
        {
            target.TimeZone = (TimeZoneInfo.Local);
            Assert.AreEqual(TimeZoneInfo.Local.Id, target.TimeZone.Id);
        }

        /// <summary>
        /// Determines whether this instance [can get time zone default].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-21
        /// </remarks>
        [Test]
        public void CanGetTimeZoneDefault()
        {
            target = new Skill(target.Name, target.Description, target.DisplayColor, target.DefaultResolution, target.SkillType);
            Assert.AreEqual(TimeZoneInfo.Utc, target.TimeZone);
        }

        /// <summary>
        /// Verifies the time zone cannot be null.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-21
        /// </remarks>
        [Test]
        public void VerifyTimeZoneCannotBeNull()
        {
			Assert.Throws<ArgumentNullException>(() => target.TimeZone = null);
        }

        /// <summary>
        /// Verifies that name cannot be set to String.Empty
        /// </summary>
        [Test]
        public void NameCannotBeStringEmpty()
        {
			Assert.Throws<ArgumentException>(() => target.ChangeName(string.Empty));
		}

        [Test]
        public void VerifyNameCannotBeTooLong()
        {
			Assert.Throws<ArgumentException>(() => target.ChangeName(string.Empty.PadRight(51)));
        }

        [Test]
        public void ThrowsExceptionIfTryingToRemoveWorkloadNotInList()
        {
            ISkill skill = SkillFactory.CreateSkill("SkillName", SkillTypeFactory.CreateSkillType(), 15);
			Assert.Throws<MissingMemberException>(() => target.RemoveWorkload(new Workload(skill)));
        }
        
        #region Workload

        /// <summary>
        /// Test to see if we can create a forecast
        /// </summary>
        [Test]
        public void CanAddForecast()
        {
            ISkill skill = SkillFactory.CreateSkill("SkillName", SkillTypeFactory.CreateSkillType(), 15);
            IWorkload workload = new Workload(skill);
            skill.AddWorkload(workload);
            Assert.Contains(workload, skill.WorkloadCollection.ToList());
        }


        /// <summary>
        /// Determines whether this instance [can remove workload].
        /// </summary>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2007-11-19
        /// </remarks>
        [Test]
        public void CanRemoveWorkload()
        {
            ISkill skill = SkillFactory.CreateSkill("SkillName", SkillTypeFactory.CreateSkillType(), 15);
            IWorkload workload = new Workload(skill);
            Assert.Contains(workload, skill.WorkloadCollection.ToList());
            skill.RemoveWorkload(workload);
            Assert.AreEqual(0, skill.WorkloadCollection.Count());
        }

        /// <summary>
        /// Verifies that the reference back to Skill works from a forecast.
        /// </summary>
        [Test]
        public void VerifyReferenceBackToSkillWorksFromAForecast()
        {
            ISkill skill = SkillFactory.CreateSkill("SkillName", SkillTypeFactory.CreateSkillType(), 15);

            IWorkload workload = new Workload(skill);
            skill.AddWorkload(workload);

            Assert.AreSame(skill, workload.Skill);
        }

        /// <summary>
        /// Null layers are not allowed.
        /// </summary>
        [Test]
        public void NullForecastsAreNotAllowed()
        {
            ISkill skill = SkillFactory.CreateSkill("SkillName", SkillTypeFactory.CreateSkillType(), 15);
			Assert.Throws<ArgumentNullException>(() => skill.AddWorkload(null));
        }

        #endregion

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

        [Test]
        public void VerifyToString()
        {
            Assert.AreEqual("Skill - Name, Skill, no id", target.ToString());
        }

        [Test]
        public void VerifyThresholds()
        {
            Assert.AreEqual(StaffingThresholds.DefaultValues(),target.StaffingThresholds);
            StaffingThresholds staffingThresholds = new StaffingThresholds(new Percent(0.2), new Percent(0.3),
                                                                           new Percent(0.4));
            target.StaffingThresholds = staffingThresholds;
            Assert.AreEqual(staffingThresholds,target.StaffingThresholds);
        }

        [Test]
        public void CanSetDefaultTemplates()
        {
            IScenario scenario = ScenarioFactory.CreateScenarioAggregate();
            var date = new DateOnly(2008, 7, 1);
            SkillDay skillDay1 = new SkillDay(date, target, scenario, new List<IWorkloadDay>(), new List<ISkillDataPeriod>());
            SkillDay skillDay2 = new SkillDay(date.AddDays(1), target, scenario, new List<IWorkloadDay>(), new List<ISkillDataPeriod>());
            var calculator = new SkillDayCalculator(target, new List<ISkillDay> { skillDay1, skillDay2 }, new DateOnlyPeriod(date, date.AddDays(2)));

            target.SetDefaultTemplates(calculator.SkillDays);

            Assert.AreEqual(DayOfWeek.Tuesday, skillDay1.TemplateReference.DayOfWeek.Value);
            Assert.AreEqual(DayOfWeek.Wednesday, skillDay2.TemplateReference.DayOfWeek.Value);
        }

        [Test]
        public void CanGetTemplates()
        {
            var result = target.GetTemplates(TemplateTarget.Skill);
            Assert.AreEqual(7, result.Count);
            Assert.IsInstanceOf<SkillDayTemplate>(result[0]);
        }
        [Test]
        public void CanApplyTemplateByName()
        {
            ISkillDay skillDay1 = SkillDayFactory.CreateSkillDay(target, new DateOnly(2008, 7, 1));
            ISkillDay skillDay2 = SkillDayFactory.CreateSkillDay(target, new DateOnly(2008, 7, 2));

            IList<ITemplateSkillDataPeriod> skillDataPeriodCollection = new List<ITemplateSkillDataPeriod>();
            ITemplateSkillDataPeriod skillDataPeriod = new TemplateSkillDataPeriod(
                            new ServiceAgreement(
                                new ServiceLevel(new Percent(0.8), 
                                    20),
                                    new Percent(0.3), 
                                    new Percent(0.9)),
                            new SkillPersonData(
                                0,
                                0),
                            new DateTimePeriod(
                DateTime.SpecifyKind(SkillDayTemplate.BaseDate.Date.Add(TimeSpan.FromHours(0)), DateTimeKind.Utc),
                DateTime.SpecifyKind(SkillDayTemplate.BaseDate.Date.Add(TimeSpan.FromDays(1)), DateTimeKind.Utc)));

            skillDataPeriod.Shrinkage = new Percent(0);
            skillDataPeriodCollection.Add(skillDataPeriod);

            const string templateName = "<Extra>";
            SkillDayCalculator calculator = new SkillDayCalculator(target, new List<ISkillDay> { skillDay1, skillDay2 }, new DateOnlyPeriod(new DateOnly(2008, 7, 1), new DateOnly(2008, 7, 2)));
            Assert.IsNotNull(calculator);
            ISkillDayTemplate skillDayTemplate = new SkillDayTemplate(templateName, skillDataPeriodCollection);
            skillDayTemplate.SetId(Guid.NewGuid());
            target.SetTemplateAt(7,skillDayTemplate);

            target.SetTemplatesByName(TemplateTarget.Skill, templateName, new List<ITemplateDay> { skillDay1, skillDay2 });

            Assert.AreEqual(templateName, skillDay1.TemplateReference.TemplateName);
            Assert.AreEqual(templateName, skillDay2.TemplateReference.TemplateName);
        }

        [Test]
        public void VerifyCanRefreshTemplatesFromNewlyLoadedInstance()
        {
            target.SetId(Guid.NewGuid());

            ISkillDayTemplate skillDayTemplate1 = new SkillDayTemplate("To be removed",
                                                                       new List<ITemplateSkillDataPeriod>());
            target.AddTemplate(skillDayTemplate1);

            ISkill clonedSkill = target.EntityClone();
            ISkillDayTemplate skillDayTemplate2 = new SkillDayTemplate("To be added",
                                                                       new List<ITemplateSkillDataPeriod>());
            clonedSkill.RemoveTemplate("To be removed");
            clonedSkill.AddTemplate(skillDayTemplate2);

            Assert.AreEqual(8,target.TemplateWeekCollection.Count);
            Assert.IsTrue(target.TemplateWeekCollection.Values.Contains(skillDayTemplate1));
            Assert.IsFalse(target.TemplateWeekCollection.Values.Contains(skillDayTemplate2));
            target.RefreshTemplates(clonedSkill);
            Assert.AreEqual(8, target.TemplateWeekCollection.Count);
            Assert.IsFalse(target.TemplateWeekCollection.Values.Contains(skillDayTemplate1));
            Assert.IsTrue(target.TemplateWeekCollection.Values.Contains(skillDayTemplate2));
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
        public void CanClone()
        {
            target.SetId(Guid.NewGuid());
            ISkill skillClone = (ISkill)target.Clone();
            Assert.IsFalse(skillClone.Id.HasValue);
            Assert.AreEqual(target.TemplateWeekCollection.Count, skillClone.TemplateWeekCollection.Count);
            Assert.AreSame(target, target.TemplateWeekCollection[0].Parent);
            Assert.AreSame(skillClone, skillClone.TemplateWeekCollection[0].Parent);
            Assert.AreEqual(target.Description, skillClone.Description);
            Assert.AreEqual(target.BusinessUnit, skillClone.BusinessUnit);
            Assert.AreEqual(target.Name, skillClone.Name);
            Assert.AreEqual(target.WorkloadCollection.Count(), skillClone.WorkloadCollection.Count());
            Assert.AreEqual(target.SkillType, skillClone.SkillType);

            skillClone = target.NoneEntityClone();
            Assert.IsFalse(skillClone.Id.HasValue);
            Assert.AreEqual(target.TemplateWeekCollection.Count, skillClone.TemplateWeekCollection.Count);
            Assert.AreSame(target, target.TemplateWeekCollection[0].Parent);
            Assert.AreSame(skillClone, skillClone.TemplateWeekCollection[0].Parent);
            Assert.AreEqual(target.Description, skillClone.Description);
            Assert.AreEqual(target.BusinessUnit, skillClone.BusinessUnit);
            Assert.AreEqual(target.Name, skillClone.Name);
            Assert.AreEqual(target.WorkloadCollection.Count(), skillClone.WorkloadCollection.Count());
            Assert.AreEqual(target.SkillType, skillClone.SkillType);

            skillClone = target.EntityClone();
            Assert.AreEqual(target.Id.Value, skillClone.Id.Value);
            Assert.AreEqual(target.TemplateWeekCollection.Count, skillClone.TemplateWeekCollection.Count);
            Assert.AreSame(target, target.TemplateWeekCollection[0].Parent);
            Assert.AreSame(skillClone, skillClone.TemplateWeekCollection[0].Parent);
            Assert.AreEqual(target.Description, skillClone.Description);
            Assert.AreEqual(target.BusinessUnit, skillClone.BusinessUnit);
            Assert.AreEqual(target.Name, skillClone.Name);
            Assert.AreEqual(target.WorkloadCollection.Count(), skillClone.WorkloadCollection.Count());
            Assert.AreEqual(target.SkillType, skillClone.SkillType);
        }

        [Test]
        public void VerifyPriorityAndValue()
        {
            Assert.AreEqual(4, _skillPriorityProvider.GetPriority(target));
            Assert.AreEqual(1, _skillPriorityProvider.GetPriorityValue(target));
            
            ((ISkillPriority)target).Priority = 1;
            Assert.AreEqual(1, _skillPriorityProvider.GetPriority(target));
            Assert.AreEqual(.16, _skillPriorityProvider.GetPriorityValue(target));

			((ISkillPriority)target).Priority = 2;
            Assert.AreEqual(2, _skillPriorityProvider.GetPriority(target));
            Assert.AreEqual(.32, _skillPriorityProvider.GetPriorityValue(target));

			((ISkillPriority)target).Priority = 3;
            Assert.AreEqual(3, _skillPriorityProvider.GetPriority(target));
            Assert.AreEqual(.64, _skillPriorityProvider.GetPriorityValue(target));

			((ISkillPriority)target).Priority = 5;
            Assert.AreEqual(5, _skillPriorityProvider.GetPriority(target));
            Assert.AreEqual(4, _skillPriorityProvider.GetPriorityValue(target));

			((ISkillPriority)target).Priority = 6;
            Assert.AreEqual(6, _skillPriorityProvider.GetPriority(target));
            Assert.AreEqual(16, _skillPriorityProvider.GetPriorityValue(target));

			((ISkillPriority)target).Priority = 7;
            Assert.AreEqual(7, _skillPriorityProvider.GetPriority(target));
            Assert.AreEqual(256, _skillPriorityProvider.GetPriorityValue(target));
        }

        [Test]
        public void VerifyPriorityMustBeBetweenOneAndSeven()
        {
			Assert.Throws<ArgumentOutOfRangeException>(() => ((ISkillPriority)target).Priority = 8);
        }

        [Test]
        public void VerifyPriorityMustBeBetweenOneAndSeven2()
        {
			Assert.Throws<ArgumentOutOfRangeException>(() => ((ISkillPriority)target).Priority = 0);
        }

        [Test]
        public void VerifyOverUnderStaff()
        {
            Assert.AreEqual(new Percent(.50), _skillPriorityProvider.GetOverstaffingFactor(target));
			((ISkillPriority)target).OverstaffingFactor = new Percent(.3);
            Assert.AreEqual(new Percent(.30), _skillPriorityProvider.GetOverstaffingFactor(target));
        }
        [Test]
        public void VerifyOverstaffToHigh()
        {
			Assert.Throws<ArgumentOutOfRangeException>(() => ((ISkillPriority)target).OverstaffingFactor = new Percent(2));
        }
        [Test]
        public void VerifyOverstaffToLow()
        {
			Assert.Throws<ArgumentOutOfRangeException>(() => ((ISkillPriority)target).OverstaffingFactor = new Percent(-1));
        }

		[Test]
		public void AddingMaxSeatSkillToAggregateShouldThrowException()
		{
			ISkill maxSeatSkill = SkillFactory.CreateSiteSkill("poff");
			Assert.Throws<InvalidOperationException>(() => target.AddAggregateSkill(maxSeatSkill));
		}

		[Test]
		public void ShouldThrowIfMaxParallelTasksIsZero()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => target.MaxParallelTasks = 0);
		}

		[Test]
		public void ShouldThrowIfMaxParallelTasksIsBiggerThanHundred()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => target.MaxParallelTasks = 101);
		}
    }
}
