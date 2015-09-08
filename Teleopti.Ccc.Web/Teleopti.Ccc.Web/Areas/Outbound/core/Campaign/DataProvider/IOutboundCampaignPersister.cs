using System;
using Teleopti.Ccc.Web.Areas.Outbound.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{
	public interface IOutboundCampaignPersister
	{
		CampaignViewModel Persist(CampaignForm form);
        IOutboundCampaign Persist(CampaignViewModel campaignViewModel);
		void PersistManualProductionPlan(ManualPlanForm manualPlan);
		void RemoveManualProductionPlan(RemoveManualPlanForm manualPlan);
		void ManualReplanCampaign(Guid campaignId);
		void PersistActualBacklog(ActualBacklogForm actualBacklog);
		void RemoveActualBacklog(RemoveActualBacklogForm removeBacklog);
		void RemoveCampaign(IOutboundCampaign campaign);
	}
}