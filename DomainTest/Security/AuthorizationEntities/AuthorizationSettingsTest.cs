using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.AuthorizationEntities
{

    [TestFixture]
    public class AuthorizationSettingsTest
    {

        #region Variables

        // Variable to hold object to be tested for reuse by init functions
        private AuthorizationSettings _target;

        #endregion

        #region SetUp and TearDown

        [SetUp]
        public void TestInit()
        {
            _target = new AuthorizationSettings();
        }

        [TearDown]
        public void TestDispose()
        {
            _target = null;
        }

        #endregion

        #region Static Method Tests

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [Test]
        public void VerifySameInstance()
        {
            AuthorizationSettings fistInstance;
            AuthorizationSettings secondInstance;
            fistInstance = AuthorizationSettings.Default;
            secondInstance = AuthorizationSettings.Default;
            Assert.AreSame(fistInstance, secondInstance);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [Test]
        public void VerifyCurrent()
        {
            AuthorizationSettings instance = new AuthorizationSettings();
            AuthorizationSettings.Current = instance;
            Assert.AreSame(instance, AuthorizationSettings.Current);

            AuthorizationSettings.Current = null;
            Assert.IsNotNull(AuthorizationSettings.Current);
            Assert.AreSame(AuthorizationSettings.Default, AuthorizationSettings.Current);
        }


        #endregion

        #region Constructor Tests

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }

        #endregion

        #region Method Tests

        #endregion

        #region Property Tests

        [Test]
        public void VerifyUseDatabaseDefinedRoles()
        {
            // Declare variable to hold property set method
            System.Boolean setValue = true;

            // Test set method
            _target.UseDatabaseDefinedRoles = setValue;

            // Declare return variable to hold property get method
            System.Boolean getValue = false;

            // Test get method
            getValue = _target.UseDatabaseDefinedRoles;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyUseActiveDirectoryDefinedRoles()
        {
            // Declare variable to hold property set method
            System.Boolean setValue = true;

            // Test set method
            _target.UseActiveDirectoryDefinedRoles = setValue;

            // Declare return variable to hold property get method
            System.Boolean getValue = false;

            // Test get method
            getValue = _target.UseActiveDirectoryDefinedRoles;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyPermissionType()
        {
            // Declare variable to hold property set method
            PermissionOption setValue = PermissionOption.Rigid;

            // Test set method
            _target.PermissionType = setValue;

            // Declare return variable to hold property get method
            PermissionOption getValue = PermissionOption.Generous;

            // Test get method
            getValue = _target.PermissionType;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyPermissionTypeRigid()
        {
            // Declare variable to hold property set method
            System.Boolean setValue = true;

            // Test set method
            _target.PermissionTypeRigid = setValue;

            // Declare return variable to hold property get method
            System.Boolean getValue = false;

            // Test get method
            getValue = _target.PermissionTypeRigid;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyPermissionTypeGenerous()
        {
            // Declare variable to hold property set method
            System.Boolean setValue = true;

            // Test set method
            _target.PermissionTypeGenerous = setValue;

            // Declare return variable to hold property get method
            System.Boolean getValue = false;

            // Test get method
            getValue = _target.PermissionTypeGenerous;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyDeletedProperty()
        {
            Assert.IsFalse(_target.IsDeleted);

            _target.SetDeleted();

            Assert.IsTrue(_target.IsDeleted);
        }

        #endregion

    }
}
