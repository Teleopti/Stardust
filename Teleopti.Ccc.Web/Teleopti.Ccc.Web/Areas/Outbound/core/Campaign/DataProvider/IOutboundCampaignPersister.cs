using Teleopti.Ccc.Web.Areas.Outbound.Models;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{
	public interface IOutboundCampaignPersister
	{
		CampaignViewModel Persist(string name);
		Domain.Outbound.Campaign Persist(CampaignForm form);
	}
}