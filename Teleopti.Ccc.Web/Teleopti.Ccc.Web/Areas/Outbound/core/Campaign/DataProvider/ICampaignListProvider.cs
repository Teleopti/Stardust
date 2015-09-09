using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.Outbound.Models;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{
	public interface ICampaignListProvider
	{
		IEnumerable<CampaignSummary> ListCampaign(CampaignStatus status);
		IEnumerable<CampaignSummary> ListScheduledCampaign();
		IEnumerable<CampaignSummary> ListPlannedCampaign();
		IEnumerable<CampaignSummary> ListOngoingCampaign();
		IEnumerable<CampaignSummary> ListDoneCampaign();

		CampaignStatistics GetCampaignStatistics();
		void LoadData();
		CampaignSummary GetCampaignById(Guid Id);
		IEnumerable<GanttCampaignViewModel> GetCampaigns(GanttPeriod period);
	}
}