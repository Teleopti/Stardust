using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting.OutboundSetting;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Rules;
using Teleopti.Ccc.Web.Core.Data;


namespace Teleopti.Ccc.Web.Areas.Outbound.Models
{
	public class OutboundWarningViewModel
	{
		public string TypeOfRule { get; set; }
		public double? Threshold { get; set; }
		public WarningThresholdType ThresholdType { get; set; }
		public double? TargetValue { get; set; }
		

		public OutboundWarningViewModel(CampaignWarning response)
		{
			TypeOfRule = response.WarningName;
			Threshold = response.Threshold;
			ThresholdType = response.WarningThresholdType;
			TargetValue = response.TargetValue;		
		}
	}

	public class SummaryForm
	{
		public Guid CampaignId;
		public IEnumerable<DateOnly> SkipDates;
	}

	public class CampaignSummaryViewModel
	{
		public Guid Id;
		public string Name;
		public DateOnly StartDate;
		public DateOnly EndDate;
	}

	public class CampaignStatusViewModel
	{
		public CampaignSummaryViewModel CampaignSummary;
		public bool IsScheduled;
		public IEnumerable<OutboundWarningViewModel> WarningInfo;
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

	public class VisualizationForm
	{
		public Guid CampaignId;
		public IEnumerable<DateOnly> SkipDates;
	}

	public class CampaignVisualizationViewModel
	{
		public IList<DateOnly> Dates;
		public IList<double> PlannedPersonHours;
		public IList<double> BacklogPersonHours;
		public IList<double> ScheduledPersonHours;
		public IList<double> OverstaffPersonHours;
		public IList<bool> IsManualPlanned;
		public IList<bool> IsCloseDays;
		public IList<bool> IsActualBacklog;
	}

	public class ManualViewModel
	{
		public DateOnly Date;
		public double Time;
	}

	public class ManualPlanForm
	{
		public Guid CampaignId;
		public IEnumerable<ManualViewModel> ManualProductionPlan;
		public IEnumerable<DateOnly> SkipDates;
	}

	public class ActualBacklogForm
	{
		public Guid CampaignId;
		public IEnumerable<ManualViewModel> ActualBacklog;
		public IEnumerable<DateOnly> SkipDates;
	}

	public class RemoveManualPlanForm
	{
		public Guid CampaignId;
		public IEnumerable<DateOnly> Dates;
		public IEnumerable<DateOnly> SkipDates;
	}

	public class PlanWithScheduleForm
	{
		public Guid CampaignId;
		public IEnumerable<DateOnly> SkipDates;
	}

	public class RemoveActualBacklogForm
	{
		public Guid CampaignId;
		public IEnumerable<DateOnly> Dates;
		public IEnumerable<DateOnly> SkipDates;
	}

	public class GanttPeriod
	{
		public DateOnly StartDate;
		public DateOnly EndDate;
	}

	public class ThresholdSettingForm
	{
		public double Value;
		public WarningThresholdType Type;
	}
}