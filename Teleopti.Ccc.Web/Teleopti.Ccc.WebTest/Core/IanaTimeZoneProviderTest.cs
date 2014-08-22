using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.WebTest.Core
{
    [TestFixture]
    public class IanaTimeZoneProviderTest
    {
        [Test]
        public void ShouldReturnTimeZoneInfoId()
        {
            var target = new IanaTimeZoneProvider();

            var result = target.IanaToWindows("Europe/Stockholm");
            result.Should().Be.EqualTo(TimeZoneInfoFactory.StockholmTimeZoneInfo().Id);
        }

        [Test]
        public void ShouldReturnIanaTimeZone()
        {
            var target = new IanaTimeZoneProvider();

            var result = target.WindowsToIana("Paraguay Standard Time");
            result.Should().Be.EqualTo("America/Asuncion");
        }
    }
}
