using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;

namespace Teleopti.Ccc.WebTest.Core.RequestContext
{
	[TestFixture]
	public class DefaultSessionSpecificCookieDataProviderSettingsTest
	{
		private DefaultSessionSpecificCookieDataProviderSettings _sessionSpecificCookieDataProviderSettings;
		
		[SetUp]
		public void SetUp()
		{
			_sessionSpecificCookieDataProviderSettings = new DefaultSessionSpecificCookieDataProviderSettings();
		}

		[Test] 
		public void ShouldHaveDefaultValues()
		{
			// Not so fun tests
			_sessionSpecificCookieDataProviderSettings.AuthenticationCookieExpirationTimeSpan.Ticks.Should().Be.GreaterThan(0);
			_sessionSpecificCookieDataProviderSettings.AuthenticationCookieName.Should().Not.Be.Empty();
			_sessionSpecificCookieDataProviderSettings.AuthenticationCookiePath.Should().Not.Be.Empty();
			
		}
	}
}