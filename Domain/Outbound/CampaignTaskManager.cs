using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Outbound
{
    public class CampaignTaskManager : IOutboundCampaignTaskManager
    {
        private readonly OutboundProductionPlanFactory _outboundProductionPlanFactory;
        private readonly IOutboundScheduledResourcesProvider _outboundScheduledResourcesProvider;

        public CampaignTaskManager(OutboundProductionPlanFactory outboundProductionPlanFactory, IOutboundScheduledResourcesProvider outboundScheduledResourcesProvider)
        {
            _outboundProductionPlanFactory = outboundProductionPlanFactory;
            _outboundScheduledResourcesProvider = outboundScheduledResourcesProvider;
        }

        public IBacklogTask GetIncomingTaskFromCampaign(IOutboundCampaign campaign)
        {
            var incomingTask = _outboundProductionPlanFactory.CreateAndMakeInitialPlan(campaign.SpanningPeriod, campaign.CampaignTasks(),
                campaign.AverageTaskHandlingTime(), campaign.WorkingHours);

            foreach (var dateOnly in incomingTask.SpanningPeriod.DayCollection())
            {
                var manualTime = campaign.GetManualProductionPlan(dateOnly);
                if (manualTime.HasValue)  incomingTask.SetTimeOnDate(dateOnly, manualTime.Value, PlannedTimeTypeEnum.Manual);
					 incomingTask.SetRealPlannedTimeOnDate(dateOnly, incomingTask.GetPlannedTimeOnDate(dateOnly));

                var scheduled = _outboundScheduledResourcesProvider.GetScheduledTimeOnDate(dateOnly, campaign.Skill);
                var forecasted = _outboundScheduledResourcesProvider.GetForecastedTimeOnDate(dateOnly, campaign.Skill);
                if (scheduled != TimeSpan.Zero)
                    incomingTask.SetTimeOnDate(dateOnly, scheduled, PlannedTimeTypeEnum.Scheduled);
                else if (forecasted != TimeSpan.Zero && !manualTime.HasValue)
                    incomingTask.SetTimeOnDate(dateOnly, forecasted, PlannedTimeTypeEnum.Calculated);
            }
            return incomingTask;
        }
    }
}
