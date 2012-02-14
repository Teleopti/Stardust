using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.AuthorizationEntities
{
    [TestFixture]
    public class AuthorizationEntityTest
    {
        private AuthorizationEntity _target;
        private const string _key = "key";
        private const string _name = "name";
        private const string _description = "description";
        private const string _value = "value";

        [SetUp]
        public void Setup()
        {
            _target = new AuthorizationEntity(_key, _name, _description, _value);
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.AreEqual(_key, _target.AuthorizationKey);
            Assert.AreEqual(_name, _target.AuthorizationName);
            Assert.AreEqual(_description, _target.AuthorizationDescription);
            Assert.AreEqual(_value, _target.AuthorizationValue);
        }

        [Test]
        public void VerifyProperties()
        {
            string key1 = "key1";
            _target.AuthorizationKey = key1;
            Assert.AreEqual(key1, _target.AuthorizationKey);

            string name1 = "name1";
            _target.AuthorizationName = name1;
            Assert.AreEqual(name1, _target.AuthorizationName);

            string description1 = "desctiption1";
            _target.AuthorizationDescription = description1;
            Assert.AreEqual(description1, _target.AuthorizationDescription);

            string value1 = "value1";
            _target.AuthorizationValue = value1;
            Assert.AreEqual(value1, _target.AuthorizationValue);
        }

    }
}
