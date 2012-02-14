using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Interfaces.Common
{
	public interface IConfigurationHandler
	{
		bool IsConfigurationValid { get; }
		IBaseConfiguration BaseConfiguration { get; }
		int? IntervalLengthInUse { get; }
		void SaveBaseConfiguration(IBaseConfiguration configuration);
	}
}