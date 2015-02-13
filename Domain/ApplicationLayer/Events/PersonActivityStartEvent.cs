using System;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonActivityStartEvent : IEvent, ILogOnInfo, IGoToHangfire
	{
		public Guid PersonId { get; set; }
		public DateTime StartTime { get; set; }
		public string Name { get; set; }
		public string Datasource { get; set; }
		public Guid BusinessUnitId { get; set; }
		public bool InAdherence { get; set; }
		public DateTime ScheduleDate { get; set; }
		public DateTime ShiftEndTime { get; set; }
	}
}