using Teleopti.Ccc.Web.Areas.Outbound.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{
	public interface IOutboundCampaignPersister
	{
		CampaignViewModel Persist(CampaignForm form);
        IOutboundCampaign Persist(CampaignViewModel campaignViewModel);
	}
}