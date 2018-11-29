using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;

using SkillFactory = Teleopti.Ccc.TestCommon.FakeData.SkillFactory;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer
{
    [TestFixture]
    public class ForecastWorkloadTransformerTest
    {
        private ForecastWorkloadTransformer _target;
        private readonly IList<ITemplateTaskPeriod> _templateTaskPeriodCollection = new List<ITemplateTaskPeriod>();
        private DateOnly _date = new DateOnly(2008, 2, 1);
        private readonly DateTime _insertDateTime = DateTime.Now;
		private readonly DateTime _updatedOnDateTime = DateTime.Now;
        private ISkill _skill1;
        private ISkill _skill2;
        private ISkill _skill3;
        private IList<ISkillDay> _skillDayCollection1;
        private IList<ISkillDay> _skillDayCollection2;
        private IList<ISkillDay> _skillDayCollection3;
        private ISkillDay _skillDay1;
        private ISkillDay _skillDay2;
        private ISkillDay _skillDay3;

        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            _target = new ForecastWorkloadTransformer(96, _insertDateTime);
            _skill1 = SkillFactory.CreateSkill("skill 1", SkillTypeFactory.CreateSkillType(), 15);
            _skill2 = SkillFactory.CreateSkill("skill 2", SkillTypeFactory.CreateSkillTypeBackoffice(), 15);
            _skill3 = SkillFactory.CreateSkill("skill 3", SkillTypeFactory.CreateSkillTypeEmail(), 60);
            _skill1.SetId(Guid.NewGuid());
            _skill2.SetId(Guid.NewGuid());
            _skill3.SetId(Guid.NewGuid());

            var period = new DateOnlyPeriod(_date, _date.AddDays(2));

			_skillDayCollection1 = ForecastFactory.CreateSkillDayCollection(period, _skill1, _updatedOnDateTime);
			_skillDayCollection2 = ForecastFactory.CreateSkillDayCollection(period, _skill2, _updatedOnDateTime);
			_skillDayCollection3 = ForecastFactory.CreateSkillDayCollection(period, _skill3, _updatedOnDateTime);

            IWorkloadDay workloadDay1 = ForecastFactory.CreateWorkload(_date, _skill1);
            IWorkloadDay workloadDay2 = ForecastFactory.CreateWorkload(_date, _skill2);
            IWorkloadDay workloadDay3 = ForecastFactory.CreateWorkload(_date, _skill3);
            foreach (ISkillDay skillDay in _skillDayCollection1)
            {
                _skillDay1 = skillDay;
                _skillDay1.AddWorkloadDay(workloadDay1);
                break;
            }
            foreach (ISkillDay skillDay in _skillDayCollection2)
            {
                _skillDay2 = skillDay;
                _skillDay2.AddWorkloadDay(workloadDay2);
                break;
            }
            foreach (ISkillDay skillDay in _skillDayCollection3)
            {
                _skillDay3 = skillDay;
                _skillDay3.AddWorkloadDay(workloadDay3);
                break;
            }
            

            //Add some tasks between 00:00 - 00:15 for Inbound Phone
            foreach (ITemplateTaskPeriod taskPeriod in workloadDay1.SortedTaskPeriodList)
            {
                if (taskPeriod.Period.StartDateTime >= _date.Date &&
                    taskPeriod.Period.EndDateTime <= _date.Date.Add(TimeSpan.FromMinutes(15)))
                {
                    taskPeriod.SetTasks(55d);
                    taskPeriod.AverageTaskTime = TimeSpan.FromSeconds(120);
                    taskPeriod.AverageAfterTaskTime = TimeSpan.FromSeconds(20);
                    taskPeriod.CampaignTasks = new Percent(0.10d);
                    taskPeriod.CampaignTaskTime = new Percent(0.15d);
                    taskPeriod.CampaignAfterTaskTime = new Percent(0.2d);

                    _templateTaskPeriodCollection.Add(taskPeriod);
                }
            }

            //Add some tasks between 00:00 - 00:15 for Backoffice
            foreach (ITemplateTaskPeriod taskPeriod in workloadDay2.SortedTaskPeriodList)
            {
                if (taskPeriod.Period.StartDateTime >= _date.Date &&
                    taskPeriod.Period.EndDateTime <= _date.Date.Add(TimeSpan.FromMinutes(15)))
                {
                    taskPeriod.SetTasks(55d);
                    taskPeriod.AverageTaskTime = TimeSpan.FromSeconds(120);
                    taskPeriod.AverageAfterTaskTime = TimeSpan.FromSeconds(20);
                    taskPeriod.CampaignTasks = new Percent(0.10d);
                    taskPeriod.CampaignTaskTime = new Percent(0.15d);
                    taskPeriod.CampaignAfterTaskTime = new Percent(0.2d);

                    _templateTaskPeriodCollection.Add(taskPeriod);
                }
            }

            //Add some tasks between 00:00 - 00:15 for Email
            foreach (ITemplateTaskPeriod taskPeriod in workloadDay3.SortedTaskPeriodList)
            {
                if (taskPeriod.Period.StartDateTime >= _date.Date &&
                    taskPeriod.Period.EndDateTime <= _date.Date.Add(TimeSpan.FromMinutes(60)))
                {
                    taskPeriod.SetTasks(55d);
                    taskPeriod.AverageTaskTime = TimeSpan.FromSeconds(120);
                    taskPeriod.AverageAfterTaskTime = TimeSpan.FromSeconds(20);
                    taskPeriod.CampaignTasks = new Percent(0.10d);
                    taskPeriod.CampaignTaskTime = new Percent(0.15d);
                    taskPeriod.CampaignAfterTaskTime = new Percent(0.2d);

                    _templateTaskPeriodCollection.Add(taskPeriod);
                }
            }
        }

        #endregion

        [Test]
        public void VerifyDataRowInboundPhone()
        {
            var workloadDay = (IWorkloadDay)_templateTaskPeriodCollection[0].Parent;
            var skillDay = (ISkillDay) workloadDay.Parent;
            IList<ITemplateTaskPeriodView> views = _templateTaskPeriodCollection[0].Split(new TimeSpan(0, 15, 0));
            ITemplateTaskPeriodView templateTaskPeriod = views[0];
            DataRow dataRow;

            using (DataTable table = new DataTable())
            {
                table.Locale = Thread.CurrentThread.CurrentCulture;
                ForecastWorkloadInfrastructure.AddColumnsToDataTable(table);

                _target.AddDataRowToDataTable(templateTaskPeriod, _skill1, table);
                Assert.AreEqual(1, table.Rows.Count);
                dataRow = table.Rows[0];
            }

            Assert.AreEqual(templateTaskPeriod.Period.StartDateTime.Date, dataRow["date"]);
            Assert.AreEqual(new Interval(templateTaskPeriod.Period.StartDateTime.Date, 96).Id, dataRow["interval_id"]);
            Assert.AreEqual(templateTaskPeriod.Period.StartDateTime, dataRow["start_time"]);
            Assert.AreEqual(workloadDay.Workload.Id, dataRow["workload_code"]);
            Assert.AreEqual(skillDay.Scenario.Id, dataRow["scenario_code"]);
            Assert.AreEqual(templateTaskPeriod.Period.EndDateTime, dataRow["end_time"]);
            Assert.AreEqual(_skill1.Id, dataRow["skill_code"]);
            Assert.AreEqual(templateTaskPeriod.TotalTasks, dataRow["forecasted_calls"]);
            Assert.AreEqual(templateTaskPeriod.CampaignTasks, dataRow["forecasted_campaign_calls"]);
            Assert.AreEqual(templateTaskPeriod.Tasks, dataRow["forecasted_calls_excl_campaign"]);
            Assert.AreEqual(templateTaskPeriod.TotalAverageTaskTime.TotalSeconds*templateTaskPeriod.TotalTasks,
                            dataRow["forecasted_talk_time_sec"]);
            Assert.AreEqual(templateTaskPeriod.CampaignTaskTime.Value*(double) dataRow["forecasted_talk_time_sec"],
                            dataRow["forecasted_campaign_talk_time_s"]);
            Assert.AreEqual(
                templateTaskPeriod.AverageTaskTime.TotalSeconds*(double) dataRow["forecasted_calls_excl_campaign"],
                dataRow["forecasted_talk_time_excl_campaign_s"]);
            Assert.AreEqual(templateTaskPeriod.TotalAverageAfterTaskTime.TotalSeconds*templateTaskPeriod.TotalTasks,
                            dataRow["forecasted_after_call_work_s"]);
            Assert.AreEqual(
                templateTaskPeriod.CampaignAfterTaskTime.Value*(double) dataRow["forecasted_after_call_work_s"],
                dataRow["forecasted_campaign_after_call_work_s"]);
            Assert.AreEqual(
                templateTaskPeriod.AverageAfterTaskTime.TotalSeconds*(double) dataRow["forecasted_calls_excl_campaign"],
                dataRow["forecasted_after_call_work_excl_campaign_s"]);
            Assert.AreEqual(
                (double) dataRow["forecasted_talk_time_sec"] + (double) dataRow["forecasted_after_call_work_s"],
                dataRow["forecasted_handling_time_s"]);
            Assert.AreEqual(
                (double) dataRow["forecasted_campaign_talk_time_s"] +
                (double) dataRow["forecasted_campaign_after_call_work_s"],
                dataRow["forecasted_campaign_handling_time_s"]);
            Assert.AreEqual(
                (double) dataRow["forecasted_talk_time_excl_campaign_s"] +
                (double) dataRow["forecasted_after_call_work_excl_campaign_s"],
                dataRow["forecasted_handling_time_excl_campaign_s"]);

            Assert.AreEqual(
                templateTaskPeriod.Period.EndDateTime.Subtract(templateTaskPeriod.Period.StartDateTime).TotalMinutes,
                dataRow["period_length_min"]);

            Assert.AreEqual(workloadDay.Workload.Skill.BusinessUnit.Id, dataRow["business_unit_code"]);
            Assert.AreEqual(workloadDay.Workload.Skill.BusinessUnit.Name, dataRow["business_unit_name"]);
            Assert.AreEqual(1, dataRow["datasource_id"]);
            Assert.AreEqual(_insertDateTime, dataRow["insert_date"]);
            Assert.AreEqual(_insertDateTime, dataRow["update_date"]);
			Assert.AreEqual(_updatedOnDateTime, dataRow["datasource_update_date"]);
			
        }

        [Test]
        public void VerifyDataRowInboundBackoffice()
        {
            var workloadDay = (IWorkloadDay)_templateTaskPeriodCollection[1].Parent;
            var skillDay = (ISkillDay)workloadDay.Parent;
            IList<ITemplateTaskPeriodView> views = _templateTaskPeriodCollection[1].Split(new TimeSpan(0, 15, 0));
            ITemplateTaskPeriodView templateTaskPeriod = views[0];
            DataRow dataRow;

            using (DataTable table = new DataTable())
            {
                table.Locale = Thread.CurrentThread.CurrentCulture;
                ForecastWorkloadInfrastructure.AddColumnsToDataTable(table);

                _target.AddDataRowToDataTable(templateTaskPeriod, _skill2, table);
                Assert.AreEqual(1, table.Rows.Count);
                dataRow = table.Rows[0];
            }

            Assert.AreEqual(templateTaskPeriod.Tasks, dataRow["forecasted_backoffice_tasks"]);

            Assert.AreEqual(templateTaskPeriod.Period.StartDateTime.Date, dataRow["date"]);
            Assert.AreEqual(new Interval(templateTaskPeriod.Period.StartDateTime.Date, 96).Id, dataRow["interval_id"]);
            Assert.AreEqual(templateTaskPeriod.Period.StartDateTime, dataRow["start_time"]);
            Assert.AreEqual(workloadDay.Workload.Id, dataRow["workload_code"]);
            Assert.AreEqual(skillDay.Scenario.Id, dataRow["scenario_code"]);
            Assert.AreEqual(templateTaskPeriod.Period.EndDateTime, dataRow["end_time"]);
            Assert.AreEqual(_skill2.Id, dataRow["skill_code"]);
            Assert.AreEqual(templateTaskPeriod.TotalTasks, dataRow["forecasted_calls"]);
            Assert.AreEqual(templateTaskPeriod.CampaignTasks, dataRow["forecasted_campaign_calls"]);
            Assert.AreEqual(templateTaskPeriod.Tasks, dataRow["forecasted_calls_excl_campaign"]);
            Assert.AreEqual(templateTaskPeriod.TotalAverageTaskTime.TotalSeconds * templateTaskPeriod.TotalTasks,
                            dataRow["forecasted_talk_time_sec"]);
            Assert.AreEqual(templateTaskPeriod.CampaignTaskTime.Value * (double)dataRow["forecasted_talk_time_sec"],
                            dataRow["forecasted_campaign_talk_time_s"]);
            Assert.AreEqual(
                templateTaskPeriod.AverageTaskTime.TotalSeconds * (double)dataRow["forecasted_calls_excl_campaign"],
                dataRow["forecasted_talk_time_excl_campaign_s"]);
            Assert.AreEqual(templateTaskPeriod.TotalAverageAfterTaskTime.TotalSeconds * templateTaskPeriod.TotalTasks,
                            dataRow["forecasted_after_call_work_s"]);
            Assert.AreEqual(
                templateTaskPeriod.CampaignAfterTaskTime.Value * (double)dataRow["forecasted_after_call_work_s"],
                dataRow["forecasted_campaign_after_call_work_s"]);
            Assert.AreEqual(
                templateTaskPeriod.AverageAfterTaskTime.TotalSeconds * (double)dataRow["forecasted_calls_excl_campaign"],
                dataRow["forecasted_after_call_work_excl_campaign_s"]);
            Assert.AreEqual(
                (double)dataRow["forecasted_talk_time_sec"] + (double)dataRow["forecasted_after_call_work_s"],
                dataRow["forecasted_handling_time_s"]);
            Assert.AreEqual(
                (double)dataRow["forecasted_campaign_talk_time_s"] +
                (double)dataRow["forecasted_campaign_after_call_work_s"],
                dataRow["forecasted_campaign_handling_time_s"]);
            Assert.AreEqual(
                (double)dataRow["forecasted_talk_time_excl_campaign_s"] +
                (double)dataRow["forecasted_after_call_work_excl_campaign_s"],
                dataRow["forecasted_handling_time_excl_campaign_s"]);

            Assert.AreEqual(
                templateTaskPeriod.Period.EndDateTime.Subtract(templateTaskPeriod.Period.StartDateTime).TotalMinutes,
                dataRow["period_length_min"]);

            Assert.AreEqual(workloadDay.Workload.Skill.BusinessUnit.Id, dataRow["business_unit_code"]);
            Assert.AreEqual(workloadDay.Workload.Skill.BusinessUnit.Name, dataRow["business_unit_name"]);
            Assert.AreEqual(1, dataRow["datasource_id"]);
            Assert.AreEqual(_insertDateTime, dataRow["insert_date"]);
            Assert.AreEqual(_insertDateTime, dataRow["update_date"]);
			Assert.AreEqual(_updatedOnDateTime, dataRow["datasource_update_date"]);
        }

        [Test]
        public void VerifyDataRowInboundEmail()
        {
            var workloadDay = (IWorkloadDay)_templateTaskPeriodCollection[2].Parent;
            var skillDay = (ISkillDay)workloadDay.Parent;
            IList<ITemplateTaskPeriodView> views = _templateTaskPeriodCollection[2].Split(new TimeSpan(0, 15, 0));
            ITemplateTaskPeriodView templateTaskPeriod = views[0];
            DataRow dataRow;

            using (DataTable table = new DataTable())
            {
                table.Locale = Thread.CurrentThread.CurrentCulture;
                ForecastWorkloadInfrastructure.AddColumnsToDataTable(table);

                _target.AddDataRowToDataTable(templateTaskPeriod, _skill3, table);
                Assert.AreEqual(1, table.Rows.Count);
                dataRow = table.Rows[0];
            }

            Assert.AreEqual(templateTaskPeriod.Tasks, dataRow["forecasted_emails"]);

            Assert.AreEqual(templateTaskPeriod.Period.StartDateTime.Date, dataRow["date"]);
            Assert.AreEqual(new Interval(templateTaskPeriod.Period.StartDateTime.Date, 96).Id, dataRow["interval_id"]);
            Assert.AreEqual(templateTaskPeriod.Period.StartDateTime, dataRow["start_time"]);
            Assert.AreEqual(workloadDay.Workload.Id, dataRow["workload_code"]);
            Assert.AreEqual(skillDay.Scenario.Id, dataRow["scenario_code"]);
            //Assert.AreEqual(templateTaskPeriod.Period.EndDateTime, dataRow["end_time"]);
            Assert.AreEqual(templateTaskPeriod.Period.EndDateTime, dataRow["end_time"]);
            Assert.AreEqual(_skill3.Id, dataRow["skill_code"]);
            Assert.AreEqual(templateTaskPeriod.TotalTasks, dataRow["forecasted_calls"]);
            Assert.AreEqual(templateTaskPeriod.CampaignTasks, dataRow["forecasted_campaign_calls"]);
            Assert.AreEqual(templateTaskPeriod.Tasks, dataRow["forecasted_calls_excl_campaign"]);
            Assert.AreEqual(templateTaskPeriod.TotalAverageTaskTime.TotalSeconds * templateTaskPeriod.TotalTasks,
                            dataRow["forecasted_talk_time_sec"]);
            Assert.AreEqual(templateTaskPeriod.CampaignTaskTime.Value * (double)dataRow["forecasted_talk_time_sec"],
                            dataRow["forecasted_campaign_talk_time_s"]);
            Assert.AreEqual(
                templateTaskPeriod.AverageTaskTime.TotalSeconds * (double)dataRow["forecasted_calls_excl_campaign"],
                dataRow["forecasted_talk_time_excl_campaign_s"]);
            Assert.AreEqual(templateTaskPeriod.TotalAverageAfterTaskTime.TotalSeconds * templateTaskPeriod.TotalTasks,
                            dataRow["forecasted_after_call_work_s"]);
            Assert.AreEqual(
                templateTaskPeriod.CampaignAfterTaskTime.Value * (double)dataRow["forecasted_after_call_work_s"],
                dataRow["forecasted_campaign_after_call_work_s"]);
            Assert.AreEqual(
                templateTaskPeriod.AverageAfterTaskTime.TotalSeconds * (double)dataRow["forecasted_calls_excl_campaign"],
                dataRow["forecasted_after_call_work_excl_campaign_s"]);
            Assert.AreEqual(
                (double)dataRow["forecasted_talk_time_sec"] + (double)dataRow["forecasted_after_call_work_s"],
                dataRow["forecasted_handling_time_s"]);
            Assert.AreEqual(
                (double)dataRow["forecasted_campaign_talk_time_s"] +
                (double)dataRow["forecasted_campaign_after_call_work_s"],
                dataRow["forecasted_campaign_handling_time_s"]);
            Assert.AreEqual(
                (double)dataRow["forecasted_talk_time_excl_campaign_s"] +
                (double)dataRow["forecasted_after_call_work_excl_campaign_s"],
                dataRow["forecasted_handling_time_excl_campaign_s"]);

            Assert.AreEqual(
                templateTaskPeriod.Period.EndDateTime.Subtract(templateTaskPeriod.Period.StartDateTime).TotalMinutes,
                dataRow["period_length_min"]);

            Assert.AreEqual(workloadDay.Workload.Skill.BusinessUnit.Id, dataRow["business_unit_code"]);
            Assert.AreEqual(workloadDay.Workload.Skill.BusinessUnit.Name, dataRow["business_unit_name"]);
            Assert.AreEqual(1, dataRow["datasource_id"]);
            Assert.AreEqual(_insertDateTime, dataRow["insert_date"]);
            Assert.AreEqual(_insertDateTime, dataRow["update_date"]);
			Assert.AreEqual(_updatedOnDateTime, dataRow["datasource_update_date"]);
        }

        [Test]
        public void VerifyDataTables()
        {
            using (DataTable dataTable1 = new DataTable())
            {
                dataTable1.Locale = Thread.CurrentThread.CurrentCulture;
                ForecastWorkloadInfrastructure.AddColumnsToDataTable(dataTable1);
                _target.Transform(_skillDayCollection1, dataTable1);
                Assert.AreEqual(672, dataTable1.Rows.Count);
            }

            using (DataTable dataTable2 = new DataTable())
            {
                dataTable2.Locale = Thread.CurrentThread.CurrentCulture;
                ForecastWorkloadInfrastructure.AddColumnsToDataTable(dataTable2);
                _target.Transform(_skillDayCollection2, dataTable2);
                Assert.AreEqual(672, dataTable2.Rows.Count);
            }

            using (DataTable dataTable3 = new DataTable())
            {
                dataTable3.Locale = Thread.CurrentThread.CurrentCulture;
                ForecastWorkloadInfrastructure.AddColumnsToDataTable(dataTable3);
                _target.Transform(_skillDayCollection3, dataTable3);
                Assert.AreEqual(672, dataTable3.Rows.Count);
            }
        }
    }
}