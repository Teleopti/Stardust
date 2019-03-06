using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Threading;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer
{
    [TestFixture]
    public class ActivityTransformerTest
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            _activityCollection = ActivityFactory.CreateActivityCollection();
            ActivityTransformer activityTransformer = new ActivityTransformer(_insertDateTime);
            _table = new DataTable();
            _table.Locale = Thread.CurrentThread.CurrentCulture;
            ActivityInfrastructure.AddColumnsToDataTable(_table);
            activityTransformer.Transform(_activityCollection, _table);
        }

        #endregion

        private IList<IActivity> _activityCollection;
        private readonly DateTime _insertDateTime = DateTime.Now;
        private DataTable _table;

        [Test]
        public void VerifyActivity()
        {
            Assert.AreEqual(4, _table.Rows.Count);
            Assert.AreEqual(_activityCollection[0].Id, _table.Rows[0]["activity_code"]);
            Assert.AreEqual(_activityCollection[2].Description.Name, _table.Rows[2]["activity_name"]);
            Assert.AreEqual(_activityCollection[2].DisplayColor.ToArgb(), Color.FromArgb((int) _table.Rows[2]["display_color"]).ToArgb());
            Assert.AreEqual(_activityCollection[0].InReadyTime, _table.Rows[0]["in_ready_time"]);
        }

        [Test]
        public void VerifyAggregateRoot()
        {
            //BusinessUnit
            Assert.AreEqual(_activityCollection[0].GetOrFillWithBusinessUnit_DONTUSE().Id, _table.Rows[0]["business_unit_code"]);
            Assert.AreEqual(_activityCollection[1].GetOrFillWithBusinessUnit_DONTUSE().Description.Name, _table.Rows[1]["business_unit_name"]);
            //UpdatedOn
            Assert.AreEqual(RaptorTransformerHelper.GetUpdatedDate(_activityCollection[0]),
                            _table.Rows[0]["datasource_update_date"]);
        }

        [Test]
        public void VerifyTheMatrixInternalData()
        {
            Assert.AreEqual(1, _table.Rows[0]["datasource_id"]);
            Assert.AreEqual(_insertDateTime, _table.Rows[1]["insert_date"]);
            Assert.AreEqual(_insertDateTime, _table.Rows[2]["update_date"]);
        }

        [Test]
        public void VerifyIsDeleted()
        {
            Assert.IsFalse((bool)_table.Rows[0]["is_deleted"]);
            Assert.IsTrue((bool)_table.Rows[3]["is_deleted"]);
        }
    }
}