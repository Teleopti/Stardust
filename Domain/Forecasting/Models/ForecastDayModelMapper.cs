using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Models
{
	public class ForecastDayModelMapper
	{
		public void SetCampaignAndOverride(IWorkloadDayBase workLoadDay, ForecastDayModel forecastDayModel)
		{
			if (hasCampaign(workLoadDay) && hasOverride(workLoadDay))
			{
				setCampaign(forecastDayModel, workLoadDay);
				setOverride(forecastDayModel, workLoadDay);
			}
			else if (hasOverride(workLoadDay))
			{
				setOverride(forecastDayModel, workLoadDay);
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

		private void setOverride(ForecastDayModel forecastDay, IWorkloadDayBase workloadDay)
		{
			forecastDay.HasOverride = true;
			forecastDay.OverrideTasks = workloadDay.OverrideTasks;
			forecastDay.OverrideAverageTaskTime = workloadDay.OverrideAverageTaskTime?.TotalSeconds;
			forecastDay.OverrideAverageAfterTaskTime = workloadDay.OverrideAverageAfterTaskTime?.TotalSeconds;

			forecastDay.TotalTasks = workloadDay.OverrideTasks ?? forecastDay.Tasks;
			forecastDay.TotalAverageTaskTime = workloadDay.OverrideAverageTaskTime?.TotalSeconds ??
											   forecastDay.TotalAverageTaskTime;
			forecastDay.TotalAverageAfterTaskTime = workloadDay.OverrideAverageAfterTaskTime?.TotalSeconds ??
													forecastDay.TotalAverageAfterTaskTime;
		}

		private bool hasCampaign(IWorkloadDayBase workloadDay)
		{
			return workloadDay.CampaignTasks.Value > 0d ||
				   workloadDay.CampaignTaskTime.Value > 0d ||
				   workloadDay.CampaignAfterTaskTime.Value > 0d;
		}

		private bool hasOverride(IWorkloadDayBase workloadDay)
		{
			return workloadDay.OverrideTasks.HasValue ||
				   workloadDay.OverrideAverageTaskTime.HasValue ||
				   workloadDay.OverrideAverageAfterTaskTime.HasValue;
		}
	}
}