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
    public class AbsenceTransformerTest
    {
        private DataTable _dataTable;
        private AbsenceTransformer _target;
        private IList<IAbsence> _absenceList;
        private readonly DateTime _insertDateTime = DateTime.Now;

        [SetUp]
        public void Setup()
        {
            _dataTable = new DataTable();
            _dataTable.Locale = Thread.CurrentThread.CurrentCulture;
            _absenceList = AbsenceFactory.CreateAbsenceCollection();
            AbsenceInfrastructure.AddColumnsToDataTable(_dataTable);
            _target = new AbsenceTransformer(_insertDateTime);
            _target.Transform(_absenceList, _dataTable);
        }

        [Test]
        public void VerifyAbsence()
        {
            Assert.AreEqual(3, _dataTable.Rows.Count);
            Assert.AreEqual(_absenceList[0].Id, _dataTable.Rows[0]["absence_code"]);
            Assert.AreEqual(_absenceList[1].Description.Name, _dataTable.Rows[1]["absence_name"]);
            Assert.AreEqual(_absenceList[0].DisplayColor.ToArgb(), _dataTable.Rows[0]["display_color"]);
        }

        [Test]
        public void VerifyAggregateRoot()
        {
            //BusinessUnit
            Assert.AreEqual(_absenceList[0].GetOrFillWithBusinessUnit_DONTUSE().Id, _dataTable.Rows[0]["business_unit_code"]);
            Assert.AreEqual(_absenceList[1].GetOrFillWithBusinessUnit_DONTUSE().Description.Name, _dataTable.Rows[1]["business_unit_name"]);
            //UpdatedOn
            Assert.AreEqual(RaptorTransformerHelper.GetUpdatedDate(_absenceList[0]),
                            _dataTable.Rows[0]["datasource_update_date"]);
        }

        [Test]
        public void VerifyTheMatrixInternalData()
        {
            Assert.AreEqual(1, _dataTable.Rows[0]["datasource_id"]);
            Assert.AreEqual(_insertDateTime, _dataTable.Rows[0]["insert_date"]);
            Assert.AreEqual(_insertDateTime, _dataTable.Rows[1]["update_date"]);
        }

        [Test]
        public void VerifyIsDeleted()
        {
            Assert.IsFalse((bool)_dataTable.Rows[0]["is_deleted"]);
            Assert.IsTrue((bool)_dataTable.Rows[2]["is_deleted"]);
        }
    }
}