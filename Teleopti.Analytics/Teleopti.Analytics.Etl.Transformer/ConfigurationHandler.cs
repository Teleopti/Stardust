using System;
using System.Globalization;
using Teleopti.Analytics.Etl.Interfaces.Common;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using log4net;

namespace Teleopti.Analytics.Etl.Transformer
{
	public class ConfigurationHandler : IConfigurationHandler
	{
		private readonly IGeneralFunctions _generalFunctions;
		readonly ILog _logger = LogManager.GetLogger(typeof(ConfigurationHandler));
		private IBaseConfiguration _baseConfiguration;

		public ConfigurationHandler(IGeneralFunctions generalFunctions)
		{
			_generalFunctions = generalFunctions;
		}

		public bool IsConfigurationValid
		{
			get
			{
				if (!isCultureValid(BaseConfiguration.CultureId))
					return false;
				if (!isIntervalLengthValid(BaseConfiguration.IntervalLength))
					return false;

				return isTimeZoneValid(BaseConfiguration.TimeZoneCode);
			}
		}

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

		private bool isCultureValid(int? uiCultureId)
		{
			if (!uiCultureId.HasValue)
			{
				_logger.Warn("Configuration for Culture in db is invalid It cannot be null.");
				return false;
			}

			try
			{
				var culture = CultureInfo.GetCultureInfo(uiCultureId.Value);
				_logger.DebugFormat(CultureInfo.InvariantCulture, "Configuration for Culture in db is valid: '{0}'.", culture.LCID);
			}
			catch (Exception ex)
			{
				_logger.WarnFormat(CultureInfo.InvariantCulture,
								   "Configuration for Culture in db is invalid: '{0}'. ExceptionMessage: '{1}'", uiCultureId,
								   ex.Message);
				return false;
			}

			return true;
		}

		private bool isIntervalLengthValid(int? intervalLength)
		{
			if (!intervalLength.HasValue)
			{
				_logger.Warn("Configuration for TimeZone in db is invalid It cannot be null or empty.");
				return false;
			}

			if (intervalLength != 10 & intervalLength != 15 & intervalLength != 30 & intervalLength != 60)
			{
				_logger.WarnFormat(CultureInfo.InvariantCulture, "Configuration for IntervalLength in db is invalid: '{0}'.", intervalLength);
				return false;
			}

			return true;
		}

		private bool isTimeZoneValid(string timeZone)
		{
			if (string.IsNullOrEmpty(timeZone))
			{
				_logger.Warn("Configuration for TimeZone in db is invalid It cannot be null or empty.");
				return false;
			}

			try
			{
				var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
				_logger.DebugFormat(CultureInfo.InvariantCulture, "Configuration for TimeZone in db is valid: '{0}'.", tz.Id);
			}
			catch (Exception ex)
			{
				_logger.WarnFormat(CultureInfo.InvariantCulture,
								   "Configuration for TimeZone in db is invalid: '{0}'. ExceptionMessage: '{1}'", timeZone,
								   ex.Message);
				return false;
			}

			return true;
		}
	}
}