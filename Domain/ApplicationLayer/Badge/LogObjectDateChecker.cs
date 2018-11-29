using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Badge
{
	/// <summary>
	/// To check if the historical data is come late than expected
	/// </summary>
	public class LogObjectDateChecker: ILogObjectDateChecker
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(LogObjectDateChecker));
		private readonly IStatisticRepository _statisticRepository;

		public LogObjectDateChecker(IStatisticRepository statisticRepository)
		{
			_statisticRepository = statisticRepository;
		}

		public bool HistoricalDataIsEarlierThan(DateOnly date)
		{
			var result = false;
			var historicalDataDetails = _statisticRepository.GetLogObjectDetails();
			foreach (var detail in historicalDataDetails)
			{
				var thisDetailIsLate = false;
				var dateOnly = detail.DateValue.Date;
				if (dateOnly < date.Date)
				{
					thisDetailIsLate = true;
				}
				else if (dateOnly == date.Date)
				{
					var latestInterval = detail.IntervalsPerDay - 1;
					if (detail.IntervalValue < latestInterval)
					{
						thisDetailIsLate = true;
					}
				}

				if (thisDetailIsLate)
				{
					logger.Warn($"Latest historical data for LogObjectDetail with log_object_id=\"{detail.LogObjectId}\", "
								+ $"log_object_desc=\"{detail.LogObjectName}\", detail_id=\"{detail.DetailId}\", "
								+ $"detail_desc=\"{detail.DetailName}\" comes at date_value=\"{detail.DateValue:yyyy-MM-dd HH:mm:ss}\" "
								+ $"and int_value=\"{detail.IntervalValue}\", which is earlier than expected (latest "
								+ $"interval of {date.Date:yyyy-MM-dd})");
				}

				result = result || thisDetailIsLate;
			}

			return result;
		}
	}
}