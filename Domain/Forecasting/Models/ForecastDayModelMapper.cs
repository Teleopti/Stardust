using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Models
{
	public class ForecastDayModelMapper
	{
		private const double tolerance = 0.0001d;

		public void SetCampaignAndOverride(IWorkloadDayBase workLoadDay, ForecastDayModel forecastDayModel,
			IForecastDayOverride overrideDay)
		{
			if (hasCampaign(workLoadDay) && hasOverride(overrideDay))
			{
				setCampaign(forecastDayModel, workLoadDay);
				setOverride(forecastDayModel, overrideDay);
			}
			else if (hasOverride(overrideDay))
			{
				setOverride(forecastDayModel, overrideDay);
			}
			else if (hasCampaign(workLoadDay))
			{
				setCampaign(forecastDayModel, workLoadDay);

				forecastDayModel.TotalTasks = forecastDayModel.Tasks * (workLoadDay.CampaignTasks.Value + 1d);
				forecastDayModel.TotalAverageTaskTime =
					forecastDayModel.AverageTaskTime * (workLoadDay.CampaignTaskTime.Value + 1d);
				forecastDayModel.TotalAverageAfterTaskTime =
					forecastDayModel.AverageAfterTaskTime * (workLoadDay.CampaignAfterTaskTime.Value + 1d);
			}
		}

		private void setCampaign(ForecastDayModel forecastDay, IWorkloadDayBase workloadDay)
		{
			forecastDay.HasCampaign = true;
			forecastDay.CampaignTasksPercentage = workloadDay.CampaignTasks.Value;
		}

		private void setOverride(ForecastDayModel forecastDay, IForecastDayOverride forecastDayOverride)
		{
			forecastDay.HasOverride = true;
			forecastDay.OverrideTasks = forecastDayOverride.OverriddenTasks;
			forecastDay.OverrideAverageTaskTime = forecastDayOverride.OverriddenAverageTaskTime?.TotalSeconds;
			forecastDay.OverrideAverageAfterTaskTime = forecastDayOverride.OverriddenAverageAfterTaskTime?.TotalSeconds;

			forecastDay.TotalTasks = forecastDayOverride.OverriddenTasks ?? forecastDay.Tasks;
			forecastDay.TotalAverageTaskTime = forecastDayOverride.OverriddenAverageTaskTime?.TotalSeconds ??
											   forecastDay.TotalAverageTaskTime;
			forecastDay.TotalAverageAfterTaskTime = forecastDayOverride.OverriddenAverageAfterTaskTime?.TotalSeconds ??
													forecastDay.TotalAverageAfterTaskTime;
		}

		private bool hasCampaign(IWorkloadDayBase workloadDay)
		{
			return Math.Abs(workloadDay.CampaignTasks.Value) > tolerance ||
				   Math.Abs(workloadDay.CampaignTaskTime.Value) > tolerance ||
				   Math.Abs(workloadDay.CampaignAfterTaskTime.Value) > tolerance;
		}

		private bool hasOverride(IForecastDayOverride overrideDay)
		{
			return overrideDay != null &&
				(overrideDay.OverriddenTasks.HasValue ||
				   overrideDay.OverriddenAverageTaskTime.HasValue ||
				   overrideDay.OverriddenAverageAfterTaskTime.HasValue);
		}
	}
}