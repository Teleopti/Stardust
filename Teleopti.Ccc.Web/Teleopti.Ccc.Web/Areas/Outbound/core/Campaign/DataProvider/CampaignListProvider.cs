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

		public CampaignListProvider(IOutboundCampaignRepository outboundCampaignRepository, IOutboundScheduledResourcesProvider scheduledResourcesProvider, 
			ICampaignWarningProvider campaignWarningProvider, ICampaignListOrderProvider campaignListOrderProvider, IUserTimeZone userTimeZone)
		{
			_outboundCampaignRepository = outboundCampaignRepository;
			_scheduledResourcesProvider = scheduledResourcesProvider;
			_campaignWarningProvider = campaignWarningProvider;
			_campaignListOrderProvider = campaignListOrderProvider;
			_userTimeZone = userTimeZone;
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
			var isScheduled = campaign.SpanningPeriod.ToDateOnlyPeriod(TimeZoneInfo.Utc).DayCollection().Any(
				date => _scheduledResourcesProvider.GetScheduledTimeOnDate(date, campaign.Skill) > TimeSpan.Zero);

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
				return campaign.SpanningPeriod.ToDateOnlyPeriod(TimeZoneInfo.Utc).DayCollection().Any(
					date =>
						_scheduledResourcesProvider.GetScheduledTimeOnDate(date, campaign.Skill) > TimeSpan.Zero);
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
				return campaign.SpanningPeriod.ToDateOnlyPeriod(TimeZoneInfo.Utc).DayCollection().All(
					date =>
						_scheduledResourcesProvider.GetScheduledTimeOnDate(date, campaign.Skill) == TimeSpan.Zero);
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
				var startDate = TimeZoneHelper.ConvertFromUtc(campaign.SpanningPeriod.StartDateTime, _userTimeZone.TimeZone());
				var endDate = TimeZoneHelper.ConvertFromUtc(campaign.SpanningPeriod.EndDateTime, _userTimeZone.TimeZone());
				ganttCampaigns.Add(new GanttCampaignViewModel(){Id = (Guid) campaign.Id, Name = campaign.Name, StartDate = new DateOnly(startDate), EndDate = new DateOnly(endDate)});
			}

			return ganttCampaigns;
		}

		private DateTimePeriod getUtcPeroid(GanttPeriod period)
		{
			var start = new DateTime(period.StartDate.Date.Ticks);
			start = TimeZoneHelper.ConvertToUtc(start, _userTimeZone.TimeZone());
			var end = new DateTime(period.EndDate.Year, period.EndDate.Month, period.EndDate.Day, 23, 59, 59);
			end = TimeZoneHelper.ConvertToUtc(end, _userTimeZone.TimeZone());

			return new DateTimePeriod(start, end);
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
	}
}