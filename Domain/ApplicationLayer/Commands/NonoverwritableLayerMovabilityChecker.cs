using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class NonoverwritableLayerMovabilityChecker : INonoverwritableLayerMovabilityChecker
	{
		private readonly INonoverwritableLayerChecker _nonoverwritableLayerChecker;
		private readonly IScheduleDayProvider _scheduleDayProvider;
		private readonly IScheduleProjectionHelper _projectionHelper;

		public NonoverwritableLayerMovabilityChecker(INonoverwritableLayerChecker nonoverwritableLayerChecker, IScheduleDayProvider scheduleDayProvider, IScheduleProjectionHelper projectionHelper)
		{
			_nonoverwritableLayerChecker = nonoverwritableLayerChecker;
			_scheduleDayProvider = scheduleDayProvider;
			_projectionHelper = projectionHelper;
		}

		public bool HasNonoverwritableLayer(IScheduleDay scheduleDay, DateTimePeriod period, IActivity activity)
		{
			var person = scheduleDay.Person;
			var date = scheduleDay.DateOnlyAsPeriod.DateOnly;
			var overlappedLayers = _nonoverwritableLayerChecker.GetOverlappedLayersWhenAddingActivity(person, date, activity,
				period);
			return overlappedLayers.Any();
		}
		public bool HasNonoverwritableLayer(IPerson person, DateOnly belongsToDate, DateTimePeriod periodInUtc, IActivity activity)
		{
			var scheduleDay = _scheduleDayProvider.GetScheduleDay(belongsToDate, person);
			return HasNonoverwritableLayer(scheduleDay, periodInUtc, activity);
		}
		
		public bool IsFixableByMovingNonoverwritableLayer(IScheduleDictionary scheduleDictionary, DateTimePeriod newPeriod, IPerson person, DateOnly date)
		{
			var scheduleDay = scheduleDictionary[person].ScheduledDay(date);
			if (ContainsOverlappedNonoverwritableLayers(scheduleDictionary, person, date)) return false;
			var shiftLayersToMove = GetNonoverwritableLayersToMove(scheduleDay, newPeriod);
			return shiftLayersToMove.Count == 1;
		}

		public bool IsFixableByMovingNonoverwritableLayer(DateTimePeriod newPeriod, IPerson person, DateOnly date)
		{
			var dic = _scheduleDayProvider.GetScheduleDictionary(date, person);
			return IsFixableByMovingNonoverwritableLayer(dic, newPeriod, person, date);
		}

		public bool ContainsOverlappedNonoverwritableLayers(IScheduleDictionary scheduleDictionary, IPerson person,
			DateOnly date)
		{
			var scheduleDay = scheduleDictionary[person].ScheduledDay(date);
			var rule = new NotOverwriteLayerRule();
			var result = rule.Validate(scheduleDictionary, new List<IScheduleDay> {scheduleDay});
			return result.Any();
		}	

		public IList<ShiftLayer> GetNonoverwritableLayersToMove(IScheduleDay scheduleDay, DateTimePeriod newPeriod)
		{
			var projection = scheduleDay.ProjectionService().CreateProjection();
			if (!projection.HasLayers) return new List<ShiftLayer>();

			var conflictVisualLayers = projection.Where(l =>
			{
				return l.Payload is IActivity activityLayer && l.Period.Intersect(newPeriod) && !activityLayer.AllowOverwrite;
			}).ToList();

			return conflictVisualLayers.SelectMany(l => _projectionHelper.GetMatchedMainShiftLayers(scheduleDay, l)).ToList();
		}

		public IList<ShiftLayer> GetNonoverwritableLayersToMove(IPerson person, DateOnly date, DateTimePeriod newPeriod)
		{
			var scheduleDay = _scheduleDayProvider.GetScheduleDay(date, person);
			return GetNonoverwritableLayersToMove(scheduleDay, newPeriod);
		}
	}
}
