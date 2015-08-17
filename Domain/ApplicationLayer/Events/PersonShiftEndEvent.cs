using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonShiftEndEvent : IEvent
	{
		public Guid PersonId { get; set; }
		public DateOnly? BelongsToDate { get; set; }
		public DateTime ShiftStartTime { get; set; }
		public DateTime ShiftEndTime { get; set; }
	}
}