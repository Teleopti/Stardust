using NUnit.Framework;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence
{
    [TestFixture]
    public class ExternalLogOnPersonTest
    {
        private ExternalLogOnPerson _target;

        [SetUp]
        public void Setup()
        {
            _target = new ExternalLogOnPerson();
        }

        [Test]
        public void VerifyProperties()
        {
            IPerson person = PersonFactory.CreatePerson();

            Assert.IsNull(_target.Person);
            Assert.AreEqual(0,_target.DataSourceId);
            Assert.IsTrue(string.IsNullOrEmpty(_target.ExternalLogOn));

            _target.Person = person;
            _target.DataSourceId = 5;
            _target.ExternalLogOn = "007";

            Assert.AreEqual(person,_target.Person);
            Assert.AreEqual(5,_target.DataSourceId);
            Assert.AreEqual("007",_target.ExternalLogOn);
        }
    }
}
