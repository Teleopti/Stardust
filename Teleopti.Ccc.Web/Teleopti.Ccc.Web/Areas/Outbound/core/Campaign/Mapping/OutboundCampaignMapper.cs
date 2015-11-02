using System;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.Outbound.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping
{
	public class OutboundCampaignMapper : IOutboundCampaignMapper
	{
		private readonly IOutboundCampaignRepository _outboundCampaignRepository;
		private readonly IUserTimeZone _userTimeZone;

		public OutboundCampaignMapper(IOutboundCampaignRepository outboundCampaignRepository, IUserTimeZone userTimeZone)
		{
			_outboundCampaignRepository = outboundCampaignRepository;
			_userTimeZone = userTimeZone;
		}

		public IOutboundCampaign Map(CampaignViewModel campaignViewModel)
		{
			var campaign = _outboundCampaignRepository.Get(campaignViewModel.Id.Value);
			if (campaign == null) return null;

			var timezone = _userTimeZone.TimeZone();
			if (campaign.Skill != null)
			{
				timezone = campaign.Skill.TimeZone;
			}

	      var startDateTime = new DateTime(campaignViewModel.StartDate.Year, campaignViewModel.StartDate.Month, campaignViewModel.StartDate.Day);
			startDateTime = TimeZoneHelper.ConvertToUtc(startDateTime, timezone);
			var endDateTime = new DateTime(campaignViewModel.EndDate.Year, campaignViewModel.EndDate.Month, campaignViewModel.EndDate.Day, 23, 59, 59);
			endDateTime = TimeZoneHelper.ConvertToUtc(endDateTime, timezone);
			campaign.Name = campaignViewModel.Name;
			campaign.CallListLen = campaignViewModel.CallListLen;
			campaign.TargetRate = campaignViewModel.TargetRate;
			campaign.ConnectRate = campaignViewModel.ConnectRate;
			campaign.RightPartyAverageHandlingTime = campaignViewModel.RightPartyAverageHandlingTime;
			campaign.ConnectAverageHandlingTime = campaignViewModel.ConnectAverageHandlingTime;
			campaign.RightPartyConnectRate = campaignViewModel.RightPartyConnectRate;
			campaign.UnproductiveTime = campaignViewModel.UnproductiveTime;
			campaign.SpanningPeriod = new DateTimePeriod(startDateTime, endDateTime);
			campaign.BelongsToPeriod = new DateOnlyPeriod(campaignViewModel.StartDate, campaignViewModel.EndDate);
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