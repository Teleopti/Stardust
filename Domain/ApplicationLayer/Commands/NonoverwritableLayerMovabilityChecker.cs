using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public interface INonoverwritableLayerMovabilityChecker
	{
		bool HasNonoverwritableLayer(IScheduleDay scheduleDay, DateTimePeriod period, IActivity activity);
		bool IsFixableByMovingNonoverwritableLayer(IScheduleDictionary scheduleDictionary, DateTimePeriod newPeriod, IPerson person, DateOnly date);
		List<IShiftLayer> GetNonoverwritableLayersToMove(IScheduleDay scheduleDay, DateTimePeriod newPeriod);
		bool ContainsOverlappedNonoverwritableLayers(IScheduleDictionary scheduleDictionary, IPerson person, DateOnly date);
	}

	public class NonoverwritableLayerMovabilityChecker : INonoverwritableLayerMovabilityChecker
	{
		private readonly INonoverwritableLayerChecker _nonoverwritableLayerChecker;

		public NonoverwritableLayerMovabilityChecker(INonoverwritableLayerChecker nonoverwritableLayerChecker)
		{
			_nonoverwritableLayerChecker = nonoverwritableLayerChecker;
		}

		public bool HasNonoverwritableLayer(IScheduleDay scheduleDay, DateTimePeriod period, IActivity activity)
		{
			var person = scheduleDay.Person;
			var date = scheduleDay.DateOnlyAsPeriod.DateOnly;
			var overlappedLayers = _nonoverwritableLayerChecker.GetOverlappedLayersWhenAddingActivity(person, date, activity,
				period);
			return overlappedLayers.Any();
		}

		public bool IsFixableByMovingNonoverwritableLayer(IScheduleDictionary scheduleDictionary, DateTimePeriod newPeriod, IPerson person, DateOnly date)
		{
			var scheduleDay = scheduleDictionary[person].ScheduledDay(date);
			if (ContainsOverlappedNonoverwritableLayers(scheduleDictionary, person, date)) return false;
			var shiftLayersToMove = GetNonoverwritableLayersToMove(scheduleDay, newPeriod);
			return shiftLayersToMove.Count == 1;
		}

		public bool ContainsOverlappedNonoverwritableLayers(IScheduleDictionary scheduleDictionary, IPerson person,
			DateOnly date)
		{
			var scheduleDay = scheduleDictionary[person].ScheduledDay(date);
			var rule = new NotOverwriteLayerRule();
			var result = rule.Validate(scheduleDictionary, new List<IScheduleDay> {scheduleDay});
			return result.Any();
		}

		public List<IShiftLayer> GetNonoverwritableLayersToMove(IScheduleDay scheduleDay, DateTimePeriod newPeriod)
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
