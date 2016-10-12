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
		public string Now { get; set; }
	}

	public class HistoricalAdherenceActivityViewModel
	{
		public string Color { get; set; }
		public string StartTime { get; set; }
		public string EndTime { get; set; }
	}

}