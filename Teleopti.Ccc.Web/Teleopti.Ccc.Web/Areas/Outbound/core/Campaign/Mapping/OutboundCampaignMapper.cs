using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.Outbound.Models;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping
{
	public class OutboundCampaignMapper : IOutboundCampaignMapper
	{
		private readonly IOutboundCampaignRepository _outboundCampaignRepository;

		public OutboundCampaignMapper(IOutboundCampaignRepository outboundCampaignRepository)
		{
			_outboundCampaignRepository = outboundCampaignRepository;
		}

		public Domain.Outbound.Campaign Map(CampaignViewModel campaignViewModel)
		{
			var campaign = _outboundCampaignRepository.Get(campaignViewModel.Id.Value);
			if (campaign == null) return null;
			campaign.Name = campaignViewModel.Name;
			campaign.CallListLen = campaignViewModel.CallListLen;
			campaign.TargetRate = campaignViewModel.TargetRate;
			campaign.ConnectRate = campaignViewModel.ConnectRate;
			campaign.RightPartyAverageHandlingTime = campaignViewModel.RightPartyAverageHandlingTime;
			campaign.ConnectAverageHandlingTime = campaignViewModel.ConnectAverageHandlingTime;
			campaign.RightPartyConnectRate = campaignViewModel.RightPartyConnectRate;
			campaign.UnproductiveTime = campaignViewModel.UnproductiveTime;
			campaign.StartDate = campaignViewModel.StartDate;
			campaign.EndDate = campaignViewModel.EndDate;
			return campaign;
		}
	}
}