using System;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events
{
	public class PersonRuleChangedEvent : IRtaStoredEvent, IEvent
	{
		public Guid PersonId { get; set; }
		public DateOnly? BelongsToDate { get; set; }
		public DateTime Timestamp { get; set; }

		public string StateName { get; set; }
		public Guid? StateGroupId { get; set; }

		public string ActivityName { get; set; }
		public int? ActivityColor { get; set; }

		public string RuleName { get; set; }
		public int? RuleColor { get; set; }

		public double? StaffingEffect { get; set; }
		public bool IsAlarm { get; set; }
		public DateTime? AlarmStartTime { get; set; }
		public int? AlarmColor { get; set; }
		public EventAdherence? Adherence { get; set; }

		public QueryData QueryData() =>
			new QueryData
			{
				PersonId = PersonId,
				StartTime = Timestamp,
				EndTime = Timestamp
			};
	}
}