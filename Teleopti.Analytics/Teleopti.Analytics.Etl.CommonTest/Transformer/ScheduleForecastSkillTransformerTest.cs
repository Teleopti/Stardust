using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer
{
    [TestFixture]
    public class ScheduleForecastSkillTransformerTest
    {
        private ScheduleForecastSkillTransformer _target;
        private DateTime _startDateTime;
        private int _intervalId;
        private Guid _skillCode;
        private Dictionary<IScheduleForecastSkillKey, IScheduleForecastSkill> _scheduleForecstSkillDictionary;
        private IScheduleForecastSkillKey _scheduleForecastSkillKey;
        private IScenario _scenario;

        [SetUp]
        public void Setup()
        {
            _startDateTime = new DateTime(2010, 10, 1, 8, 0, 0, DateTimeKind.Utc);
            _target = new ScheduleForecastSkillTransformer(96, _startDateTime);
            _scenario = ScenarioFactory.CreateScenarioAggregate("Default Scenario", true);
            _scenario.SetId(Guid.NewGuid());
            _intervalId = 32;
            _skillCode = Guid.NewGuid();
            PrepareScheduleForecstSkillDictionary();
        }

        [Test]
        public void VerifyTransformAndDataStructure()
        {
            MockRepository mocks = new MockRepository();
            IScheduleForecastSkillResourceCalculation scheduleForecastSkillResourceCalculation =
                mocks.StrictMock<IScheduleForecastSkillResourceCalculation>();
            DataRow row;

            using (mocks.Record())
            {
                Expect.Call(scheduleForecastSkillResourceCalculation.GetResourceDataExcludingShrinkage(DateTime.Now)).
                    Return(new Dictionary<IScheduleForecastSkillKey, IScheduleForecastSkill>()).IgnoreArguments();
                Expect.Call(scheduleForecastSkillResourceCalculation.GetResourceDataIncludingShrinkage(DateTime.Now)).
                    Return(_scheduleForecstSkillDictionary).IgnoreArguments();
            }

            using (mocks.Playback())
            {
                using (DataTable dataTable = new DataTable())
                {
                    dataTable.Locale = Thread.CurrentThread.CurrentCulture;
                    ScheduleForecastSkillInfrastructure.AddColumnsToDataTable(dataTable);

                    _target.Transform(scheduleForecastSkillResourceCalculation, dataTable);

                    // Check that the datatable values match the ScheduleForecastSkill dictionary
                    Assert.IsNotNull(dataTable);
                    Assert.AreEqual(1, dataTable.Rows.Count);

                    row = dataTable.Rows[0];
                }
            }

            IScheduleForecastSkill scheduleForecastSkill;
            _scheduleForecstSkillDictionary.TryGetValue(_scheduleForecastSkillKey, out scheduleForecastSkill);
            Assert.IsNotNull(scheduleForecastSkill);

            Assert.AreEqual(row["date"], scheduleForecastSkill.StartDateTime.Date);
            Assert.AreEqual(row["interval_id"], scheduleForecastSkill.IntervalId);
            Assert.AreEqual(row["skill_code"], scheduleForecastSkill.SkillCode);
            Assert.AreEqual(row["scenario_code"], scheduleForecastSkill.ScenarioCode);
            Assert.AreEqual(row["forecasted_resources_m"], scheduleForecastSkill.ForecastedResourcesMinutes);
            Assert.AreEqual(row["forecasted_resources"], scheduleForecastSkill.ForecastedResources);
            Assert.AreEqual(row["forecasted_resources_incl_shrinkage_m"], scheduleForecastSkill.ForecastedResourcesIncludingShrinkageMinutes);
            Assert.AreEqual(row["forecasted_resources_incl_shrinkage"], scheduleForecastSkill.ForecastedResourcesIncludingShrinkage);
            Assert.AreEqual(row["scheduled_resources_m"], scheduleForecastSkill.ScheduledResourcesMinutes);
            Assert.AreEqual(row["scheduled_resources"], scheduleForecastSkill.ScheduledResources);
            Assert.AreEqual(row["scheduled_resources_incl_shrinkage_m"], scheduleForecastSkill.ScheduledResourcesIncludingShrinkageMinutes);
            Assert.AreEqual(row["scheduled_resources_incl_shrinkage"], scheduleForecastSkill.ScheduledResourcesIncludingShrinkage);
            Assert.AreEqual(row["forecasted_tasks"], scheduleForecastSkill.ForecastedTasks);
            Assert.AreEqual(row["estimated_tasks_answered_within_sl"], scheduleForecastSkill.EstimatedTasksAnsweredWithinSL);
			Assert.AreEqual(row["forecasted_tasks_incl_shrinkage"], scheduleForecastSkill.ForecastedTasksIncludingShrinkage);
			Assert.AreEqual(row["estimated_tasks_answered_within_sl_incl_shrinkage"], scheduleForecastSkill.EstimatedTasksAnsweredWithinSLIncludingShrinkage);
			Assert.AreEqual(row["business_unit_code"], scheduleForecastSkill.BusinessUnitCode);
            Assert.AreEqual(row["business_unit_name"], scheduleForecastSkill.BusinessUnitName);
            Assert.AreEqual(row["datasource_id"], scheduleForecastSkill.DataSourceId);
            Assert.AreEqual(row["insert_date"], scheduleForecastSkill.InsertDate);
            Assert.AreEqual(row["update_date"], scheduleForecastSkill.UpdateDate);
        }

        private void PrepareScheduleForecstSkillDictionary()
        {
            _scheduleForecastSkillKey = new ScheduleForecastSkillKey(_startDateTime, _intervalId, _skillCode, (Guid) _scenario.Id);
            IScheduleForecastSkill scheduleForecastSkill = new ScheduleForecastSkill(_scheduleForecastSkillKey.StartDateTime, _scheduleForecastSkillKey.IntervalId,
                                                                                     _scheduleForecastSkillKey.SkillCode, _scheduleForecastSkillKey.ScenarioCode);
            scheduleForecastSkill.ForecastedResourcesMinutes = 150d;
            scheduleForecastSkill.ForecastedResources = 10d;
            scheduleForecastSkill.ForecastedResourcesIncludingShrinkageMinutes = 225d;
            scheduleForecastSkill.ForecastedResourcesIncludingShrinkage = 15d;
            scheduleForecastSkill.ScheduledResourcesMinutes = 165d;
            scheduleForecastSkill.ScheduledResources = 11d;
            scheduleForecastSkill.ScheduledResourcesIncludingShrinkageMinutes = 240d;
            scheduleForecastSkill.ScheduledResourcesIncludingShrinkage = 16d;
            scheduleForecastSkill.ForecastedTasks = 201d;
            scheduleForecastSkill.EstimatedTasksAnsweredWithinSL = 21d;
            scheduleForecastSkill.ForecastedTasksIncludingShrinkage = 202d;
            scheduleForecastSkill.EstimatedTasksAnsweredWithinSLIncludingShrinkage = 22d;
            scheduleForecastSkill.BusinessUnitCode = Guid.NewGuid();
            scheduleForecastSkill.BusinessUnitName = "BU name";
            scheduleForecastSkill.DataSourceId = 1;
            scheduleForecastSkill.InsertDate = DateTime.Now;
            scheduleForecastSkill.UpdateDate = DateTime.Now;

            _scheduleForecstSkillDictionary = new Dictionary<IScheduleForecastSkillKey, IScheduleForecastSkill>();
            _scheduleForecstSkillDictionary.Add(_scheduleForecastSkillKey, scheduleForecastSkill);
        }
    }
}