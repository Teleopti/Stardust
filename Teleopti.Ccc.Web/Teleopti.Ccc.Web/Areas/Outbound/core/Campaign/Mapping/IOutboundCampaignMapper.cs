using Teleopti.Ccc.Web.Areas.Outbound.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping
{
	public interface IOutboundCampaignMapper
	{
        IOutboundCampaign Map(CampaignViewModel campaignViewModel);
	}
}