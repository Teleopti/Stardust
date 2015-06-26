using Teleopti.Ccc.Web.Areas.Outbound.Models;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{
	using Campaign = Domain.Outbound.Campaign;
	public interface IOutboundCampaignPersister
	{
		CampaignViewModel Persist(CampaignForm form);
	    Campaign Persist(CampaignViewModel campaignViewModel);
	}
}