using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.ActiveDirectory;

namespace Teleopti.Ccc.DomainTest.Security.ActiveDirectory
{
    [TestFixture]
    public class ActiveDirectoryHelperTest
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "GUID"), Test]
        public void VerifyConvertBytesToStringGUID()
        {
            byte[] input;
            input = new byte[16] {23, 146, 94, 169, 24, 174, 215, 76, 131, 212, 189, 11, 10, 26, 72, 223};
            string excpected = "17925EA9-18AE-D74C-83D4-BD0B0A1A48DF";

            string output = ActiveDirectoryHelper.ConvertBytesToStringGUID(input);

            Assert.AreEqual(excpected, output);

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "GUID"), Test]
        public void VerifyConvertBytesToStringGUIDFilter()
        {
            byte[] input;
            input = new byte[16] { 23, 146, 94, 169, 24, 174, 215, 76, 131, 212, 189, 11, 10, 26, 72, 223 };
            string excpected = "17925EA9-18AE-D74C-83D4-BD0B0A1A48DF";

            string output = ActiveDirectoryHelper.ConvertBytesToStringGUID(input);

            Assert.AreEqual(excpected, output);

        }

        [Test]
        public void VerifyConvertBytesToStringSid()
        {
            byte[] input;
            input = new byte[28] {1,5,0,0,0,0,0,5,21,0,0,0,238,200,27,18,29,211,127,17,176,145,206,59,85,13,0,0};
            string excpected = "S-1-5-21-303810798-293589789-1003393456-3413";

            string output = ActiveDirectoryHelper.ConvertBytesToStringSid(input);

            Assert.AreEqual(excpected, output);

        }

    }
}
