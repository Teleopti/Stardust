using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    /// <summary>
    /// Tests ForecastProcessReportRepository
    /// </summary>
    /// <remarks>
    /// Created by: Sachintha Weerasekara
    /// Created date: 10/3/2008
    /// </remarks>
    [TestFixture]
    [Category("BucketB")]
    public class ForecastProcessReportRepositoryTest : DatabaseTest
    {
        private ISkillType skillType;
        private ISkill skill;
        private IActivity activity;
        private IWorkload workload;
        private IScenario scenario;
        private ISkillDay skillDayLongTerm;
        private ValidatedVolumeDay validatedVolumeDay;
        private IList<IWorkloadDay> workloadDaysLongTerm;
        private DateOnlyPeriod _period;

        protected override void SetupForRepositoryTest()
        {
            DateOnly dateTime = new DateOnly(2008, 7, 2);
            _period = new DateOnlyPeriod(dateTime, dateTime.AddDays(1));

            skillType = new SkillTypePhone(new Description("Phone", "Phone"), ForecastSource.InboundTelephony);
            PersistAndRemoveFromUnitOfWork(skillType);

            activity = new Activity("Telephone");
            PersistAndRemoveFromUnitOfWork(activity);

            skill = new Skill("SkillName", "xxx", Color.DodgerBlue, 15, skillType);
            skill.Activity = activity;
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            skill.TimeZone = timeZoneInfo;
            PersistAndRemoveFromUnitOfWork(skill);

            scenario = new Scenario("Default");
            PersistAndRemoveFromUnitOfWork(scenario);

            workload = new Workload(skill);
            PersistAndRemoveFromUnitOfWork(workload);

            validatedVolumeDay = new ValidatedVolumeDay(workload, dateTime);
            PersistAndRemoveFromUnitOfWork(validatedVolumeDay);

            //creates WorkloadDayTemplate
            CreateWorkloadDayTemplate(dateTime);
        }

        private void CreateWorkloadDayTemplate(DateOnly dateTime)
        {
            var templateLongTerm = new WorkloadDayTemplate();
            IList<TimePeriod> openHours = new List<TimePeriod>();
            var timePeriod = new TimePeriod("12-17");
            openHours.Add(timePeriod);
            templateLongTerm.Create(TemplateReference.LongtermTemplateKey, DateTime.SpecifyKind(dateTime.Date,DateTimeKind.Utc), workload, openHours);

            workloadDaysLongTerm = WorkloadDayFactory.GetWorkloadDaysForTest(dateTime.Date, dateTime.Date, workload);

            var skillPersonData = new SkillPersonData(0, 0);
            ISkillDataPeriod skillDataPeriod = new SkillDataPeriod(ServiceAgreement.DefaultValues(), skillPersonData,
                                                                   TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
                                                                       _period.StartDate.Date,
                                                                       _period.EndDate.Date, TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone));

            workloadDaysLongTerm[0].CreateFromTemplate(dateTime, workload, templateLongTerm);

            IList<ISkillDataPeriod> skillDataPeriods = new List<ISkillDataPeriod> { skillDataPeriod };
            skillDayLongTerm = new SkillDay(dateTime, skill, scenario, workloadDaysLongTerm, skillDataPeriods);
            PersistAndRemoveFromUnitOfWork(skillDayLongTerm);
        }

		[Test]
        public void VerifyValidateReport()
        {
            try
            {
                CleanUpAfterTest();
                UnitOfWork.PersistAll();

                IList<IForecastProcessReport> processReports = ForecastProcessReportRepository.ValidationReport(workload, _period);
                int actualValue = processReports[0].PeriodCollection.Count;

                Assert.AreEqual(1, actualValue);
            }
            catch
            {
                Assert.Fail("An exception occured while running the test!");
            }
            finally
            {
                cleanupSetup();
            }
        }

        private void cleanupSetup()
        {
            Session.Delete(skillDayLongTerm);
            Session.Delete(validatedVolumeDay);
            Session.Delete(workload);
            Session.Delete(skill);
            Session.Delete(skillType);
            Session.Delete(activity);
            Session.Delete(scenario);
            Session.Flush();
        }

		[Test]
        public void VerifyBudgetReport()
        {
            try
            {
                CleanUpAfterTest();
                UnitOfWork.PersistAll();

                IList<IForecastProcessReport> processReports =
                    ForecastProcessReportRepository.BudgetReport(scenario, workload, _period);
                int actualValue = processReports[0].PeriodCollection.Count;

                Assert.AreEqual(1, actualValue);
            }
            catch
            {
                Assert.Fail("An exception occured while running the test!");
            }
            finally
            {
                cleanupSetup();
            }
        }

        [Test]
        public void VerifyDetailedReport()
        {
            try
            {
                CleanUpAfterTest();
                UnitOfWork.PersistAll();

                IList<IForecastProcessReport> processReports =
                    ForecastProcessReportRepository.DetailReport(scenario, workload, _period);
                int actualValue = processReports[0].PeriodCollection.Count;
                //since we did not persist any detailed workloaddays actual value is 0

                Assert.AreEqual(0, actualValue);
            }
            catch
            {
                Assert.Fail("An exception occured while running the test!");
            }
            finally
            {
                cleanupSetup();
            }
        }

    }
}
