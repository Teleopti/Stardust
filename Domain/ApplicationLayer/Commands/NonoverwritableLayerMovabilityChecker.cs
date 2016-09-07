using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class NonoverwritableLayerMovabilityChecker : INonoverwritableLayerMovabilityChecker
	{
		private readonly INonoverwritableLayerChecker _nonoverwritableLayerChecker;
		private readonly IScheduleDayProvider _scheduleDayProvider;

		public NonoverwritableLayerMovabilityChecker(INonoverwritableLayerChecker nonoverwritableLayerChecker, IScheduleDayProvider scheduleDayProvider)
		{
			_nonoverwritableLayerChecker = nonoverwritableLayerChecker;
			_scheduleDayProvider = scheduleDayProvider;
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

		public IList<IShiftLayer> GetNonoverwritableLayersToMove(IScheduleDay scheduleDay, DateTimePeriod newPeriod)
		{
			var projection = scheduleDay.ProjectionService().CreateProjection();
			if (!projection.HasLayers) return new List<IShiftLayer>(); ;

			var conflictVisualLayers = projection.Where(l =>
			{
				var activityLayer = l.Payload as IActivity;
				return (activityLayer != null && l.Period.Intersect(newPeriod) && !activityLayer.AllowOverwrite);
			}).ToList();

			return conflictVisualLayers.SelectMany(l => getMatchedMainShiftLayers(scheduleDay, l)).ToList();
		}

		public IList<IShiftLayer> GetNonoverwritableLayersToMove(IPerson person, DateOnly date, DateTimePeriod newPeriod)
		{
			var scheduleDay = _scheduleDayProvider.GetScheduleDay(date, person);
			return GetNonoverwritableLayersToMove(scheduleDay, newPeriod);
		}

		private IList<IShiftLayer> getMatchedMainShiftLayers(IScheduleDay scheduleDay, IVisualLayer layer)
		{
			var matchedLayers = new List<IShiftLayer>();
			var personAssignment = scheduleDay.PersonAssignment();
			var shiftLayersList = new List<IShiftLayer>();
			if (personAssignment != null && personAssignment.ShiftLayers.Any())
			{
				shiftLayersList = personAssignment.ShiftLayers.ToList();
			}
			foreach (var shiftLayer in shiftLayersList)
			{
				if (layer.Payload.Id.GetValueOrDefault() == shiftLayer.Payload.Id.GetValueOrDefault() && (layer.Period.Contains(shiftLayer.Period) || shiftLayer.Period.Contains(layer.Period)))
				{
					matchedLayers.Add(shiftLayer);
				}
			}
			return matchedLayers;
		}
	}
}
