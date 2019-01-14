using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), TestFixture, SetUICulture("en-US")]
    public class SkillDayTest
    {
        private SkillDay _skillDay;
        private ISkill _skill;
        private DateOnly _dt;
        private IScenario _scenario;
        private IList<ISkillDataPeriod> _skillDataPeriods;
        private SkillDayCalculator _calculator;

        [SetUp]
        public void Setup()
        {
            _dt = new DateOnly(2007, 1, 1);
            _skill = new Skill("skill1", "skill1", Color.FromArgb(255), 15, SkillTypeFactory.CreateSkillTypePhone()).WithId();
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
                    new DateTimePeriod(DateTime.SpecifyKind(_dt.Date, DateTimeKind.Utc).Add(TimeSpan.FromHours(4)),
                                       DateTime.SpecifyKind(_dt.Date, DateTimeKind.Utc).Add(TimeSpan.FromHours(19)))));
			
            _skillDay = new SkillDay(_dt, _skill, _scenario, WorkloadDayFactory.GetWorkloadDaysForTest(_dt.Date, _skill), _skillDataPeriods);
            _skillDay.SetupSkillDay();
            _calculator = new SkillDayCalculator(_skill, new List<ISkillDay> { _skillDay }, new DateOnlyPeriod(_dt,_dt.AddDays(1)));
        }

        [Test]
        public void VerifyEmptyConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_skillDay.GetType(), true));
        }

        [Test]
        public void VerifyOpenForWorkProperty()
        {
            _skillDay.RecalculateDailyTasks();
            Assert.IsTrue(_skillDay.SkillStaffPeriodCollection.Length > 0);
            Assert.IsTrue(_skillDay.OpenForWork.IsOpen);
            Assert.IsTrue(_skillDay.OpenForWork.IsOpenForIncomingWork);
        }

        [Test]
        public void VerifyHasInitialCollectionOfSkillStaffPeriods()
        {
            _skillDay.RecalculateDailyTasks(); //Makes sure the class is initialized
            Assert.AreEqual(60, _skillDay.SkillStaffPeriodCollection.Length); //The open task periods
            Assert.AreEqual(96, _skillDay.CompleteSkillStaffPeriodCollection.Length);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(_skill, _skillDay.Skill);
            Assert.AreEqual(_dt, _skillDay.CurrentDate);
            Assert.AreEqual(_scenario, _skillDay.Scenario);
            Assert.AreEqual(1, _skillDay.SkillDataPeriodCollection.Count);
            Assert.AreEqual(_skillDataPeriods[0], _skillDay.SkillDataPeriodCollection[0]);
            Assert.AreEqual(2, _skillDay.WorkloadDayCollection.Count);
            Assert.AreEqual(_calculator, _skillDay.SkillDayCalculator);
        }

        [Test]
        public void VerifyAddParentWorks()
        {
            MockRepository mocks = new MockRepository();
            ITaskOwner taskOwner = mocks.StrictMock<ITaskOwner>();

            _skillDay.AddParent(taskOwner);
        }

        [Test]
        public void VerifyTotals()
        {
            Assert.AreEqual(400, _skillDay.TotalTasks);
            Assert.AreEqual(new TimeSpan(((100 * TimeSpan.FromSeconds(120).Ticks) + (300 * TimeSpan.FromSeconds(240).Ticks)) / 400).TotalSeconds, Math.Round(_skillDay.AverageTaskTime.TotalSeconds, 2));
            Assert.AreEqual(new TimeSpan(((100 * TimeSpan.FromSeconds(20).Ticks) + (300 * TimeSpan.FromSeconds(40).Ticks)) / 400).TotalSeconds, Math.Round(_skillDay.AverageAfterTaskTime.TotalSeconds, 2));
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
			Assert.Throws<ArgumentNullException>(() => _skillDay = new SkillDay(_dt, null, _scenario, WorkloadDayFactory.GetWorkloadDaysForTest(_dt.Date, _skill), _skillDataPeriods));
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
			Assert.Throws<ArgumentNullException>(() => _skillDay = new SkillDay(_dt, _skill, null, WorkloadDayFactory.GetWorkloadDaysForTest(_dt.Date, _skill), _skillDataPeriods));
        }

        [Test]
        public void VerifySkillInWorkloadDayMustBeSameAsinSkillDay()
        {
            Skill newSkill = new Skill("skill1", "skill1", Color.Red, 15, SkillTypeFactory.CreateSkillTypePhone());
			Assert.Throws<ArgumentException>(() => _skillDay = new SkillDay(_dt, newSkill, _scenario, WorkloadDayFactory.GetWorkloadDaysForTest(_dt.Date, _skill), _skillDataPeriods));
        }

        [Test]
        public void VerifyDateInWorkloadDayMustBeSameAsinSkillDay()
        {
            var dt = new DateOnly(2007, 1, 2);
			Assert.Throws<ArgumentException>(() => _skillDay = new SkillDay(dt, _skill, _scenario, WorkloadDayFactory.GetWorkloadDaysForTest(_dt.Date, _skill), _skillDataPeriods));
        }

        [Test]
        public void VerifyWorkloadDaysCanCloseAndOpenAgain()
        {
            _skillDay.WorkloadDayCollection[0].Close();
            _skillDay.WorkloadDayCollection[1].Close();
            _skillDay.WorkloadDayCollection[0].MakeOpen24Hours();
            _skillDay.WorkloadDayCollection[0].Tasks = 200;
            Assert.AreEqual(125, Math.Round(_skillDay.Tasks, 2)); //Only task periods between 04:00-19:00 will be valid because there are only skill data periods for those hours
            _skillDay.WorkloadDayCollection[1].MakeOpen24Hours();
            Assert.AreEqual(125, Math.Round(_skillDay.Tasks, 2)); //Only task periods between 04:00-19:00 will be valid because there are only skill data periods for those hours
        }

        /// <summary>
        /// Verifies the lock and release.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-22
        /// </remarks>
        [Test]
        public void VerifyLockAndRelease()
        {
            _skillDay.Lock();
            _skillDay.Release();
        }

        /// <summary>
        /// Verifies the release with parent.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-23
        /// </remarks>
        [Test]
        public void VerifyReleaseWithParent()
        {
            MockRepository mocks = new MockRepository();
            ITaskOwner taskOwner = mocks.StrictMock<ITaskOwner>();

            taskOwner.Lock();
            LastCall.Repeat.Once();

            taskOwner.Release();
            LastCall.Repeat.Once();

            mocks.ReplayAll();

            _skillDay.AddParent(taskOwner);
            _skillDay.Lock();
            _skillDay.Release();

            mocks.VerifyAll();
        }

        /// <summary>
        /// Verifies the release with dirty set.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-23
        /// </remarks>
        [Test]
        public void VerifyReleaseWithDirtySet()
        {
            _skillDay.Lock();
            _skillDay.SetDirty();
            _skillDay.Release();
        }

        /// <summary>
        /// Verifies the add workload day works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-28
        /// </remarks>
        [Test]
        public void VerifyAddWorkloadDayWorks()
        {
            IList<IWorkloadDay> workloadDaysToAdd = WorkloadDayFactory.GetWorkloadDaysForTest(_dt.Date, _skill);

            Assert.AreEqual(2, _skillDay.WorkloadDayCollection.Count);
            _skillDay.AddWorkloadDay(workloadDaysToAdd[0]);
            Assert.AreEqual(3, _skillDay.WorkloadDayCollection.Count);
        }

        /// <summary>
        /// Verifies the is closed works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-28
        /// </remarks>
        [Test]
        public void VerifyIsClosedWorks()
        {
            Assert.IsTrue(_skillDay.OpenForWork.IsOpen);
            foreach (var workloadDay in _skillDay.WorkloadDayCollection)
            {
                workloadDay.Close();
            }
            Assert.IsFalse(_skillDay.OpenForWork.IsOpen);
            foreach (var workloadDay in _skillDay.WorkloadDayCollection)
            {
                workloadDay.MakeOpen24Hours();
            }
            Assert.IsTrue(_skillDay.OpenForWork.IsOpen);
        }

        /// <summary>
        /// Verifies the is locked works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-28
        /// </remarks>
        [Test]
        public void VerifyIsLockedWorks()
        {
            Assert.IsFalse(_skillDay.IsLocked);
            _skillDay.Lock();
            Assert.IsTrue(_skillDay.IsLocked);
            _skillDay.Release();
            Assert.IsFalse(_skillDay.IsLocked);
        }

        /// <summary>
        /// Verifies the remove parent works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-28
        /// </remarks>
        [Test]
        public void VerifyRemoveParentWorks()
        {
            MockRepository mocks = new MockRepository();
            ITaskOwner parent = mocks.StrictMock<ITaskOwner>();

            mocks.ReplayAll();

            _skillDay.AddParent(parent);
            _skillDay.RemoveParent(parent);
            _skillDay.RemoveParent(parent);

            mocks.VerifyAll();
        }

        /// <summary>
        /// Verifies total values for staff minutes
        /// </summary>
        /// <remarks>
        /// Created by: cs
        /// </remarks>
        [Test]
        public void VerifyCalculatedStaffTimeTotal()
        {
            _skillDay.RecalculateDailyTasks();
            TimeSpan ts = TimeSpan.FromMinutes(_skillDay.SkillStaffPeriodCollection.Sum(s => s.ForecastedIncomingDemand().TotalMinutes));

            Assert.AreEqual(ts.TotalMinutes, _skillDay.ForecastedIncomingDemand.TotalMinutes);
        }

        [Test]
        public void VerifyCalculateStaffWithShrinkage()
        {
            _skillDay.SkillDataPeriodCollection[0].Shrinkage = new Percent(0.1);
            _skillDay.RecalculateDailyTasks();
            Assert.AreEqual(60, _skillDay.SkillStaffPeriodCollection.Length);
            Assert.AreEqual(new Percent(0.1), _skillDay.SkillStaffPeriodCollection[0].Payload.Shrinkage);
            Assert.AreEqual(_skillDay.SkillStaffPeriodCollection[0].Payload.ForecastedIncomingDemand * 0.9, _skillDay.SkillStaffPeriodCollection[0].Payload.CalculatedTrafficIntensityWithShrinkage);
        }

        /// <summary>
        /// Verifies the split skill data period.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-15
        /// </remarks>
        [Test]
        public void VerifySplitSkillDataPeriod()
        {
            Assert.AreEqual(1, _skillDay.SkillDataPeriodCollection.Count);
            _skillDay.SplitSkillDataPeriods(
                new List<ISkillDataPeriod>
                    { 
                    _skillDay.SkillDataPeriodCollection[0] 
                });
            Assert.AreEqual(15 * 4, _skillDay.SkillDataPeriodCollection.Count);
        }

        /// <summary>
        /// Verifies the merge skill data period.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-15
        /// </remarks>
        [Test]
        public void VerifyMergeSkillDataPeriod()
        {
            Assert.AreEqual(1, _skillDay.SkillDataPeriodCollection.Count);
            _skillDay.SplitSkillDataPeriods(
                new List<ISkillDataPeriod>
                    { 
                    _skillDay.SkillDataPeriodCollection[0] 
                });
            Assert.AreEqual(15 * 4, _skillDay.SkillDataPeriodCollection.Count);
            _skillDay.MergeSkillDataPeriods(
                new List<ISkillDataPeriod>(_skillDay.SkillDataPeriodCollection));
            Assert.AreEqual(1, _skillDay.SkillDataPeriodCollection.Count);
        }

        [Test]
        public void CanCreateFromTemplate()
        {
            SkillDayTemplate skillDayTemplate = CreateTestTemplate();
            var createDate = new DateOnly(2008, 01, 14);

            SkillDay skillDay = new SkillDay();
            skillDay.SkillDayCalculator = new SkillDayCalculator(_skill, new List<ISkillDay> { skillDay }, new DateOnlyPeriod());
            skillDay.CreateFromTemplate(createDate, _skill, _scenario, skillDayTemplate);

	        var skillDataPeriod = skillDay.SkillDataPeriodCollection[32];
	        Assert.AreEqual(skillDataPeriod.ServiceAgreement.ServiceLevel, skillDayTemplate.TemplateSkillDataPeriodCollection[0].ServiceAgreement.ServiceLevel);
            Assert.AreEqual(skillDataPeriod.ServiceAgreement.MinOccupancy, skillDayTemplate.TemplateSkillDataPeriodCollection[0].ServiceAgreement.MinOccupancy);
            Assert.AreEqual(skillDataPeriod.ServiceAgreement.MaxOccupancy, skillDayTemplate.TemplateSkillDataPeriodCollection[0].ServiceAgreement.MaxOccupancy);
            Assert.AreEqual(skillDataPeriod.SkillPersonData, skillDayTemplate.TemplateSkillDataPeriodCollection[0].SkillPersonData);
            Assert.AreEqual(skillDataPeriod.Period.StartDateTime.TimeOfDay, skillDayTemplate.TemplateSkillDataPeriodCollection[0].Period.StartDateTime.TimeOfDay);
            Assert.AreEqual(skillDataPeriod.Shrinkage, skillDayTemplate.TemplateSkillDataPeriodCollection[0].Shrinkage);
            Assert.AreEqual(skillDayTemplate.Id ?? Guid.Empty, skillDay.TemplateReference.TemplateId);
            Assert.AreEqual(skillDayTemplate.VersionNumber, skillDay.TemplateReference.VersionNumber);
        }

        private SkillDayTemplate CreateTestTemplate()
        {
            const string name = "<YUCATAN>";
            IList<ITemplateSkillDataPeriod> templateSkillDataPeriods = new List<ITemplateSkillDataPeriod>();

            DateTimePeriod timePeriod = new DateTimePeriod(
               TimeZoneHelper.ConvertToUtc(SkillDayTemplate.BaseDate.Date, _skill.TimeZone).Add(new TimeSpan(8, 0, 0)),
               TimeZoneHelper.ConvertToUtc(SkillDayTemplate.BaseDate.Date.Add(new TimeSpan(22, 0, 0)), _skill.TimeZone));


            foreach (SkillDataPeriod skillDataPeriod in _skillDataPeriods)
            {
                SkillPersonData skillPersonData = skillDataPeriod.SkillPersonData;

                ITemplateSkillDataPeriod period = new TemplateSkillDataPeriod(skillDataPeriod.ServiceAgreement, skillPersonData, timePeriod);//skillDataPeriod.Period);
                period.Shrinkage = new Percent(0.2);
                templateSkillDataPeriods.Add(period);

            }
            SkillDayTemplate skillDayTemplate = new SkillDayTemplate(name, templateSkillDataPeriods);
            return skillDayTemplate;
        }

        [Test]
        public void VerifyTemplateNameIsChangedWhenManipulatingServiceAgreementIntradayInformation()
        {
            IList<ITemplateSkillDataPeriod> skillDataPeriods = new List<ITemplateSkillDataPeriod>();

            DateTimePeriod timePeriod = new DateTimePeriod(
             TimeZoneHelper.ConvertToUtc(SkillDayTemplate.BaseDate.Date.Add(new TimeSpan(4, 0, 0)), _skill.TimeZone),
             TimeZoneHelper.ConvertToUtc(SkillDayTemplate.BaseDate.Date.Add(new TimeSpan(19, 0, 0)), _skill.TimeZone));

            skillDataPeriods.Add(
                new TemplateSkillDataPeriod(
                    new ServiceAgreement(
                        new ServiceLevel(
                            new Percent(0.8), 20),
                                new Percent(0.5),
                                new Percent(0.7)),
                                new SkillPersonData(), timePeriod));

            string templateName = "<JOHANNESDÖPARENSHIMMELSFÄRDSDAG>";
            SkillDayTemplate skillDayTemplate = CreateAndAddSkillDayTemplate(templateName, skillDataPeriods);

            _skillDay.CreateFromTemplate(_dt, _skill, _scenario, skillDayTemplate);

            Assert.AreEqual(_skillDay.TemplateReference.TemplateName, skillDayTemplate.Name);

            _skillDay.SkillDataPeriodCollection[0].ServiceAgreement = ServiceAgreement.DefaultValues();

            Assert.AreNotEqual(_skillDay.TemplateReference.TemplateName, skillDayTemplate.Name);

        }

        [Test]
        public void VerifyTemplateNameIsChangedWhenManipulatingSkillPersonIntradayInformation()
        {
            IList<ITemplateSkillDataPeriod> skillDataPeriods = new List<ITemplateSkillDataPeriod>();
            SkillPersonData skillPersonData = new SkillPersonData(12, 23);
            
            DateTimePeriod timePeriod = new DateTimePeriod(
             TimeZoneHelper.ConvertToUtc(SkillDayTemplate.BaseDate.Date.Add(new TimeSpan(4, 0, 0)), _skill.TimeZone),
             TimeZoneHelper.ConvertToUtc(SkillDayTemplate.BaseDate.Date.Add(new TimeSpan(19, 0, 0)), _skill.TimeZone));

            skillDataPeriods.Add(
           new TemplateSkillDataPeriod(
                    new ServiceAgreement(
                        new ServiceLevel(
                            new Percent(0.8), 20),
                                new Percent(0.5),
                                new Percent(0.7)),
                                skillPersonData,
                                timePeriod));

            string templateName = "<GROUNDHOGDAY>";
            SkillDayTemplate skillDayTemplate = CreateAndAddSkillDayTemplate(templateName, skillDataPeriods);

            _skillDay.CreateFromTemplate(_dt, _skill, _scenario, skillDayTemplate);

            Assert.AreEqual(_skillDay.TemplateReference.TemplateName, skillDayTemplate.Name);

            _skillDay.SkillDataPeriodCollection[0].SkillPersonData = new SkillPersonData();

            Assert.AreNotEqual(_skillDay.TemplateReference.TemplateName, skillDayTemplate.Name);
        }

        [Test]
        public void CanApplyTemplate()
        {
            const string baseTemplateName = "GROUNDHOGDAY";
            const string templateName = "<" + baseTemplateName + ">";

            CreateAndApplySkillDayTemplate(templateName);
        }

        [Test]
        public void CanApplySplitTemplateToMergedDay()
        {
            const string baseTemplateName = "GROUNDHOGDAY";
            const string templateName = "<" + baseTemplateName + ">";

            ISkillDayTemplate template = CreateAndApplySkillDayTemplate(templateName);
            template.SplitTemplateSkillDataPeriods(new List<ITemplateSkillDataPeriod>(template.TemplateSkillDataPeriodCollection));

            _skillDay.ApplyTemplate(template);

            Assert.Greater(_skillDay.SkillDataPeriodCollection.Count,2);
        }

        [Test]
        public void VerifyOldAndDeletedTemplate()
        {
            const string baseTemplateName = "GROUNDHOGDAY";
            const string templateName = "<" + baseTemplateName + ">";

            SkillDayTemplate skillDayTemplate = CreateAndApplySkillDayTemplate(templateName);

            // change template, should make reference "OLD"
            skillDayTemplate.TemplateSkillDataPeriodCollection[0].ServiceAgreement =                    
                new ServiceAgreement(
                        new ServiceLevel(
                            new Percent(0.88), 19),
                                new Percent(0.3),
                                new Percent(0.88));
            Assert.Greater(skillDayTemplate.VersionNumber, 0);
            Assert.Less(_skillDay.TemplateReference.VersionNumber, skillDayTemplate.VersionNumber);

			var dateTime = new DateTime(2008, 12, 9, 0, 0, 0, DateTimeKind.Utc);
			var localDateTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime, _skill.TimeZone);
			_skillDay.TemplateReference.UpdatedDate = localDateTime;
			var expectedTemplateName = string.Format(CultureInfo.CurrentUICulture, "<{0} {1} {2}>", baseTemplateName, localDateTime.ToShortDateString(), localDateTime.ToShortTimeString());
			Assert.AreEqual(expectedTemplateName, _skillDay.TemplateReference.TemplateName);

            //Apply the template
            _skillDay.ApplyTemplate(skillDayTemplate);
            Assert.AreEqual(_skillDay.TemplateReference.VersionNumber, skillDayTemplate.VersionNumber);
            Assert.AreEqual(templateName, _skillDay.TemplateReference.TemplateName);
        }

        [Test]
        public void VerifySkillDayTemplateReferenceRenamedWithoutOld()
        {
            const string originalName = "Original name";
            const string newName = "New name";

            SkillDayTemplate skillDayTemplate = CreateAndApplySkillDayTemplate(originalName);

            int unmodifiedVersionNumber = skillDayTemplate.VersionNumber;

            skillDayTemplate.Name = newName;
            Assert.AreEqual(newName, _skillDay.TemplateReference.TemplateName);
            Assert.AreEqual(unmodifiedVersionNumber, skillDayTemplate.VersionNumber);
            Assert.AreEqual(unmodifiedVersionNumber, _skillDay.TemplateReference.VersionNumber);
        }


        private SkillDayTemplate CreateAndApplySkillDayTemplate(string templateName)
        {
            IList<ITemplateSkillDataPeriod> skillDataPeriods = new List<ITemplateSkillDataPeriod>();
            SkillPersonData skillPersonData = new SkillPersonData(11, 27);

            DateTimePeriod timePeriod = new DateTimePeriod(
             TimeZoneHelper.ConvertToUtc(SkillDayTemplate.BaseDate.Date.Add(new TimeSpan(4, 0, 0)), _skill.TimeZone),
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

            SkillDayTemplate skillDayTemplate = CreateAndAddSkillDayTemplate(templateName, skillDataPeriods);

            //Sholud not be equal
            Assert.AreNotEqual(_skillDay.SkillDataPeriodCollection[0].SkillPersonData, skillDayTemplate.TemplateSkillDataPeriodCollection[0].SkillPersonData);
            Assert.AreNotEqual(_skillDay.SkillDataPeriodCollection[0].ServiceAgreement, skillDayTemplate.TemplateSkillDataPeriodCollection[0].ServiceAgreement);
            Assert.AreNotEqual(templateName, _skillDay.TemplateReference.TemplateName);
            //Apply the template
            _skillDay.ApplyTemplate(skillDayTemplate);
            Assert.AreEqual(_skillDay.TemplateReference.VersionNumber, skillDayTemplate.VersionNumber);
            
            //Should be equal
            Assert.AreEqual(_skillDay.SkillDataPeriodCollection[0].SkillPersonData, skillDayTemplate.TemplateSkillDataPeriodCollection[0].SkillPersonData);
            Assert.AreEqual(_skillDay.SkillDataPeriodCollection[0].ServiceAgreement, skillDayTemplate.TemplateSkillDataPeriodCollection[0].ServiceAgreement);
            Assert.AreEqual(templateName, _skillDay.TemplateReference.TemplateName);
            Assert.AreEqual(_skillDay.SkillDataPeriodCollection[0].Shrinkage, skillDayTemplate.TemplateSkillDataPeriodCollection[0].Shrinkage);
            return skillDayTemplate;
        }

        private SkillDayTemplate CreateAndAddSkillDayTemplate(string templateName, IList<ITemplateSkillDataPeriod> skillDataPeriods)
        {
            SkillDayTemplate skillDayTemplate = new SkillDayTemplate(templateName, skillDataPeriods);
            // set id to simulate nhibernate persistance behaviour
            ((IEntity)skillDayTemplate).SetId(Guid.NewGuid());
            _skill.AddTemplate(skillDayTemplate);
            return skillDayTemplate;
        }

        /// <summary>
        /// Verifies the initialize works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-03
        /// </remarks>
        [Test]
        public void VerifyInitializeWorks()
        {
            #region Ordinary

            _skillDay.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField,
                null, _skillDay, new object[] { false }, CultureInfo.InvariantCulture);
            double value = _skillDay.Tasks;
            Assert.IsTrue((bool)_skillDay.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                null, _skillDay, null, CultureInfo.InvariantCulture));

            _skillDay.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField,
                null, _skillDay, new object[] { false }, CultureInfo.InvariantCulture);
            TimeSpan timeValue = _skillDay.AverageTaskTime;
            Assert.IsTrue((bool)_skillDay.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                null, _skillDay, null, CultureInfo.InvariantCulture));

            _skillDay.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField,
                null, _skillDay, new object[] { false }, CultureInfo.InvariantCulture);
            timeValue = _skillDay.AverageAfterTaskTime;
            Assert.IsTrue((bool)_skillDay.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                null, _skillDay, null, CultureInfo.InvariantCulture));

            #endregion

            #region Totals

            _skillDay.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField,
                null, _skillDay, new object[] { false }, CultureInfo.InvariantCulture);
            value = _skillDay.TotalTasks;
            Assert.IsTrue((bool)_skillDay.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                null, _skillDay, null, CultureInfo.InvariantCulture));

            _skillDay.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField,
                null, _skillDay, new object[] { false }, CultureInfo.InvariantCulture);
            timeValue = _skillDay.TotalAverageTaskTime;
            Assert.IsTrue((bool)_skillDay.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                null, _skillDay, null, CultureInfo.InvariantCulture));

            _skillDay.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField,
                null, _skillDay, new object[] { false }, CultureInfo.InvariantCulture);
            timeValue = _skillDay.TotalAverageAfterTaskTime;
            Assert.IsTrue((bool)_skillDay.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                null, _skillDay, null, CultureInfo.InvariantCulture));

            #endregion

            #region Statistics

            _skillDay.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField,
                null, _skillDay, new object[] { false }, CultureInfo.InvariantCulture);
            value = _skillDay.TotalStatisticAbandonedTasks;
            Assert.IsTrue((bool)_skillDay.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                null, _skillDay, null, CultureInfo.InvariantCulture));

            _skillDay.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField,
                null, _skillDay, new object[] { false }, CultureInfo.InvariantCulture);
            value = _skillDay.TotalStatisticAnsweredTasks;
            Assert.IsTrue((bool)_skillDay.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                null, _skillDay, null, CultureInfo.InvariantCulture));

            _skillDay.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField,
                null, _skillDay, new object[] { false }, CultureInfo.InvariantCulture);
            value = _skillDay.TotalStatisticCalculatedTasks;
            Assert.IsTrue((bool)_skillDay.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                null, _skillDay, null, CultureInfo.InvariantCulture));

            _skillDay.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField,
                null, _skillDay, new object[] { false }, CultureInfo.InvariantCulture);
            timeValue = _skillDay.TotalStatisticAverageTaskTime;
            Assert.IsTrue((bool)_skillDay.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                null, _skillDay, null, CultureInfo.InvariantCulture));

            _skillDay.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField,
                null, _skillDay, new object[] { false }, CultureInfo.InvariantCulture);
            timeValue = _skillDay.TotalStatisticAverageAfterTaskTime;
            Assert.IsTrue((bool)_skillDay.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                null, _skillDay, null, CultureInfo.InvariantCulture));

            #endregion

            #region Scheduler properties

            _skillDay.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField,
                null, _skillDay, new object[] { false }, CultureInfo.InvariantCulture);
            timeValue = _skillDay.ForecastedIncomingDemand;
            Assert.IsTrue((bool)_skillDay.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                null, _skillDay, null, CultureInfo.InvariantCulture));

            _skillDay.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField,
                null, _skillDay, new object[] { false }, CultureInfo.InvariantCulture);
            timeValue = _skillDay.ForecastedIncomingDemandWithShrinkage;
            Assert.IsTrue((bool)_skillDay.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                null, _skillDay, null, CultureInfo.InvariantCulture));

            _skillDay.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField,
                null, _skillDay, new object[] { false }, CultureInfo.InvariantCulture);
            timeValue = _skillDay.ForecastedDistributedDemand;
            Assert.IsTrue((bool)_skillDay.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                null, _skillDay, null, CultureInfo.InvariantCulture));

            _skillDay.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField,
                null, _skillDay, new object[] { false }, CultureInfo.InvariantCulture);
            timeValue = _skillDay.ForecastedDistributedDemandWithShrinkage;
            Assert.IsTrue((bool)_skillDay.GetType().InvokeMember("_initialized",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                null, _skillDay, null, CultureInfo.InvariantCulture));

            #endregion

            Assert.AreEqual(0d, value);
            Assert.AreNotEqual(TimeSpan.Zero, timeValue);
        }

        /// <summary>
        /// Verifies the recalculate daily task statistics works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-03
        /// </remarks>
        [Test]
        public void VerifyRecalculateDailyTaskStatisticsWorks()
        {
            Assert.AreEqual(0d, _skillDay.TotalStatisticCalculatedTasks);
            Assert.AreEqual(0d, _skillDay.TotalStatisticAbandonedTasks);
            Assert.AreEqual(0d, _skillDay.TotalStatisticAnsweredTasks);
            _skillDay.WorkloadDayCollection[0].TaskPeriodList[0].StatisticTask.StatCalculatedTasks = 100d;
            _skillDay.WorkloadDayCollection[0].TaskPeriodList[0].StatisticTask.StatAbandonedTasks = 50d;
            _skillDay.WorkloadDayCollection[0].TaskPeriodList[0].StatisticTask.StatAnsweredTasks = 150d;
            _skillDay.WorkloadDayCollection[0].RecalculateDailyStatisticTasks();
            _skillDay.RecalculateDailyStatisticTasks();
            Assert.AreEqual(100d, _skillDay.TotalStatisticCalculatedTasks);
            Assert.AreEqual(50d, _skillDay.TotalStatisticAbandonedTasks);
            Assert.AreEqual(150d, _skillDay.TotalStatisticAnsweredTasks);
        }

        /// <summary>
        /// Verifies the recalculate daily average statistic times works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-03
        /// </remarks>
        [Test]
        public void VerifyRecalculateDailyAverageStatisticTimesWorks()
        {
            Assert.AreEqual(TimeSpan.Zero, _skillDay.TotalStatisticAverageTaskTime);
            Assert.AreEqual(TimeSpan.Zero, _skillDay.TotalStatisticAverageAfterTaskTime);
            _skillDay.WorkloadDayCollection[0].TaskPeriodList[0].StatisticTask.StatAverageTaskTimeSeconds = 120 * 96 * 2; //96 periods and 2 workload days
            _skillDay.WorkloadDayCollection[0].TaskPeriodList[0].StatisticTask.StatAverageAfterTaskTimeSeconds = 60 * 96 * 2; //96 periods and 2 workload days
            _skillDay.WorkloadDayCollection[0].RecalculateDailyAverageStatisticTimes();
            _skillDay.RecalculateDailyAverageStatisticTimes();
            Assert.AreEqual(TimeSpan.FromSeconds(120), _skillDay.TotalStatisticAverageTaskTime);
            Assert.AreEqual(TimeSpan.FromSeconds(60), _skillDay.TotalStatisticAverageAfterTaskTime);
        }

        /// <summary>
        /// Verifies the recalculate daily average statistic times works with tasks.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-03
        /// </remarks>
        [Test]
        public void VerifyRecalculateDailyAverageStatisticTimesWorksWithTasks()
        {
            Assert.AreEqual(TimeSpan.Zero, _skillDay.TotalStatisticAverageTaskTime);
            Assert.AreEqual(TimeSpan.Zero, _skillDay.TotalStatisticAverageAfterTaskTime);
            _skillDay.WorkloadDayCollection[0].TaskPeriodList[0].StatisticTask.StatAnsweredTasks = 10d;
            _skillDay.WorkloadDayCollection[0].RecalculateDailyStatisticTasks();
            _skillDay.WorkloadDayCollection[0].TaskPeriodList[0].StatisticTask.StatAverageTaskTimeSeconds = 120;
            _skillDay.WorkloadDayCollection[0].TaskPeriodList[0].StatisticTask.StatAverageAfterTaskTimeSeconds = 60;
            _skillDay.WorkloadDayCollection[0].RecalculateDailyAverageStatisticTimes();
            _skillDay.RecalculateDailyAverageStatisticTimes();
            Assert.AreEqual(TimeSpan.FromSeconds(120), _skillDay.TotalStatisticAverageTaskTime);
            Assert.AreEqual(TimeSpan.FromSeconds(60), _skillDay.TotalStatisticAverageAfterTaskTime);
        }

        /// <summary>
        /// Verifies the recalculate daily average statistic times works with tasks when locked.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-03
        /// </remarks>
        [Test]
        public void VerifyRecalculateDailyAverageStatisticTimesWorksWithTasksWhenLocked()
        {
            Assert.AreEqual(TimeSpan.Zero, _skillDay.TotalStatisticAverageTaskTime);
            Assert.AreEqual(TimeSpan.Zero, _skillDay.TotalStatisticAverageAfterTaskTime);
            _skillDay.Lock();
            _skillDay.WorkloadDayCollection[0].TaskPeriodList[0].StatisticTask.StatAnsweredTasks = 10d;
            _skillDay.WorkloadDayCollection[0].RecalculateDailyStatisticTasks();
            _skillDay.WorkloadDayCollection[0].TaskPeriodList[0].StatisticTask.StatAverageTaskTimeSeconds = 120;
            _skillDay.WorkloadDayCollection[0].TaskPeriodList[0].StatisticTask.StatAverageAfterTaskTimeSeconds = 60;
            _skillDay.WorkloadDayCollection[0].RecalculateDailyAverageStatisticTimes();
            _skillDay.RecalculateDailyStatisticTasks();
            _skillDay.RecalculateDailyAverageStatisticTimes();
            Assert.AreEqual(TimeSpan.Zero, _skillDay.TotalStatisticAverageTaskTime);
            Assert.AreEqual(TimeSpan.Zero, _skillDay.TotalStatisticAverageAfterTaskTime);
            _skillDay.Release();
            Assert.AreEqual(TimeSpan.FromSeconds(120), _skillDay.TotalStatisticAverageTaskTime);
            Assert.AreEqual(TimeSpan.FromSeconds(60), _skillDay.TotalStatisticAverageAfterTaskTime);
        }

        /// <summary>
        /// Verifies the campaign times can be set.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-05
        /// </remarks>
        [Test]
        public void VerifyCampaignTimesCannotBeSet()
        {
			Assert.Throws<NotImplementedException>(() => _skillDay.CampaignTaskTime = new Percent(0.25d));
        }

        [Test]
        public void VerifyCampaignAfterTaskTimesCannotBeSet()
        {
			Assert.Throws<NotImplementedException>(() => _skillDay.CampaignAfterTaskTime = new Percent(0.5d));
        }

        /// <summary>
        /// Verifies the campaign tasks and times are correct set.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-06
        /// </remarks>
        [Test]
        public void VerifyCampaignTasksAndTimesAreCorrectSet()
        {
            foreach (var wld in _skillDay.WorkloadDayCollection)
            {
                wld.Tasks = 0d;
            }

            foreach (var wld in _skillDay.WorkloadDayCollection)
            {
                wld.Tasks = 100d;
                wld.AverageTaskTime = TimeSpan.FromSeconds(40);
                wld.AverageAfterTaskTime = TimeSpan.FromSeconds(80);
            }
            Assert.AreEqual(2, _skillDay.WorkloadDayCollection.Count);

            _skillDay.WorkloadDayCollection[0].CampaignTasks = new Percent(0.4d);
            _skillDay.WorkloadDayCollection[0].CampaignTaskTime = new Percent(0.5d);
            _skillDay.WorkloadDayCollection[0].CampaignAfterTaskTime = new Percent(0.6d);

            //Actually campaign information is not aggregated to skill day level!
            Assert.AreEqual(0, _skillDay.CampaignTasks.Value);
            Assert.AreEqual(0, _skillDay.CampaignTaskTime.Value);
            Assert.AreEqual(0, _skillDay.CampaignAfterTaskTime.Value);
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
            IRestrictionSet<ISkillDay> restrictions = _skillDay.RestrictionSet;
            _skillDay.CheckRestrictions();

            Assert.IsNotNull(restrictions);
        }

        [Test]
        public void VerifyZeroAverageAfterTaskTime()
        {
            _skillDay.WorkloadDayCollection[0].AverageAfterTaskTime = TimeSpan.FromSeconds(0);
            _skillDay.WorkloadDayCollection[1].AverageAfterTaskTime = TimeSpan.FromSeconds(0);
            Assert.AreEqual(0, _skillDay.SkillTaskPeriodCollection()[44].AverageAfterTaskTime.TotalSeconds);
            Assert.AreEqual(0, _skillDay.SkillTaskPeriodCollection()[44].TotalAverageAfterTaskTime.TotalSeconds);
            Assert.AreEqual(0, _skillDay.SkillTaskPeriodCollection()[44].CampaignAfterTaskTime.Value);  //returns NaN, thats the problem
        }

        /// <summary>
        /// Verifies the constructor with null as collection.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-22
        /// </remarks>
        [Test]
        public void VerifyConstructorWithNullAsCollection()
        {
            _skillDay = new SkillDay(_dt, _skill, _scenario, new List<IWorkloadDay>(), null);
            Assert.IsNotNull(_skillDay);
            Assert.AreEqual(96, _skillDay.SkillDataPeriodCollection.Count);
        }


        [Test]
        public void VerifyOpenHoursFromWorkloads()
        {
            _skillDay = new SkillDay(_dt, _skill, _scenario, new List<IWorkloadDay>(), null);
            Workload workload = new Workload(_skill);
            WorkloadDay w1 = new WorkloadDay();
            WorkloadDay w2 = new WorkloadDay();
            WorkloadDay w3 = new WorkloadDay();
            List<TimePeriod> openHours = new List<TimePeriod>();

            openHours.Add(new TimePeriod(new TimeSpan(10, 0, 0), new TimeSpan(14, 0, 0)));
            w1.Create(_dt, workload, openHours);

            _skillDay.AddWorkloadDay(w1);
            Assert.AreEqual(_skillDay.OpenHours(), openHours, "Verify same openHours as only Workload");

            w2.Create(_dt, workload, openHours);
            _skillDay.AddWorkloadDay(w2);
            Assert.AreEqual(_skillDay.OpenHours(), openHours, "Verify same openHours as both Workloads");

            openHours.Clear();
            openHours.Add(new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(12)));
            openHours.Add(new TimePeriod(TimeSpan.FromHours(18), TimeSpan.FromHours(22)));

            w3.Create(_dt, workload, openHours);
            _skillDay.AddWorkloadDay(w3);

            Assert.AreEqual(openHours.Max(t => t.EndTime), _skillDay.OpenHours().Max(t => t.EndTime), "Verify EndTime");
            Assert.AreEqual(openHours.Min(t => t.StartTime), _skillDay.OpenHours().Min(t => t.StartTime), "Verify StartTime");

            openHours.Add(new TimePeriod(new TimeSpan(10, 0, 0), new TimeSpan(14, 0, 0)));
            Assert.AreEqual(TimePeriod.Combine(openHours), _skillDay.OpenHours(), "Verify Combine is applied on all workloads");
        }

        
        /// <summary>
        /// Verifies the event is triggered when calculation is done.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-06
        /// </remarks>
        [Test]
        public void VerifyEventIsTriggeredWhenCalculationIsDone()
        {
            bool eventFired = false;
            object currentSkillDay = null;
            _skillDay.StaffRecalculated += (sender, eventArgs) =>
                {
                    currentSkillDay = sender;
                    eventFired = true;
                };
            _skillDay.RecalculateStaff();

            Assert.IsTrue(eventFired);
            Assert.AreEqual(_skillDay, currentSkillDay);
        }

        /// <summary>
        /// Verifies the set skill staff periods works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-07
        /// </remarks>
        [Test]
        public void VerifySetSkillStaffPeriodsWorks()
        {
            Assert.AreEqual(60, _skillDay.SkillStaffPeriodCollection.Length);

            ISkillStaffPeriod skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(
                new DateTimePeriod(DateTime.SpecifyKind(_dt.Date, DateTimeKind.Utc), DateTime.SpecifyKind(_dt.Date.AddHours(6),DateTimeKind.Utc)),
                new Task(200, TimeSpan.FromSeconds(120), TimeSpan.FromSeconds(140)),
                ServiceAgreement.DefaultValues());

            var newSkillStaffPeriods = new NewSkillStaffPeriodValues(new List<ISkillStaffPeriod> {skillStaffPeriod});
            _skillDay.SetCalculatedStaffCollection(newSkillStaffPeriods);
            newSkillStaffPeriods.BatchCompleted();

            Assert.AreEqual(1, _skillDay.SkillStaffPeriodCollection.Length);
            Assert.AreEqual(skillStaffPeriod.Payload.TaskData, _skillDay.SkillStaffPeriodCollection[0].Payload.TaskData);
            Assert.AreEqual(skillStaffPeriod.Payload.ServiceAgreementData, _skillDay.SkillStaffPeriodCollection[0].Payload.ServiceAgreementData);
            Assert.AreEqual(skillStaffPeriod.Payload.SkillPersonData, _skillDay.SkillStaffPeriodCollection[0].Payload.SkillPersonData);
            Assert.AreEqual(skillStaffPeriod.Payload.Shrinkage, _skillDay.SkillStaffPeriodCollection[0].Payload.Shrinkage);
            Assert.IsTrue(_skillDay.SkillStaffPeriodCollection[0].Payload.IsCalculated);
            Assert.IsTrue(_skillDay.SkillStaffPeriodCollection[0].IsAvailable);
        }

        /// <summary>
        /// Verifies the calculate child skill staff periods works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-07
        /// </remarks>
        [Test]
        public void VerifyCalculateChildSkillStaffPeriodsWorks()
        {
            Assert.AreNotEqual(200d, _skillDay.TotalTasks);
            Assert.AreNotEqual(120d, _skillDay.TotalAverageTaskTime.TotalSeconds);
            Assert.AreNotEqual(140d, _skillDay.TotalAverageAfterTaskTime.TotalSeconds);

            ISkillStaffPeriod skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(
                new DateTimePeriod(DateTime.SpecifyKind(_dt.Date, DateTimeKind.Utc), DateTime.SpecifyKind(_dt.Date.AddHours(6), DateTimeKind.Utc)),
                new Task(200, TimeSpan.FromSeconds(120), TimeSpan.FromSeconds(140)),
                ServiceAgreement.DefaultValues());

            var newSkillStaffPeriods = new NewSkillStaffPeriodValues(new List<ISkillStaffPeriod> { skillStaffPeriod });
            _skillDay.SetCalculatedStaffCollection(newSkillStaffPeriods);
            newSkillStaffPeriods.BatchCompleted();

            Assert.AreEqual(200d, _skillDay.TotalTasks);
            Assert.AreEqual(120d, _skillDay.TotalAverageTaskTime.TotalSeconds);
            Assert.AreEqual(140d, _skillDay.TotalAverageAfterTaskTime.TotalSeconds);
        }

        /// <summary>
        /// Verifies the calculate child skill staff periods works with zero tasks.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-07
        /// </remarks>
        [Test]
        public void VerifyCalculateChildSkillStaffPeriodsWorksWithZeroTasks()
        {
            Assert.AreNotEqual(0d, _skillDay.TotalTasks);
            Assert.AreNotEqual(120d, _skillDay.TotalAverageTaskTime.TotalSeconds);
            Assert.AreNotEqual(140d, _skillDay.TotalAverageAfterTaskTime.TotalSeconds);

            ISkillStaffPeriod skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(
                new DateTimePeriod(DateTime.SpecifyKind(_dt.Date, DateTimeKind.Utc), DateTime.SpecifyKind(_dt.Date.AddHours(6), DateTimeKind.Utc)),
                new Task(0, TimeSpan.FromSeconds(120), TimeSpan.FromSeconds(140)),
                ServiceAgreement.DefaultValues());

            var newSkillStaffPeriods = new NewSkillStaffPeriodValues(new List<ISkillStaffPeriod> { skillStaffPeriod });
            _skillDay.SetCalculatedStaffCollection(newSkillStaffPeriods);
            newSkillStaffPeriods.BatchCompleted();

            Assert.AreEqual(0d, _skillDay.TotalTasks);
            Assert.AreEqual(120d, _skillDay.TotalAverageTaskTime.TotalSeconds);
            Assert.AreEqual(140d, _skillDay.TotalAverageAfterTaskTime.TotalSeconds);
        }

        /// <summary>
        /// Verifies the calculate child skill staff periods works no items.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-07
        /// </remarks>
        [Test]
        public void VerifyCalculateChildSkillStaffPeriodsWorksNoItems()
        {
            Assert.AreNotEqual(0d, _skillDay.TotalTasks);
            Assert.AreNotEqual(0d, _skillDay.TotalAverageTaskTime.TotalSeconds);
            Assert.AreNotEqual(0d, _skillDay.TotalAverageAfterTaskTime.TotalSeconds);

            var newSkillStaffPeriods = new NewSkillStaffPeriodValues(new List<ISkillStaffPeriod> ());
            _skillDay.SetCalculatedStaffCollection(newSkillStaffPeriods);
            newSkillStaffPeriods.BatchCompleted();

            Assert.AreEqual(0d, _skillDay.TotalTasks);
            Assert.AreEqual(0d, _skillDay.TotalAverageTaskTime.TotalSeconds);
            Assert.AreEqual(0d, _skillDay.TotalAverageAfterTaskTime.TotalSeconds);
        }

        [Test]
        public void CanReset()
        {
	        Assert.Throws<NotImplementedException>(() => _skillDay.ResetTaskOwner());
        }

        /// <summary>
        /// Verifies the get all workload days work.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-28
        /// </remarks>
        [Test]
        public void VerifyGetAllWorkloadDaysWork()
        {
            var date = new DateOnly(2007, 8, 1);
            IList<ISkillDay> skillDays = new List<ISkillDay> { 
                new SkillDay(date, _skill, _scenario, new List<IWorkloadDay>(), _skillDataPeriods),
                new SkillDay(date.AddDays(1), _skill, _scenario, new List<IWorkloadDay>(), _skillDataPeriods)
            };
            IWorkload workload = WorkloadFactory.CreateWorkload(_skill);
			var helper = new WorkloadDayHelper();
			helper.CreateLongtermWorkloadDays(_skill,skillDays);
            IList<IWorkloadDayBase> workloadDays = helper.GetWorkloadDaysFromSkillDays(skillDays, workload);

            Assert.AreEqual(2, workloadDays.Count);
        }

        /// <summary>
        /// Verifies the get all workload days work with longterm template.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-01
        /// </remarks>
        [Test]
        public void VerifyGetAllWorkloadDaysWorkWithLongtermTemplate()
        {
            var date = new DateOnly(2007, 8, 1);
            IList<ISkillDay> skillDays = new List<ISkillDay> { 
                new SkillDay(date, _skill, _scenario, new List<IWorkloadDay>(), _skillDataPeriods),
                new SkillDay(date.AddDays(1), _skill, _scenario, new List<IWorkloadDay>(), _skillDataPeriods)
            };

            IWorkload workload = WorkloadFactory.CreateWorkload(_skill);
            WorkloadDayTemplate newTemplate = new WorkloadDayTemplate();
            newTemplate.Create("Template",
                new DateTime(2008, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                workload, new List<TimePeriod> { new TimePeriod("10:00-19:00") });

            workload.SetTemplateAt((int)DayOfWeek.Wednesday, newTemplate);
        	var helper = new WorkloadDayHelper();
			helper.CreateLongtermWorkloadDays(_skill,skillDays);
			IList<IWorkloadDayBase> workloadDays = helper.GetWorkloadDaysFromSkillDays(skillDays, workload);

            Assert.AreEqual(2, workloadDays.Count);
            Assert.AreEqual(1, workloadDays[0].TaskPeriodList.Count);
            Assert.AreEqual(0, workloadDays[1].TaskPeriodList.Count); //Closed!
        }

        /// <summary>
        /// Verifies the get workload days from skill days works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-28
        /// </remarks>
        [Test]
        public void VerifyGetWorkloadDaysFromSkillDaysWorks()
        {
            Assert.Greater(_skillDay.WorkloadDayCollection.Count, 1);

            IList<IWorkloadDayBase> workloadDays = new WorkloadDayHelper().GetWorkloadDaysFromSkillDays(new List<ISkillDay> { _skillDay }, _skill.WorkloadCollection.First());

            Assert.AreEqual(1, workloadDays.Count);
            Assert.AreEqual(_skill.WorkloadCollection.First(), workloadDays.First().Workload);
        }

        [Test]
        public void VerifyReferencesAreNotUsedWhenMerging()
        {
            _skillDay.SplitSkillDataPeriods(_skillDataPeriods);
            Assert.AreEqual(60, _skillDay.SkillDataPeriodCollection.Count);

            _skillDay.SkillDataPeriodCollection[0].ServiceLevelPercent = new Percent(0.6);
            _skillDay.SkillDataPeriodCollection[1].ServiceLevelPercent = new Percent(0.5);

            Assert.AreEqual(new Percent(0.6), _skillDay.SkillDataPeriodCollection[0].ServiceLevelPercent);
            Assert.AreEqual(new Percent(0.5), _skillDay.SkillDataPeriodCollection[1].ServiceLevelPercent);
        }

        [Test]
        public void VerifyForecastedIncomingDemandOnDayLevel()
        {
            ISkillStaffPeriod skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(
                new DateTimePeriod(_skillDataPeriods[0].Period.StartDateTime, _skillDataPeriods[0].Period.StartDateTime.AddMinutes(15)),
                new Task(10d, TimeSpan.FromSeconds(25), TimeSpan.FromSeconds(35)),
                ServiceAgreement.DefaultValues());
            var newSkillStaffPeriods = new NewSkillStaffPeriodValues(new List<ISkillStaffPeriod> { skillStaffPeriod });
            _skillDay.SetCalculatedStaffCollection(newSkillStaffPeriods);
            newSkillStaffPeriods.BatchCompleted();
			skillStaffPeriod.SetSkillDay(_skillDay);
            skillStaffPeriod.CalculateStaff();
            Assert.AreEqual(skillStaffPeriod.ForecastedIncomingDemand(), _skillDay.ForecastedIncomingDemand);
        }

        [Test]
        public void VerifyForecastedIncomingHoursWithShrinkageOnDayLevel()
        {
            ISkillStaffPeriod skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(
                new DateTimePeriod(_skillDataPeriods[0].Period.StartDateTime, _skillDataPeriods[0].Period.StartDateTime.AddMinutes(15)),
                new Task(10d, TimeSpan.FromSeconds(25), TimeSpan.FromSeconds(35)),
                ServiceAgreement.DefaultValues());
            skillStaffPeriod.Payload.Shrinkage = new Percent(0.2);
			skillStaffPeriod.SetSkillDay(_skillDay);
            skillStaffPeriod.CalculateStaff();

            var newSkillStaffPeriods = new NewSkillStaffPeriodValues(new List<ISkillStaffPeriod> { skillStaffPeriod });
            _skillDay.SetCalculatedStaffCollection(newSkillStaffPeriods);
            newSkillStaffPeriods.BatchCompleted();

            Assert.AreEqual(skillStaffPeriod.ForecastedIncomingDemandWithShrinkage(), _skillDay.ForecastedIncomingDemandWithShrinkage);
        }

        [Test]
        public void VerifyForecastedIncomingDemandWithNoSkillStaffPeriods()
        {
            var newSkillStaffPeriods = new NewSkillStaffPeriodValues(new List<ISkillStaffPeriod>());
            _skillDay.SetCalculatedStaffCollection(newSkillStaffPeriods);
            newSkillStaffPeriods.BatchCompleted();

            Assert.AreEqual(TimeSpan.Zero, _skillDay.ForecastedIncomingDemand);
            Assert.AreEqual(TimeSpan.Zero, _skillDay.ForecastedIncomingDemandWithShrinkage);
        }

        [Test]
        public void VerifyForecastedDistributedDemandOnDayLevel()
        {
            ISkillStaffPeriod skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(
                new DateTimePeriod(_skillDataPeriods[0].Period.StartDateTime, _skillDataPeriods[0].Period.StartDateTime.AddMinutes(15)),
                new Task(10d, TimeSpan.FromSeconds(25), TimeSpan.FromSeconds(35)),
                ServiceAgreement.DefaultValues());
            var newSkillStaffPeriods = new NewSkillStaffPeriodValues(new List<ISkillStaffPeriod> { skillStaffPeriod });
            _skillDay.SetCalculatedStaffCollection(newSkillStaffPeriods);
            newSkillStaffPeriods.BatchCompleted();
			skillStaffPeriod.SetSkillDay(_skillDay);
            skillStaffPeriod.CalculateStaff();
            Assert.AreEqual(TimeSpan.FromMinutes(skillStaffPeriod.ForecastedDistributedDemand * skillStaffPeriod.Period.ElapsedTime().TotalMinutes), _skillDay.ForecastedDistributedDemand);
        }

        [Test]
        public void VerifyForecastedDistributedHoursWithShrinkageOnDayLevel()
        {
            ISkillStaffPeriod skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(
                new DateTimePeriod(_skillDataPeriods[0].Period.StartDateTime, _skillDataPeriods[0].Period.StartDateTime.AddMinutes(15)),
                new Task(10d, TimeSpan.FromSeconds(25), TimeSpan.FromSeconds(35)),
                ServiceAgreement.DefaultValues());
            skillStaffPeriod.Payload.Shrinkage = new Percent(0.2);
			skillStaffPeriod.SetSkillDay(_skillDay);
            skillStaffPeriod.CalculateStaff();

            var newSkillStaffPeriods = new NewSkillStaffPeriodValues(new List<ISkillStaffPeriod> { skillStaffPeriod });
            _skillDay.SetCalculatedStaffCollection(newSkillStaffPeriods);
            newSkillStaffPeriods.BatchCompleted();

            Assert.AreEqual(TimeSpan.FromMinutes(skillStaffPeriod.ForecastedDistributedDemandWithShrinkage * skillStaffPeriod.Period.ElapsedTime().TotalMinutes), _skillDay.ForecastedDistributedDemandWithShrinkage);
        }

        [Test]
        public void VerifyForecastedDistributedDemandWithNoSkillStaffPeriods()
        {
            var newSkillStaffPeriods = new NewSkillStaffPeriodValues(new List<ISkillStaffPeriod> ());
            _skillDay.SetCalculatedStaffCollection(newSkillStaffPeriods);
            newSkillStaffPeriods.BatchCompleted();

            Assert.AreEqual(TimeSpan.Zero, _skillDay.ForecastedDistributedDemand);
            Assert.AreEqual(TimeSpan.Zero, _skillDay.ForecastedDistributedDemandWithShrinkage);
        }

        [Test]
        public void VerifyCanSetSkillDayCalculator()
        {
            SkillDayCalculator calculator = new SkillDayCalculator(_skill, new List<ISkillDay> { _skillDay }, new DateOnlyPeriod(_skillDay.CurrentDate,_skillDay.CurrentDate.AddDays(2)));
            Assert.AreEqual(calculator, _skillDay.SkillDayCalculator);
        }

        [Test]
        public void VerifyNoSkillDayCalculatorThrowsException()
        {
            _skillDay.SkillDayCalculator = null;
			Assert.Throws<InvalidOperationException>(() => _skillDay.SkillTaskPeriodCollection());
        }

        [Test]
        public void VerifyNoSkillDayCalculatorGivesNoResultForChildSkill()
        {
	        _skillDay = new SkillDay(_skillDay.CurrentDate,
		        new ChildSkill("test", "test", Color.Red, new MultisiteSkill("M","", Color.Red,15, _skill.SkillType)),
		        _scenario, new List<IWorkloadDay>(), new List<ISkillDataPeriod>()) {SkillDayCalculator = null};
	        Assert.AreEqual(0, _skillDay.SkillTaskPeriodCollection().Count);
        }

        [Test]
        public void VerifyCanSkipSpilloverToNextOpenDay()
        {
            Assert.IsTrue(_skillDay.EnableSpillover);
            _skillDay.EnableSpillover = false;
            Assert.IsFalse(_skillDay.EnableSpillover);
        }

        [Test]
        public void VerifySetTasksNotImplemented()
        {
	        Assert.Throws<NotImplementedException>(() => _skillDay.Tasks = 10d);
        }

        [Test]
        public void VerifySetAverageTaskTimeNotImplemented()
        {
	        Assert.Throws<NotImplementedException>(() => _skillDay.AverageTaskTime = TimeSpan.FromSeconds(10d));
        }

        [Test]
        public void VerifySetAverageAfterTaskTimeNotImplemented()
        {
			Assert.Throws<NotImplementedException>(() => _skillDay.AverageAfterTaskTime = TimeSpan.FromSeconds(10d));
        }

        [Test]
        public void VerifySetCampaignTasksNotImplemented()
        {
			Assert.Throws<NotImplementedException>(() => _skillDay.CampaignTasks = new Percent(0.1d));
        }

        [Test]
        public void VerifyCannotHaveWorkloadDayWithOtherDateTime()
        {
            WorkloadDay day = new WorkloadDay();
            day.Create(_skillDay.CurrentDate.AddDays(2), _skill.WorkloadCollection.First(), new List<TimePeriod>());
			Assert.Throws<ArgumentException>(() => _skillDay.AddWorkloadDay(day));
        }

        [Test]
        public void VerifySkillStaffPeriodsAreNotReplacedWhenUsingChildSkill()
        {
            _skill = new ChildSkill("test", "test", Color.Red, new MultisiteSkill("M", "", Color.Red, 15, _skill.SkillType));
            _skillDay = new SkillDay(_skillDay.CurrentDate, _skill, _scenario, new List<IWorkloadDay>(), _skillDataPeriods);
            _calculator = new SkillDayCalculator(_skill, new List<ISkillDay> { _skillDay }, new DateOnlyPeriod());
            var newSkillStaffPeriods = new NewSkillStaffPeriodValues(new List<ISkillStaffPeriod>
                                                       {
                                                           SkillStaffPeriodFactory.CreateSkillStaffPeriod(
                                                               new DateOnlyPeriod(_dt,_dt).ToDateTimePeriod(_skill.TimeZone), new Task(),
                                                               ServiceAgreement.DefaultValues())
                                                       });
            _skillDay.SetCalculatedStaffCollection(newSkillStaffPeriods);
            newSkillStaffPeriods.BatchCompleted();
            _skillDay.RecalculateStaff();
            Assert.AreEqual(1, _skillDay.SkillStaffPeriodCollection.Length);
        }

        [Test]
        public void VerifyNoneEntityClone()
        {
            _skillDay = new SkillDay(_dt, _skill, _scenario, WorkloadDayFactory.GetWorkloadDaysForTest(_dt.Date, _skill), _skillDataPeriods);
			_skillDay.SetId(Guid.NewGuid());

            _calculator = new SkillDayCalculator(_skill, new List<ISkillDay> { _skillDay }, new DateOnlyPeriod());

            var newSkillStaffPeriods = new NewSkillStaffPeriodValues(new List<ISkillStaffPeriod>
                                                       {
                                                           SkillStaffPeriodFactory.CreateSkillStaffPeriod(
                                                               new DateOnlyPeriod(_dt,_dt).ToDateTimePeriod(_skill.TimeZone), new Task(),
                                                               ServiceAgreement.DefaultValues())
                                                       });
            _skillDay.SetCalculatedStaffCollection(newSkillStaffPeriods);
            newSkillStaffPeriods.BatchCompleted();
            
            SkillDay clonedSkillDay = (SkillDay) _skillDay.NoneEntityClone();
            Assert.AreSame(clonedSkillDay, clonedSkillDay.SkillDataPeriodCollection[0].Parent);
            Assert.IsNotNull(clonedSkillDay);
            Assert.AreNotSame(clonedSkillDay, _skillDay);
            Assert.AreNotEqual(_skillDay.Id, clonedSkillDay.Id);
            Assert.AreEqual(_skillDay.AverageAfterTaskTime, clonedSkillDay.AverageAfterTaskTime);
            Assert.AreEqual(_skillDay.AverageTaskTime, clonedSkillDay.AverageTaskTime);

            Assert.AreEqual(_skillDay.CampaignAfterTaskTime, clonedSkillDay.CampaignAfterTaskTime);
            Assert.AreEqual(_skillDay.CampaignTasks, clonedSkillDay.CampaignTasks);
            Assert.AreEqual(_skillDay.CampaignTaskTime, clonedSkillDay.CampaignTaskTime);

            Assert.AreNotSame(_skillDay.WorkloadDayCollection, clonedSkillDay.WorkloadDayCollection);
            Assert.AreNotSame(_skillDay.WorkloadDayCollection[0], clonedSkillDay.WorkloadDayCollection[0]);
			
            Assert.AreEqual(_skillDay.WorkloadDayCollection.Count, clonedSkillDay.WorkloadDayCollection.Count);
        }

        [Test]
        public void VerifyEntityCloneToScenario()
        {
            IScenario scenario = ScenarioFactory.CreateScenarioAggregate();
            ISkillDay clonedSkillDay = _skillDay.NoneEntityClone(scenario);
            Assert.AreNotSame(clonedSkillDay.Scenario, _skillDay.Scenario);
        }

        [Test]
        public void ShouldHaveMinStaffingFirstAndLastDuringDaylightSavingsTransition()
        {
            _skill.TimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            var createDate = new DateOnly(2009, 10, 25);
            IList<ISkillDataPeriod> skillDataPeriods = new List<ISkillDataPeriod>();

	        var localStartDateTime = createDate.Date.Add(_skill.MidnightBreakOffset);
	        skillDataPeriods.Add(
		        new SkillDataPeriod(
			        new ServiceAgreement(
				        new ServiceLevel(
					        new Percent(0.8), 20),
					        new Percent(0.5),
					        new Percent(0.7)),
				        new SkillPersonData(),
				        TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(localStartDateTime,
					        localStartDateTime.Add(TimeSpan.FromHours(24)),_skill.TimeZone)));

            ISkillDay skillDay = new SkillDay(createDate, _skill, _scenario, WorkloadDayFactory.GetWorkloadDaysForTest(createDate.Date, _skill), skillDataPeriods);
            skillDay.SetupSkillDay();
            skillDay.SkillDayCalculator = new SkillDayCalculator(_skill, new List<ISkillDay> { skillDay }, new DateOnlyPeriod(createDate, createDate.AddDays(1)));

            ISkillDayTemplate skillDayTemplate = CreateTestTemplateSevenToEight();
            skillDay.CreateFromTemplate(createDate, _skill, _scenario, skillDayTemplate);
			
			var lookup =
				skillDay.SkillDataPeriodCollection.ToLookup(s => s.Period.StartDateTimeLocal(_skill.TimeZone).TimeOfDay);

			Assert.AreEqual(2, lookup[TimeSpan.FromHours(7)].First().MinimumPersons); //The actual 2 is currently one hour later
			Assert.AreEqual(2, lookup[TimeSpan.FromHours(19.75)].First().MinimumPersons); //The actual 2 is currently one hour later
		}

        [Test]
        public void ShouldHaveMinStaffingFirstAndLastDuringDaylightSavingsTransitionToSummertime()
        {
            _skill.TimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            var createDate = new DateOnly(2010, 03, 28);
            IList<ISkillDataPeriod> skillDataPeriods = new List<ISkillDataPeriod>();

	        var localStartDateTime = createDate.Date.Add(_skill.MidnightBreakOffset);
	        skillDataPeriods.Add(
                new SkillDataPeriod(
                    new ServiceAgreement(
                        new ServiceLevel(
                            new Percent(0.8), 20),
                                new Percent(0.5),
                                new Percent(0.7)),
                                new SkillPersonData(),
								TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(localStartDateTime, localStartDateTime.Add(TimeSpan.FromHours(24)), _skill.TimeZone)));

            var skillDay = new SkillDay(createDate, _skill, _scenario, WorkloadDayFactory.GetWorkloadDaysForTest(createDate.Date, _skill), skillDataPeriods);
            skillDay.SetupSkillDay();
            skillDay.SkillDayCalculator = new SkillDayCalculator(_skill, new List<ISkillDay> { skillDay }, new DateOnlyPeriod(createDate,createDate));

            var skillDayTemplate = CreateTestTemplateSevenToEight();
            skillDay.CreateFromTemplate(createDate, _skill, _scenario, skillDayTemplate);

	        var lookup =
		        skillDay.SkillDataPeriodCollection.ToLookup(s => s.Period.StartDateTimeLocal(_skill.TimeZone).TimeOfDay);
			
            Assert.AreEqual(2, lookup[TimeSpan.FromHours(7)].First().MinimumPersons); //The actual 2 is currently one hour later
            Assert.AreEqual(2, lookup[TimeSpan.FromHours(19.75)].First().MinimumPersons); //The actual 2 is currently one hour later
        }

        [Test]
        public void ShouldHaveMinStaffingFirstWhenNoDaylightSavingChange()
        {
            _skill.TimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            var createDate = new DateOnly(2010, 03, 30);
            IList<ISkillDataPeriod> skillDataPeriods = new List<ISkillDataPeriod>();

			var localStartDateTime = createDate.Date.Add(_skill.MidnightBreakOffset);
			skillDataPeriods.Add(
                new SkillDataPeriod(
                    new ServiceAgreement(
                        new ServiceLevel(
                            new Percent(0.8), 20),
                        new Percent(0.5),
                        new Percent(0.7)),
                    new SkillPersonData(),
					TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(localStartDateTime, localStartDateTime.Add(TimeSpan.FromHours(24)), _skill.TimeZone)));

            ISkillDay skillDay = new SkillDay(createDate, _skill, _scenario, WorkloadDayFactory.GetWorkloadDaysForTest(createDate.Date, _skill), skillDataPeriods);
            skillDay.SetupSkillDay();
            skillDay.SkillDayCalculator = new SkillDayCalculator(_skill, new List<ISkillDay> { skillDay }, new DateOnlyPeriod(createDate,createDate));

            ISkillDayTemplate skillDayTemplate = CreateTestTemplateSevenToEight();
            skillDay.CreateFromTemplate(createDate, _skill, _scenario, skillDayTemplate);

			var lookup =
				skillDay.SkillDataPeriodCollection.ToLookup(s => s.Period.StartDateTimeLocal(_skill.TimeZone).TimeOfDay);

			Assert.AreEqual(2, lookup[TimeSpan.FromHours(7)].First().MinimumPersons); //The actual 2 is currently one hour later
			Assert.AreEqual(2, lookup[TimeSpan.FromHours(19.75)].First().MinimumPersons); //The actual 2 is currently one hour later
		}
        private ISkillDayTemplate CreateTestTemplateSevenToEight()
        {
            const string name = "<SOME>";
            ServiceAgreement serviceAgreement = ServiceAgreement.DefaultValues();
            DateTime startDateUtc = TimeZoneInfo.ConvertTimeToUtc(SkillDayTemplate.BaseDate.Date, _skill.TimeZone);
            DateTimePeriod timePeriod = new DateTimePeriod(
                startDateUtc, startDateUtc.AddDays(1)).MovePeriod(_skill.MidnightBreakOffset);
            ITemplateSkillDataPeriod templateSkillDataPeriod = new TemplateSkillDataPeriod(serviceAgreement,
                                                                                          new SkillPersonData(),
                                                                                          timePeriod);
            ISkillDayTemplate skillDayTemplate = new SkillDayTemplate(name,
                                                                     new List<ITemplateSkillDataPeriod> { templateSkillDataPeriod });
			skillDayTemplate.SetParent(_skill);

            skillDayTemplate.SplitTemplateSkillDataPeriods(skillDayTemplate.TemplateSkillDataPeriodCollection.ToList());

            ITemplateSkillDataPeriod templateSkillDataPeriodSeven =
                skillDayTemplate.TemplateSkillDataPeriodCollection.FirstOrDefault(
                    t => t.Period.StartDateTime == startDateUtc.AddHours(7));
            templateSkillDataPeriodSeven.MinimumPersons = 2;
            ITemplateSkillDataPeriod templateSkillDataPeriodEight =
                skillDayTemplate.TemplateSkillDataPeriodCollection.FirstOrDefault(
                    t => t.Period.StartDateTime == startDateUtc.AddHours(19.75));
            templateSkillDataPeriodEight.MinimumPersons = 2;

            return skillDayTemplate;
        }

        [Test]
        public void VerifyCampaignTasks()
        {
            Percent percent = _skillDay.CampaignTasks;
            Assert.AreEqual(new Percent(0), percent);
        }

        [Test]
        public void VerifyCampaignTaskTime()
        {
            Percent percent = _skillDay.CampaignTaskTime;
            Assert.AreEqual(new Percent(0), percent);
        }

        [Test]
        public void VerifyCampaignAfterTaskTime()
        {
            Percent percent = _skillDay.CampaignAfterTaskTime;
            Assert.AreEqual(new Percent(0), percent);
        }

        [Test]
        public void VerifySkillStaffPeriodViewCollection()
        {
            // we have 60 15 minutes periods , we want 180 5 minutes periods
            var views = _skillDay.SkillStaffPeriodViewCollection(new TimeSpan(0, 5, 0));
            Assert.AreEqual(180,views.Length);
            Assert.AreEqual(5, views[0].Period.ElapsedTime().TotalMinutes);
        }
    }
}
