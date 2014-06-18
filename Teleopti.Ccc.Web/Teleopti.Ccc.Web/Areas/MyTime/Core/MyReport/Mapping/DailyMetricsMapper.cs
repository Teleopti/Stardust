using System;
using System.Globalization;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.WebReports;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MyReport;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.Mapping
{
	public class DailyMetricsMapper : IDailyMetricsMapper
	{
		private readonly IUserCulture _userCulture;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IToggleManager _toggleManager;

		public DailyMetricsMapper(IUserCulture userCulture, IPermissionProvider permissionProvider, IToggleManager toggleManager)
		{
			_userCulture = userCulture;
			_permissionProvider = permissionProvider;
			_toggleManager = toggleManager;
		}

		public DailyMetricsViewModel Map(DailyMetricsForDayResult dataModel)
		{
			if (dataModel == null)
			{
				return new DailyMetricsViewModel {DataAvailable = false};
			}
			var culture = _userCulture == null ? CultureInfo.InvariantCulture : _userCulture.GetCulture();
			var queueMetricsEnabled =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.MyReportQueueMetrics) &&
				_toggleManager.IsEnabled(Toggles.MyReport_AgentQueueMetrics_22254);
			var avgHandlingTime = Math.Round(dataModel.HandlingTimeAverage.TotalSeconds, 0) ==
									Math.Round(dataModel.TalkTimeAverage.TotalSeconds, 0) +
									Math.Round(dataModel.AfterCallWorkTimeAverage.TotalSeconds, 0)
										? dataModel.HandlingTimeAverage
										: TimeSpan.FromSeconds(Math.Round(dataModel.TalkTimeAverage.TotalSeconds, 0) +
				                       Math.Round(dataModel.AfterCallWorkTimeAverage.TotalSeconds, 0));

			return new DailyMetricsViewModel
			{
				AnsweredCalls = dataModel.AnsweredCalls,
				AverageAfterCallWork = dataModel.AfterCallWorkTimeAverage.TotalSeconds.ToString(culture),
				AverageTalkTime = dataModel.TalkTimeAverage.TotalSeconds.ToString(culture),
				AverageHandlingTime = avgHandlingTime.TotalSeconds.ToString(culture),
				ReadyTimePerScheduledReadyTime = dataModel.ReadyTimePerScheduledReadyTime.ValueAsPercent().ToString(culture),
				Adherence = dataModel.Adherence.ValueAsPercent().ToString(culture),
				DataAvailable = true,
				QueueMetricsEnabled = queueMetricsEnabled
			};
		}
	}
}