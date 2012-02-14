using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.AuthorizationEntities
{
    /// <summary>
    /// Test cases for SiteAuthorizationEntityConverter class
    /// </summary>
    [TestFixture]
    public class SiteAuthorizationEntityConverterTest
    {
        private SiteAuthorizationEntityConverter _target;
        private Site _site;
        private ApplicationRole _applicationRole;
        
        [SetUp]
        public void Setup()
        {
            _site = new Site("Site1");
            _site.Description = new Description("Site1", "S1");
            _applicationRole = ApplicationRoleFactory.CreateRole("Role", "Role");
            _target = new SiteAuthorizationEntityConverter(_site, _applicationRole);
        }

        [Test]
        public void VerifyConstructor1()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyConstructor2()
        {
            _target = new SiteAuthorizationEntityConverter();
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreSame(_site, _target.ContainedEntity);
            Assert.AreEqual(_target.AuthorizationKey, _site.Id.ToString());
            Assert.AreEqual(_target.AuthorizationName, _site.Description.ToString());
            Assert.AreEqual(_target.AuthorizationDescription, _site.Description.ToString() + " Site");
            Assert.AreEqual(_target.AuthorizationValue, _applicationRole.Name);
        }
    }
}
