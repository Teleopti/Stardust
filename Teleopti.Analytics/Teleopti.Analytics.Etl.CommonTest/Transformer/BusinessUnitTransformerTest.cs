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
    public class BusinessUnitTransformerTest
    {
        private BusinessUnitTransformer _target;
        private DataTable _dataTable;
        private IBusinessUnit _bu;
        private readonly DateTime _insertDateTime = DateTime.Now;

        [SetUp]
        public void Setup()
        {
            _dataTable= new DataTable();
            _dataTable.Locale = Thread.CurrentThread.CurrentCulture;
            BusinessUnitInfrastructure.AddColumnsToDataTable(_dataTable);
            _bu = BusinessUnitFactory.CreateSimpleBusinessUnit("BU1");
            IList<IBusinessUnit> buList = new List<IBusinessUnit> {_bu};
            _target = new BusinessUnitTransformer(_insertDateTime);
            _target.Transform(buList, _dataTable);
        }

        [Test]
        public void VerifyBusinessUnit()
        {
            Assert.AreEqual(_bu.Id, _dataTable.Rows[0]["business_unit_code"]);

        }
    }
}
