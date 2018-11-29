using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.Outbound.Models;


namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{
	public class CampaignListProvider : ICampaignListProvider
	{
		private readonly IOutboundCampaignRepository _outboundCampaignRepository;
		private readonly IOutboundScheduledResourcesProvider _scheduledResourcesProvider;
		private readonly ICampaignWarningProvider _campaignWarningProvider;
		private readonly IOutboundScheduledResourcesCacher _outboundScheduledResourcesCacher;

		public CampaignListProvider(IOutboundCampaignRepository outboundCampaignRepository, IOutboundScheduledResourcesProvider scheduledResourcesProvider, 
			ICampaignWarningProvider campaignWarningProvider, IOutboundScheduledResourcesCacher outboundScheduledResourcesCacher)
		{
			_outboundCampaignRepository = outboundCampaignRepository;
			_scheduledResourcesProvider = scheduledResourcesProvider;
			_campaignWarningProvider = campaignWarningProvider;
			_outboundScheduledResourcesCacher = outboundScheduledResourcesCacher;
		}

		public void LoadData(GanttPeriod period)
		{
			var campaigns = getCampaigns(period);
		
			loadScheduleData(campaigns);
		}

		public void CheckAndUpdateCache(GanttPeriod period)
		{
			var campaigns = getCampaigns(period);

			if (_outboundScheduledResourcesCacher.FilterNotCached(campaigns).ToList().Count > 0)
			{
				loadScheduleData(campaigns);
			}
		}

		private IList<IOutboundCampaign> getCampaigns(GanttPeriod period)
		{
			var campaigns = period == null
				? _outboundCampaignRepository.LoadAll().ToList()
				: _outboundCampaignRepository.GetCampaigns(new DateOnlyPeriod(period.StartDate, period.EndDate));

			return campaigns;
		}

		private void loadScheduleData(IList<IOutboundCampaign> campaigns)
		{
			if (campaigns.Count == 0) return;

			var campaignPeriod = campaigns.Select(c => c.SpanningPeriod).Aggregate((ac, p) => ac.MaximumPeriod(p)).ToDateOnlyPeriod(TimeZoneInfo.Utc);

			_scheduledResourcesProvider.Load(campaigns, campaignPeriod);
			updateCache(campaigns);
		}

		public void ResetCache()
		{
			_outboundScheduledResourcesCacher.Reset();
		}

		public IEnumerable<CampaignStatusViewModel> GetCampaignsStatus(GanttPeriod period)
		{
			var campaigns = _outboundCampaignRepository.GetCampaigns(new DateOnlyPeriod(period.StartDate, period.EndDate));

			return campaigns.Select(campaign => GetCampaignStatus(campaign.Id.GetValueOrDefault(), new List<DateOnly>())).ToList();
		}

		public CampaignStatusViewModel GetCampaignStatus(Guid id, IEnumerable<DateOnly> skipDates)
		{
			var campaign = _outboundCampaignRepository.Get(id);
			if (campaign == null) return null;

			var warningViewModel = _campaignWarningProvider.CheckCampaign(campaign, skipDates).Select(warning => new OutboundWarningViewModel(warning));
			var period = campaign.SpanningPeriod.ToDateOnlyPeriod(campaign.Skill.TimeZone);

			var schedules = _outboundScheduledResourcesCacher.GetScheduledTime(campaign);
			bool isScheduled;
			if (schedules == null)
			{
				isScheduled = period.DayCollection().Any(
					date => _scheduledResourcesProvider.GetScheduledTimeOnDate(date, campaign.Skill) > TimeSpan.Zero);
			}
			else
			{
				isScheduled = schedules.Keys.Any(d => schedules[d] > TimeSpan.Zero);
			}

			return new CampaignStatusViewModel
			{
				CampaignSummary = new CampaignSummaryViewModel
				{
				Id = campaign.Id.GetValueOrDefault(),
				Name = campaign.Name,
					StartDate = period.StartDate,
					EndDate = period.EndDate
				},
				IsScheduled = isScheduled,
				WarningInfo = warningViewModel
			};
		}

		public IEnumerable<CampaignSummaryViewModel> GetCampaigns(GanttPeriod period)
		{
			var campaigns = _outboundCampaignRepository.GetCampaigns(new DateOnlyPeriod(period.StartDate, period.EndDate));

			var ganttCampaigns = new List<CampaignSummaryViewModel>();
			foreach (var campaign in campaigns)
			{
				var campaignPeriod = campaign.SpanningPeriod.ToDateOnlyPeriod(campaign.Skill.TimeZone);
				
				ganttCampaigns.Add(new CampaignSummaryViewModel
				{
					Id = campaign.Id.GetValueOrDefault(),
					Name = campaign.Name,
					StartDate = campaignPeriod.StartDate,
					EndDate = campaignPeriod.EndDate
				});
			}
			return ganttCampaigns;
		}

		private void updateCache(IEnumerable<IOutboundCampaign> campaigns)
		{
			var notCachedCampaigns = _outboundScheduledResourcesCacher.FilterNotCached(campaigns).ToList();

			foreach (var campaign in notCachedCampaigns)
			{
				var dates = campaign.SpanningPeriod.ToDateOnlyPeriod(campaign.Skill.TimeZone).DayCollection();
				var schedules = dates.ToDictionary(d => d,
					d => _scheduledResourcesProvider.GetScheduledTimeOnDate(d, campaign.Skill))
					.Where(kvp => kvp.Value > TimeSpan.Zero).ToDictionary(d => d.Key, d => d.Value);
				var forecasts = dates.ToDictionary(d => d,
					d => _scheduledResourcesProvider.GetForecastedTimeOnDate(d, campaign.Skill))
					.Where(kvp => kvp.Value > TimeSpan.Zero).ToDictionary(d => d.Key, d => d.Value);
				_outboundScheduledResourcesCacher.SetScheduledTime(campaign, schedules);
				_outboundScheduledResourcesCacher.SetForecastedTime(campaign, forecasts);
			}
		}
	}
}