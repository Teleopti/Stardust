using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), TestFixture, SetUICulture("en-US")]
    public class SkillDayBug14724Test
    {
        private SkillDay _skillDay1;
        private SkillDay _skillDay2;
        private SkillDay _skillDay3;
        private ISkill _skill;
        private DateOnly _dt;
        private IScenario _scenario;
        private IList<ISkillDataPeriod> _skillDataPeriods1;
        private IList<ISkillDataPeriod> _skillDataPeriods2;
        private IList<ISkillDataPeriod> _skillDataPeriods3;
        private SkillDayCalculator _calculator;
        private IWorkload _workload;

        [SetUp]
        public void Setup()
        {
            _dt = new DateOnly(2007, 1, 1);
            _skill = SkillFactory.CreateSkill("E-mail",SkillTypeFactory.CreateSkillTypeEmail(),60);
            _skill.TimeZone = TimeZoneInfoFactory.UtcTimeZoneInfo();
            _workload = WorkloadFactory.CreateWorkload(_skill);
            _scenario = ScenarioFactory.CreateScenarioAggregate();
            _skillDataPeriods1 = new[]
                                    {
                                        new SkillDataPeriod(ServiceAgreement.DefaultValuesEmail(), new SkillPersonData(),
                                                            new DateTimePeriod(DateTime.SpecifyKind(_dt.Date,DateTimeKind.Utc).Add(TimeSpan.Zero),
                                                                               DateTime.SpecifyKind(_dt.Date,DateTimeKind.Utc).Add(TimeSpan.FromHours(24))))
                                    };
            _skillDataPeriods2 = new[]
                                    {
                                        new SkillDataPeriod(ServiceAgreement.DefaultValuesEmail(), new SkillPersonData(),
                                                            new DateTimePeriod(DateTime.SpecifyKind(_dt.Date,DateTimeKind.Utc).AddDays(1).Add(TimeSpan.Zero),
                                                                               DateTime.SpecifyKind(_dt.Date,DateTimeKind.Utc).AddDays(1).Add(TimeSpan.FromHours(24))))
                                    };
            _skillDataPeriods3 = new[]
                                    {
                                        new SkillDataPeriod(ServiceAgreement.DefaultValuesEmail(), new SkillPersonData(),
                                                            new DateTimePeriod(DateTime.SpecifyKind(_dt.Date,DateTimeKind.Utc).AddDays(2).Add(TimeSpan.Zero),
                                                                               DateTime.SpecifyKind(_dt.Date,DateTimeKind.Utc).AddDays(2).Add(TimeSpan.FromHours(24))))
                                    };

            _skill.SetId(Guid.NewGuid());

            _skillDay1 = new SkillDay(_dt, _skill, _scenario, WorkloadDayFactory.GetWorkloadDaysForTest(_dt.Date,_dt.Date, _workload), _skillDataPeriods1);
            _skillDay1.SetupSkillDay();

            _skillDay2 = new SkillDay(_dt.AddDays(1), _skill, _scenario, WorkloadDayFactory.GetWorkloadDaysForTest(_dt.AddDays(1).Date, _dt.AddDays(1).Date, _workload), _skillDataPeriods2);
            _skillDay2.SetupSkillDay();

            _skillDay3 = new SkillDay(_dt.AddDays(2), _skill, _scenario, WorkloadDayFactory.GetWorkloadDaysForTest(_dt.AddDays(2).Date, _dt.AddDays(2).Date, _workload), _skillDataPeriods3);
            _skillDay3.SetupSkillDay();

            _calculator = new SkillDayCalculator(_skill, new [] { _skillDay1,_skillDay2,_skillDay3 }, new DateOnlyPeriod(_dt, _dt.AddDays(1)));
        }

        [Test]
        public void ShouldHandleClosedDayProperly()
        {
            SetInitialEmails();
            AddNewClosedTemplateToAllWorkloads();
            CloseAllWorkloadDaysForSecondDayInSeclection();

            _skillDay2.WorkloadDayCollection[0].TotalTasks.Should().Be.EqualTo(240);
            _skillDay2.TotalTasks.Should().Be.EqualTo(0);
            _skillDay3.SkillStaffPeriodCollection[0].Payload.TaskData.Tasks.Should().Be.EqualTo(250);
            _skillDay3.SkillStaffPeriodCollection[0].ForecastedDistributedDemand.Should().Be.GreaterThan(0);
        }

        private void AddNewClosedTemplateToAllWorkloads()
        {
            var template = new WorkloadDayTemplate();
            template.Create("Closed",DateTime.SpecifyKind(_dt.Date,DateTimeKind.Utc),_workload,new List<TimePeriod>());
            _workload.AddTemplate(template);
        }

        private void CloseAllWorkloadDaysForSecondDayInSeclection()
        {
            var workloadDay = _skillDay2.WorkloadDayCollection[0];
            workloadDay.ApplyTemplate(
				(IWorkloadDayTemplate)_workload.TryFindTemplateByName(TemplateTarget.Workload, "Closed"), day => day.Lock(), day => day.Release());
        }

        private void SetInitialEmails()
        {
            foreach (var skillDay in _calculator.SkillDays)
            {
                var workloadDay = skillDay.WorkloadDayCollection[0];
                workloadDay.MakeOpen24Hours();
                workloadDay.Tasks = 240;
                workloadDay.AverageTaskTime = TimeSpan.FromSeconds(10);

                skillDay.RecalculateStaff();
            }
        }
    }
}
