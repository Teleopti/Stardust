using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Common.DataProvider
{
    class FakeOutboundRuleChecker : IOutboundRuleChecker
    {
        Dictionary<string, List<OutboundRuleResponse>> ruleCheckResult = new Dictionary<string, List<OutboundRuleResponse>>(); 


        public IEnumerable<OutboundRuleResponse> CheckCampaign(IOutboundCampaign campaign)
        {
            if (ruleCheckResult.ContainsKey(campaign.Name))
                return ruleCheckResult[campaign.Name];
            return new List<OutboundRuleResponse>();
        }

        public void SetCampaignRuleCheckResponse(IOutboundCampaign campaign, IEnumerable<OutboundRuleResponse> response)
        {
            ruleCheckResult.Add(campaign.Name, response.ToList()) ;
        }
    }
}
