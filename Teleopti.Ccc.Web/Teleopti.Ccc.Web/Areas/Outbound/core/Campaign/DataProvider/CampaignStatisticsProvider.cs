using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.Outbound.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{
	public class CampaignStatisticsProvider : ICampaignStatisticsProvider
	{
		private readonly IOutboundCampaignRepository _outboundCampaignRepository;
		private readonly IOutboundScheduledResourcesProvider _scheduledResourcesProvider;

		public CampaignStatisticsProvider(IOutboundCampaignRepository outboundCampaignRepository, IOutboundScheduledResourcesProvider scheduledResourcesProvider)
		{
			_outboundCampaignRepository = outboundCampaignRepository;
			_scheduledResourcesProvider = scheduledResourcesProvider;
		}

		public CampaignStatistics GetWholeStatistics()
		{
			return new CampaignStatistics()
			{
				Planned = GetPlannedCampaigns().Count,
				OnGoing = _outboundCampaignRepository.GetOnGoingCampaigns().Count,
				Done = _outboundCampaignRepository.GetDoneCampaigns().Count
			};
		}

		public IList<IOutboundCampaign> GetScheduledCampaigns()
		{
			var campaigns = new List<IOutboundCampaign>();
			var all = _outboundCampaignRepository.LoadAll();

			foreach (var campaign in all)
			{
				if (campaign.SpanningPeriod.StartDate <= DateOnly.Today) continue;

				foreach (var date in campaign.SpanningPeriod.DayCollection())
				{
					var scheduled = _scheduledResourcesProvider.GetScheduledTimeOnDate(date, campaign.Skill);
					if (scheduled != TimeSpan.Zero)
					{
						campaigns.Add(campaign);
						break;
					}
				}
			}

			return campaigns;
		}

		public IList<IOutboundCampaign> GetPlannedCampaigns()
		{
			var scheduled = GetScheduledCampaigns();
			var allPlanned = _outboundCampaignRepository.GetPlannedCampaigns();

			var planned = new List<IOutboundCampaign>();
			foreach (var campaign in allPlanned)
			{
				if (scheduled.Count == 0) return allPlanned;
				planned.AddRange(from scheduledCampaign in scheduled where campaign.Id != scheduledCampaign.Id select campaign);
			}

			return planned;
		}
	}
}