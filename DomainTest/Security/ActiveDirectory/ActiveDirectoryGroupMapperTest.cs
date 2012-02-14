using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.ActiveDirectory;

namespace Teleopti.Ccc.DomainTest.Security.ActiveDirectory
{
    [TestFixture]
    public class ActiveDirectoryGroupMapperTest
    {
        [Test]
        public void VerifyAllGroupProperties()
        {
            List<string> allProperties = ActiveDirectoryGroupMapper.AllGroupProperties();
            
            Assert.AreEqual(9, allProperties.Count);
        }
    }
}
