using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Outbound;


namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{
	public interface IOutboundCampaignTaskManager
	{
		IBacklogTask GetIncomingTaskFromCampaign(IOutboundCampaign campaign);
		IBacklogTask GetIncomingTaskFromCampaign(IOutboundCampaign campaign, IList<DateOnly> skipScheduleOnDates);
	}

	public class CampaignTaskManager : IOutboundCampaignTaskManager
	{
		private readonly OutboundProductionPlanFactory _outboundProductionPlanFactory;
		private readonly IOutboundScheduledResourcesProvider _outboundScheduledResourcesProvider;
		private readonly IOutboundScheduledResourcesCacher _outboundScheduledResourcesCacher;

		public CampaignTaskManager(OutboundProductionPlanFactory outboundProductionPlanFactory, IOutboundScheduledResourcesProvider outboundScheduledResourcesProvider, IOutboundScheduledResourcesCacher outboundScheduledResourcesCacher)
		{
			_outboundProductionPlanFactory = outboundProductionPlanFactory;
			_outboundScheduledResourcesProvider = outboundScheduledResourcesProvider;
			_outboundScheduledResourcesCacher = outboundScheduledResourcesCacher;
		}

		public IBacklogTask GetIncomingTaskFromCampaign(IOutboundCampaign campaign)
		{
			return GetIncomingTaskFromCampaign(campaign, new List<DateOnly>());
		}

		public IBacklogTask GetIncomingTaskFromCampaign(IOutboundCampaign campaign, IList<DateOnly> skipScheduleOnDates)
		{
			var incomingTask = _outboundProductionPlanFactory.CreateAndMakeInitialPlan(campaign.SpanningPeriod.ToDateOnlyPeriod(campaign.Skill.TimeZone), campaign.CampaignTasks(), campaign.AverageTaskHandlingTime(), campaign.WorkingHours);
			var dates = incomingTask.SpanningPeriod.DayCollection();
			var schedules = _outboundScheduledResourcesCacher.GetScheduledTime(campaign);
			if (schedules == null)
			{
				schedules = dates.ToDictionary(d => d, d => _outboundScheduledResourcesProvider.GetScheduledTimeOnDate(d, campaign.Skill))
					.Where(kvp => kvp.Value > TimeSpan.Zero).ToDictionary(d => d.Key, d => d.Value);
				_outboundScheduledResourcesCacher.SetScheduledTime(campaign, schedules);
			}

			var forecasts = _outboundScheduledResourcesCacher.GetForecastedTime(campaign);
			if (forecasts == null)
			{
				forecasts = dates.ToDictionary(d => d, d => _outboundScheduledResourcesProvider.GetForecastedTimeOnDate(d, campaign.Skill))
					.Where(kvp => kvp.Value > TimeSpan.Zero).ToDictionary(d => d.Key, d => d.Value);
				_outboundScheduledResourcesCacher.SetForecastedTime(campaign, forecasts);
			}

			foreach (var dateOnly in incomingTask.SpanningPeriod.DayCollection())
			{
				var manualTime = campaign.GetManualProductionPlan(dateOnly);
				if (manualTime.HasValue)
				{
					incomingTask.SetTimeOnDate(dateOnly, manualTime.Value, PlannedTimeTypeEnum.Manual);
					incomingTask.SetManualPlannedInfoOnDate(dateOnly, true);
				}
				else
				{
					incomingTask.SetManualPlannedInfoOnDate(dateOnly, false);
				}
				incomingTask.SetRealPlannedTimeOnDate(dateOnly, incomingTask.GetTimeOnDate(dateOnly));
				incomingTask.SetRealScheduledTimeOnDate(dateOnly, TimeSpan.Zero);

				if (schedules.ContainsKey(dateOnly) && !skipScheduleOnDates.Contains(dateOnly))
				{
					incomingTask.SetRealScheduledTimeOnDate(dateOnly, schedules[dateOnly]);
					if (schedules[dateOnly] > TimeSpan.Zero) incomingTask.SetTimeOnDate(dateOnly, schedules[dateOnly], PlannedTimeTypeEnum.Scheduled);
				}
				else if (forecasts.ContainsKey(dateOnly) && !manualTime.HasValue)
				{
					if (forecasts[dateOnly] > TimeSpan.Zero) incomingTask.SetTimeOnDate(dateOnly, forecasts[dateOnly], PlannedTimeTypeEnum.Calculated);
					incomingTask.SetRealPlannedTimeOnDate(dateOnly, forecasts[dateOnly]);
				}

				var actualBacklog = campaign.GetActualBacklog(dateOnly);
				if (actualBacklog.HasValue)
				{
					incomingTask.SetActualBacklogOnDate(dateOnly, actualBacklog.Value);
				}
			}
			return incomingTask;
		}
	}
}