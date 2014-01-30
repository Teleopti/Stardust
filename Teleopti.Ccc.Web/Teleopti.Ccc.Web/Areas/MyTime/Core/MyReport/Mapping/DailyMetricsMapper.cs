using System.Globalization;
using Teleopti.Ccc.Infrastructure.WebReports;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MyReport;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.Mapping
{
	public class DailyMetricsMapper : IDailyMetricsMapper
	{
		private readonly IUserCulture _userCulture;

		public DailyMetricsMapper(IUserCulture userCulture)
		{
			_userCulture = userCulture;
		}

		public DailyMetricsViewModel Map(DailyMetricsForDayResult dataModel)
		{
			if (dataModel == null)
			{
				return new DailyMetricsViewModel {DataAvailable = false};
			}
			var culture = _userCulture == null ? CultureInfo.InvariantCulture : _userCulture.GetCulture();

			var shortDatePattern = culture.DateTimeFormat.ShortDatePattern;
			return new DailyMetricsViewModel
			{
				AnsweredCalls = dataModel.AnsweredCalls,
				AverageAfterCallWork = dataModel.AfterCallWorkTimeAverage.TotalSeconds.ToString(culture),
				AverageTalkTime = dataModel.TalkTimeAverage.TotalSeconds.ToString(culture),
				AverageHandlingTime = dataModel.HandlingTimeAverage.TotalSeconds.ToString(culture),
				ReadyTimePerScheduledReadyTime = dataModel.ReadyTimePerScheduledReadyTime.ValueAsPercent().ToString(culture),
				Adherence = dataModel.Adherence.ValueAsPercent().ToString(culture),
				DataAvailable = true,
				DatePickerFormat = shortDatePattern
			};
		}
	}
}