using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Wfm.Adherence.Domain.ApprovePeriodAsInAdherence;

namespace Teleopti.Wfm.Adherence.Domain.AgentAdherenceDay
{
	public interface IAgentAdherenceDay
	{
		DateTimePeriod Period();
		IEnumerable<HistoricalChangeModel> Changes();
		IEnumerable<AdherencePeriod> RecordedOutOfAdherences();
		IEnumerable<ApprovedPeriod> ApprovedPeriods();
		IEnumerable<AdherencePeriod> OutOfAdherences();
		int? Percentage();
	}
}