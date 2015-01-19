using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public interface IOvertimeDateTimePeriodExtractor
	{
		IList<IOvertimeDateTimePeriodHolder> Extract(int minimumResolution, MinMax<TimeSpan> overtimeDuration, IVisualLayerCollection visualLayerCollection, DateTimePeriod specificPeriod, IEnumerable<DateTimePeriod> openHoursList);
	}

	public class OvertimeDateTimePeriodExtractor : IOvertimeDateTimePeriodExtractor
	{
		public IList<IOvertimeDateTimePeriodHolder> Extract(int minimumResolution, MinMax<TimeSpan> overtimeDuration, IVisualLayerCollection visualLayerCollection, DateTimePeriod specificPeriod, IEnumerable<DateTimePeriod> openHoursList)
		{
			IList<IOvertimeDateTimePeriodHolder>  dateTimePeriodHolders = new List<IOvertimeDateTimePeriodHolder>();
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
				var openPeriodBefore = openOvertimePeriod(periodBefore, openHoursList);

				if (openPeriodBefore.HasValue)
				{
					if(openPeriodBefore.Value.ElapsedTime() >= overtimeDuration.Minimum)
					{
						if (specificPeriod.Contains(openPeriodBefore.Value))
						{
							var firstLayer = visualLayerCollection.First();
							var firstLayerDefinitionSet = firstLayer.DefinitionSet;
							var firstLayerIsAbsence = ((VisualLayer) firstLayer).HighestPriorityAbsence != null;

							if (!firstLayerIsAbsence &&
							    (firstLayerDefinitionSet == null || firstLayerDefinitionSet.MultiplicatorType != MultiplicatorType.Overtime))
							{
								var dateTimePeriodHolderBefore = new OvertimeDateTimePeriodHolder();
								dateTimePeriodHolderBefore.Add(periodBefore);
								dateTimePeriodHolders.Add(dateTimePeriodHolderBefore);
							}
						}
					}
				}

				var periodAfter = new DateTimePeriod(shiftEnd, shiftEnd.Add(duration));
				var openPeriodAfter = openOvertimePeriod(periodAfter, openHoursList);

				if (openPeriodAfter.HasValue)
				{
					if (openPeriodAfter.Value.ElapsedTime() >= overtimeDuration.Minimum)
					{
						if (specificPeriod.Contains(openPeriodAfter.Value))
						{
							var lastLayer = visualLayerCollection.Last();
							var lastLayerDefinitionSet = lastLayer.DefinitionSet;
							var lastLayerIsAbsence = ((VisualLayer) lastLayer).HighestPriorityAbsence != null;

							if (!lastLayerIsAbsence &&
							    (lastLayerDefinitionSet == null || lastLayerDefinitionSet.MultiplicatorType != MultiplicatorType.Overtime))
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



		private DateTimePeriod? openOvertimePeriod(DateTimePeriod overtimePeriod, IEnumerable<DateTimePeriod> openHoursList)
		{
			if (openHoursList == null || !openHoursList.Any())
				return null;
			foreach (var period in openHoursList)
			{
				var intersection = overtimePeriod.Intersection(period);
				if (intersection.HasValue)
					return intersection; // enough to return the first found period
			}
			return null;
		}
	}
}
