using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.Outbound.Models;


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

		public CampaignVisualizationViewModel ProvideVisualization(Guid id, params DateOnly[] skipDates)
		{
			var visualizationVM =  new CampaignVisualizationViewModel()
			{
				Dates = new List<DateOnly>(),
				PlannedPersonHours = new List<double>(),
				BacklogPersonHours = new List<double>(),
				ScheduledPersonHours = new List<double>(),
				OverstaffPersonHours = new List<double>(),
				IsManualPlanned = new List<bool>(),
				IsCloseDays = new List<bool>(),
				IsActualBacklog = new List<bool>()
			};

			IOutboundCampaign campaign = _campaignRepository.Get(id);
			if (campaign == null) return visualizationVM;

			var incomingTask = skipDates != null?  _campaignTaskManager.GetIncomingTaskFromCampaign(campaign, skipDates)
												: _campaignTaskManager.GetIncomingTaskFromCampaign(campaign);
			TimeSpan? backlogPreviousDay = null;

			foreach (var date in campaign.SpanningPeriod.ToDateOnlyPeriod(campaign.Skill.TimeZone).DayCollection())
			{
				var plannedHours = incomingTask.GetRealPlannedTimeOnDate(date);
				var backlogHours = incomingTask.GetBacklogOnDate(date);
				var scheduledHours = incomingTask.GetRealScheduledTimeOnDate(date);
				var overstaffHours = TimeSpan.Zero;

				visualizationVM.Dates.Add(date);

				if (backlogHours == TimeSpan.Zero && !incomingTask.GetActualBacklogOnDate(date).HasValue)
				{
					var backlog = backlogPreviousDay.HasValue ? backlogPreviousDay.Value : incomingTask.GetBacklogOnDate(date.AddDays(-1));
					overstaffHours = (scheduledHours > TimeSpan.Zero)
						? scheduledHours - backlog
						: plannedHours - backlog;
				}
													
				visualizationVM.PlannedPersonHours.Add(convertTimespanToDouble(plannedHours));
				visualizationVM.BacklogPersonHours.Add(convertTimespanToDouble(backlogHours));
				visualizationVM.ScheduledPersonHours.Add(convertTimespanToDouble(scheduledHours));
				visualizationVM.IsManualPlanned.Add(incomingTask.GetManualPlannedInfoOnDate(date));
				visualizationVM.IsCloseDays.Add(incomingTask.PlannedTimeTypeOnDate(date) == PlannedTimeTypeEnum.Closed);
				visualizationVM.IsActualBacklog.Add(incomingTask.GetActualBacklogOnDate(date).HasValue);
				visualizationVM.OverstaffPersonHours.Add(convertTimespanToDouble(overstaffHours));

				backlogPreviousDay = backlogHours;
			}
			
			return visualizationVM;
		}

		private double convertTimespanToDouble(TimeSpan t)
		{
			return t.Days*24 + t.Hours + (double) t.Minutes/60;
		}
	}
}