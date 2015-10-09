using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Outbound.Rules
{
    public class OutboundOverstaffRule : OutboundRule
    {

        private readonly IOutboundCampaignTaskManager _campaignTaskManager;

        private double threshold;
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
	      
	        var totalWorkloadMinutes =
		        campaignTasks.GetEstimatedIncomingBacklogOnDate(campaignTasks.GetActivePeriod().DayCollection().First()).TotalMinutes;

	        var overstaffMinutes =
		        campaignTasks.GetActivePeriod().DayCollection().Sum(d => campaignTasks.GetOverstaffTimeOnDate(d).TotalMinutes);

			if (checkAgainstThreshold(overstaffMinutes, totalWorkloadMinutes))
				response.Add(new OutboundRuleResponse
				{
					IsValid = false,
					TypeOfRule = typeof(OutboundOverstaffRule),
					Campaign = campaign,
					Threshold = threshold,
					ThresholdType = thresholdType,
					TargetValue = overstaffMinutes
				});
		
            return response;
        }

        private bool checkAgainstThreshold(double overstaffMinutes, double totalWorkloadMinutes)
        {
			if (overstaffMinutes < 1) return false;
            switch (thresholdType)
            {
                case ThresholdType.Absolute:
					return overstaffMinutes > threshold;
                case ThresholdType.Relative:
					return overstaffMinutes > totalWorkloadMinutes * threshold;
                default:
                    return false;
            }
        }
    }
}
