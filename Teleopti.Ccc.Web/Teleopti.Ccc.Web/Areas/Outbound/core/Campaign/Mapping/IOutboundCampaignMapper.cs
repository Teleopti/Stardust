using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.Outbound.Models;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping
{
	public interface IOutboundCampaignMapper
	{
        IOutboundCampaign Map(CampaignViewModel campaignViewModel);
	}
}