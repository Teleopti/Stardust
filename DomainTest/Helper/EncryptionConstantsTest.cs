using NUnit.Framework;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Helper
{
    [TestFixture]
    public class EncryptionConstantsTest
    {
        [Test]
        public void VerifyCanGetValues()
        {
            Assert.IsNotNull(EncryptionConstants.Image1);
            Assert.IsNotNull(EncryptionConstants.Image2);
        }
    }
}
