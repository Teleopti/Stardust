using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

using System.Drawing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting
{
    /// <summary>
    /// Tests for the multisite day class
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-05-06
    /// </remarks>
    [TestFixture, SetUICulture("en-US")]
    public class MultisiteDayTest
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

        /// <summary>
        /// Setups this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-06
        /// </remarks>
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
        }

        /// <summary>
        /// Verifies the empty constructor.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-06
        /// </remarks>
        [Test]
        public void VerifyEmptyConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(target.GetType(), true));
        }

        /// <summary>
        /// Verifies the properties.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-06
        /// </remarks>
        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(_skill, target.Skill);
            Assert.AreEqual(_dt, target.MultisiteDayDate);
            Assert.AreEqual(_scenario, target.Scenario);
            Assert.AreEqual(1, target.MultisitePeriodCollection.Count);
            Assert.AreEqual(_multisitePeriods[0], target.MultisitePeriodCollection[0]);
            Assert.AreEqual(calculator, _multisiteSkillDay.SkillDayCalculator);
        }

        /// <summary>
        /// Verifies null as skill gives exception.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-04
        /// </remarks>
        [Test]
        public void VerifyNullAsSkillGivesException()
        {
			Assert.Throws<ArgumentNullException>(() => target = new MultisiteDay(_dt, null, _scenario));
        }

        /// <summary>
        /// Verifies null as scenario gives exception.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-04
        /// </remarks>
        [Test]
        public void VerifyNullAsScenarioGivesException()
        {
			Assert.Throws<ArgumentNullException>(() => target = new MultisiteDay(_dt, _skill, null));
        }

        /// <summary>
        /// Verifies the split multisite period.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-15
        /// </remarks>
        [Test]
        public void VerifySplitMultisitePeriod()
        {
            Assert.AreEqual(1, target.MultisitePeriodCollection.Count);
            target.SplitMultisitePeriods(
                new List<IMultisitePeriod> 
                { 
                    target.MultisitePeriodCollection[0] 
                });
            Assert.AreEqual(15 * 4, target.MultisitePeriodCollection.Count);
        }

        /// <summary>
        /// Verifies the merge multisite period.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-15
        /// </remarks>
        [Test]
        public void VerifyMergeMultisitePeriod()
        {
            Assert.AreEqual(1, target.MultisitePeriodCollection.Count);
            target.SplitMultisitePeriods(
                new List<IMultisitePeriod> 
                { 
                    target.MultisitePeriodCollection[0] 
                });
            Assert.AreEqual(15 * 4, target.MultisitePeriodCollection.Count);
            var discardedItem = target.MultisitePeriodCollection[0];
            target.MergeMultisitePeriods(
                new List<IMultisitePeriod>(target.MultisitePeriodCollection));
            Assert.AreEqual(1, target.MultisitePeriodCollection.Count);
            Assert.IsNotNull(discardedItem.Parent);
        }

        /// <summary>
        /// Determines whether this instance [can create from template].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-06
        /// </remarks>
        [Test]
        public void CanCreateFromTemplate()
        {
            string name = "<YUCATAN>";
            var createDate = new DateOnly(2008, 01, 14);
            IList<ITemplateMultisitePeriod> templateMultisitePeriods = new List<ITemplateMultisitePeriod>();

            DateTimePeriod timePeriod = new DateTimePeriod(
               TimeZoneInfo.ConvertTimeToUtc(SkillDayTemplate.BaseDate.Date, _skill.TimeZone).Add(new TimeSpan(8, 0, 0)),
               TimeZoneInfo.ConvertTimeToUtc(SkillDayTemplate.BaseDate.Date.Add(new TimeSpan(22, 0, 0)), _skill.TimeZone));

            TemplateMultisitePeriod period =
                new TemplateMultisitePeriod(timePeriod,
                    new Dictionary<IChildSkill, Percent>());
            templateMultisitePeriods.Add(period);

            MultisiteDayTemplate multisiteDayTemplate = new MultisiteDayTemplate(name, templateMultisitePeriods);

            MultisiteDay multisiteDay = new MultisiteDay();
            multisiteDay.CreateFromTemplate(createDate, _skill, _scenario, multisiteDayTemplate);

            Assert.AreEqual(multisiteDay.MultisitePeriodCollection[0].Period.StartDateTime.TimeOfDay, multisiteDayTemplate.TemplateMultisitePeriodCollection[0].Period.StartDateTime.TimeOfDay);
        }

        /// <summary>
        /// Determines whether this instance [can apply template].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-22
        /// </remarks>
        [Test]
        public void CanApplyTemplate()
        {
            const string baseTemplateName = "GROUNDHOGDAY";
            const string templateName = "<" + baseTemplateName + ">";

            ApplyMultisiteDayTemplate(templateName);
        }

        [Test]
        public void VerifyOldAndDeletedTemplate()
        {
            const string baseTemplateName = "GROUNDHOGDAY";
            const string templateName = "<" + baseTemplateName + ">";

            CreateTemplateApplyItAndModifiyIt(baseTemplateName, templateName);
            
            _skill.RemoveTemplate(TemplateTarget.Multisite, templateName);
            Assert.AreEqual("<DELETED>", target.TemplateReference.TemplateName);
        }

        [Test]
        public void VerifyRenameAndDeletedTemplate()
        {
            const string baseTemplateName = "GROUNDHOGDAY";
            const string templateName = "<" + baseTemplateName + ">";

            IMultisiteDayTemplate multisiteDayTemplate = CreateTemplateApplyItAndModifiyIt(baseTemplateName, templateName);

            const string newBaseTemplateName = "NYÅRSDAGEN";
            const string newTemplateName = "<" + newBaseTemplateName + ">";
            multisiteDayTemplate.Name = newTemplateName;

            _skill.RemoveTemplate(TemplateTarget.Multisite, newTemplateName);
            Assert.AreEqual("<DELETED>", target.TemplateReference.TemplateName);
        }
        
        private IMultisiteDayTemplate CreateTemplateApplyItAndModifiyIt(string baseTemplateName, string templateName)
        {
            IMultisiteDayTemplate multisiteDayTemplate = ApplyMultisiteDayTemplate(templateName);
            int templateVersionNumberBeforeChange = multisiteDayTemplate.VersionNumber;

            // modify template to make reference old
            IList<ITemplateMultisitePeriod> multisitePeriods2 = new List<ITemplateMultisitePeriod>();

            DateTimePeriod timePeriod2 = new DateTimePeriod(
                TimeZoneInfo.ConvertTimeToUtc(SkillDayTemplate.BaseDate.Date, _skill.TimeZone).Add(new TimeSpan(1, 0, 0)),
                TimeZoneInfo.ConvertTimeToUtc(SkillDayTemplate.BaseDate.Date.Add(new TimeSpan(2, 0, 0)), _skill.TimeZone));
            multisitePeriods2.Add(
                new TemplateMultisitePeriod(timePeriod2,
                                            new Dictionary<IChildSkill, Percent>()));

            IChildSkill childSkill = new ChildSkill("hej", "kom och hjälp", Color.Black, _skill); 
            
            multisiteDayTemplate.TemplateMultisitePeriodCollection[0].SetPercentage(childSkill, new Percent(0.1));
            int templateVersionNumberAfterChange = multisiteDayTemplate.VersionNumber;
            Assert.Greater(templateVersionNumberAfterChange, templateVersionNumberBeforeChange);
            Assert.Greater(multisiteDayTemplate.VersionNumber, target.TemplateReference.VersionNumber);


			var dateTime = new DateTime(2008, 12, 9, 0, 0, 0, DateTimeKind.Utc);
        	var localDateTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime, _skill.TimeZone);
			target.TemplateReference.UpdatedDate = localDateTime;
			var expectedTemplateName = string.Format(CultureInfo.CurrentUICulture, "<{0} {1} {2}>", baseTemplateName, localDateTime.ToShortDateString(), localDateTime.ToShortTimeString());

			Assert.AreEqual(expectedTemplateName, target.TemplateReference.TemplateName);

            target.ApplyTemplate(multisiteDayTemplate);
            Assert.AreEqual(templateVersionNumberAfterChange, multisiteDayTemplate.VersionNumber);
            Assert.AreEqual(multisiteDayTemplate.VersionNumber, target.TemplateReference.VersionNumber);
            Assert.AreEqual(templateName, target.TemplateReference.TemplateName);
            return multisiteDayTemplate;
        }

        private IMultisiteDayTemplate ApplyMultisiteDayTemplate(string templateName)
        {
            IList<ITemplateMultisitePeriod> multisitePeriods = new List<ITemplateMultisitePeriod>();

            DateTimePeriod timePeriod = new DateTimePeriod(
                TimeZoneInfo.ConvertTimeToUtc(SkillDayTemplate.BaseDate.Date, _skill.TimeZone).Add(new TimeSpan(4, 0, 0)),
                TimeZoneInfo.ConvertTimeToUtc(SkillDayTemplate.BaseDate.Date.Add(new TimeSpan(19, 0, 0)), _skill.TimeZone));

            multisitePeriods.Add(
                new TemplateMultisitePeriod(timePeriod,
                                            new Dictionary<IChildSkill, Percent>()));

            IMultisiteDayTemplate multisiteDayTemplate = CreateAndAddMultisiteDayTemplate(templateName, multisitePeriods);

            //Sholud not be equal
            Assert.AreNotEqual(templateName, target.TemplateReference.TemplateName);

            //Apply the template
            target.ApplyTemplate(multisiteDayTemplate);

            //Should be equal
            Assert.AreEqual(multisiteDayTemplate.VersionNumber, target.TemplateReference.VersionNumber);
            Assert.AreEqual(templateName, target.TemplateReference.TemplateName);
            return multisiteDayTemplate;
        }

        [Test]
        public void VerifyMultisiteDayTemplateReferenceRenamedWithoutOld()
        {
            const string originalName = "Original name";
            const string newName = "New name";
            
            IMultisiteDayTemplate multisiteDayTemplate = ApplyMultisiteDayTemplate(originalName);

            int unmodifiedVersionNumber = multisiteDayTemplate.VersionNumber;

            multisiteDayTemplate.Name = newName;
            Assert.AreEqual(newName, target.TemplateReference.TemplateName);
            Assert.AreEqual(unmodifiedVersionNumber, multisiteDayTemplate.VersionNumber);
            Assert.AreEqual(unmodifiedVersionNumber, target.TemplateReference.VersionNumber);
        }

        private IMultisiteDayTemplate CreateAndAddMultisiteDayTemplate(string templateName, IList<ITemplateMultisitePeriod> multisitePeriods)
        {
            IMultisiteDayTemplate multisiteDayTemplate = new MultisiteDayTemplate(templateName, multisitePeriods);
            multisiteDayTemplate.SetId(Guid.NewGuid());
            _skill.AddTemplate(multisiteDayTemplate);
            return multisiteDayTemplate;
        }

        [Test]
        public void VerifyApplyTemplateTriggersRedistribution()
        {
            IList<ITemplateMultisitePeriod> multisitePeriods = new List<ITemplateMultisitePeriod>();
            DateTimePeriod timePeriod = new DateTimePeriod(
                TimeZoneInfo.ConvertTimeToUtc(SkillDayTemplate.BaseDate.Date, _skill.TimeZone).Add(new TimeSpan(4, 0, 0)),
                TimeZoneInfo.ConvertTimeToUtc(SkillDayTemplate.BaseDate.Date.Add(new TimeSpan(19, 0, 0)),
                                                 _skill.TimeZone));

            IDictionary<IChildSkill, Percent> distributions = new Dictionary<IChildSkill, Percent>();
            distributions.Add(_childSkill1, new Percent(0.1));
            distributions.Add(_childSkill2, new Percent(0.9));
            multisitePeriods.Add(new TemplateMultisitePeriod(timePeriod, distributions));

            IMultisiteDayTemplate multisiteDayTemplate = CreateAndAddMultisiteDayTemplate("mall", multisitePeriods);

            //Apply the template
            target.MultisiteSkillDay = _multisiteSkillDay;
            target.SetChildSkillDays(_childSkillDays);
            target.RedistributeChilds();
            double originalAmountChild1 = target.ChildSkillDays[0].TotalTasks;
            double originalAmountChild2 = target.ChildSkillDays[1].TotalTasks;
            target.ApplyTemplate(multisiteDayTemplate);
            Assert.Less(target.ChildSkillDays[0].TotalTasks, originalAmountChild1);
            Assert.Greater(target.ChildSkillDays[1].TotalTasks, originalAmountChild2);
        }

        [Test]
        public void VerifyRedistributeChildsDoesNotChangeTime()
        {
            var timePeriod1200A = new DateTimePeriod(new DateTime(2012, 07, 25, 12, 00, 00, DateTimeKind.Utc), new DateTime(2012, 07, 25, 12, 15, 00, DateTimeKind.Utc));
            var timePeriod1215A = new DateTimePeriod(new DateTime(2012, 07, 25, 12, 15, 00, DateTimeKind.Utc), new DateTime(2012, 07, 25, 12, 30, 00, DateTimeKind.Utc));
            var timePeriod1200M = new DateTimePeriod(new DateTime(2012, 07, 25, 12, 00, 00, DateTimeKind.Utc), new DateTime(2012, 07, 25, 12, 15, 00, DateTimeKind.Utc));
            var timePeriod1215M = new DateTimePeriod(new DateTime(2012, 07, 25, 12, 15, 00, DateTimeKind.Utc), new DateTime(2012, 07, 25, 12, 30, 00, DateTimeKind.Utc));

            var serviceAgreement = _childSkillDays[0].SkillDataPeriodCollection[0].ServiceAgreement;
            var skillDataPeriodA = new SkillDataPeriod(serviceAgreement, new SkillPersonData(5,10),timePeriod1200A);
            var skillDataPeriodB = new SkillDataPeriod(serviceAgreement, new SkillPersonData(0,0),timePeriod1215A);
            var list = new List<ISkillDataPeriod> { skillDataPeriodA, skillDataPeriodB };
            _childSkillDays.Clear();
            _childSkillDays.Add(SkillDayFactory.CreateSkillDay(_childSkill1, new DateOnly(2012, 07, 25)));
            _childSkillDays[0].SetNewSkillDataPeriodCollection(list);
            _childSkillDays.Add(SkillDayFactory.CreateSkillDay(_childSkill2, new DateOnly(2012, 07, 25)));
            _childSkillDays[1].SetNewSkillDataPeriodCollection(list);
            
            _multisitePeriods.Add(new MultisitePeriod(timePeriod1200M, _multisitePeriods[0].Distribution));
            _multisitePeriods.Add(new MultisitePeriod(timePeriod1215M, _multisitePeriods[0].Distribution));
            
            var workload = WorkloadFactory.CreateWorkload(_skill);
            var multisiteSkilldDay = SkillDayFactory.CreateSkillDay(_skill, timePeriod1200A.StartDateTime.AddHours(-11), workload, workload);
            multisiteSkilldDay.SetupSkillDay();

            multisiteSkilldDay.SkillDayCalculator = calculator;
            _childSkillDays[0].SkillDayCalculator = calculator;
            _childSkillDays[1].SkillDayCalculator = calculator;

            target.MultisiteSkillDay = multisiteSkilldDay;
            target.SetChildSkillDays(_childSkillDays);
            target.SetMultisitePeriodCollection(_multisitePeriods);
            

            target.RedistributeChilds();
            Assert.AreEqual(10, _childSkillDays[0].SkillStaffPeriodCollection[0].Payload.SkillPersonData.MaximumPersons);
            Assert.AreEqual(0, _childSkillDays[0].SkillStaffPeriodCollection[1].Payload.SkillPersonData.MaximumPersons);
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
            IRestrictionSet<IMultisiteDay> restrictions = target.RestrictionSet;
            target.CheckRestrictions();

            Assert.IsNotNull(restrictions);
        }

        /// <summary>
        /// Verifies the child skill days.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-29
        /// </remarks>
        [Test]
        public void VerifyChildSkillDays()
        {
            Assert.AreEqual(0, target.ChildSkillDays.Count);
            target.SetChildSkillDays(_childSkillDays);
            Assert.AreEqual(_childSkillDays.Count, target.ChildSkillDays.Count);
            Assert.AreEqual(_childSkillDays[0], target.ChildSkillDays[0]);
            target.ChildSkillDays.Clear();
            Assert.AreEqual(0, target.ChildSkillDays.Count);
        }

        /// <summary>
        /// Verifies the multisite skill day.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-29
        /// </remarks>
        [Test]
        public void VerifyMultisiteSkillDay()
        {
            Assert.IsNull(target.MultisiteSkillDay);
            target.MultisiteSkillDay = _multisiteSkillDay;
            Assert.AreEqual(_multisiteSkillDay, target.MultisiteSkillDay);
            target.MultisiteSkillDay = null;
            Assert.IsNull(target.MultisiteSkillDay);
        }

        /// <summary>
        /// Calculates the multisite data.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-29
        /// </remarks>
        [Test]
        public void CalculateMultisiteData()
        {
            target.SetChildSkillDays(_childSkillDays);
            _childSkillDays[0].SkillDayCalculator = calculator;
            _childSkillDays[1].SkillDayCalculator = calculator;
			
			_childSkillDays[0].Skill.AbandonRate = Percent.Zero;
			_childSkillDays[1].Skill.AbandonRate = Percent.Zero;
			_multisiteSkillDay.Skill.AbandonRate = Percent.Zero;

            _multisiteSkillDay.WorkloadDayCollection[0].Tasks = 750;
            _multisiteSkillDay.WorkloadDayCollection[1].Tasks = 750;

            target.MultisiteSkillDay = _multisiteSkillDay;
            target.RedistributeChilds();

            Assert.AreEqual(1500, Math.Round(_multisiteSkillDay.TotalTasks, 2));
            Assert.AreEqual(900, Math.Round(_childSkillDays[0].TotalTasks, 2));
            Assert.AreEqual(600, Math.Round(_childSkillDays[1].TotalTasks, 2));
            Assert.AreEqual(_multisiteSkillDay.SkillStaffPeriodCollection[50].Payload.CalculatedOccupancy,
                _childSkillDays[0].SkillStaffPeriodCollection[40].Payload.ServiceAgreementData.MinOccupancy.Value);
            Assert.AreEqual(_multisiteSkillDay.SkillStaffPeriodCollection[50].Payload.CalculatedOccupancy,
                _childSkillDays[0].SkillStaffPeriodCollection[40].Payload.ServiceAgreementData.MaxOccupancy.Value);
            Assert.AreEqual(_multisiteSkillDay.SkillStaffPeriodCollection[50].Payload.CalculatedOccupancy,
                _childSkillDays[1].SkillStaffPeriodCollection[40].Payload.ServiceAgreementData.MinOccupancy.Value);
            Assert.AreEqual(_multisiteSkillDay.SkillStaffPeriodCollection[50].Payload.CalculatedOccupancy,
                _childSkillDays[1].SkillStaffPeriodCollection[40].Payload.ServiceAgreementData.MaxOccupancy.Value);
            Assert.AreEqual(
                Math.Round(_multisiteSkillDay.SkillStaffPeriodCollection[50].Payload.ForecastedIncomingDemand, 3),
                Math.Round(
                    _childSkillDays[0].SkillStaffPeriodCollection[40].Payload.ForecastedIncomingDemand +
                    _childSkillDays[1].SkillStaffPeriodCollection[40].Payload.ForecastedIncomingDemand, 3));
        }

        /// <summary>
        /// Calculates the multisite data with out child skill days.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-06
        /// </remarks>
        [Test]
        public void CalculateMultisiteDataWithoutChildSkillDays()
        {
            target.MultisiteSkillDay = _multisiteSkillDay;

            _multisiteSkillDay.WorkloadDayCollection[0].Tasks = 250;
            _multisiteSkillDay.WorkloadDayCollection[1].Tasks = 250;
            target.RedistributeChilds();

            Assert.AreEqual(500, Math.Round(_multisiteSkillDay.Tasks, 2));
        }

        /// <summary>
        /// Calculates the multisite data with out multisite skill.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-06
        /// </remarks>
        [Test]
        public void CalculateMultisiteDataWithoutMultisiteSkill()
        {
            _childSkillDays[0].SkillDayCalculator = calculator;
            _childSkillDays[1].SkillDayCalculator = calculator;

            target.SetChildSkillDays(_childSkillDays);
            target.RedistributeChilds();

            Assert.AreEqual(0, Math.Round(_childSkillDays[0].Tasks, 2));
            Assert.AreEqual(0, Math.Round(_childSkillDays[1].Tasks, 2));
        }

        /// <summary>
        /// Verifies the calculation on skill day triggers multisite distribution.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-06
        /// </remarks>
        [Test]
        public void VerifyCalculationOnSkillDayTriggersMultisiteDistribution()
        {
            _childSkillDays[0].SkillDayCalculator = calculator;
            _childSkillDays[1].SkillDayCalculator = calculator;

            target.MultisiteSkillDay = _multisiteSkillDay;
            target.SetChildSkillDays(_childSkillDays);

            Assert.AreEqual(0, Math.Round(_childSkillDays[0].Tasks, 2));
            Assert.AreEqual(0, Math.Round(_childSkillDays[1].Tasks, 2));

            //_multisiteSkillDay.Lock();
            _multisiteSkillDay.WorkloadDayCollection[0].Tasks = 250;
            _multisiteSkillDay.WorkloadDayCollection[1].Tasks = 250;
            //_multisiteSkillDay.Release();

            Assert.AreEqual(500, Math.Round(_multisiteSkillDay.TotalTasks, 2));
            Assert.AreEqual(300, Math.Round(_childSkillDays[0].TotalTasks, 2));
            Assert.AreEqual(200, Math.Round(_childSkillDays[1].TotalTasks, 2));
        }

        /// <summary>
        /// Verifies the update template name works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-22
        /// </remarks>
        [Test]
        public void VerifyUpdateTemplateNameWorks()
        {
            Assert.AreNotEqual("<NONE>", target.TemplateReference.TemplateName);
            target.UpdateTemplateName();
            Assert.AreEqual("<NONE>", target.TemplateReference.TemplateName);
        }

        /// <summary>
        /// Verifies the template name is reset when changing child.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-22
        /// </remarks>
        [Test]
        public void VerifyTemplateNameIsResetWhenChangingChild()
        {
            Assert.AreNotEqual("<NONE>", target.TemplateReference.TemplateName);
            target.MultisitePeriodCollection[0].SetPercentage(_childSkill2, new Percent(0.3));
            Assert.AreEqual("<NONE>", target.TemplateReference.TemplateName);
        }

        [Test]
        public void VerifyCannotHaveMultiplePeriodsWithSameStartTime()
        {
	        _multisitePeriods.Add(
		        new MultisitePeriod(
			        new DateTimePeriod(DateTime.SpecifyKind(_dt.Date, DateTimeKind.Utc).Add(TimeSpan.FromHours(4)),
				        DateTime.SpecifyKind(_dt.Date, DateTimeKind.Utc).Add(TimeSpan.FromHours(17))),
			        new Dictionary<IChildSkill, Percent>()));
			Assert.Throws<InvalidOperationException>(() => target.SetMultisitePeriodCollection(_multisitePeriods));
        }

        [Test]
        public void VerifyCanClone()
        {
            target.SetId(Guid.NewGuid());
            IMultisiteDay multisiteDayClone = (IMultisiteDay)target.Clone();
            Assert.IsFalse(multisiteDayClone.Id.HasValue);
            Assert.AreEqual(target.MultisitePeriodCollection.Count, multisiteDayClone.MultisitePeriodCollection.Count);
            Assert.AreSame(target, target.MultisitePeriodCollection[0].Parent);
            Assert.AreSame(multisiteDayClone, multisiteDayClone.MultisitePeriodCollection[0].Parent);
            Assert.AreEqual(target.Skill, multisiteDayClone.Skill);
            Assert.AreEqual(target.ChildSkillDays.Count, multisiteDayClone.ChildSkillDays.Count);
            Assert.AreNotSame(target.ChildSkillDays, multisiteDayClone.ChildSkillDays);
            Assert.AreEqual(target.MultisiteDayDate, multisiteDayClone.MultisiteDayDate);
            Assert.AreEqual(target.Scenario, multisiteDayClone.Scenario);

            multisiteDayClone = target.NoneEntityClone();
            Assert.IsFalse(multisiteDayClone.Id.HasValue);
            Assert.AreEqual(target.MultisitePeriodCollection.Count, multisiteDayClone.MultisitePeriodCollection.Count);
            Assert.AreSame(target, target.MultisitePeriodCollection[0].Parent);
            Assert.AreSame(multisiteDayClone, multisiteDayClone.MultisitePeriodCollection[0].Parent);
            Assert.AreEqual(target.Skill, multisiteDayClone.Skill);
            Assert.AreEqual(target.ChildSkillDays.Count, multisiteDayClone.ChildSkillDays.Count);
            Assert.AreNotSame(target.ChildSkillDays, multisiteDayClone.ChildSkillDays);
            Assert.AreEqual(target.MultisiteDayDate, multisiteDayClone.MultisiteDayDate);
            Assert.AreEqual(target.Scenario, multisiteDayClone.Scenario);

            multisiteDayClone = target.EntityClone();
            Assert.AreEqual(target.Id.Value, multisiteDayClone.Id.Value);
            Assert.AreEqual(target.MultisitePeriodCollection.Count, multisiteDayClone.MultisitePeriodCollection.Count);
            Assert.AreSame(target, target.MultisitePeriodCollection[0].Parent);
            Assert.AreSame(multisiteDayClone, multisiteDayClone.MultisitePeriodCollection[0].Parent);
            Assert.AreEqual(target.Skill, multisiteDayClone.Skill);
            Assert.AreEqual(target.ChildSkillDays.Count, multisiteDayClone.ChildSkillDays.Count);
            Assert.AreNotSame(target.ChildSkillDays, multisiteDayClone.ChildSkillDays);
            Assert.AreEqual(target.MultisiteDayDate, multisiteDayClone.MultisiteDayDate);
            Assert.AreEqual(target.Scenario, multisiteDayClone.Scenario);
        }
    }
}
