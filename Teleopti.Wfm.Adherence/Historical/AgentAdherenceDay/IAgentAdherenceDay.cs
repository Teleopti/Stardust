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
		IEnumerable<AdherencePeriod> ApprovedPeriods();
		IEnumerable<AdherencePeriod> OutOfAdherences();
		int? Percentage();
		int? SecondsInAdherence();
		int? SecondsOutOfAdherence();
	}
}