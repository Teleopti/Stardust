using System;
using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.Outbound.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{
	public class CampaignVisualizationProvider : ICampaignVisualizationProvider
	{
		private readonly IOutboundCampaignRepository _campaignRepository;
		private readonly IOutboundCampaignTaskManager _campaignTaskManager;

		public CampaignVisualizationProvider(IOutboundCampaignRepository campaignRepository, IOutboundCampaignTaskManager campaignTaskManager)
		{
			_campaignRepository = campaignRepository;
			_campaignTaskManager = campaignTaskManager;
		}

		public CampaignVisualizationViewModel ProvideVisualization(Guid id)
		{
			var visualizationVM =  new CampaignVisualizationViewModel()
			{
				Dates = new List<DateOnly>(),
				PlannedPersonHours = new List<double>(),
				BacklogPersonHours = new List<double>(),
				ScheduledPersonHours = new List<double>(),
				ManualPlanHours = new List<double>()
			};

			IOutboundCampaign campaign = _campaignRepository.Get(id);
			if (campaign == null) return visualizationVM;

			var incomingTask = _campaignTaskManager.GetIncomingTaskFromCampaign(campaign);
			foreach (var date in campaign.SpanningPeriod.DayCollection())
			{
				var plannedHours = incomingTask.GetRealPlannedTimeOnDate(date);
				var backlogHours = incomingTask.GetBacklogOnDate(date);
				var scheduledHours = incomingTask.GetRealScheduledTimeOnDate(date);
				var manualPlannedHours = incomingTask.GetManualPlannedTimeOnDate(date);

				visualizationVM.Dates.Add(date);
				visualizationVM.PlannedPersonHours.Add(plannedHours.Days*24 + plannedHours.Hours + (double)plannedHours.Minutes / 60);
				visualizationVM.BacklogPersonHours.Add(backlogHours.Days*24 + backlogHours.Hours + (double)backlogHours.Minutes/60);
				visualizationVM.ScheduledPersonHours.Add(scheduledHours.Days*24 + scheduledHours.Hours + (double)scheduledHours.Minutes / 60);
				visualizationVM.ManualPlanHours.Add(manualPlannedHours.Days * 24 + manualPlannedHours.Hours + (double)manualPlannedHours.Minutes / 60);
			}
			
			return visualizationVM;
		}
	}
}