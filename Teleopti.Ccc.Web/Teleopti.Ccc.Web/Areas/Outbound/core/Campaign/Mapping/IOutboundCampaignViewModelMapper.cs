using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.Outbound.Models;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping
{
	public interface IOutboundCampaignViewModelMapper
	{
		IEnumerable<CampaignViewModel> Map(IEnumerable<Domain.Outbound.Campaign> campaigns);
		CampaignViewModel Map(Domain.Outbound.Campaign campaign);
	}
}