using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Rules;


namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{
	public class CampaignWarningProvider : ICampaignWarningProvider
	{
		private readonly ICampaignWarningConfigurationProvider _campaignWarningConfigurationProvider;
		private readonly CampaignUnderServiceLevelRule _outboundUnderSlaRule;
		private readonly CampaignOverstaffRule _outboundOverstaffRule;

		public CampaignWarningProvider(ICampaignWarningConfigurationProvider campaignWarningConfigurationProvider, CampaignUnderServiceLevelRule outboundUnderSlaRule, CampaignOverstaffRule outboundOverstaffRule)
		{
			_campaignWarningConfigurationProvider = campaignWarningConfigurationProvider;
			_outboundUnderSlaRule = outboundUnderSlaRule;
			_outboundOverstaffRule = outboundOverstaffRule;		
		}

		public IEnumerable<CampaignWarning> CheckCampaign(IOutboundCampaign campaign, IEnumerable<DateOnly> skipDates)
		{
			var response = new List<CampaignWarning>();

			foreach (var rule in new List<CampaignWarningCheckRule> { _outboundOverstaffRule, _outboundUnderSlaRule })
			{
				var config = _campaignWarningConfigurationProvider.GetConfiguration(rule.GetType());
				rule.Configure(config);
				var result = rule.Validate(campaign, skipDates).ToList();
				if (result.Count > 0)
				{
					response.Add(result.First());
				}
			}
			return response;
		}       

	}
}