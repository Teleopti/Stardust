﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Rules;
using Teleopti.Ccc.Web.Areas.Outbound.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{
	public class CampaignListProvider : ICampaignListProvider
	{
		private readonly IOutboundCampaignRepository _outboundCampaignRepository;
		private readonly IOutboundScheduledResourcesProvider _scheduledResourcesProvider;
		private readonly ICampaignWarningProvider _campaignWarningProvider;
		private readonly ICampaignListOrderProvider _campaignListOrderProvider;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IOutboundScheduledResourcesCacher _outboundScheduledResourcesCacher;

		public CampaignListProvider(IOutboundCampaignRepository outboundCampaignRepository, IOutboundScheduledResourcesProvider scheduledResourcesProvider, 
			ICampaignWarningProvider campaignWarningProvider, ICampaignListOrderProvider campaignListOrderProvider, IUserTimeZone userTimeZone, IOutboundScheduledResourcesCacher outboundScheduledResourcesCacher)
		{
			_outboundCampaignRepository = outboundCampaignRepository;
			_scheduledResourcesProvider = scheduledResourcesProvider;
			_campaignWarningProvider = campaignWarningProvider;
			_campaignListOrderProvider = campaignListOrderProvider;
			_userTimeZone = userTimeZone;
			_outboundScheduledResourcesCacher = outboundScheduledResourcesCacher;
		}

		public void LoadData(GanttPeriod period)
		{			
			var campaigns = period == null
				? _outboundCampaignRepository.LoadAll()
				: _outboundCampaignRepository.GetCampaigns(getUtcPeroid(period));
			if (campaigns.Count == 0) return;
		
			loadScheduleData(campaigns);
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

		public IEnumerable<PeriodCampaignSummaryViewModel> GetPeriodCampaignsSummary(GanttPeriod period)
		{
			var campaigns = _outboundCampaignRepository.GetCampaigns(getUtcPeroid(period));

			return campaigns.Select(campaign => GetCampaignSummary((Guid) campaign.Id)).ToList();
		}

		public PeriodCampaignSummaryViewModel GetCampaignSummary(Guid id)
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

			return new PeriodCampaignSummaryViewModel()
			{
				Id = (Guid)campaign.Id,
				Name = campaign.Name,
				IsScheduled = isScheduled,
				StartDate = campaign.SpanningPeriod.ToDateOnlyPeriod(campaign.Skill.TimeZone).StartDate,
				EndDate = campaign.SpanningPeriod.ToDateOnlyPeriod(campaign.Skill.TimeZone).EndDate,
				WarningInfo = warningViewModel
			};
		}

		public IEnumerable<CampaignSummary> ListScheduledCampaign(GanttPeriod peroid)
		{
			var campaigns = peroid == null ? _outboundCampaignRepository.GetPlannedCampaigns()
													 : _outboundCampaignRepository.GetPlannedCampaigns(getUtcPeroid(peroid));

			Func<IOutboundCampaign, bool> campaignHasSchedulePredicate = campaign =>
			{
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
				return isScheduled;
			};

			return campaigns.Where(campaignHasSchedulePredicate).Select(campaign =>
				assembleSummary(campaign, CampaignStatus.Scheduled, _campaignWarningProvider.CheckCampaign(campaign)));
		}

		public IEnumerable<CampaignSummary> ListPlannedCampaign(GanttPeriod peroid)
		{						
			var campaigns = peroid == null ? _outboundCampaignRepository.GetPlannedCampaigns()
													 : _outboundCampaignRepository.GetPlannedCampaigns(getUtcPeroid(peroid));

			Func<IOutboundCampaign, bool> campaignNoSchedulePredicate = campaign =>
			{
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
				return !isScheduled;
			};

			return campaigns.Where(campaignNoSchedulePredicate).Select(campaign =>
				assembleSummary(campaign, CampaignStatus.Planned, _campaignWarningProvider.CheckCampaign(campaign)));
		}

		public IEnumerable<CampaignSummary> ListOngoingCampaign(GanttPeriod peroid)
		{
			if (peroid == null)
			{
				return _outboundCampaignRepository.GetOnGoingCampaigns().Select(campaign =>
					assembleSummary(campaign, CampaignStatus.Ongoing, _campaignWarningProvider.CheckCampaign(campaign)));
			}

			return _outboundCampaignRepository.GetOnGoingCampaigns(getUtcPeroid(peroid)).Select(campaign =>
				assembleSummary(campaign, CampaignStatus.Ongoing, _campaignWarningProvider.CheckCampaign(campaign)));
		}

		public IEnumerable<CampaignSummary> ListDoneCampaign(GanttPeriod peroid)
		{
			if (peroid == null)
			{
				return _outboundCampaignRepository.GetDoneCampaigns().Select(campaign =>
					assembleSummary(campaign, CampaignStatus.Done, _campaignWarningProvider.CheckCampaign(campaign)));
			}

			return _outboundCampaignRepository.GetDoneCampaigns(getUtcPeroid(peroid)).Select(campaign =>
					assembleSummary(campaign, CampaignStatus.Done, _campaignWarningProvider.CheckCampaign(campaign)));
		}

		public IEnumerable<GanttCampaignViewModel> GetCampaigns(GanttPeriod period)
		{
			var campaigns = _outboundCampaignRepository.GetCampaigns(getUtcPeroid(period));

			var ganttCampaigns = new List<GanttCampaignViewModel>();
			foreach (var campaign in campaigns)
			{
				var startDateTime = TimeZoneHelper.ConvertFromUtc(campaign.SpanningPeriod.StartDateTime, campaign.Skill.TimeZone);
				var endDateTime = TimeZoneHelper.ConvertFromUtc(campaign.SpanningPeriod.EndDateTime, campaign.Skill.TimeZone);

				var startDateAsUtc = new DateOnly(DateTime.SpecifyKind(startDateTime, DateTimeKind.Utc));
				var endDateAsUtc = new DateOnly(DateTime.SpecifyKind(endDateTime, DateTimeKind.Utc));

				ganttCampaigns.Add(new GanttCampaignViewModel()
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

		private CampaignSummary assembleSummary(IOutboundCampaign campaign, CampaignStatus status, IEnumerable<CampaignWarning> warnings)
		{
			var startDateTime = TimeZoneHelper.ConvertFromUtc(campaign.SpanningPeriod.StartDateTime, campaign.Skill.TimeZone);
			var endDateTime = TimeZoneHelper.ConvertFromUtc(campaign.SpanningPeriod.EndDateTime, campaign.Skill.TimeZone);

			var startDateAsUtc = new DateOnly(DateTime.SpecifyKind(startDateTime, DateTimeKind.Utc));
			var endDateAsUtc = new DateOnly(DateTime.SpecifyKind(endDateTime, DateTimeKind.Utc));

			return new CampaignSummary
			{
				Id = campaign.Id,
				Name = campaign.Name,
				StartDate = startDateAsUtc,
				EndDate = endDateAsUtc,
				Status = status,
				WarningInfo = warnings
			};
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

		public void CheckAndUpdateCache(GanttPeriod period)
		{
			var campaigns = period == null
				? _outboundCampaignRepository.LoadAll()
				: _outboundCampaignRepository.GetCampaigns(getUtcPeroid(period));

			if (_outboundScheduledResourcesCacher.FilterNotCached(campaigns).ToList().Count > 0)
			{
				loadScheduleData(campaigns);
			}
		}
	}
}