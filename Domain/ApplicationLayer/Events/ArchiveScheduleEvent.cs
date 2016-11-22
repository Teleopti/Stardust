using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class ArchiveScheduleEvent : EventWithLogOnContext
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public Guid FromScenario { get; set; }
		public Guid ToScenario { get; set; }
		public Guid JobResultId { get; set; }
		public ICollection<Guid> PersonIds { get; }
		public int TotalMessages { get; set; }

		public ArchiveScheduleEvent()
		{
			PersonIds = new HashSet<Guid>();
		}

		public ArchiveScheduleEvent(params Guid[] personIds) : this()
		{
			foreach(var item in personIds)
				PersonIds.Add(item);
		}
	}
}