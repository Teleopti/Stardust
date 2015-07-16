using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Outbound.Rules
{
    public class OutboundUnderSLARule : OutboundRule
    {
        private readonly IOutboundCampaignTaskManager _campaignTaskManager;

        private int threshold;
        private ThresholdType thresholdType;

        public OutboundUnderSLARule(IOutboundCampaignTaskManager campaignTaskManager)
        {
            _campaignTaskManager = campaignTaskManager;
        }

        public override void Configure(OutboundRuleConfiguration inputConfiguration)
        {
            var configuration = inputConfiguration as OutboundUnderSLARuleConfiguration;
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

            var outsideSlaTime = campaignTasks.GetTimeOutsideSLA();
            var totalWorkTime = campaignTasks.TotalWorkTime;

            if (checkAgainstThreshold(outsideSlaTime, totalWorkTime))
            {
                response.Add(new OutboundRuleResponse
                {
                    IsValid = false,
                    TypeOfRule = typeof(OutboundUnderSLARule),
                    Campaign = campaign,
                    Threshold = threshold,
                    ThresholdType = thresholdType,
                    TargetValue = (int)outsideSlaTime.TotalMinutes,
                });
            }
            return response;
        }


        private bool checkAgainstThreshold(TimeSpan outsideSlaTime, TimeSpan totalWorkTime)
        {
            if (outsideSlaTime < TimeSpan.FromMinutes(1)) return false;
            switch (thresholdType)
            {
                case ThresholdType.Absolute:
                    return outsideSlaTime.TotalMinutes > threshold;
                case ThresholdType.Relative:
                    return (outsideSlaTime.TotalMinutes - totalWorkTime.TotalMinutes) > totalWorkTime.TotalMinutes * threshold / 100.0;
                default:
                    return false;
            }
        }
    }
}
