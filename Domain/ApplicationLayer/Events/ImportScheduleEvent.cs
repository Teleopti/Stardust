using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class ImportScheduleEvent : EventWithLogOnContext
	{
		public DateOnly StartDate { get; set; }
		public DateOnly EndDate { get; set; }
		public Guid FromScenario { get; set; }
		public Guid ToScenario { get; set; }
		public Guid JobResultId { get; set; }
		public ICollection<Guid> PersonIds { get; }
		public int TotalMessages { get; set; }

		public ImportScheduleEvent()
		{
			PersonIds = new HashSet<Guid>();
		}

		public ImportScheduleEvent(params Guid[] personIds) : this()
		{
			foreach (var item in personIds)
				PersonIds.Add(item);
		}
	}
}