using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public interface IOvertimeDateTimePeriodExtractor
	{
		IList<IOvertimeDateTimePeriodHolder> Extract(int minimumResolution, MinMax<TimeSpan> overtimeDuration,
			IVisualLayerCollection visualLayerCollection, DateTimePeriod specificPeriod,
			IList<IOvertimeSkillIntervalData> skillIntervalDataList);
	}

	public class OvertimeDateTimePeriodExtractor : IOvertimeDateTimePeriodExtractor
	{
		public IList<IOvertimeDateTimePeriodHolder> Extract(int minimumResolution, MinMax<TimeSpan> overtimeDuration,
			IVisualLayerCollection visualLayerCollection, DateTimePeriod specificPeriod,
			IList<IOvertimeSkillIntervalData> skillIntervalDataList)
		{
			IList<IOvertimeDateTimePeriodHolder> dateTimePeriodHolders = new List<IOvertimeDateTimePeriodHolder>();
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
				//var openPeriodBefore = openOvertimePeriod(periodBefore, openHoursList);

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
							if (checkIfInsideOpenHour(periodBefore, skillIntervalDataList))
							{
								var dateTimePeriodHolderBefore = new OvertimeDateTimePeriodHolder();
								dateTimePeriodHolderBefore.Add(periodBefore);
								dateTimePeriodHolders.Add(dateTimePeriodHolderBefore);
							}
						}
					}
				}

				var periodAfter = new DateTimePeriod(shiftEnd, shiftEnd.Add(duration));
				//var openPeriodAfter = openOvertimePeriod(periodAfter, openHoursList);

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
							if (checkIfInsideOpenHour(periodAfter, skillIntervalDataList))
							{
								var dateTimePeriodHolderAfter = new OvertimeDateTimePeriodHolder();
								dateTimePeriodHolderAfter.Add(periodAfter);
								dateTimePeriodHolders.Add(dateTimePeriodHolderAfter);
							}
						}
					}
				}
			}

			return dateTimePeriodHolders;
		}

		private bool checkIfInsideOpenHour(DateTimePeriod period, IList<IOvertimeSkillIntervalData> skillIntervalDataList)
		{
			var startOfPeriodToCheck = period.StartDateTime;
			while (startOfPeriodToCheck < period.EndDateTime)
			{
				var found = false;
				foreach (var overtimeSkillIntervalData in skillIntervalDataList)
				{
					if (overtimeSkillIntervalData.Period.Contains(startOfPeriodToCheck))
					{
						found = true;
						break;
					}
				}
				startOfPeriodToCheck = startOfPeriodToCheck.AddMinutes(5);

				if (!found)
					return false;
			}

			return true;
		}
	}
}
