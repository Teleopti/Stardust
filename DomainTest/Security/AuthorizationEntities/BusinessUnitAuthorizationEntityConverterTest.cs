using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.AuthorizationEntities
{
    /// <summary>
    /// Test cases for BusinessUnitAuthorizationEntityConverter class
    /// </summary>
    [TestFixture]
    public class BusinessUnitAuthorizationEntityConverterTest
    {
        private BusinessUnitAuthorizationEntityConverter _target;
        private BusinessUnit _businessUnit;
        private ApplicationRole _applicationRole;
        
        [SetUp]
        public void Setup()
        {
            _applicationRole = ApplicationRoleFactory.CreateRole("Test role", "Test role");
            _businessUnit = new BusinessUnit("BusinessUnit1");
            _businessUnit.Description = new Description("BusinessUnit1", "BU1");
            _target = new BusinessUnitAuthorizationEntityConverter(_businessUnit, _applicationRole);
        }

        [Test]
        public void VerifyConstructor1()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyConstructor2()
        {
            _target = new BusinessUnitAuthorizationEntityConverter();
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreSame(_businessUnit, _target.ContainedEntity);
            Assert.AreEqual(_target.AuthorizationKey, _businessUnit.Id.ToString());
            Assert.AreEqual(_target.AuthorizationName, _businessUnit.Description.ToString());
            Assert.AreEqual(_target.AuthorizationDescription, _businessUnit.Description.ToString() + " Business Unit");
            Assert.AreEqual(_target.AuthorizationValue, _applicationRole.Name);
        }
    }
}
