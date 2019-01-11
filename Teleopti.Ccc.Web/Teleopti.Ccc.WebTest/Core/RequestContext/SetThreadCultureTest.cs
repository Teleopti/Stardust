using System.Globalization;
using System.Threading;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Core.RequestContext;

namespace Teleopti.Ccc.WebTest.Core.RequestContext
{
    [TestFixture]
    public class SetThreadCultureTest
    {
		private CultureInfo _cultureBeforeTest;
		private CultureInfo _uiCultureBeforeTest;

        [SetUp]
        public void Setup()
        {
            _cultureBeforeTest = CultureInfo.CurrentCulture;
            _uiCultureBeforeTest = CultureInfo.CurrentUICulture;
        }

        [TearDown]
        public void TearDown()
        {
            Thread.CurrentThread.CurrentCulture = _cultureBeforeTest;
            Thread.CurrentThread.CurrentUICulture = _uiCultureBeforeTest;
        }

        [Test]
        public void ShouldSetCultureFromRegional()
        {
            var arabicCulture = CultureInfo.GetCultureInfo("ar-SA");
        	var target = new SetThreadCulture();
        	var regional = new Regional(TimeZoneInfoFactory.StockholmTimeZoneInfo(), arabicCulture, arabicCulture);

			target.SetCulture(regional);

			Assert.That(Thread.CurrentThread.CurrentCulture, Is.EqualTo(arabicCulture));
			Assert.That(Thread.CurrentThread.CurrentUICulture, Is.EqualTo(arabicCulture));
        }

		[Test]
		public void ShouldNotSetThreadCultureIfRegionalHasNoCulture()
		{
			var target = new SetThreadCulture();
			var regional = new Regional(TimeZoneInfoFactory.StockholmTimeZoneInfo(), null, null);

			target.SetCulture(regional);

			Assert.That(Thread.CurrentThread.CurrentCulture, Is.EqualTo(_cultureBeforeTest));
			Assert.That(Thread.CurrentThread.CurrentUICulture, Is.EqualTo(_uiCultureBeforeTest));
		}

    }
}
