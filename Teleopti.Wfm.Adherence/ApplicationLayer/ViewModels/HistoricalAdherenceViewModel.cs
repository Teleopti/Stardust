using System;
using System.Collections.Generic;

namespace Teleopti.Wfm.Adherence.ApplicationLayer.ViewModels
{
	public class HistoricalAdherenceViewModel
	{
		public Guid PersonId { get; set; }
		public string AgentName { get; set; }
		public IEnumerable<HistoricalAdherenceActivityViewModel> Schedules { get; set; }
		public IEnumerable<OutOfAdherenceViewModel> OutOfAdherences { get; set; }
		public IEnumerable<OutOfAdherenceViewModel> RecordedOutOfAdherences { get; set; }
		public IEnumerable<ApprovedPeriodViewModel> ApprovedPeriods { get; set; }
		public IEnumerable<HistoricalChangeViewModel> Changes { get; set; }
		public string Now { get; set; }
		public ScheduleTimeline Timeline { get; set; }
		public int? AdherencePercentage { get; set; }
		public HistoricalAdherenceNavigationViewModel Navigation { get; set; }
	}

	public class OutOfAdherenceViewModel
	{
		public string StartTime { get; set; }
		public string EndTime { get; set; }
	}

	public class ApprovedPeriodViewModel
	{
		public string StartTime { get; set; }
		public string EndTime { get; set; }
	}

	public class HistoricalAdherenceNavigationViewModel
	{
		public string First { get; set; }
		public string Last { get; set; }
	}

	public class HistoricalAdherenceActivityViewModel
	{
		public string Color { get; set; }
		public string StartTime { get; set; }
		public string EndTime { get; set; }
		public string Name { get; set; }
	}

	public class HistoricalChangeViewModel
	{
		public string Time { get; set; }
		public string Activity { get; set; }
		public string ActivityColor { get; set; }
		public string State { get; set; }
		public string Rule { get; set; }
		public string RuleColor { get; set; }
		public string Adherence { get; set; }
		public string AdherenceColor { get; set; }
		public int? LateForWorkMinutes { get; set; }
		public string Duration { get; set; }
	}

	public class ScheduleTimeline
	{
		public string StartTime { get; set; }
		public string EndTime { get; set; }
	}
}