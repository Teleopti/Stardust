using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public class HistoricalAdherenceViewModel
	{
		public Guid PersonId { get; set; }
		public string AgentName { get; set; }
		public IEnumerable<HistoricalAdherenceActivityViewModel> Schedules { get; set; }
		public IEnumerable<AgentOutOfAdherenceViewModel> OutOfAdherences { get; set; }
		public IEnumerable<HistoricalChangeViewModel> Changes { get; set; }
		public string Now { get; set; }
		public ScheduleTimeline Timeline { get; set; }
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
	}

	public class ScheduleTimeline
	{
		public string StartTime { get; set; }
		public string EndTime { get; set; }
	}
}