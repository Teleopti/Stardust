using System.Collections.Generic;
using Teleopti.Wfm.Adherence.Historical.Events;
using Teleopti.Wfm.Adherence.States.Events;

namespace Teleopti.Wfm.Adherence.Historical.AgentAdherenceDay
{
	public interface IAgentAdherenceDay
	{
		DateTimePeriod DisplayPeriod();
		IEnumerable<HistoricalChangeModel> Changes();
		IEnumerable<AdherencePeriod> RecordedOutOfAdherences();
		IEnumerable<AdherencePeriod> RecordedNeutralAdherences();
		IEnumerable<AdherencePeriod> ApprovedPeriods();
		IEnumerable<AdherencePeriod> OutOfAdherences();
		IEnumerable<AdherencePeriod> NeutralAdherences();
		IEnumerable<AdherencePeriod> AdjustedPeriods();
		int? Percentage();
		int? SecondsInAdherence();
		int? SecondsOutOfAdherence();
	}
}