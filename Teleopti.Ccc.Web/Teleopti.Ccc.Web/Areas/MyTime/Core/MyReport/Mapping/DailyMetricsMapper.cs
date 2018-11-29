using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.WebReports;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MyReport;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.Mapping
{
	public class DailyMetricsMapper : IDailyMetricsMapper
	{
		private readonly IUserCulture _userCulture;
		private readonly IPermissionProvider _permissionProvider;

		public DailyMetricsMapper(IUserCulture userCulture, IPermissionProvider permissionProvider)
		{
			_userCulture = userCulture;
			_permissionProvider = permissionProvider;
		}

		public DailyMetricsViewModel Map(DailyMetricsForDayResult dataModel)
		{
			if (dataModel == null)
			{
				return new DailyMetricsViewModel {DataAvailable = false};
			}
			var culture = _userCulture == null ? CultureInfo.InvariantCulture : _userCulture.GetCulture();
			var queueMetricsEnabled =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.MyReportQueueMetrics);

			return new DailyMetricsViewModel
			{
				AnsweredCalls = dataModel.AnsweredCalls,
				AverageAfterCallWork = dataModel.AfterCallWorkTimeAverage.TotalSeconds.ToString(culture),
				AverageTalkTime = dataModel.TalkTimeAverage.TotalSeconds.ToString(culture),
				AverageHandlingTime = dataModel.HandlingTimeAverage.TotalSeconds.ToString(culture),
				ReadyTimePerScheduledReadyTime = dataModel.ReadyTimePerScheduledReadyTime.ValueAsPercent().ToString(culture),
				Adherence = dataModel.Adherence.ValueAsPercent().ToString(culture),
				DataAvailable = true,
				QueueMetricsEnabled = queueMetricsEnabled
			};
		}
	}
}