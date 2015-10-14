using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{
	class OutboundCampaignResourceCacheable
	{
		public Dictionary<DateOnly, TimeSpan> Schedules { get; set; }
		public Dictionary<DateOnly, TimeSpan> Forecasts { get; set; }
	}

	class OutboundCacheBody
	{
		public Dictionary<Guid, OutboundCampaignResourceCacheable> OutboundCampaignResources { get; set; }  
	}

	public class OutboundScheduledResourcesCacher : IOutboundScheduledResourcesCacher
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly object cacheLockObject = new object();

		public OutboundScheduledResourcesCacher(ILoggedOnUser loggedOnUser)
		{
			_loggedOnUser = loggedOnUser;
		}

		public Dictionary<DateOnly, TimeSpan> GetScheduledTime(IOutboundCampaign campaign)
		{
			return null;
			lock (cacheLockObject)
			{
				if (!campaign.Id.HasValue) return null;
				var cacheBody = readFromCache();
				if (cacheBody == null || !cacheBody.OutboundCampaignResources.ContainsKey(campaign.Id.Value)) return null;
				return cacheBody.OutboundCampaignResources[campaign.Id.Value].Schedules;
			}
		}

		public Dictionary<DateOnly, TimeSpan> GetForecastedTime(IOutboundCampaign campaign)
		{
			return null;
			lock (cacheLockObject)
			{
				if (!campaign.Id.HasValue) return null;
				var cacheBody = readFromCache();
				if (cacheBody == null || !cacheBody.OutboundCampaignResources.ContainsKey(campaign.Id.Value)) return null;
				return cacheBody.OutboundCampaignResources[campaign.Id.Value].Forecasts;
			}
		}

		public void SetScheduledTime(IOutboundCampaign campaign, Dictionary<DateOnly, TimeSpan> value)
		{
			lock (cacheLockObject)
			{
				if (!campaign.Id.HasValue) return;
				writeToCache(campaign.Id.Value, value, null);
			}		
		}

		public void SetForecastedTime(IOutboundCampaign campaign, Dictionary<DateOnly, TimeSpan> value)
		{
			lock (cacheLockObject)
			{
				if (!campaign.Id.HasValue) return;
				writeToCache(campaign.Id.Value, null, value);
			}
		}

		public void Reset()
		{
			lock (cacheLockObject)
			{
				reset();
			}
		}

		private void updateCacheBody(OutboundCacheBody body, Guid campaignId, Dictionary<DateOnly, TimeSpan> schedules,
			Dictionary<DateOnly, TimeSpan> forecasts)
		{
			if (body.OutboundCampaignResources.ContainsKey(campaignId))
			{
				if (forecasts != null) body.OutboundCampaignResources[campaignId].Forecasts = forecasts;
				if (schedules != null) body.OutboundCampaignResources[campaignId].Schedules = schedules;
			}
			else
			{
				body.OutboundCampaignResources.Add(campaignId, new OutboundCampaignResourceCacheable
				{
					Forecasts = forecasts,
					Schedules = schedules
				});
			}
		}


		private void writeToCache(Guid campaignId, Dictionary<DateOnly, TimeSpan> schedules, Dictionary<DateOnly, TimeSpan> forecasts )
		{		
			var cachePolicy = new CacheItemPolicy { SlidingExpiration = new TimeSpan(0, 60, 0) };
			var cacheBody = readFromCache() ?? new OutboundCacheBody
			{
				OutboundCampaignResources = new Dictionary<Guid, OutboundCampaignResourceCacheable>()
			};

			updateCacheBody(cacheBody, campaignId, schedules, forecasts);			
			MemoryCache.Default.Set(getCacheKey(), cacheBody, cachePolicy);			
		}

		private OutboundCacheBody readFromCache()
		{
			return MemoryCache.Default.Get(getCacheKey()) as OutboundCacheBody;		
		}

		private void reset()
		{
			MemoryCache.Default.Remove(getCacheKey());
		}

		private string getCacheKey()
		{
			return "Outbound/" + _loggedOnUser.CurrentUser().Id;
		}
	}
}