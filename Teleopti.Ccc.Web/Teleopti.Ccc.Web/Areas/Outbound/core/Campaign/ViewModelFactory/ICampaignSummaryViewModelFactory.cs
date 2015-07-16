using Teleopti.Ccc.Web.Areas.Outbound.Models;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.ViewModelFactory
{
    public interface ICampaignSummaryViewModelFactory
    {
        CampaignSummaryListViewModel GetCampaignSummaryList(CampaignStatus status);
        CampaignStatistics GetCampaignStatistics();
    }
}