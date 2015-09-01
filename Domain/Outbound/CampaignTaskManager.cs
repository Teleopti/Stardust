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
			 var incomingTask = _outboundProductionPlanFactory.CreateAndMakeInitialPlan(campaign.SpanningPeriod.ToDateOnlyPeriod(campaign.Skill.TimeZone), campaign.CampaignTasks(), campaign.AverageTaskHandlingTime(), campaign.WorkingHours);

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

			    var scheduled = _outboundScheduledResourcesProvider.GetScheduledTimeOnDate(dateOnly, campaign.Skill);
			    incomingTask.SetRealScheduledTimeOnDate(dateOnly, scheduled);
			    var forecasted = _outboundScheduledResourcesProvider.GetForecastedTimeOnDate(dateOnly, campaign.Skill);
			    if (scheduled != TimeSpan.Zero)
				    incomingTask.SetTimeOnDate(dateOnly, scheduled, PlannedTimeTypeEnum.Scheduled);
			    else if (forecasted != TimeSpan.Zero && !manualTime.HasValue)
			    {
				    incomingTask.SetTimeOnDate(dateOnly, forecasted, PlannedTimeTypeEnum.Calculated);
				    incomingTask.SetRealPlannedTimeOnDate(dateOnly, incomingTask.GetTimeOnDate(dateOnly));
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
