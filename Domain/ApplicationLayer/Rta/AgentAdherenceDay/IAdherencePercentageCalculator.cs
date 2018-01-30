using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.AgentAdherenceDay
{
	public interface IAdherencePercentageCalculator
	{
		int? CalculatePercentage(DateTime? shiftStartTime, DateTime? shiftEndTime,
			IEnumerable<HistoricalAdherenceReadModel> data);
	}
}