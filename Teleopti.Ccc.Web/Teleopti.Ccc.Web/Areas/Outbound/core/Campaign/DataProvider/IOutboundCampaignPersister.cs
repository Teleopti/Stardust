using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Web.Areas.Outbound.Models;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{
	using Campaign = Domain.Outbound.Campaign;
	public interface IOutboundCampaignPersister
	{
		CampaignViewModel Persist(string name);
	    Campaign Persist(CampaignViewModel campaignViewModel);
		Campaign Persist(CampaignWorkingPeriodAssignmentForm form);
		CampaignWorkingPeriod Persist(CampaignWorkingPeriodForm form);
	}
}