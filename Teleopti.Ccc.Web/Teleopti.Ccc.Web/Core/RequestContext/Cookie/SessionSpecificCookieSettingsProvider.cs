namespace Teleopti.Ccc.Web.Core.RequestContext.Cookie
{
	public class SessionSpecificCookieSettingsProvider
	{
		private readonly DefaultSessionSpecificWfmCookieSettings _defaultSessionSpecificWfmCookieSettings;
		private readonly DefaultSessionSpecificTeleoptiCookieSettings _defaultSessionSpecificTeleoptiCookieSettings;

		public SessionSpecificCookieSettingsProvider()
		{
			_defaultSessionSpecificWfmCookieSettings = new DefaultSessionSpecificWfmCookieSettings();
			_defaultSessionSpecificTeleoptiCookieSettings = new DefaultSessionSpecificTeleoptiCookieSettings();
		}
		public ISessionSpecificCookieSettings ForWfm()
		{
			return _defaultSessionSpecificWfmCookieSettings;
		}

		public ISessionSpecificCookieSettings ForTeleopti()
		{
			return _defaultSessionSpecificTeleoptiCookieSettings;
		}
	}
}