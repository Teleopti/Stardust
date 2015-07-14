using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.Outbound.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping
{
	public interface IOutboundCampaignViewModelMapper
	{
        IEnumerable<CampaignViewModel> Map(IEnumerable<IOutboundCampaign> campaigns);
        CampaignViewModel Map(IOutboundCampaign campaign);
	}
}