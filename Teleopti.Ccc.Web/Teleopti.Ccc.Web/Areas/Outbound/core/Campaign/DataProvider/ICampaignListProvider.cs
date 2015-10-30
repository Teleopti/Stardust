using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.Outbound.Models;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{
	public interface ICampaignListProvider
	{
		IEnumerable<CampaignStatusViewModel> GetCampaignsStatus(GanttPeriod period);
		CampaignStatusViewModel GetCampaignStatus(Guid id);

		void LoadData(GanttPeriod peroid);
		void ResetCache();
		void CheckAndUpdateCache(GanttPeriod period);
		IEnumerable<CampaignSummaryViewModel> GetCampaigns(GanttPeriod period);
	}
}