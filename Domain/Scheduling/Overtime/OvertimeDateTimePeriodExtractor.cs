using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public interface IOvertimeDateTimePeriodExtractor
	{
		IEnumerable<DateTimePeriod> Extract(int minimumResolution, MinMax<TimeSpan> overtimeDuration,
			IVisualLayerCollection visualLayerCollection, DateTimePeriod specificPeriod,
			IList<IOvertimeSkillIntervalData> skillIntervalDataList);
	}

	public class OvertimeDateTimePeriodExtractor : IOvertimeDateTimePeriodExtractor
	{
		public IEnumerable<DateTimePeriod> Extract(int minimumResolution, MinMax<TimeSpan> overtimeDuration,
			IVisualLayerCollection visualLayerCollection, DateTimePeriod specificPeriod,
			IList<IOvertimeSkillIntervalData> skillIntervalDataList)
		{
			IList<DateTimePeriod> dateTimePeriodHolders = new List<DateTimePeriod>();
			var shiftPeriod = visualLayerCollection.Period().GetValueOrDefault();
			var shiftStart = shiftPeriod.StartDateTime;
			var shiftEnd = shiftPeriod.EndDateTime;

			var intersection = specificPeriod.Intersection(shiftPeriod);
			if (intersection == null && !specificPeriod.AdjacentTo(shiftPeriod)) return dateTimePeriodHolders;

			for (var minutes = minimumResolution; minutes <= overtimeDuration.Maximum.TotalMinutes; minutes += minimumResolution)
			{
				var duration = TimeSpan.FromMinutes(minutes);
				if (duration < overtimeDuration.Minimum) continue;

				var periodBefore = new DateTimePeriod(shiftStart.Add(-duration), shiftStart);

				if (periodBefore.ElapsedTime() >= overtimeDuration.Minimum)
				{
					if (specificPeriod.Contains(periodBefore))
					{
						var firstLayer = visualLayerCollection.First();
						var firstLayerDefinitionSet = firstLayer.DefinitionSet;
						var firstLayerIsAbsence = ((VisualLayer) firstLayer).HighestPriorityAbsence != null;

						if (!firstLayerIsAbsence &&
						    (firstLayerDefinitionSet == null || firstLayerDefinitionSet.MultiplicatorType != MultiplicatorType.Overtime))
						{
							if (checkIfEndOrStartInsideOpenHours(periodBefore, skillIntervalDataList))
							{
								dateTimePeriodHolders.Add(periodBefore);
							}
						}
					}
				}

				var periodAfter = new DateTimePeriod(shiftEnd, shiftEnd.Add(duration));

				if (periodAfter.ElapsedTime() >= overtimeDuration.Minimum)
				{
					if (specificPeriod.Contains(periodAfter))
					{
						var lastLayer = visualLayerCollection.Last();
						var lastLayerDefinitionSet = lastLayer.DefinitionSet;
						var lastLayerIsAbsence = ((VisualLayer) lastLayer).HighestPriorityAbsence != null;

						if (!lastLayerIsAbsence &&
						    (lastLayerDefinitionSet == null || lastLayerDefinitionSet.MultiplicatorType != MultiplicatorType.Overtime))
						{
							if (checkIfEndOrStartInsideOpenHours(periodAfter, skillIntervalDataList))
							{
								dateTimePeriodHolders.Add(periodAfter);
							}
						}
					}
				}
			}

			return dateTimePeriodHolders;
		}

		private bool checkIfEndOrStartInsideOpenHours(DateTimePeriod period, IList<IOvertimeSkillIntervalData> skillIntervalDataList)
		{
			if (skillIntervalDataList.Any(x => x.Period.Contains(period.StartDateTime) || x.Period.Contains(period.EndDateTime)))
				return true;

			return false;
		}
	}
}
