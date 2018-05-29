using System;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events
{
	public interface ISolidProof
	{
		DateTime Timestamp { get; set; }
		string StateName { get; set; }
		string ActivityName { get; set; }
		int? ActivityColor { get; set; }
		string RuleName { get; set; }
		int? RuleColor { get; set; }
		EventAdherence? Adherence { get; set; }
	}

	public class PersonStateChangedEvent : IRtaStoredEvent, IEvent, ISolidProof
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

		public EventAdherence? Adherence { get; set; }

		public QueryData QueryData() =>
			new QueryData
			{
				PersonId = PersonId,
				StartTime = Timestamp,
				EndTime = Timestamp
			};
	}

	public class PersonInAdherenceAfterLateForWorkEvent : IRtaStoredEvent, IEvent, ISolidProof
	{
		public Guid PersonId { get; set; }

		public DateTime Timestamp { get; set; }

		public DateTime ShiftStart { get; set; }

		public string StateName { get; set; }
		public string ActivityName { get; set; }
		public int? ActivityColor { get; set; }
		public string RuleName { get; set; }
		public int? RuleColor { get; set; }
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