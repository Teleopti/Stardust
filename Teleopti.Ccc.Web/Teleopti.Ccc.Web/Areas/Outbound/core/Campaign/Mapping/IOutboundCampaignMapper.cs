using Teleopti.Ccc.Web.Areas.Outbound.Models;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping
{
	public interface IOutboundCampaignMapper
	{
		Domain.Outbound.Campaign Map(CampaignForm form);
	}
}