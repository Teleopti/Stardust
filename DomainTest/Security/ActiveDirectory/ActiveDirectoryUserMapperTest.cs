using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.ActiveDirectory;

namespace Teleopti.Ccc.DomainTest.Security.ActiveDirectory
{
    [TestFixture]
    public class ActiveDirectoryUserMapperTest
    {
        [Test]
        public void VerifyAllUserProperties()
        {
            List<string> allProperties = ActiveDirectoryUserMapper.AllUserProperties();
            
            Assert.AreEqual(36, allProperties.Count);
        }
    }
}
