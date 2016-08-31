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
			var projection = scheduleDay.ProjectionService().CreateProjection();

			var rules = NewBusinessRuleCollection.New();
			rules.Add(new NotOverwriteLayerRule());
			var result = rules.CheckRules(scheduleDictionary, new List<IScheduleDay> {scheduleDay});

			if (result.Any())
				return false;

			if (!projection.HasLayers) return false;
			var conflictLayers = projection.Where(l =>
			{
				var activityLayer = l.Payload as IActivity;
				return (activityLayer != null && l.Period.Intersect(newPeriod) && !activityLayer.AllowOverwrite);
			}).ToList();
			if (conflictLayers.Count > 1) return false;

			var shiftLayerIds = getMatchedMainShiftLayers(scheduleDay, conflictLayers.SingleOrDefault());

			return shiftLayerIds.Count == 1;
		}

		private IList<Guid> getMatchedMainShiftLayers(IScheduleDay scheduleDay, IVisualLayer layer)
		{
			var matchedLayerIds = new List<Guid>();
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
					matchedLayerIds.Add(shiftLayer.Id.GetValueOrDefault());
				}
			}
			return matchedLayerIds;
		}
	}
}
