using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{
	using TimeSeries = Dictionary<DateOnly, TimeSpan>;

	class OutboundCampaignResourceCacheable
	{
		public TimeSeries Schedules { get; set; }
		public TimeSeries Forecasts { get; set; }
	}

	class OutboundCacheBody
	{
		public Dictionary<Guid, OutboundCampaignResourceCacheable> OutboundCampaignResources { get; set; }  
	}

	public class OutboundScheduledResourcesCacher : IOutboundScheduledResourcesCacher
	{
		private readonly ILoggedOnUser _loggedOnUser;
		static private readonly object cacheLockObject = new object();

		public OutboundScheduledResourcesCacher(ILoggedOnUser loggedOnUser)
		{
			_loggedOnUser = loggedOnUser;
		}

		public IEnumerable<IOutboundCampaign> FilterNotCached(IEnumerable<IOutboundCampaign> campaigns)
		{
			lock (cacheLockObject)
			{
				var cacheBody = readFromCache();
				if (cacheBody == null) return campaigns;

				return campaigns.Where(campaign => 
					campaign.Id.HasValue && !cacheBody.OutboundCampaignResources.ContainsKey(campaign.Id.Value));						
			}
		}  

		public TimeSeries GetScheduledTime(IOutboundCampaign campaign)
		{			
			lock (cacheLockObject)
			{
				if (!campaign.Id.HasValue) return null;
				var cacheBody = readFromCache();
				if (cacheBody == null || !cacheBody.OutboundCampaignResources.ContainsKey(campaign.Id.Value)) return null;
				return cacheBody.OutboundCampaignResources[campaign.Id.Value].Schedules;
			}
		}

		public TimeSeries GetForecastedTime(IOutboundCampaign campaign)
		{		
			lock (cacheLockObject)
			{
				if (!campaign.Id.HasValue) return null;
				var cacheBody = readFromCache();
				if (cacheBody == null || !cacheBody.OutboundCampaignResources.ContainsKey(campaign.Id.Value)) return null;
				return cacheBody.OutboundCampaignResources[campaign.Id.Value].Forecasts;
			}
		}

		public void SetScheduledTime(IOutboundCampaign campaign, TimeSeries value)
		{
			lock (cacheLockObject)
			{
				if (!campaign.Id.HasValue) return;
				writeToCache(campaign.Id.Value, value, null);
			}		
		}

		public void SetForecastedTime(IOutboundCampaign campaign,TimeSeries value)
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

		private void updateCacheBody(OutboundCacheBody body, Guid campaignId, TimeSeries schedules, TimeSeries forecasts)
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


		private void writeToCache(Guid campaignId, TimeSeries schedules, TimeSeries forecasts )
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