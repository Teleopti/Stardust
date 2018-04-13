using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public class ConfigurationHandler : IConfigurationHandler
	{
		private readonly IGeneralFunctions _generalFunctions;
		private IBaseConfiguration _baseConfiguration;
		private readonly BaseConfigurationValidator _baseConfigurationValidator;

		public ConfigurationHandler(IGeneralFunctions generalFunctions, BaseConfigurationValidator baseConfigurationValidator)
		{
			_generalFunctions = generalFunctions;
			_baseConfigurationValidator = baseConfigurationValidator;
		}

		public bool IsConfigurationValid => _baseConfigurationValidator.isCultureValid(BaseConfiguration.CultureId) &&
											_baseConfigurationValidator.isIntervalLengthValid(BaseConfiguration.IntervalLength) &&
											_baseConfigurationValidator.isTimeZoneValid(BaseConfiguration.TimeZoneCode);

		public IBaseConfiguration BaseConfiguration
		{
			get
			{
				return _baseConfiguration ?? (_baseConfiguration = _generalFunctions.LoadBaseConfiguration());
			}
		}

		public int? IntervalLengthInUse
		{
			get { return _generalFunctions.LoadIntervalLengthInUse(); }
		}

		public void SaveBaseConfiguration(IBaseConfiguration configuration)
		{
			_generalFunctions.SaveBaseConfiguration(configuration);
			_baseConfiguration = configuration;
		}

		public void SetConnectionString(string dataMartConnectionString)
		{
			_generalFunctions.SetConnectionString(dataMartConnectionString);
		}


	}
}