using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer
{
    [TestFixture]
    public class KpiTargetTeamTransformerTest
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            _kpiTargetTeamCollection = KpiTargetTeamFactory.CreateKpiTargetCollection();
            _target = new KpiTargetTeamTransformer(_insertDateTime);

            using (DataTable dataTable = new DataTable())
            {
                dataTable.Locale = Thread.CurrentThread.CurrentCulture;
                KpiTargetTeamInfrastructure.AddColumnsToDataTable(dataTable);
                _target.Transform(_kpiTargetTeamCollection, dataTable);

                Assert.Greater(dataTable.Rows.Count, 1);
                _dataRow0 = dataTable.Rows[0];
                _dataRow1 = dataTable.Rows[1];
            }
        }

        #endregion

        private DataRow _dataRow0;
        private DataRow _dataRow1;
        private IList<IKpiTarget> _kpiTargetTeamCollection;
        private KpiTargetTeamTransformer _target;
        private readonly DateTime _insertDateTime = DateTime.Now;

        [Test]
        public void VerifyAggregateRoot()
        {
            //BusinessUnit
            Assert.AreEqual(_kpiTargetTeamCollection[0].GetOrFillWithBusinessUnit_DONTUSE().Id, _dataRow0["business_unit_code"]);
            Assert.AreEqual(_kpiTargetTeamCollection[0].GetOrFillWithBusinessUnit_DONTUSE().Description.Name,
                            _dataRow0["business_unit_name"]);
            //UpdatedOn
            Assert.AreEqual(RaptorTransformerHelper.GetUpdatedDate(_kpiTargetTeamCollection[0]),
                            _dataRow0["datasource_update_date"]);
        }

        [Test]
        public void VerifyKpiTarget()
        {
            Assert.AreEqual(_kpiTargetTeamCollection[0].KeyPerformanceIndicator.Id, _dataRow0["kpi_code"]);
            Assert.AreEqual(_kpiTargetTeamCollection[1].Team.Id, _dataRow1["team_code"]);
            Assert.AreEqual(_kpiTargetTeamCollection[0].TargetValue, _dataRow0["target_value"]);
            Assert.AreEqual(_kpiTargetTeamCollection[1].MinValue, _dataRow1["min_value"]);
            Assert.AreEqual(_kpiTargetTeamCollection[0].MaxValue, _dataRow0["max_value"]);
            Assert.AreEqual(_kpiTargetTeamCollection[1].BetweenColor.ToArgb(), _dataRow1["between_color"]);
            Assert.AreEqual(_kpiTargetTeamCollection[0].LowerThanMinColor.ToArgb(),
                            _dataRow0["lower_than_min_color"]);
            Assert.AreEqual(_kpiTargetTeamCollection[1].HigherThanMaxColor.ToArgb(),
                            _dataRow1["higher_than_max_color"]);
        }

        [Test]
        public void VerifyTheMatrixInternalData()
        {
            Assert.AreEqual(1, _dataRow0["datasource_id"]);
            Assert.AreEqual(_insertDateTime, _dataRow0["insert_date"]);
            Assert.AreEqual(_insertDateTime, _dataRow0["update_date"]);
        }
    }
}