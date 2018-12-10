using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public interface INonoverwritableLayerMovabilityChecker
	{
		bool HasNonoverwritableLayer(IScheduleDay scheduleDay, DateTimePeriod period, IActivity activity);
		bool HasNonoverwritableLayer(IPerson person, DateOnly belongsToDate, DateTimePeriod periodInUtc, IActivity activity);
		bool IsFixableByMovingNonoverwritableLayer(IScheduleDictionary scheduleDictionary, DateTimePeriod newPeriod, IPerson person, DateOnly date);
		bool IsFixableByMovingNonoverwritableLayer(DateTimePeriod newPeriod, IPerson person, DateOnly date);
		IList<ShiftLayer> GetNonoverwritableLayersToMove(IScheduleDay scheduleDay, DateTimePeriod newPeriod);
		IList<ShiftLayer> GetNonoverwritableLayersToMove(IPerson person, DateOnly date, DateTimePeriod newPeriod);
		bool ContainsOverlappedNonoverwritableLayers(IScheduleDictionary scheduleDictionary, IPerson person, DateOnly date);
	}
}