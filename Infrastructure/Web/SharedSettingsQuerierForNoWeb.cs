namespace Teleopti.Ccc.Infrastructure.Web
{
	public class SharedSettingsQuerierForNoWeb : ISharedSettingsQuerier
	{
		public SharedSettings GetSharedSettings()
		{
			return new SharedSettings();
		}
	}
}