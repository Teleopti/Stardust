using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Wfm.Adherence.Domain.Events
{
	public class PersonAdherenceDayStartEvent : IEvent, ISolidProof
	{
		public Guid PersonId { get; set; }
		public DateTime Timestamp { get; set; }

		public string StateName { get; set; }
		public string ActivityName { get; set; }
		public int? ActivityColor { get; set; }
		public string RuleName { get; set; }
		public int? RuleColor { get; set; }
		public EventAdherence? Adherence { get; set; }

//		public DateOnly? BelongsToDate { get; set; }
//		public DateTime ShiftStartTime { get; set; }
//		public DateTime ShiftEndTime { get; set; }
	}
}