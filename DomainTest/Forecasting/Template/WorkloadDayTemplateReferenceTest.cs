using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting.Template
{
    [TestFixture, SetUICulture("en-US")]
    public class WorkloadDayTemplateReferenceTest
    {
        private IWorkload _workload;
        private TestTemplateReference _templateReference;
        private WorkloadDayTemplateReference _target;

        [SetUp]
        public void Setup()
        {
            _target = new WorkloadDayTemplateReference();
            ISkill skill = SkillFactory.CreateSkill("Pets");
            skill.MidnightBreakOffset = TimeSpan.FromHours(2);
            _workload = WorkloadFactory.CreateWorkload(skill);
        }

        [Test]
        public void VerifyProtectedSetDoesNotThrowException()
        {
            // For coverage resons. Perhaps it should not be possible to set the name.
            _templateReference = new TestTemplateReference();
            string name = "Test";
            _templateReference.SetTemplateName(name);
            Assert.AreEqual("<NONE>", _templateReference.TemplateName);
        }
        [Test]
        public void VerifyPropertiesWorks()
        {
            _target.Workload = null;
            _target.Workload = _workload;
            Assert.AreEqual(_workload, _target.Workload);
        }

        [Test]
        public void CanReturnCorrectTemplateName()
        {
            var createDate = new DateOnly(2008, 01, 14);
			var createLocalDate = TimeZoneInfo.ConvertTimeFromUtc(createDate.Date, _workload.Skill.TimeZone);
            const string baseTemplateName = "JULDAGEN";
            const string templateName = "<" + baseTemplateName + ">";
            var workloadDayTemplate = CreateAndAddWorkloadDayTemplate(templateName, createDate.Date);

            var workloadDay = new WorkloadDay();
            workloadDay.CreateFromTemplate(createDate, _workload,  workloadDayTemplate);
            var originalTemplateVersionNumber = workloadDayTemplate.VersionNumber;
            Assert.AreEqual(originalTemplateVersionNumber, workloadDay.TemplateReference.VersionNumber);

            ModifyTemplate(workloadDayTemplate);

            var latestTemplateVersionNumber = workloadDayTemplate.VersionNumber;
            Assert.Less(originalTemplateVersionNumber, latestTemplateVersionNumber);
            Assert.AreEqual(templateName, workloadDayTemplate.Name);
			var expectedTemplateName = string.Format(CultureInfo.CurrentUICulture, "<{0} {1} {2}>", baseTemplateName, createLocalDate.ToShortDateString(), createLocalDate.ToShortTimeString());
			Assert.AreEqual(expectedTemplateName, workloadDay.TemplateReference.TemplateName);

            _workload.RemoveTemplate(TemplateTarget.Workload, templateName);
            Assert.AreEqual("<DELETED>", workloadDay.TemplateReference.TemplateName);
        }

        [Test]
        public void VerifyRenameWorksAsExpected()
        {
            var createDate = new DateOnly(2008, 01, 14);
			var createLocalDate = TimeZoneInfo.ConvertTimeFromUtc(createDate.Date, _workload.Skill.TimeZone);
            const string baseTemplateName = "JULDAGEN";
            const string templateName = "<" + baseTemplateName + ">";
            var workloadDayTemplate = CreateAndAddWorkloadDayTemplate(templateName, createDate.Date);

            var workloadDay = new WorkloadDay();
            workloadDay.CreateFromTemplate(createDate, _workload,  workloadDayTemplate);
            var originalTemplateVersionNumber = workloadDayTemplate.VersionNumber;
            Assert.AreEqual(originalTemplateVersionNumber, workloadDay.TemplateReference.VersionNumber);

            ModifyTemplate(workloadDayTemplate);

            var latestTemplateVersionNumber = workloadDayTemplate.VersionNumber;
            Assert.Less(originalTemplateVersionNumber, latestTemplateVersionNumber);
            Assert.AreEqual(templateName, workloadDayTemplate.Name);
			var expectedTemplateName = string.Format(CultureInfo.CurrentUICulture, "<{0} {1} {2}>", baseTemplateName, createLocalDate.ToShortDateString(), createLocalDate.ToShortTimeString());
			Assert.AreEqual(expectedTemplateName, workloadDay.TemplateReference.TemplateName);

            const string newBaseTemplateName = "NYÅRSDAGEN";
            const string newTemplateName = "<" + newBaseTemplateName + ">";
            workloadDayTemplate.Name = newTemplateName;
			expectedTemplateName = string.Format(CultureInfo.CurrentUICulture, "<{0} {1} {2}>", newBaseTemplateName, createLocalDate.ToShortDateString(), createLocalDate.ToShortTimeString());
			Assert.AreEqual(expectedTemplateName, workloadDay.TemplateReference.TemplateName);

            _workload.RemoveTemplate(TemplateTarget.Workload, newTemplateName);
            Assert.AreEqual("<DELETED>", workloadDay.TemplateReference.TemplateName);
        }

		[Test]
		public void VerifyRenameWhenCreatingNewStandardTemplates()
		{
			string originalName = string.Format(CultureInfo.CurrentUICulture, "<{0}>",
			                                    CultureInfo.CurrentUICulture.DateTimeFormat.GetAbbreviatedDayName(
			                                    	DayOfWeek.Friday)).ToUpper(CultureInfo.InvariantCulture);
			string newName = string.Format(CultureInfo.CurrentUICulture, "<{0} {1} {2}>",
												  originalName.TrimEnd('>').TrimStart('<'), DateTime.UtcNow.ToShortDateString(),
												  DateTime.UtcNow.ToShortTimeString());

			var createDate = new DateOnly(2008, 01, 14);
			var workloadDay = new WorkloadDay();
			var templateDay = (IWorkloadDayTemplate)_workload.GetTemplate(TemplateTarget.Workload, DayOfWeek.Friday);
			templateDay.SetId(Guid.NewGuid());
			workloadDay.CreateFromTemplate(createDate, _workload, templateDay);
			Statistic statistic = new Statistic(_workload);

			Assert.AreEqual(originalName, workloadDay.TemplateReference.TemplateName);

			statistic.CalculateTemplateDays(new List<IWorkloadDayBase>());

			Assert.AreEqual(newName, workloadDay.TemplateReference.TemplateName);
		}




        /// <summary>
        /// Add som changes to the template
        /// </summary>
        private static void ModifyTemplate(IWorkloadDayTemplate workloadDayTemplate)
        {

            workloadDayTemplate.SortedTaskPeriodList[0].SetTasks(500);
            workloadDayTemplate.SortedTaskPeriodList[0].AverageTaskTime = new TimeSpan(0, 0, 10);
            workloadDayTemplate.SortedTaskPeriodList[0].AverageAfterTaskTime = new TimeSpan(0, 8, 4);

            workloadDayTemplate.SortedTaskPeriodList[1].SetTasks(500);
            workloadDayTemplate.SortedTaskPeriodList[1].AverageTaskTime = new TimeSpan(0, 0, 10);
            workloadDayTemplate.SortedTaskPeriodList[1].AverageAfterTaskTime = new TimeSpan(0, 8, 4);

            workloadDayTemplate.SortedTaskPeriodList[2].SetTasks(500);
            workloadDayTemplate.SortedTaskPeriodList[2].AverageTaskTime = new TimeSpan(0, 0, 10);
            workloadDayTemplate.SortedTaskPeriodList[2].AverageAfterTaskTime = new TimeSpan(0, 8, 4);

            workloadDayTemplate.SortedTaskPeriodList[3].SetTasks(500);
            workloadDayTemplate.SortedTaskPeriodList[3].AverageTaskTime = new TimeSpan(0, 0, 10);
            workloadDayTemplate.SortedTaskPeriodList[3].AverageAfterTaskTime = new TimeSpan(0, 8, 4);
        }

        private IWorkloadDayTemplate CreateAndAddWorkloadDayTemplate(string templateName, DateTime createDate)
        {
            IWorkloadDayTemplate workloadDayTemplate = new WorkloadDayTemplate();

            IList<TimePeriod> openHours = new List<TimePeriod>();
            openHours.Add(new TimePeriod(new TimeSpan(12, 0, 0), new TimeSpan(1, 2, 0, 0)));
            openHours.Add(new TimePeriod(new TimeSpan(6, 0, 0), new TimeSpan(9, 0, 0)));
            workloadDayTemplate.SetId(Guid.NewGuid());
            workloadDayTemplate.Create(templateName, DateTime.SpecifyKind(createDate,DateTimeKind.Utc), _workload, openHours);
            _workload.AddTemplate(workloadDayTemplate);
            return workloadDayTemplate;
        }

        internal class TestTemplateReference:WorkloadDayTemplateReference
    {
        public void SetTemplateName(string name)
        {
            TemplateName = name;
        }
    }
    }
}
