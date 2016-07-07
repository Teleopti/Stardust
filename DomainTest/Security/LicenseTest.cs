using NUnit.Framework;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security
{
    [TestFixture]
    public class LicenseTest
    {
        private ILicense _license;

        [SetUp]
        public void Setup()
        {
            _license = new License();
        }

        [Test]
        public void VerifyDefaultProperties()
        {
            Assert.AreEqual(null, _license.XmlString);
        }

        [Test]
        public void VerifyProperties()
        {
            const string dummyXml = "<foo></foo>";
            _license.XmlString = dummyXml;
            Assert.AreEqual(dummyXml, _license.XmlString);
        }

        [Test]
        public void VerifyEmptyConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_license.GetType(), true));
        }
    }
}
