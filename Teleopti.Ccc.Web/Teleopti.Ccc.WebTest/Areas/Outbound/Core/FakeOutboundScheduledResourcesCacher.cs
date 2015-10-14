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
			/*
			 * Do nothing
			 */
		}

		public void SetForecastedTime(IOutboundCampaign campaign, Dictionary<DateOnly, TimeSpan> value)
		{
			/*
			 * Do nothing
			 */
		}

		public void AddCampaignSchedule(Dictionary<Guid, Dictionary<DateOnly, TimeSpan>> schedules)
		{
			campaignSchedules = schedules;
		}

		public void SetCampaignForecasts(Dictionary<Guid, Dictionary<DateOnly, TimeSpan>> forecasts)
		{
			campaignForecasts = forecasts;
		}

		public void Reset()
		{
			campaignSchedules = new Dictionary<Guid, Dictionary<DateOnly, TimeSpan>>();
			campaignForecasts = new Dictionary<Guid, Dictionary<DateOnly, TimeSpan>>();
		}
	}
}