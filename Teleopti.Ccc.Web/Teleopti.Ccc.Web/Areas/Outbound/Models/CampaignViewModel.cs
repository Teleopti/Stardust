using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Outbound.Models
{
    [Flags]
    public enum CampaignStatus
    {
        None = 0,
        Planned = 1,
        Scheduled = 2,
        Ongoing = 4,
        Done = 8        
    }

    public class CampaignSummary
    {
        public Guid? Id;
        public string Name;
        public DateOnly StartDate;
        public DateOnly EndDate;
        public CampaignStatus Status;
        public IEnumerable<OutboundRuleResponse> WarningInfo;
    }

    public class OutboundWarningViewModel
    {
        public string TypeOfRule { get; set; }
        public int? Threshold { get; set; }
        public ThresholdType ThresholdType { get; set; }
        public int? TargetValue { get; set; }
        public DateOnly Date { get; set; }

        public OutboundWarningViewModel(OutboundRuleResponse response)
        {
            TypeOfRule = response.TypeOfRule.Name;
            Threshold = response.Threshold;
            ThresholdType = response.ThresholdType;
            TargetValue = response.TargetValue;
            Date = new DateOnly(response.Date);
        }
    }

    public class CampaignSummaryViewModel
    {
        public Guid? Id;
        public string Name;
        public DateOnly StartDate;
        public DateOnly EndDate;
        public CampaignStatus Status;
        public IEnumerable<OutboundWarningViewModel> WarningInfo;

        public CampaignSummaryViewModel(CampaignSummary campaign)
        {
            Id = campaign.Id;
            Name = campaign.Name;
            StartDate = campaign.StartDate;
            EndDate = campaign.EndDate;
            Status = campaign.Status;
            WarningInfo = campaign.WarningInfo.Select(w => new OutboundWarningViewModel(w));
        }
    }

    public class CampaignSummaryListViewModel
    {
        public List<CampaignSummaryViewModel> CampaignsWithWarning;
        public List<CampaignSummaryViewModel> CampaignsWithoutWarning;
    }

	public class CampaignViewModel
	{
		public Guid? Id;
		public string Name;
		public ActivityViewModel Activity;  
		public int CallListLen;
		public int TargetRate;
		public int ConnectRate;
		public int RightPartyConnectRate;
		public int ConnectAverageHandlingTime;
		public int RightPartyAverageHandlingTime;
		public int UnproductiveTime;
		public DateOnly StartDate;
		public DateOnly EndDate;
		public IEnumerable<CampaignWorkingHour> WorkingHours { get; set; }
	}

	public class CampaignStatistics
	{
		public int Planned;
		public int PlannedWarning;
		public int Scheduled;
		public int ScheduledWarning;
		public int OnGoing;
		public int OnGoingWarning;
		public int Done;
	}
}