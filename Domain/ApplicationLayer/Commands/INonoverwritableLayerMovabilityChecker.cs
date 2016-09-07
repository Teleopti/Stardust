using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public interface INonoverwritableLayerMovabilityChecker
	{
		bool HasNonoverwritableLayer(IScheduleDay scheduleDay, DateTimePeriod period, IActivity activity);
		bool HasNonoverwritableLayer(IPerson person, DateOnly belongsToDate, DateTimePeriod periodInUtc, IActivity activity);
		bool IsFixableByMovingNonoverwritableLayer(IScheduleDictionary scheduleDictionary, DateTimePeriod newPeriod, IPerson person, DateOnly date);
		bool IsFixableByMovingNonoverwritableLayer(DateTimePeriod newPeriod, IPerson person, DateOnly date);
		IList<IShiftLayer> GetNonoverwritableLayersToMove(IScheduleDay scheduleDay, DateTimePeriod newPeriod);
		IList<IShiftLayer> GetNonoverwritableLayersToMove(IPerson person, DateOnly date, DateTimePeriod newPeriod);
		bool ContainsOverlappedNonoverwritableLayers(IScheduleDictionary scheduleDictionary, IPerson person, DateOnly date);
	}
}