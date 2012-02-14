using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.DomainTest.FakeData;
using Teleopti.Ccc.WinCode.PeopleAdmin;

namespace Teleopti.Ccc.WinCodeTest.PersonAdmin
{
    /// <summary>
    /// Tests for PersonRoleViewer
    /// </summary>
    [TestFixture]
    public class PersonRoleViewerTest
    {
        private PersonRoleViewer _target;
        
        [SetUp]
        public void Setup()
        {
            ApplicationRole applicationRole = ApplicationRoleFactory.CreateRole("Test", "Application role for test"); 
            bool isInRole = true;
            _target = new PersonRoleViewer(isInRole, applicationRole);
        }


        [Test]
        public void VerifyPropertiesNotNull()
        {
            Assert.IsNotNull(_target.ApplicationRole);
            Assert.IsNotEmpty(_target.ApplicationRoleDescription);
            Assert.AreEqual(true, _target.IsInRole);
            _target.TriState = 1;
            Assert.AreEqual(1, _target.TriState);
        }

        [Test]
        public void VerifySetIsRoleProperty()
        {
            Assert.AreEqual(true, _target.IsInRole);
            _target.IsInRole = false;
            Assert.AreEqual(false, _target.IsInRole);
        }
       
    }
   
}
