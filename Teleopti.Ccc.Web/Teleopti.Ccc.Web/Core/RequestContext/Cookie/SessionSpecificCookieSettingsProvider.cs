namespace Teleopti.Ccc.Web.Core.RequestContext.Cookie
{
	public class SessionSpecificCookieSettingsProvider
	{
		private readonly DefaultSessionSpecificCookieDataProviderSettings _defaultSessionSpecificCookieDataProviderSettings;
		private readonly DefaultSessionSpecificCookieForIdentityProviderDataProviderSettings _defaultSessionSpecificCookieForIdentityProviderDataProviderSettings;

		public SessionSpecificCookieSettingsProvider()
		{
			_defaultSessionSpecificCookieDataProviderSettings = new DefaultSessionSpecificCookieDataProviderSettings();
			_defaultSessionSpecificCookieForIdentityProviderDataProviderSettings = new DefaultSessionSpecificCookieForIdentityProviderDataProviderSettings();
		}
		public ISessionSpecificCookieSettings ForWfm()
		{
			return _defaultSessionSpecificCookieDataProviderSettings;
		}

		public ISessionSpecificCookieSettings ForTeleoptiIdentityProvider()
		{
			return _defaultSessionSpecificCookieForIdentityProviderDataProviderSettings;
		}
	}
}