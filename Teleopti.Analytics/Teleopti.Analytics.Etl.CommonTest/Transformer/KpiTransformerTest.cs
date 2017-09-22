using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Kpi;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer
{
    [TestFixture]
    public class KpiTransformerTest
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            _kpiList = new List<IKeyPerformanceIndicator>();

            _kpi = new KeyPerformanceIndicator();
            ((IEntity) _kpi).SetId(Guid.NewGuid());

            _kpiList.Add(_kpi);

            RaptorTransformerHelper.SetUpdatedOn(_kpi, _insertDateTime);

            using (DataTable dataTable = new DataTable())
            {
                dataTable.Locale = Thread.CurrentThread.CurrentCulture;
                KpiInfrastructure.AddColumnsToDataTable(dataTable);

                _target = new KpiTransformer(_insertDateTime);
                _target.Transform(_kpiList, dataTable);

                Assert.Greater(dataTable.Rows.Count, 0);
                _dataRow = dataTable.Rows[0];    
            }
        }

        #endregion

        private KeyPerformanceIndicator _kpi;
        private readonly DateTime _insertDateTime = DateTime.Now;
        private KpiTransformer _target;
        private IList<IKeyPerformanceIndicator> _kpiList;
        private DataRow _dataRow;

        [Test]
        public void VerifyAggregateRoot()
        {
            //UpdatedOn
            Assert.AreEqual(RaptorTransformerHelper.GetUpdatedDate(_kpiList[0]),
                            _dataRow["datasource_update_date"]);
        }

        [Test]
        public void VerifyKeyPerformanceIndicator()
        {
            Assert.AreEqual(_kpiList[0].Id, _dataRow["kpi_code"]);
            Assert.AreEqual(_kpiList[0].Name, _dataRow["kpi_name"]);
            Assert.AreEqual(_kpiList[0].ResourceKey, _dataRow["resource_key"]);
            Assert.AreEqual((int) _kpiList[0].TargetValueType, _dataRow["target_value_type"]);
            Assert.AreEqual(_kpiList[0].DefaultTargetValue, _dataRow["default_target_value"]);
            Assert.AreEqual(_kpiList[0].DefaultMinValue, _dataRow["default_min_value"]);
            Assert.AreEqual(_kpiList[0].DefaultMaxValue, _dataRow["default_max_value"]);
            Assert.AreEqual(_kpiList[0].DefaultBetweenColor.ToArgb(), _dataRow["default_between_color"]);
            Assert.AreEqual(_kpiList[0].DefaultLowerThanMinColor.ToArgb(),
                            _dataRow["default_lower_than_min_color"]);
            Assert.AreEqual(_kpiList[0].DefaultHigherThanMaxColor.ToArgb(),
                            _dataRow["default_higher_than_max_color"]);
        }

        [Test]
        public void VerifyTheMatrixInternalData()
        {
            Assert.AreEqual(1, _dataRow["datasource_id"]);
            Assert.AreEqual(_insertDateTime, _dataRow["insert_date"]);
            Assert.AreEqual(_insertDateTime, _dataRow["update_date"]);
        }
    }
}