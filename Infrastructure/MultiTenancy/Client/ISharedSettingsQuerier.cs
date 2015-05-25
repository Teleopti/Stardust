namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public interface ISharedSettingsQuerier
	{
		SharedSettings GetSharedSettings();
	}
}