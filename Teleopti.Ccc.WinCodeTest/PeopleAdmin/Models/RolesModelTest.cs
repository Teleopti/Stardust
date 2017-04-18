using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Models
{
    /// <summary>
    /// Test class for RolesModel
    /// </summary>
    /// <remarks>
    /// Created by: Sachintha Weerasekara
    /// Created date: 6/15/2008
    /// </remarks>
    [TestFixture]
    public class RolesModelTest
    {
        private RolesModel _target;
        private ApplicationRole _base;

        [SetUp]
        public void TestInit()
        {
            _base = new ApplicationRole();
            _base.DescriptionText = "TestText";
            _target = new RolesModel();
            _target.ContainedEntity = _base;
        }

        /// <summary>
        /// Verifies the role.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 6/15/2008
        /// </remarks>
        [Test]
        public void VerifyRole()
        {
            Assert.AreEqual(_target.Role, _base);
        }

        /// <summary>
        /// Verifies the state of the set tri.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 6/15/2008
        /// </remarks>
        [Test]
        public void VerifySetTriState()
        {
            int actualtValue = 3;
            _target.TriState = actualtValue;

            // Test the set method
            int expectedValue = 0;
            expectedValue = _target.TriState;

            // Performs the test
            Assert.AreEqual(expectedValue, actualtValue);
        }

        /// <summary>
        /// Verifies the role exists in person count.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 6/15/2008
        /// </remarks>
        [Test]
        public void VerifyRoleExistsInPersonCount()
        {
            int actualtValue = 3;
            _target.RoleExistsInPersonCount = actualtValue;

            // Test the get method
            int expectedValue = 0;
            expectedValue = _target.RoleExistsInPersonCount;

            // Performs the test
            Assert.AreEqual(expectedValue, actualtValue);
        }

        /// <summary>
        /// Verifies the description text.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 6/15/2008
        /// </remarks>
        [Test]
        public void VerifyDescriptionText()
        {
            Assert.AreEqual(_target.DescriptionText, _base.DescriptionText);
        }
    }
}
