using NUnit.Framework;
using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.DomainTest.Helper
{
    /// <summary>
    /// Test class for StringHelper
    /// </summary>
    /// <remarks>
    /// Created by: henryg
    /// Created date: 2007-12-13
    /// </remarks>
    [TestFixture]
    public class StringHelperTest
    {
        private string myName = "henry matias greijer";

        [Test]
        public void CapitalizeWorksJustFine()
        {
            Assert.IsTrue("Henry Matias Greijer" == StringHelper.Capitalize(myName));
        }

        [Test]
        public void VerifySplit()
        {
            string[] strings = myName.Split(" ");
            Assert.AreEqual(3, strings.Length);
        }
    }
}
