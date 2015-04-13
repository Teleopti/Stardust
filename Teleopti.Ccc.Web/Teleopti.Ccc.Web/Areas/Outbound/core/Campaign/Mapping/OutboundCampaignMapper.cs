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

		public Domain.Outbound.Campaign Map(CampaignForm form)
		{
			var campaign = _outboundCampaignRepository.Get(form.Id.Value);
			if (campaign == null) return null;
			campaign.Name = form.Name;
			campaign.CallListLen = form.CallListLen;
			campaign.TargetRate = form.TargetRate;
			campaign.ConnectRate = form.ConnectRate;
			campaign.RightPartyAverageHandlingTime = form.RightPartyAverageHandlingTime;
			campaign.ConnectAverageHandlingTime = form.ConnectAverageHandlingTime;
			campaign.RightPartyConnectRate = form.RightPartyConnectRate;
			campaign.UnproductiveTime = form.UnproductiveTime;
			//campaign.StartDate = form.DurationStart;
			//campaign.EndDate = form.DurationEnd;
			return campaign;
		}
	}
}