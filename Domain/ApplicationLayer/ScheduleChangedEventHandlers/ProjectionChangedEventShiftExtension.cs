using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	public static class ProjectionChangedEventShiftExtension
	{

		public static IEnumerable<ProjectionChangedEventLayer> FilterLayers(this ProjectionChangedEventShift theShift,
			DateTimePeriod periodToSearch)
		{

			var retColl = new List<ProjectionChangedEventLayer>();
			int collCount = theShift.Layers.Count();
			var unMergedCollection = theShift.Layers.ToList();
			if (collCount > 0)
			{
				DateTime endDateTimeSearch = periodToSearch.EndDateTime;
				DateTime startDateTimeSearch = periodToSearch.StartDateTime;
				IFilterOnPeriodOptimizer opt = new NextPeriodOptimizer();

				int foundIndex = collCount - 1; //if no hits
				int startIndex = 0;
				if (collCount > 1)
					startIndex = FindStartIndex(unMergedCollection, startDateTimeSearch);

				for (int index = startIndex; index < collCount; index++)
				{
					var layer = unMergedCollection[index];
					DateTime layerPeriodStartDateTime = layer.StartDateTime;
					var layerPeriod = new DateTimePeriod(layerPeriodStartDateTime, layer.EndDateTime);
					if (endDateTimeSearch <= layerPeriodStartDateTime)
					{
						foundIndex = index == 0 ? 0 : index - 1;
						break;
					}
					DateTimePeriod? intersectionPeriod = layerPeriod.Intersection(periodToSearch);
					if (intersectionPeriod.HasValue)
					{
						var newLayer = new ProjectionChangedEventLayer
						{
							DisplayColor = layer.DisplayColor,
							StartDateTime = intersectionPeriod.Value.StartDateTime,
							EndDateTime = intersectionPeriod.Value.EndDateTime,
							WorkTime = layer.WorkTime == TimeSpan.Zero ? TimeSpan.Zero: intersectionPeriod.Value.ElapsedTime(),
							ContractTime = layer.ContractTime == TimeSpan.Zero ? TimeSpan.Zero : intersectionPeriod.Value.ElapsedTime(),
							PaidTime = layer.PaidTime == TimeSpan.Zero ? TimeSpan.Zero : intersectionPeriod.Value.ElapsedTime(),
							Overtime = layer.Overtime == TimeSpan.Zero ? TimeSpan.Zero : intersectionPeriod.Value.ElapsedTime(),
							Name = layer.Name,
							ShortName = layer.ShortName,
							IsAbsence = layer.IsAbsence,
							IsAbsenceConfidential = layer.IsAbsenceConfidential,
							PayloadId = layer.PayloadId,
							MultiplicatorDefinitionSetId = layer.MultiplicatorDefinitionSetId
						};
						retColl.Add(newLayer);
						foundIndex = index;
					}
				}
				opt.FoundEndIndex(foundIndex);
			}
			return retColl;
		}

		public static int FindStartIndex(IList<ProjectionChangedEventLayer> unmergedCollection, DateTime start)
		{
			var layer = unmergedCollection.LastOrDefault(x => x.StartDateTime <= start && x.EndDateTime > start);
			if (layer == null)
				return 0;

			return unmergedCollection.IndexOf(layer);

		}
	}
}