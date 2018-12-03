using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel
{
	public static class ProjectionChangedEventLayerExtensions
	{
		public static IEnumerable<ResourceLayer> ToResourceLayers(this IEnumerable<ProjectionChangedEventLayer> resourceLayerCollection,
		                                                          int minutesSplit)
		{
			if (!resourceLayerCollection.Any()) yield break;
		    if (minutesSplit < 1) minutesSplit = 15;

			DateTime startTime = resourceLayerCollection.First().StartDateTime;
			var rest = startTime.Minute%minutesSplit;
			if (rest != 0)
				startTime = startTime.AddMinutes(-rest);
			foreach (var layer in resourceLayerCollection)
			{
				while (startTime < layer.EndDateTime)
				{
					double startDiff = 0;
					double endDiff = 0;
					if (startTime < layer.StartDateTime)
						startDiff = layer.StartDateTime.Subtract(startTime).TotalMinutes;
					if (startTime.AddMinutes(minutesSplit) > layer.EndDateTime)
						endDiff = startTime.AddMinutes(minutesSplit).Subtract(layer.EndDateTime).TotalMinutes;

					var resources = (minutesSplit - startDiff - endDiff)/minutesSplit;
					if (resources > 0d)
					{
						yield return
							new ResourceLayer
								{
									Resource = resources,
									PayloadId = layer.PayloadId,
									RequiresSeat = layer.RequiresSeat,
									Period = new DateTimePeriod(startTime, startTime.AddMinutes(minutesSplit))
								};
					}
					if (startTime.AddMinutes(minutesSplit) >= layer.EndDateTime)
					{
						break;
					}
					startTime = startTime.AddMinutes(minutesSplit);
				}
			}
		}
	}
}