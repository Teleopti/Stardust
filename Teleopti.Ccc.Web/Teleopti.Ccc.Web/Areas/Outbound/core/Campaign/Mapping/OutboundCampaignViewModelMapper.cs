using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Web.Areas.Outbound.Models;
using Teleopti.Ccc.Web.Core.Data;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping
{
	public class OutboundCampaignViewModelMapper : IOutboundCampaignViewModelMapper
	{
		public CampaignViewModel Map(IOutboundCampaign campaign)
		{
			if (campaign == null) return null;

			var workingHours = campaign.WorkingHours.Select(workingHour => new CampaignWorkingHour() { WeekDay = workingHour.Key, StartTime = workingHour.Value.StartTime, EndTime = workingHour.Value.EndTime });

	      var startDate = new DateOnly(TimeZoneHelper.ConvertFromUtc(campaign.SpanningPeriod.StartDateTime, campaign.Skill.TimeZone).Date);
			var endDate = new DateOnly(TimeZoneHelper.ConvertFromUtc(campaign.SpanningPeriod.EndDateTime, campaign.Skill.TimeZone).Date);
			var campaignVm = new CampaignViewModel
			{
				Id = campaign.Id,
				Name = campaign.Name,
				Activity = new ActivityViewModel { Id = campaign.Skill.Activity.Id, Name= campaign.Skill.Activity.Name} ,
				CallListLen = campaign.CallListLen,
				TargetRate = campaign.TargetRate,
				ConnectRate = campaign.ConnectRate,
				RightPartyConnectRate = campaign.RightPartyConnectRate,
				ConnectAverageHandlingTime = campaign.ConnectAverageHandlingTime,
				RightPartyAverageHandlingTime = campaign.RightPartyAverageHandlingTime,
				UnproductiveTime = campaign.UnproductiveTime,
				StartDate = startDate,
				EndDate = endDate,
				WorkingHours = workingHours
			};

			return campaignVm;
		}

		public IEnumerable<CampaignViewModel> Map(IEnumerable<IOutboundCampaign> campaigns)
		{
			return campaigns.Select(Map);
		}
	}
}