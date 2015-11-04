using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Ccc.Domain.Security.Matrix;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer
{
    [TestFixture]
    public class PermissionReportTransformerTest
    {
        private PermissionReportTransformer _target;
        private ITeam _team1;
        private ITeam _team2;
        private IPerson _person;
        private IList<MatrixPermissionHolder> _permissionCollection;
        private MatrixPermissionHolder _permissionHolder1;
        private MatrixPermissionHolder _permissionHolder2;
        private MatrixPermissionHolder _permissionHolder3;

        [SetUp]
        public void Setup()
        {
            IBusinessUnit businessUnitGraph = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams();
            businessUnitGraph.SetId(Guid.NewGuid());
            _team1 = businessUnitGraph.TeamCollection()[0];
            _team2 = businessUnitGraph.TeamCollection()[1];
            _team1.SetId(Guid.NewGuid());
            _team2.SetId(Guid.NewGuid());
            _person = PersonFactory.CreatePerson("Johnnie", "Begood");
            _person.SetId(Guid.NewGuid());
            IList<IApplicationFunction> reportCollection = ApplicationFunctionFactory.CreateApplicationFunctionWithMatrixReports();
            _permissionHolder1 = new MatrixPermissionHolder(_person, _team1, false, reportCollection[3]);
            _permissionHolder2 = new MatrixPermissionHolder(_person, _team2, true, reportCollection[5]);
            _permissionHolder3 = new MatrixPermissionHolder(_person, _team2, true, reportCollection[6]);
            _permissionCollection = new List<MatrixPermissionHolder> {_permissionHolder1, _permissionHolder2, _permissionHolder3};
            _target = new PermissionReportTransformer();
        }

        [Test]
        public void VerifyTransform()
        {
            DataRow row1;
            DataRow row2;
            using (var table = new DataTable())
            {
                table.Locale = Thread.CurrentThread.CurrentCulture;
                PermissionReportInfrastructure.AddColumnsToDataTable(table);

                _target.Transform(_permissionCollection, table);
                row1 = table.Rows[0];
                row2 = table.Rows[1];

                Assert.IsNotNull(table);
                Assert.AreEqual(3, table.Rows.Count);    
            }

            Assert.AreEqual(_permissionHolder1.Person.Id, row1["person_code"]);
            Assert.AreEqual(new Guid(_permissionHolder1.ApplicationFunction.ForeignId), row1["ReportId"]);
            Assert.AreEqual(_permissionHolder1.Team.Id, row1["team_id"]);
            Assert.AreEqual(_permissionHolder1.IsMy, row1["my_own"]);

            Assert.AreEqual(_permissionHolder2.Person.Id, row2["person_code"]);
            Assert.AreEqual(new Guid(_permissionHolder2.ApplicationFunction.ForeignId),
                            row2["ReportId"]);
            Assert.AreEqual(_permissionHolder2.Team.Id, row2["team_id"]);
            Assert.AreEqual(_permissionHolder2.IsMy, row2["my_own"]);
        }
    }
}
