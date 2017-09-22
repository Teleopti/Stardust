using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.Outbound.Models;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping
{
	public interface IOutboundCampaignViewModelMapper
	{
        IEnumerable<CampaignViewModel> Map(IEnumerable<IOutboundCampaign> campaigns);
        CampaignViewModel Map(IOutboundCampaign campaign);
	}
}