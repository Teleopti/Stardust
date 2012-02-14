using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.AuthorizationEntities
{
    /// <summary>
    /// Test cases for PersonAuthorizationEntityConverter class
    /// </summary>
    [TestFixture]
    public class PersonAuthorizationEntityConverterTest
    {
        private PersonAuthorizationEntityConverter _target;
        private IPerson _person;
        private IApplicationRole _applicationRole;
        
        [SetUp]
        public void Setup()
        {
            _person = PersonFactory.CreatePerson("FirstName", "LastName");
            _applicationRole = ApplicationRoleFactory.CreateRole("Role", "Role");
            _target = new PersonAuthorizationEntityConverter(_person, _applicationRole);
        }

        [Test]
        public void VerifyConstructor1()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyConstructor2()
        {
            _target = new PersonAuthorizationEntityConverter();
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreSame(_person, _target.ContainedEntity);
            Assert.AreEqual(_target.AuthorizationKey, _person.Id.ToString());
            Assert.AreEqual(_target.AuthorizationName, _person.Name.ToString());
            Assert.AreEqual(_target.AuthorizationDescription, _person.Name + " Agent");
            Assert.AreEqual(_target.AuthorizationValue, _applicationRole.Name);
        }
    }
}
