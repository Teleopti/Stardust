using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Outbound.Rules
{
    public class OutboundOverstaffRule : OutboundRule
    {

        private readonly IOutboundCampaignTaskManager _campaignTaskManager;

        private int threshold;
        private ThresholdType thresholdType;

        public OutboundOverstaffRule(IOutboundCampaignTaskManager campaignTaskManager)
        {
            _campaignTaskManager = campaignTaskManager;
        }
       
        public override void Configure(OutboundRuleConfiguration inputConfiguration)
        {
            var configuration = inputConfiguration as OutboundOverstaffRuleConfiguration;
            if (configuration != null)
            {
                thresholdType = configuration.ThresholdType;
                threshold = configuration.Threshold;
            }
            else
            {
                thresholdType = ThresholdType.Absolute;
                threshold = 1;
            }
        }

        public override IEnumerable<OutboundRuleResponse> Validate(IOutboundCampaign campaign)
        {
            var campaignTasks = _campaignTaskManager.GetIncomingTaskFromCampaign(campaign);
            var response = new List<OutboundRuleResponse>();

			foreach (var dateOnly in campaignTasks.GetActivePeriod().DayCollection())
            {
                var callTime = campaignTasks.GetEstimatedOutgoingBacklogOnDate(dateOnly);
                var overstaffTime = campaignTasks.GetOverstaffTimeOnDate(dateOnly);

                if (checkAgainstThreshold(overstaffTime, callTime))
                    response.Add(new OutboundRuleResponse
                    {
                        IsValid = false,
                        TypeOfRule = typeof(OutboundOverstaffRule),
                        Campaign = campaign,
                        Threshold = threshold,
                        ThresholdType = thresholdType,
                        TargetValue = (int)overstaffTime.TotalMinutes,
                        Date = dateOnly.Date
                    });
            }
            return response;
        }

        private bool checkAgainstThreshold(TimeSpan overstaffTime, TimeSpan callTime)
        {
            if (overstaffTime < TimeSpan.FromMinutes(1)) return false;
            switch (thresholdType)
            {
                case ThresholdType.Absolute:
                    return overstaffTime.TotalMinutes > threshold;
                case ThresholdType.Relative:
                    return (overstaffTime.TotalMinutes - callTime.TotalMinutes) > callTime.TotalMinutes * threshold / 100.0;
                default:
                    return false;
            }
        }
    }
}
