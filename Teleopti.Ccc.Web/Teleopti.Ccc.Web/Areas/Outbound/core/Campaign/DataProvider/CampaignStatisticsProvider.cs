using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.Outbound.Models;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{
	public class CampaignStatisticsProvider : ICampaignStatisticsProvider
	{
		private readonly IOutboundCampaignRepository _outboundCampaignRepository;

		public CampaignStatisticsProvider(IOutboundCampaignRepository outboundCampaignRepository)
		{
			_outboundCampaignRepository = outboundCampaignRepository;
		}

		public CampaignStatistics GetWholeStatistics()
		{
			return new CampaignStatistics()
			{
				Planned = _outboundCampaignRepository.GetPlannedCampaigns().Count,
				OnGoing = _outboundCampaignRepository.GetOnGoingCampaigns().Count,
				Done = _outboundCampaignRepository.GetDoneCampaigns().Count
			};
		}
	}
}