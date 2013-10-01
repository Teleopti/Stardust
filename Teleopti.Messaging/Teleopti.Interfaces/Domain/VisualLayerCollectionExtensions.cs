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

					var payload = layer.Payload.UnderlyingPayload;
					var requiresSeat = false;
					var activity = payload as IActivity;
					if (activity != null)
					{
						requiresSeat = activity.RequiresSeat;
					}
					yield return
						new ResourceLayer
							{
								Resource = 1d * (1 * ((minutesSplit - startDiff - endDiff) / minutesSplit)),
								PayloadId = payload.Id.GetValueOrDefault(),
								RequiresSeat = requiresSeat,
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

		public static void AddScheduleDayToContainer(this IResourceCalculationDataContainerWithSingleOperation resources, IScheduleDay scheduleDay, int minutesSplit)
		{
            if (!scheduleDay.HasProjection()) return;

			var projection = scheduleDay.ProjectionService().CreateProjection();
			var resourceLayers = projection.ToResourceLayers(minutesSplit);
			foreach (var resourceLayer in resourceLayers)
			{
				resources.AddResources(scheduleDay.Person, scheduleDay.DateOnlyAsPeriod.DateOnly, resourceLayer);
			}
		}

		public static void RemoveScheduleDayFromContainer(this IResourceCalculationDataContainerWithSingleOperation resources, IScheduleDay scheduleDay, int minutesSplit)
		{
            if (!scheduleDay.HasProjection()) return;

			var projection = scheduleDay.ProjectionService().CreateProjection();
			var resourceLayers = projection.ToResourceLayers(minutesSplit);
			foreach (var resourceLayer in resourceLayers)
			{
				resources.RemoveResources(scheduleDay.Person, scheduleDay.DateOnlyAsPeriod.DateOnly, resourceLayer);
			}
		}
	}

	public interface IResourceCalculationDataContainerWithSingleOperation : IResourceCalculationDataContainer
	{
		void AddResources(IPerson person, DateOnly personDate, ResourceLayer resourceLayer);
		void RemoveResources(IPerson person, DateOnly personDate, ResourceLayer resourceLayer);
	}

	public interface IResourceCalculationDataContainer
	{
		void Clear();
		bool HasItems();

		Tuple<double,double> SkillResources(ISkill skill, DateTimePeriod period);
		double ActivityResourcesWhereSeatRequired(ISkill skill, DateTimePeriod period);
		IDictionary<string, AffectedSkills> AffectedResources(IActivity activity, DateTimePeriod periodToCalculate);
		int MinSkillResolution { get; }
	}

	public struct AffectedSkills
	{
		public IEnumerable<ISkill> Skills { get; set; }
		public IDictionary<Guid, double> SkillEffiencies { get; set; }
		public double Resource { get; set; }
		public double Count { get; set; }
	}
}