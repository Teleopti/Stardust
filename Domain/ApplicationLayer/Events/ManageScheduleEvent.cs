using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class ImportScheduleEvent : ManageScheduleBaseEvent
	{
		public ImportScheduleEvent()
		{
			
		}
		public ImportScheduleEvent(params Guid[] personIds) : base(personIds)
		{
		}
	}

	public class CopyScheduleEvent : ManageScheduleBaseEvent
	{
		public CopyScheduleEvent()
		{

		}
		public CopyScheduleEvent(params Guid[] personIds) : base(personIds)
		{
		}
	}


	public abstract class ManageScheduleBaseEvent : EventWithLogOnContext
	{
		public DateOnly StartDate { get; set; }
		public DateOnly EndDate { get; set; }
		public Guid FromScenario { get; set; }
		public Guid ToScenario { get; set; }
		public Guid JobResultId { get; set; }
		public ICollection<Guid> PersonIds { get; }
		public int TotalMessages { get; set; }

		protected ManageScheduleBaseEvent()
		{
			PersonIds = new HashSet<Guid>();
		}

		protected ManageScheduleBaseEvent(params Guid[] personIds) : this()
		{
			foreach (var item in personIds)
				PersonIds.Add(item);
		}
	}
}