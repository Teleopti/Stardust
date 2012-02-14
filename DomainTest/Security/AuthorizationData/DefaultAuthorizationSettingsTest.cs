using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.AuthorizationData
{

    [TestFixture]
    public class DefaultAuthorizationSettingsTest
    {

        #region Variables

        // Variable to hold object to be tested for reuse by init functions
        private AuthorizationSettings _target;

        #endregion

        #region SetUp and TearDown

        [SetUp]
        public void TestInit()
        {
            _target = AuthorizationSettings.Default;
        }

        [TearDown]
        public void TestDispose()
        {
            _target = null;
        }

        #endregion

        #region Constructor Tests

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }

        #endregion

        #region Property Tests

        [Test]
        public void VerifyUseDatabaseDefinedRoles()
        {
            // Declare variable to hold property set method
            System.Boolean expectedValue = true;

            // Test get method
            System.Boolean getValue = _target.UseDatabaseDefinedRoles;

            // Perform Assert Tests
            Assert.AreEqual(expectedValue, getValue);
        }

        [Test]
        public void VerifyUseActiveDirectoryDefinedRoles()
        {
            // Declare variable to hold property set method
            bool expectedValue = false;

            // Test get method
            bool getValue = _target.UseActiveDirectoryDefinedRoles;

            // Perform Assert Tests
            Assert.AreEqual(expectedValue, getValue);
        }

        [Test]
        public void VerifyPermissionType()
        {
            // Declare variable to hold property set method
            PermissionOption expectedValue = PermissionOption.Generous;

            // Test get method
            PermissionOption getValue = _target.PermissionType;

            // Perform Assert Tests
            Assert.AreEqual(expectedValue, getValue);
        }

        #endregion

    }
}
