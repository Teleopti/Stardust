using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.Outbound.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping
{
	public class OutboundCampaignMapper : IOutboundCampaignMapper
	{
		private readonly IOutboundCampaignRepository _outboundCampaignRepository;

		public OutboundCampaignMapper(IOutboundCampaignRepository outboundCampaignRepository)
		{
			_outboundCampaignRepository = outboundCampaignRepository;
		}

        public IOutboundCampaign Map(CampaignViewModel campaignViewModel)
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
			campaign.SpanningPeriod = new DateOnlyPeriod(campaignViewModel.StartDate, campaignViewModel.EndDate);
			campaign.WorkingHours.Clear();
			if (campaignViewModel.WorkingHours != null)
			{
				foreach (CampaignWorkingHour workingHour in campaignViewModel.WorkingHours)
				{
					campaign.WorkingHours.Add(workingHour.WeekDay, new TimePeriod(workingHour.StartTime, workingHour.EndTime));
				}
			}
			return campaign;
		}
	}
}