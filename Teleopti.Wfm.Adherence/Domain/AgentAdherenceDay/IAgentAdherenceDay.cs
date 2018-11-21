using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Wfm.Adherence.Domain.ApprovePeriodAsInAdherence;

namespace Teleopti.Wfm.Adherence.Domain.AgentAdherenceDay
{
	public interface IAgentAdherenceDay
	{
		DateTimePeriod DisplayPeriod();
		IEnumerable<HistoricalChangeModel> Changes();
		IEnumerable<AdherencePeriod> RecordedOutOfAdherences();
		IEnumerable<ApprovedPeriod> ApprovedPeriods(); // change type to something without person id. AdherencePeriod maybe?
		IEnumerable<AdherencePeriod> OutOfAdherences();
		int? Percentage();
		int? SecondsInAdherence();
		int? SecondsOutOfAdherence();
	}
}