using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.Outbound.Models;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{
	public interface ICampaignListProvider
	{
		IEnumerable<CampaignSummary> ListScheduledCampaign(GanttPeriod peroid);
		IEnumerable<CampaignSummary> ListPlannedCampaign(GanttPeriod peroid);
		IEnumerable<CampaignSummary> ListOngoingCampaign(GanttPeriod peroid);
		IEnumerable<CampaignSummary> ListDoneCampaign(GanttPeriod peroid);
		IEnumerable<PeriodCampaignSummaryViewModel> GetPeriodCampaignsSummary(GanttPeriod period);
		PeriodCampaignSummaryViewModel GetCampaignSummary(Guid id);

		void LoadData(GanttPeriod peroid);
		void ResetCache();
		void CheckAndUpdateCache(GanttPeriod period);
		IEnumerable<GanttCampaignViewModel> GetCampaigns(GanttPeriod period);
	}
}