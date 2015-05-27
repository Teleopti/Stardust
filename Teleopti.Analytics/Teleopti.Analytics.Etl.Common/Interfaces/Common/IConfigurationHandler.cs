namespace Teleopti.Analytics.Etl.Common.Interfaces.Common
{
	public interface IConfigurationHandler
	{
		bool IsConfigurationValid { get; }
		IBaseConfiguration BaseConfiguration { get; }
		int? IntervalLengthInUse { get; }
		void SaveBaseConfiguration(IBaseConfiguration configuration);
	}
}