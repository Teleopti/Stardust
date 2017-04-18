using System.Globalization;
using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin
{
    /// <summary>
    /// Tests for Culture
    /// </summary>
    [TestFixture]
    public class CultureTest
    {
        [Test]
        public void VerifyGetLanguageInfoByDisplayName()
        {
            string language = CultureInfo.CurrentCulture.DisplayName;
            Culture culture = new Culture(CultureInfo.CurrentCulture.LCID, language);
            Culture comparedValue = Culture.GetLanguageInfoByDisplayName(language);

            Assert.AreEqual(comparedValue.Id, culture.Id);

            Culture RetVal = new Culture(0, "");
            comparedValue = Culture.GetLanguageInfoByDisplayName("This is not in culture");

            Assert.AreEqual(RetVal.Id, comparedValue.Id);

        }


    }
}
