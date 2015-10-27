using System;
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

		public void LoadData(GanttPeriod peroid)
		{
			var campaigns = peroid == null
				? _outboundCampaignRepository.LoadAll()
				: _outboundCampaignRepository.GetCampaigns(getUtcPeroid(peroid));
			if (campaigns.Count == 0) return;

			var earliestStart = campaigns.Min(c => c.SpanningPeriod.ToDateOnlyPeriod(TimeZoneInfo.Utc).StartDate);
			var latestEnd = campaigns.Max(c => c.SpanningPeriod.ToDateOnlyPeriod(TimeZoneInfo.Utc).EndDate);
			var campaignPeriod = new DateOnlyPeriod(earliestStart, latestEnd);

			_scheduledResourcesProvider.Load(campaigns, campaignPeriod);

			updateCache(campaigns);
		}

		public void ResetCache()
		{
			_outboundScheduledResourcesCacher.Reset();
		}

		public CampaignStatistics GetCampaignStatistics(GanttPeriod peroid)
		{
			Func<CampaignSummary, bool> campaignHasWarningPredicate = campaign => campaign.WarningInfo.Any();

			var plannedCampaigns = ListPlannedCampaign(peroid).ToList();
			var ongoingCampaigns = ListOngoingCampaign(peroid).ToList();
			var ongoingWarningCampaigns = ongoingCampaigns.Where(campaignHasWarningPredicate).ToList();
			var scheduledCampaigns = ListScheduledCampaign(peroid).ToList();
			var scheduledWarningCampaigns = scheduledCampaigns.Where(campaignHasWarningPredicate).ToList();
			var doneCampaigns = ListDoneCampaign(peroid).ToList();

			return new CampaignStatistics()
			{
				Planned = plannedCampaigns.Count,
				OnGoing = ongoingCampaigns.Count,
				OnGoingWarning = ongoingWarningCampaigns.Count,
				Scheduled = scheduledCampaigns.Count,
				ScheduledWarning = scheduledWarningCampaigns.Count,
				Done = doneCampaigns.Count
			};
		}

		public IEnumerable<CampaignSummary> ListCampaign(CampaignStatus status, GanttPeriod period)
		{
			if (status == CampaignStatus.None)
			{
				status = CampaignStatus.Done | CampaignStatus.Ongoing | CampaignStatus.Planned | CampaignStatus.Scheduled;
			}

			switch (status)
			{
				case CampaignStatus.Done:
					return ListDoneCampaign(period);
				case CampaignStatus.Scheduled:
					return ListScheduledCampaign(period);
				case CampaignStatus.Planned:
					return ListPlannedCampaign(period);
				case CampaignStatus.Ongoing:
					return ListOngoingCampaign(period);
				default:
					var result = new List<CampaignSummary>();
					foreach (var _status in _campaignListOrderProvider.GetCampaignListOrder().Where(_status => status.HasFlag(_status)))
					{
						result.AddRange(ListCampaign(_status, period));
					}
					return result;
			}
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

		public CampaignSummary GetCampaignById(Guid Id)
		{
			return ListCampaign(CampaignStatus.None, null).FirstOrDefault(c => c.Id == Id);
		}

		public IEnumerable<GanttCampaignViewModel> GetCampaigns(GanttPeriod period)
		{
			var campaigns = (period == null) ? _outboundCampaignRepository.LoadAll() : _outboundCampaignRepository.GetCampaigns(getUtcPeroid(period));

			var ganttCampaigns = new List<GanttCampaignViewModel>();
			foreach (var campaign in campaigns)
			{
				var startDateTime = TimeZoneHelper.ConvertFromUtc(campaign.SpanningPeriod.StartDateTime, campaign.Skill.TimeZone);
				var endDateTime = TimeZoneHelper.ConvertFromUtc(campaign.SpanningPeriod.EndDateTime, campaign.Skill.TimeZone);

				var startDateAsUtc = new DateOnly(DateTime.SpecifyKind(startDateTime, DateTimeKind.Utc));
				var endDateAsUtc = new DateOnly(DateTime.SpecifyKind(endDateTime, DateTimeKind.Utc));

				ganttCampaigns.Add(new GanttCampaignViewModel() { Id = (Guid)campaign.Id, Name = campaign.Name, StartDate = startDateAsUtc, EndDate = endDateAsUtc });
			}

			return ganttCampaigns;
		}

		private DateTimePeriod getUtcPeroid(GanttPeriod period)
		{
			return new DateOnlyPeriod(period.StartDate, period.EndDate.AddDays(1)).ToDateTimePeriod(_userTimeZone.TimeZone());
		}

		private CampaignSummary assembleSummary(IOutboundCampaign campaign, CampaignStatus status, IEnumerable<CampaignWarning> warnings)
		{
			return new CampaignSummary
			{
				Id = campaign.Id,
				Name = campaign.Name,
				StartDate = campaign.SpanningPeriod.ToDateOnlyPeriod(campaign.Skill.TimeZone).StartDate,
				EndDate = campaign.SpanningPeriod.ToDateOnlyPeriod(campaign.Skill.TimeZone).EndDate,
				Status = status,
				WarningInfo = warnings
			};
		}

		private void updateCache(IEnumerable<IOutboundCampaign> campaigns)
		{
			foreach (var campaign in campaigns)
			{
				if (_outboundScheduledResourcesCacher.GetForecastedTime(campaign) == null)
				{
					var dates = campaign.SpanningPeriod.DateCollection();
					var schedules = dates.ToDictionary(d => new DateOnly(d), d => _scheduledResourcesProvider.GetScheduledTimeOnDate(new DateOnly(d), campaign.Skill))
						.Where(kvp => kvp.Value > TimeSpan.Zero).ToDictionary(d => d.Key, d => d.Value);
					var forecasts = dates.ToDictionary(d => new DateOnly(d), d => _scheduledResourcesProvider.GetForecastedTimeOnDate(new DateOnly(d), campaign.Skill))
						.Where(kvp => kvp.Value > TimeSpan.Zero).ToDictionary(d => d.Key, d => d.Value);
					_outboundScheduledResourcesCacher.SetScheduledTime(campaign, schedules);
					_outboundScheduledResourcesCacher.SetForecastedTime(campaign, forecasts);
				}
			}
		}

		public void CheckAndUpdateCache(GanttPeriod period)
		{
			var campaigns = period == null
				? _outboundCampaignRepository.LoadAll()
				: _outboundCampaignRepository.GetCampaigns(getUtcPeroid(period));

			foreach (var campaign in campaigns)
			{
				if (_outboundScheduledResourcesCacher.GetForecastedTime(campaign) == null)
				{
					LoadData(period);
					break;
				}
			}
		}
	}
}