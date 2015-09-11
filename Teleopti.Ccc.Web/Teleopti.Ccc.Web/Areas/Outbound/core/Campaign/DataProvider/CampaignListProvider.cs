using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.Outbound.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{
	public class CampaignListProvider : ICampaignListProvider
	{
		private readonly IOutboundCampaignRepository _outboundCampaignRepository;
		private readonly IOutboundScheduledResourcesProvider _scheduledResourcesProvider;
		private readonly IOutboundRuleChecker _outboundRuleChecker;
		private readonly ICampaignListOrderProvider _campaignListOrderProvider;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IToggleManager _toggleManager;

		public CampaignListProvider(IOutboundCampaignRepository outboundCampaignRepository, IOutboundScheduledResourcesProvider scheduledResourcesProvider, 
			IOutboundRuleChecker outboundRuleChecker, ICampaignListOrderProvider campaignListOrderProvider, IUserTimeZone userTimeZone, IToggleManager toggleManager)
		{
			_outboundCampaignRepository = outboundCampaignRepository;
			_scheduledResourcesProvider = scheduledResourcesProvider;
			_outboundRuleChecker = outboundRuleChecker;
			_campaignListOrderProvider = campaignListOrderProvider;
			_userTimeZone = userTimeZone;
			_toggleManager = toggleManager;
		}

		public void LoadData(GanttPeriod peroid)
		{
			var campaigns = !_toggleManager.IsEnabled(Toggles.Wfm_Outbound_Campaign_GanttChart_34259) ? _outboundCampaignRepository.LoadAll() 
																																	: _outboundCampaignRepository.GetCampaigns(getUtcPeroid(peroid));
			if (campaigns.Count == 0) return;

			var earliestStart = campaigns.Min(c => c.SpanningPeriod.ToDateOnlyPeriod(TimeZoneInfo.Utc).StartDate);
			var latestEnd = campaigns.Max(c => c.SpanningPeriod.ToDateOnlyPeriod(TimeZoneInfo.Utc).EndDate);
			var campaignPeriod = new DateOnlyPeriod(earliestStart, latestEnd);

			_scheduledResourcesProvider.Load(campaigns, campaignPeriod);
		}

		public CampaignStatistics GetCampaignStatistics()
		{
			Func<CampaignSummary, bool> campaignHasWarningPredicate = campaign => campaign.WarningInfo.Any();

			var plannedCampaigns = ListPlannedCampaign().ToList();
			var ongoingCampaigns = ListOngoingCampaign().ToList();
			var ongoingWarningCampaigns = ongoingCampaigns.Where(campaignHasWarningPredicate).ToList();
			var scheduledCampaigns = ListScheduledCampaign().ToList();
			var scheduledWarningCampaigns = scheduledCampaigns.Where(campaignHasWarningPredicate).ToList();
			var doneCampaigns = ListDoneCampaign().ToList();

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

		public IEnumerable<CampaignSummary> ListCampaign(CampaignStatus status)
		{
			if (status == CampaignStatus.None)
			{
				status = CampaignStatus.Done | CampaignStatus.Ongoing | CampaignStatus.Planned | CampaignStatus.Scheduled;
			}

			switch (status)
			{
				case CampaignStatus.Done:
					return ListDoneCampaign();
				case CampaignStatus.Scheduled:
					return ListScheduledCampaign();
				case CampaignStatus.Planned:
					return ListPlannedCampaign();
				case CampaignStatus.Ongoing:
					return ListOngoingCampaign();
				default:
					var result = new List<CampaignSummary>();
					foreach (var _status in _campaignListOrderProvider.GetCampaignListOrder().Where(_status => status.HasFlag(_status)))
					{
						result.AddRange(ListCampaign(_status));
					}
					return result;
			}
		}

		public IEnumerable<CampaignSummary> ListScheduledCampaign()
		{
			var campaigns = _outboundCampaignRepository.GetPlannedCampaigns();

			Func<IOutboundCampaign, bool> campaignHasSchedulePredicate = campaign =>
			{
				return campaign.SpanningPeriod.ToDateOnlyPeriod(TimeZoneInfo.Utc).DayCollection().Any(
					date =>
						_scheduledResourcesProvider.GetScheduledTimeOnDate(date, campaign.Skill) > TimeSpan.Zero);
			};

			return campaigns.Where(campaignHasSchedulePredicate).Select(campaign =>
				assembleSummary(campaign, CampaignStatus.Scheduled, _outboundRuleChecker.CheckCampaign(campaign)));
		}

		public IEnumerable<CampaignSummary> ListPlannedCampaign()
		{
			var campaigns = _outboundCampaignRepository.GetPlannedCampaigns();

			Func<IOutboundCampaign, bool> campaignNoSchedulePredicate = campaign =>
			{
				return campaign.SpanningPeriod.ToDateOnlyPeriod(TimeZoneInfo.Utc).DayCollection().All(
					date =>
						_scheduledResourcesProvider.GetScheduledTimeOnDate(date, campaign.Skill) == TimeSpan.Zero);
			};

			return campaigns.Where(campaignNoSchedulePredicate).Select(campaign =>
				assembleSummary(campaign, CampaignStatus.Planned, _outboundRuleChecker.CheckCampaign(campaign)));
		}


		public IEnumerable<CampaignSummary> ListOngoingCampaign()
		{
			return _outboundCampaignRepository.GetOnGoingCampaigns().Select(campaign =>
				assembleSummary(campaign, CampaignStatus.Ongoing, _outboundRuleChecker.CheckCampaign(campaign)));
		}

		public IEnumerable<CampaignSummary> ListDoneCampaign()
		{
			return _outboundCampaignRepository.GetDoneCampaigns().Select(campaign =>
				assembleSummary(campaign, CampaignStatus.Done, _outboundRuleChecker.CheckCampaign(campaign)));
		}

		public CampaignSummary GetCampaignById(Guid Id)
		{
			return ListCampaign(CampaignStatus.None).FirstOrDefault(c => c.Id == Id);
		}

		public IEnumerable<GanttCampaignViewModel> GetCampaigns(GanttPeriod period)
		{
			var campaigns = _outboundCampaignRepository.GetCampaigns(getUtcPeroid(period));

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

		private CampaignSummary assembleSummary(IOutboundCampaign campaign, CampaignStatus status)
		{
			return assembleSummary(campaign, status, new List<OutboundRuleResponse>());
		}


		private CampaignSummary assembleSummary(IOutboundCampaign campaign, CampaignStatus status, IEnumerable<OutboundRuleResponse> warnings)
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