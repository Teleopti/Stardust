using System;
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

        public CampaignSummaryListViewModel GetCampaignSummaryList(CampaignStatus status)
        {
            Func<CampaignSummary, bool> campaignHasWarningPredicate = campaign => campaign.WarningInfo.Any();
            
            var campaigns = _campaignListProvider.ListCampaign(status).ToList();
            var campaignsWithWarning = campaigns.Where(campaignHasWarningPredicate).ToList();
            var campaignsWithoutWarning = campaigns.Except(campaignsWithWarning);

            return new CampaignSummaryListViewModel
            {
                CampaignsWithWarning = campaignsWithWarning.Select(c => new CampaignSummaryViewModel(c)).ToList(),
                CampaignsWithoutWarning = campaignsWithoutWarning.Select(c => new CampaignSummaryViewModel(c)).ToList()
            };
        }

        public CampaignStatistics GetCampaignStatistics()
        {
            return _campaignListProvider.GetCampaignStatistics();
        }
    }
}