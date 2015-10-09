using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Outbound.Rules
{
    public class OutboundUnderSLARule : OutboundRule
    {
        private readonly IOutboundCampaignTaskManager _campaignTaskManager;

        private double threshold;
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

			var totalWorkloadMinutes =
			   campaignTasks.GetEstimatedIncomingBacklogOnDate(campaignTasks.GetActivePeriod().DayCollection().First()).TotalMinutes;
            var outsideSlaMinutes = campaignTasks.GetTimeOutsideSLA().TotalMinutes;

			if (checkAgainstThreshold(outsideSlaMinutes, totalWorkloadMinutes))
            {
                response.Add(new OutboundRuleResponse
                {
                    IsValid = false,
                    TypeOfRule = typeof(OutboundUnderSLARule),
                    Campaign = campaign,
                    Threshold = threshold,
                    ThresholdType = thresholdType,
					TargetValue = outsideSlaMinutes,
                });
            }
            return response;
        }


        private bool checkAgainstThreshold(double outsideSlaMinutes, double totalWorkloadMinutes)
        {
			if (outsideSlaMinutes < 1) return false;
            switch (thresholdType)
            {
                case ThresholdType.Absolute:
					return outsideSlaMinutes > threshold;
                case ThresholdType.Relative:
					return outsideSlaMinutes > totalWorkloadMinutes * threshold ;
                default:
                    return false;
            }
        }
    }
}
