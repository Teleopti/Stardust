using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.Outbound.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping
{
	public interface IOutboundCampaignListViewModelMapper
	{
		CampaignListViewModel Map(IOutboundCampaign campaign, string status);
		IList<CampaignListViewModel> Map(IList<IOutboundCampaign> campaigns, string status);
	}
}