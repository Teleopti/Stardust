using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public class ConfigurationHandler : IConfigurationHandler
	{
		private readonly IGeneralFunctions _generalFunctions;
		private IBaseConfiguration _baseConfiguration;
		private readonly BaseConfigurationValidator _baseConfigurationValidator;

		public ConfigurationHandler(IGeneralFunctions generalFunctions,
			BaseConfigurationValidator baseConfigurationValidator)
		{
			_generalFunctions = generalFunctions;
			_baseConfigurationValidator = baseConfigurationValidator;
		}

		public bool IsConfigurationValid => _baseConfigurationValidator.IsConfigurationValid(BaseConfiguration);

		public IBaseConfiguration BaseConfiguration =>
			_baseConfiguration ?? (_baseConfiguration = _generalFunctions.LoadBaseConfiguration());

		public int? IntervalLengthInUse => _generalFunctions.LoadIntervalLengthInUse();

		public void SaveBaseConfiguration(IBaseConfiguration configuration)
		{
			_generalFunctions.SaveBaseConfiguration(configuration);
			_baseConfiguration = configuration;
		}

		public void SetConnectionString(string dataMartConnectionString)
		{
			_generalFunctions.SetConnectionString(dataMartConnectionString);
			_baseConfiguration = _generalFunctions.LoadBaseConfiguration();
		}
	}
}