using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Web.Areas.Outbound.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping
{
	public class OutboundCampaignListViewModelMapper : IOutboundCampaignListViewModelMapper
	{
		public CampaignListViewModel Map(IOutboundCampaign campaign, string status)
		{
			if (campaign == null) return null;

			var campaignListVm = new CampaignListViewModel()
			{
				Name = campaign.Name,
				StartDate = campaign.SpanningPeriod.StartDate,
				EndDate = campaign.SpanningPeriod.EndDate,
				Status = status
			};

			return campaignListVm;
		}

		public IList<CampaignListViewModel> Map(IList<IOutboundCampaign> campaigns, string status)
		{
			return campaigns.Select(campaign => Map(campaign, status)).ToList();
		}
	}
}