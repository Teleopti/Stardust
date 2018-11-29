using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Rules;


namespace Teleopti.Ccc.WebTest.Core.Common.DataProvider
{
	class FakeCampaignWarningProvider : ICampaignWarningProvider
    {
		Dictionary<string, List<CampaignWarning>> ruleCheckResult = new Dictionary<string, List<CampaignWarning>>(); 


        public IEnumerable<CampaignWarning> CheckCampaign(IOutboundCampaign campaign, IEnumerable<DateOnly> skipDates)
        {
            if (ruleCheckResult.ContainsKey(campaign.Name))
                return ruleCheckResult[campaign.Name];
			return new List<CampaignWarning>();
        }

		public void SetCampaignRuleCheckResponse(IOutboundCampaign campaign, IEnumerable<CampaignWarning> response)
        {
            ruleCheckResult.Add(campaign.Name, response.ToList()) ;
        }
    }
}
