using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Ccc.Web.Areas.Outbound.Models;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.ViewModelFactory
{
    public class CampaignSummaryViewModelFactory : ICampaignSummaryViewModelFactory
    {
        private readonly ICampaignListProvider _campaignListProvider;

        public CampaignSummaryViewModelFactory(ICampaignListProvider campaignListProvider)
        {
            _campaignListProvider = campaignListProvider;
        }

        public List<CampaignSummaryViewModel> GetCampaignSummaryList(CampaignStatus status)
        {
            Func<CampaignSummary, bool> campaignHasWarningPredicate = campaign => campaign.WarningInfo.Any();
            
            var campaigns = _campaignListProvider.ListCampaign(status).ToList();
            var campaignsWithWarning = campaigns.Where(campaignHasWarningPredicate).ToList();
            var campaignsWithoutWarning = campaigns.Except(campaignsWithWarning);

		      var campaings = campaignsWithWarning.Select(c => new CampaignSummaryViewModel(c)).ToList();
		      var noWarningCampaigns = campaignsWithoutWarning.Select(c => new CampaignSummaryViewModel(c)).ToList();
				campaings.AddRange(noWarningCampaigns);

	        return campaings;
        }

        public CampaignStatistics GetCampaignStatistics()
        {
            return _campaignListProvider.GetCampaignStatistics();
        }

		public CampaignSummaryViewModel GetCampaignSummary(Guid id)
	    {
		    return new CampaignSummaryViewModel(_campaignListProvider.GetCampaignById(id));
	    }
    }
}