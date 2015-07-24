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
				PlannedPersonHours = new List<TimeSpan>(),
				BacklogPersonHours = new List<TimeSpan>(),
				ScheduledPersonHours = new List<TimeSpan>()
			};

			IOutboundCampaign campaign = _campaignRepository.Get(id);
			if (campaign == null) return visualizationVM;

			var incomingTask = _campaignTaskManager.GetIncomingTaskFromCampaign(campaign);
			foreach (var date in campaign.SpanningPeriod.DayCollection())
			{
				visualizationVM.Dates.Add(date);
				visualizationVM.PlannedPersonHours.Add(incomingTask.GetPlannedTimeOnDate(date));
				visualizationVM.BacklogPersonHours.Add(incomingTask.GetTimeOnDate(date));
				visualizationVM.ScheduledPersonHours.Add(incomingTask.GetScheduledTimeOnDate(date));
			}
			
			return visualizationVM;
		}
	}
}