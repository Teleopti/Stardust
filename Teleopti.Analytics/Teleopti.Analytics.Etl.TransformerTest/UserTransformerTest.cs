using System.Collections.Generic;
using System.Data;
using System.Threading;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerTest
{
    [TestFixture]
    public class UserTransformerTest
    {
        private UserTransformer _target;
        private IList<IPerson> _personCollection;
        private DataRow _row0;
        private DataRow _row1;

        [SetUp]
        public void Setup()
        {
            _personCollection = FakeData.UserFactory.CreatePersonUserCollection();
            _target = new UserTransformer();

            using (DataTable table = new DataTable())
            {
                table.Locale = Thread.CurrentThread.CurrentCulture;
                UserInfrastructure.AddColumnsToDataTable(table);

                _target.Transform(_personCollection, table);

                Assert.AreEqual(2, table.Rows.Count);
                _row0 = table.Rows[0];
                _row1 = table.Rows[1];
            }
            
        }

        [Test]
        public void VerifyUser()
        {
            IPerson personUser = _personCollection[0];

            Assert.AreEqual(personUser.Id, _row0["person_code"]);
            Assert.AreEqual(personUser.Name.FirstName, _row0["person_first_name"]);
            Assert.AreEqual(personUser.Name.LastName, _row0["person_last_name"]);
            Assert.AreEqual(personUser.ApplicationAuthenticationInfo.ApplicationLogOnName,
                            _row0["application_logon_name"]);
            Assert.AreEqual(personUser.WindowsAuthenticationInfo.WindowsLogOnName,
                            _row0["windows_logon_name"]);
            Assert.AreEqual(personUser.WindowsAuthenticationInfo.DomainName,
                            _row0["windows_domain_name"]);
            Assert.AreEqual(personUser.ApplicationAuthenticationInfo.Password, _row0["password"]);
            Assert.AreEqual(personUser.Email, _row0["email"]);
            Assert.AreEqual(personUser.PermissionInformation.UICultureLCID().GetValueOrDefault(-1), _row0["language_id"]);
            Assert.AreEqual(System.DBNull.Value, _row0["language_name"]);
            Assert.AreEqual(personUser.PermissionInformation.CultureLCID().GetValueOrDefault(-1), _row0["culture"]);
            Assert.AreEqual(personUser.UpdatedOn, _row0["datasource_update_date"]);
        }

        [Test]
        public void VerifyUserWithoutCultureSet()
        {
            IPerson personUser = _personCollection[1];

            Assert.AreEqual(personUser.PermissionInformation.UICultureLCID().GetValueOrDefault(-1), _row1["language_id"]);
            Assert.AreEqual(System.DBNull.Value, _row1["language_name"]);
            Assert.AreEqual(personUser.PermissionInformation.CultureLCID().GetValueOrDefault(-1), _row1["culture"]);
        }
    }
}
