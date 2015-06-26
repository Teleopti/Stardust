using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Web.Areas.Outbound.Models;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping
{
	using Campaign = Domain.Outbound.Campaign;

	public class OutboundCampaignViewModelMapper : IOutboundCampaignViewModelMapper
	{
		public CampaignViewModel Map(Campaign campaign)
		{
			if (campaign == null) return null;

			var workingHours = campaign.WorkingHours.Select(workingHour => new CampaignWorkingHour() { WeekDay = workingHour.Key, StartTime = workingHour.Value.StartTime, EndTime = workingHour.Value.EndTime });

			var campaignVm = new CampaignViewModel
			{
				Id = campaign.Id,
				Name = campaign.Name,
				ActivityId = (Guid) campaign.Skill.Activity.Id,
				CallListLen = campaign.CallListLen,
				TargetRate = campaign.TargetRate,
				ConnectRate = campaign.ConnectRate,
				RightPartyConnectRate = campaign.RightPartyConnectRate,
				ConnectAverageHandlingTime = campaign.ConnectAverageHandlingTime,
				RightPartyAverageHandlingTime = campaign.RightPartyAverageHandlingTime,
				UnproductiveTime = campaign.UnproductiveTime,
				StartDate = campaign.SpanningPeriod.StartDate,
				EndDate = campaign.SpanningPeriod.EndDate,
				WorkingHours = workingHours
			};

			return campaignVm;
		}

		public IEnumerable<CampaignViewModel> Map(IEnumerable<Campaign> campaigns)
		{
			return campaigns.Select(Map);
		}
	}
}