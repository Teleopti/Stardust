using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// 
	/// </summary>
	public static class VisualLayerCollectionExtensions
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="resourceLayerCollection"></param>
		/// <param name="minutesSplit"></param>
		/// <returns></returns>
		public static IEnumerable<ResourceLayer> ToResourceLayers(this IVisualLayerCollection resourceLayerCollection, int minutesSplit)
		{
			if (!resourceLayerCollection.Any()) yield break;

			DateTime startTime = resourceLayerCollection.First().Period.StartDateTime;
			var rest = startTime.Minute % minutesSplit;
			if (rest != 0)
				startTime = startTime.AddMinutes(-rest);
			foreach (var layer in resourceLayerCollection)
			{
				while (startTime < layer.Period.EndDateTime)
				{
					double startDiff = 0;
					double endDiff = 0;
					if (startTime < layer.Period.StartDateTime)
						startDiff = layer.Period.StartDateTime.Subtract(startTime).TotalMinutes;
					if (startTime.AddMinutes(minutesSplit) > layer.Period.EndDateTime)
						endDiff = startTime.AddMinutes(minutesSplit).Subtract(layer.Period.EndDateTime).TotalMinutes;
					yield return
						new ResourceLayer
							{
								Resource = 1d * (1 * ((minutesSplit - startDiff - endDiff) / minutesSplit)),
								Activity = layer.Payload.UnderlyingPayload.Id.GetValueOrDefault(),
								Period = new DateTimePeriod(startTime, startTime.AddMinutes(minutesSplit))
							};
					if (startTime.AddMinutes(minutesSplit) >= layer.Period.EndDateTime)
					{
						break;
					}
					startTime = startTime.AddMinutes(minutesSplit);
				}
			}
		}

		public static void AddScheduleDayToContainer(this IResourceCalculationDataContainer resources, IScheduleDay scheduleDay, int minutesSplit)
		{
			var projection = scheduleDay.ProjectionService().CreateProjection();
			var resourceLayers = projection.ToResourceLayers(minutesSplit);
			foreach (var resourceLayer in resourceLayers)
			{
				resources.AddResources(resourceLayer.Period, resourceLayer.Activity, scheduleDay.Person, scheduleDay.DateOnlyAsPeriod.DateOnly, 1d);
			}
		}
	}

	public interface IResourceCalculationDataContainer
	{
		void Clear();
		bool HasItems();

		void AddResources(DateTimePeriod period, Guid activity, IPerson person, DateOnly personDate,
										  double resource);

		double SkillResources(ISkill skill, DateTimePeriod period);
		bool AllIsSingleSkill();
		double ActivityResources(Func<IActivity, bool> activitiesToLookFor, ISkill skill, DateTimePeriod period);
		IDictionary<string, Tuple<IEnumerable<ISkill>, double>> AffectedResources(IActivity activity, DateTimePeriod periodToCalculate);
	}
}