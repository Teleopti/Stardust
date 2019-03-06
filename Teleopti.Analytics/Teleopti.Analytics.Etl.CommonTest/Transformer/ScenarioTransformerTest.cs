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
    public class ScenarioTransformerTest
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            _scenarioCollection = ScenarioFactory.CreateScenarioCollection();
            var scenarioTransformer = new ScenarioTransformer(_insertDateTime);

            using (var table = new DataTable())
            {
                table.Locale = Thread.CurrentThread.CurrentCulture;
                ScenarioInfrastructure.AddColumnsToDataTable(table);

                scenarioTransformer.Transform(_scenarioCollection, table);

                Assert.AreEqual(4, table.Rows.Count);

                _dataRow0 = table.Rows[0];
                _dataRow1 = table.Rows[1];
                _dataRow2 = table.Rows[2];
                _dataRow3 = table.Rows[3];
            }
        }

        #endregion

        private readonly DateTime _insertDateTime = DateTime.Now;
        private IList<IScenario> _scenarioCollection;
        private DataRow _dataRow0;
        private DataRow _dataRow1;
        private DataRow _dataRow2;
        private DataRow _dataRow3;

        [Test]
        public void VerifyAggregateRoot()
        {
            //BusinessUnit
            Assert.AreEqual(_scenarioCollection[2].GetOrFillWithBusinessUnit_DONTUSE().Id, _dataRow2["business_unit_code"]);
            Assert.AreEqual(_scenarioCollection[0].GetOrFillWithBusinessUnit_DONTUSE().Description.Name, _dataRow0["business_unit_name"]);
            //UpdatedOn
            Assert.AreEqual(RaptorTransformerHelper.GetUpdatedDate(_scenarioCollection[0]),
                            _dataRow0["datasource_update_date"]);
        }

        [Test]
        public void VerifyScenario()
        {
            Assert.AreEqual(_scenarioCollection[0].Id, _dataRow0["scenario_code"]);
            Assert.AreEqual(_scenarioCollection[2].Description.Name, _dataRow2["scenario_name"]);
            Assert.AreEqual(_scenarioCollection[1].DefaultScenario, _dataRow1["default_scenario"]);
        }

        [Test]
        public void VerifyTheMatrixInternalData()
        {
            Assert.AreEqual(1, _dataRow0["datasource_id"]);
            Assert.AreEqual(_insertDateTime, _dataRow1["insert_date"]);
            Assert.AreEqual(_insertDateTime, _dataRow2["update_date"]);
        }

        [Test]
        public void VerifyIsDeleted()
        {
            Assert.IsFalse((bool)_dataRow0["is_deleted"]);
            Assert.IsTrue((bool)_dataRow3["is_deleted"]);
        }
    }
}