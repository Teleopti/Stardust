﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	public static class  ProjectionChangedEventShiftExtension
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
							//Skicka med Ready Time eller själva hämta det från Aktiviteten
							//Skicka med Paid Time eller själva hämta det från Aktiviteten eller frånvaron
							Name = layer.Name, //struntar vi i egentligen
							ShortName = layer.ShortName, //struntar vi i egentligen
							IsAbsence = layer.IsAbsence,
							//Vi behöver också IsOvertime eller tiden
							IsAbsenceConfidential = layer.IsAbsenceConfidential, //struntar vi i egentligen
							PayloadId = layer.PayloadId //activityid eller absenceid
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