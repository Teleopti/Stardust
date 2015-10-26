using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.Outbound.Models;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{
	public interface ICampaignListProvider
	{
		IEnumerable<CampaignSummary> ListCampaign(CampaignStatus status, GanttPeriod period);
		IEnumerable<CampaignSummary> ListScheduledCampaign(GanttPeriod peroid);
		IEnumerable<CampaignSummary> ListPlannedCampaign(GanttPeriod peroid);
		IEnumerable<CampaignSummary> ListOngoingCampaign(GanttPeriod peroid);
		IEnumerable<CampaignSummary> ListDoneCampaign(GanttPeriod peroid);
		IEnumerable<PeriodCampaignSummaryViewModel> GetPeriodCampaignsSummary(GanttPeriod period);
		PeriodCampaignSummaryViewModel GetCampaignSummary(Guid id);

		CampaignStatistics GetCampaignStatistics(GanttPeriod peroid);
		void LoadData(GanttPeriod peroid);
		void ResetCache();
		CampaignSummary GetCampaignById(Guid Id);
		IEnumerable<GanttCampaignViewModel> GetCampaigns(GanttPeriod period);
	}
}