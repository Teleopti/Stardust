using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.Outbound.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{
	public class CampaignListProvider : ICampaignListProvider
	{
		private readonly IOutboundCampaignRepository _outboundCampaignRepository;
		private readonly IOutboundScheduledResourcesProvider _scheduledResourcesProvider;
		private readonly ICampaignWarningProvider _campaignWarningProvider;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IOutboundScheduledResourcesCacher _outboundScheduledResourcesCacher;

		public CampaignListProvider(IOutboundCampaignRepository outboundCampaignRepository, IOutboundScheduledResourcesProvider scheduledResourcesProvider, 
			ICampaignWarningProvider campaignWarningProvider, IUserTimeZone userTimeZone, IOutboundScheduledResourcesCacher outboundScheduledResourcesCacher)
		{
			_outboundCampaignRepository = outboundCampaignRepository;
			_scheduledResourcesProvider = scheduledResourcesProvider;
			_campaignWarningProvider = campaignWarningProvider;
			_userTimeZone = userTimeZone;
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
				? _outboundCampaignRepository.LoadAll()
				: _outboundCampaignRepository.GetCampaigns(getUtcPeroid(period));

			return campaigns;
		}

		private void loadScheduleData(IList<IOutboundCampaign> campaigns)
		{
			if (campaigns.Count == 0) return;

			var campaignPeriod = campaigns.Select(c => c.SpanningPeriod).Aggregate((ac, p) => ac.MaximumPeriod(p)).ToDateOnlyPeriod(TimeZoneInfo.Utc);
			var maximumPeriod = new DateOnlyPeriod(campaignPeriod.StartDate.AddDays(-1), campaignPeriod.EndDate.AddDays(1));

			_scheduledResourcesProvider.Load(campaigns, maximumPeriod);
			updateCache(campaigns);
		}

		public void ResetCache()
		{
			_outboundScheduledResourcesCacher.Reset();
		}

		public IEnumerable<CampaignStatusViewModel> GetCampaignsStatus(GanttPeriod period)
		{
			var campaigns = _outboundCampaignRepository.GetCampaigns(getUtcPeroid(period));

			return campaigns.Select(campaign => GetCampaignStatus((Guid) campaign.Id)).ToList();
		}

		public CampaignStatusViewModel GetCampaignStatus(Guid id)
		{
			var campaign = _outboundCampaignRepository.Get(id);
			if (campaign == null) return null;

			var warningViewModel = _campaignWarningProvider.CheckCampaign(campaign).Select(warning => new OutboundWarningViewModel(warning));
			
			var schedules = _outboundScheduledResourcesCacher.GetScheduledTime(campaign);
			bool isScheduled;
			if (schedules == null)
			{
				isScheduled = campaign.SpanningPeriod.ToDateOnlyPeriod(TimeZoneInfo.Utc).DayCollection().Any(
					date => _scheduledResourcesProvider.GetScheduledTimeOnDate(date, campaign.Skill) > TimeSpan.Zero);
			}
			else
			{
				isScheduled = schedules.Keys.Any(d => schedules[d] > TimeSpan.Zero);
			}

			return new CampaignStatusViewModel()
			{
				CampaignSummary = new CampaignSummaryViewModel()
				{
				Id = (Guid)campaign.Id,
				Name = campaign.Name,
					StartDate = campaign.SpanningPeriod.ToDateOnlyPeriod(campaign.Skill.TimeZone).StartDate,
					EndDate = campaign.SpanningPeriod.ToDateOnlyPeriod(campaign.Skill.TimeZone).EndDate
				},
				IsScheduled = isScheduled,
				WarningInfo = warningViewModel
			};
		}

		public IEnumerable<CampaignSummaryViewModel> GetCampaigns(GanttPeriod period)
		{
			var campaigns = _outboundCampaignRepository.GetCampaigns(getUtcPeroid(period));

			var ganttCampaigns = new List<CampaignSummaryViewModel>();
			foreach (var campaign in campaigns)
			{
				var startDateTime = TimeZoneHelper.ConvertFromUtc(campaign.SpanningPeriod.StartDateTime, campaign.Skill.TimeZone);
				var endDateTime = TimeZoneHelper.ConvertFromUtc(campaign.SpanningPeriod.EndDateTime, campaign.Skill.TimeZone);

				var startDateAsUtc = new DateOnly(DateTime.SpecifyKind(startDateTime, DateTimeKind.Utc));
				var endDateAsUtc = new DateOnly(DateTime.SpecifyKind(endDateTime, DateTimeKind.Utc));

				ganttCampaigns.Add(new CampaignSummaryViewModel()
				{
					Id = (Guid) campaign.Id,
					Name = campaign.Name,
					StartDate = startDateAsUtc,
					EndDate = endDateAsUtc
				});
			}
			return ganttCampaigns;
		}

		private DateTimePeriod getUtcPeroid(GanttPeriod period)
		{
			var start = TimeZoneHelper.ConvertToUtc(period.StartDate.Date, _userTimeZone.TimeZone());
			var end = TimeZoneHelper.ConvertToUtc(period.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59), _userTimeZone.TimeZone());

			return new DateTimePeriod(start, end);
		}

		private void updateCache(IEnumerable<IOutboundCampaign> campaigns)
		{
			var notCachedCampaigns = _outboundScheduledResourcesCacher.FilterNotCached(campaigns).ToList();

			foreach (var campaign in notCachedCampaigns)
			{
				var dates = campaign.SpanningPeriod.DateCollection();
				var schedules = dates.ToDictionary(d => new DateOnly(d),
					d => _scheduledResourcesProvider.GetScheduledTimeOnDate(new DateOnly(d), campaign.Skill))
					.Where(kvp => kvp.Value > TimeSpan.Zero).ToDictionary(d => d.Key, d => d.Value);
				var forecasts = dates.ToDictionary(d => new DateOnly(d),
					d => _scheduledResourcesProvider.GetForecastedTimeOnDate(new DateOnly(d), campaign.Skill))
					.Where(kvp => kvp.Value > TimeSpan.Zero).ToDictionary(d => d.Key, d => d.Value);
				_outboundScheduledResourcesCacher.SetScheduledTime(campaign, schedules);
				_outboundScheduledResourcesCacher.SetForecastedTime(campaign, forecasts);
			}
		}
	}
}