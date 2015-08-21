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
        private readonly IOutboundRuleChecker _outboundRuleChecker;
        private readonly ICampaignListOrderProvider _campaignListOrderProvider;

        public CampaignListProvider(IOutboundCampaignRepository outboundCampaignRepository, IOutboundScheduledResourcesProvider scheduledResourcesProvider, IOutboundRuleChecker outboundRuleChecker, ICampaignListOrderProvider campaignListOrderProvider)
        {
            _outboundCampaignRepository = outboundCampaignRepository;
            _scheduledResourcesProvider = scheduledResourcesProvider;
            _outboundRuleChecker = outboundRuleChecker;
            _campaignListOrderProvider = campaignListOrderProvider;
        }

	    public void LoadData()
	    {
		    var campaigns = _outboundCampaignRepository.LoadAll();
		    if (campaigns.Count == 0) return;
		    var earliestStart = campaigns.Min(c => c.SpanningPeriod.StartDate);
		    var latestEnd = campaigns.Max(c => c.SpanningPeriod.EndDate);
		    var period = new DateOnlyPeriod(earliestStart, latestEnd);

		    _scheduledResourcesProvider.Load(campaigns, period);
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
                return campaign.SpanningPeriod.DayCollection().Any(
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
                return campaign.SpanningPeriod.DayCollection().All(
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
                StartDate = campaign.SpanningPeriod.StartDate,
                EndDate = campaign.SpanningPeriod.EndDate,
                Status = status,
                WarningInfo = warnings
            };
        }

    }
}