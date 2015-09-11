using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.Outbound.Models;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.ViewModelFactory
{
    public interface ICampaignSummaryViewModelFactory
    {
		 List<CampaignSummaryViewModel> GetCampaignSummaryList(CampaignStatus status);
        CampaignStatistics GetCampaignStatistics();
		CampaignSummaryViewModel GetCampaignSummary(Guid id);
    }
}