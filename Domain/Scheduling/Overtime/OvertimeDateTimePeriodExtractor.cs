using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public interface IOvertimeDateTimePeriodExtractor
	{
		IList<IOvertimeDateTimePeriodHolder> Extract(int minimumResolution, MinMax<TimeSpan> overtimeDurantion, IVisualLayerCollection visualLayerCollection, DateTimePeriod specificPeriod);
	}

	public class OvertimeDateTimePeriodExtractor : IOvertimeDateTimePeriodExtractor
	{
		public IList<IOvertimeDateTimePeriodHolder> Extract(int minimumResolution, MinMax<TimeSpan> overtimeDurantion, IVisualLayerCollection visualLayerCollection, DateTimePeriod specificPeriod)
		{
			IList<IOvertimeDateTimePeriodHolder>  dateTimePeriodHolders = new List<IOvertimeDateTimePeriodHolder>();
			var shiftPeriod = visualLayerCollection.Period().GetValueOrDefault();
			var shiftStart = shiftPeriod.StartDateTime;
			var shiftEnd = shiftPeriod.EndDateTime;

			var intersection = specificPeriod.Intersection(shiftPeriod);
			if (intersection == null && !specificPeriod.AdjacentTo(shiftPeriod)) return dateTimePeriodHolders;

			for (var minutes = minimumResolution; minutes <= overtimeDurantion.Maximum.TotalMinutes; minutes += minimumResolution)
			{
				var duration = TimeSpan.FromMinutes(minutes);
				if (duration < overtimeDurantion.Minimum) continue;

				var periodBefore = new DateTimePeriod(shiftStart.Add(-duration), shiftStart);

				if (specificPeriod.Contains(periodBefore))
				{
					var firstLayer = visualLayerCollection.First();
					var firstLayerDefinitionSet = firstLayer.DefinitionSet;
					var firstLayerIsAbsence = ((VisualLayer) firstLayer).HighestPriorityAbsence != null;

					if (!firstLayerIsAbsence && (firstLayerDefinitionSet == null || firstLayerDefinitionSet.MultiplicatorType != MultiplicatorType.Overtime))
					{
						var dateTimePeriodHolderBefore = new OvertimeDateTimePeriodHolder();
						dateTimePeriodHolderBefore.Add(periodBefore);
						dateTimePeriodHolders.Add(dateTimePeriodHolderBefore);
					}
				}

				var periodAfter = new DateTimePeriod(shiftEnd, shiftEnd.Add(duration));
				if (specificPeriod.Contains(periodAfter))
				{
					var lastLayer = visualLayerCollection.Last();
					var lastLayerDefinitionSet = lastLayer.DefinitionSet;
					var lastLayerIsAbsence = ((VisualLayer)lastLayer).HighestPriorityAbsence != null;

					if (!lastLayerIsAbsence && (lastLayerDefinitionSet == null || lastLayerDefinitionSet.MultiplicatorType != MultiplicatorType.Overtime))
					{
						var dateTimePeriodHolderAfter = new OvertimeDateTimePeriodHolder();
						dateTimePeriodHolderAfter.Add(periodAfter);
						dateTimePeriodHolders.Add(dateTimePeriodHolderAfter);
					}
				}	
			}

			return dateTimePeriodHolders;
		}
	}
}
