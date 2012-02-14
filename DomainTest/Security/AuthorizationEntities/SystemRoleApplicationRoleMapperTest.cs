using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.AuthorizationEntities
{
    [TestFixture]
    public class SystemRoleApplicationRoleMapperTest
    {
        private SystemRoleApplicationRoleMapper _target;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _target = new SystemRoleApplicationRoleMapper();
        }

        [Test]
        public void VerifySystemRoleNameProperty()
        {
            string roleName = "testName";
            _target.SystemRoleLongName = roleName;
            Assert.AreEqual(roleName, _target.SystemRoleLongName);
        }

        [Test]
        public void VerifySystemNameProperty()
        {
            string systemName = "testName";
            _target.SystemName = systemName;
            Assert.AreEqual(systemName, _target.SystemName);
        }

        [Test]
        public void VerifyApplicationRoleProperty()
        {
            ApplicationRole role = ApplicationRoleFactory.CreateRole("testRole", "desc");
            _target.ApplicationRole = role;
            Assert.AreSame(role, _target.ApplicationRole);
        }

        [Test]
        public void VerifyDeletedProperty()
        {
            Assert.IsFalse(_target.IsDeleted);

            _target.SetDeleted();

            Assert.IsTrue(_target.IsDeleted);
        }

        #region IAuthorizationEntity Members test

        [Test]
        public void VerifyAuthorizationName()
        {

            // Declare return variable to hold property get method
            string getNameValue;
            string expectedNameValue = "testName";

            _target.SystemRoleLongName = expectedNameValue;

            // call get method
            getNameValue = ((IAuthorizationEntity)_target).AuthorizationName;


            // Assert result
            Assert.AreEqual(expectedNameValue, getNameValue);
        }

        [Test]
        public void VerifyAuthorizationDescription()
        {

            // Declare variable to hold property set method
            string setDescription = "testRole";

            ApplicationRole role = ApplicationRoleFactory.CreateRole(setDescription, "desc");

            // assign value
            _target.ApplicationRole = role;

            // Declare return variable to hold property get method
            string getDescriptionValue;

            // call get method
            getDescriptionValue = ((IAuthorizationEntity)_target).AuthorizationDescription;

            // Assert result
            Assert.AreEqual(setDescription, getDescriptionValue);
        }

        [Test]
        public void VerifyAuthorizationValue()
        {
               Assert.AreEqual(string.Empty, _target.AuthorizationValue);
        }


        #endregion

    }
}
