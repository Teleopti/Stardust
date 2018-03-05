using System.Collections.Generic;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.ApprovePeriodAsInAdherence;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay
{
	public interface IAgentAdherenceDay
	{
		DateTimePeriod Period();
		IEnumerable<HistoricalChangeModel> Changes();
		IEnumerable<OutOfAdherencePeriod> RecordedOutOfAdherences();
		IEnumerable<ApprovedPeriod> ApprovedPeriods();
		IEnumerable<OutOfAdherencePeriod> OutOfAdherences();
		int? Percentage();
	}
}