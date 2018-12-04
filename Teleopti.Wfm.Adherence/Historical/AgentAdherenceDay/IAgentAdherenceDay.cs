using System.Collections.Generic;
using Teleopti.Wfm.Adherence.Historical.ApprovePeriodAsInAdherence;

namespace Teleopti.Wfm.Adherence.Historical.AgentAdherenceDay
{
	// change type of ApprovedPeriods() to AdherencePeriod when removing toggle RTA_ReviewHistoricalAdherence_74770
	public interface IAgentAdherenceDay
	{
		DateTimePeriod DisplayPeriod();
		IEnumerable<HistoricalChangeModel> Changes();
		IEnumerable<AdherencePeriod> RecordedOutOfAdherences();
		IEnumerable<ApprovedPeriod> ApprovedPeriods();
		IEnumerable<AdherencePeriod> OutOfAdherences();
		int? Percentage();
		int? SecondsInAdherence();
		int? SecondsOutOfAdherence();
	}
}