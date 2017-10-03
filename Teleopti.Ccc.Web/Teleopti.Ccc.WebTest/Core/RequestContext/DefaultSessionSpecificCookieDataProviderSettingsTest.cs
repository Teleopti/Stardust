using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;

namespace Teleopti.Ccc.WebTest.Core.RequestContext
{
	[TestFixture]
	public class DefaultSessionSpecificCookieDataProviderSettingsTest
	{
		private DefaultSessionSpecificWfmCookieSettings _sessionSpecificWfmCookieSettings;
		
		[SetUp]
		public void SetUp()
		{
			_sessionSpecificWfmCookieSettings = new DefaultSessionSpecificWfmCookieSettings();
		}

		[Test] 
		public void ShouldHaveDefaultValues()
		{
			// Not so fun tests
			_sessionSpecificWfmCookieSettings.AuthenticationCookieExpirationTimeSpan.Ticks.Should().Be.GreaterThan(0);
			_sessionSpecificWfmCookieSettings.AuthenticationCookieName.Should().Not.Be.Empty();
			_sessionSpecificWfmCookieSettings.AuthenticationCookiePath.Should().Not.Be.Empty();
			
		}
	}
}