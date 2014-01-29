﻿using System.Globalization;
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
			var shortDatePattern = _userCulture == null ? 
						string.Empty : 
						_userCulture.GetCulture().DateTimeFormat.ShortDatePattern;
			return new DailyMetricsViewModel
			{
				AnsweredCalls = dataModel.AnsweredCalls,
				AverageAfterCallWork = dataModel.AfterCallWorkTimeAverage.TotalSeconds.ToString(CultureInfo.InvariantCulture),
				AverageTalkTime = dataModel.TalkTimeAverage.TotalSeconds.ToString(CultureInfo.InvariantCulture),
				AverageHandlingTime = dataModel.HandlingTimeAverage.TotalSeconds.ToString(CultureInfo.InvariantCulture),
				ReadyTimePerScheduledReadyTime = dataModel.ReadyTimePerScheduledReadyTime.ValueAsPercent().ToString(CultureInfo.InvariantCulture),
				Adherence = dataModel.Adherence.ValueAsPercent().ToString(CultureInfo.InvariantCulture),
				DataAvailable = true,
				DatePickerFormat = shortDatePattern
			};
		}
	}
}