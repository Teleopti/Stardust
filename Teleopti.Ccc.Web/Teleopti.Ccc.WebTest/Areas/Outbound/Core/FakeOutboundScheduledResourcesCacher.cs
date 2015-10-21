using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Outbound.Core
{
	public class FakeOutboundScheduledResourcesCacher : IOutboundScheduledResourcesCacher
	{
		private Dictionary<Guid, Dictionary<DateOnly, TimeSpan>> campaignSchedules = new Dictionary<Guid, Dictionary<DateOnly, TimeSpan>>();
		private Dictionary<Guid, Dictionary<DateOnly, TimeSpan>> campaignForecasts = new Dictionary<Guid, Dictionary<DateOnly, TimeSpan>>();
 
		public Dictionary<DateOnly, TimeSpan> GetScheduledTime(IOutboundCampaign campaign)
		{
			if (!campaign.Id.HasValue) return null;
			return campaignSchedules.ContainsKey(campaign.Id.Value) ? campaignSchedules[campaign.Id.Value] : null;
		}

		public Dictionary<DateOnly, TimeSpan> GetForecastedTime(IOutboundCampaign campaign)
		{
			if (!campaign.Id.HasValue) return null;
			return campaignForecasts.ContainsKey(campaign.Id.Value) ? campaignForecasts[campaign.Id.Value] : null;
		}

		public void SetScheduledTime(IOutboundCampaign campaign, Dictionary<DateOnly, TimeSpan> value)
		{
			if (!campaign.Id.HasValue) return;
			campaignSchedules.Add(campaign.Id.Value, value);
		}

		public void SetForecastedTime(IOutboundCampaign campaign, Dictionary<DateOnly, TimeSpan> value)
		{
			if (!campaign.Id.HasValue) return;
			campaignForecasts.Add(campaign.Id.Value, value);
		}

		public void AddCampaignSchedule(IOutboundCampaign campaign, Dictionary<DateOnly, TimeSpan> schedules)
		{
			if (!campaign.Id.HasValue) return;
			campaignSchedules.Add(campaign.Id.Value, schedules); 
		}

		public void AddCampaignForecasts(IOutboundCampaign campaign,  Dictionary<DateOnly, TimeSpan> forecasts)
		{
			if (!campaign.Id.HasValue) return;
			campaignForecasts.Add(campaign.Id.Value, forecasts); 			
		}

		public void Reset()
		{
			campaignSchedules = new Dictionary<Guid, Dictionary<DateOnly, TimeSpan>>();
			campaignForecasts = new Dictionary<Guid, Dictionary<DateOnly, TimeSpan>>();
		}
	}
}