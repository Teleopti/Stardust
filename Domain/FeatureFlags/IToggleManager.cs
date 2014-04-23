namespace Teleopti.Ccc.Domain.FeatureFlags
{
	public interface IToggleManager
	{
		bool IsEnabled(Toggles toggle);
	}
}