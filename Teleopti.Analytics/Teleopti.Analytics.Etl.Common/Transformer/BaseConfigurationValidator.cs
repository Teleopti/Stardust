using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public class BaseConfigurationValidator
	{
		readonly ILog _logger = LogManager.GetLogger(typeof(ConfigurationHandler));

		public bool isCultureValid(int? uiCultureId)
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

		public bool isIntervalLengthValid(int? intervalLength)
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

		public bool isTimeZoneValid(string timeZone)
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
