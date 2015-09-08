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
					var currentIntervalPeriod = new DateTimePeriod(startTime, startTime.AddMinutes(minutesSplit));
					if (currentIntervalPeriod.Intersect(layer.Period))
					{
						DateTimePeriod? fractionPeriod = null;
						if (fractionOfLayerShouldBeIncluded(currentIntervalPeriod, layer))
						{
							fractionPeriod = layer.Period.Intersection(currentIntervalPeriod);
						}

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
									Resource = fractionPeriod.HasValue ? fractionPeriod.Value.ElapsedTime().TotalMinutes/minutesSplit : 1d,
									PayloadId = payload.Id.GetValueOrDefault(),
									RequiresSeat = requiresSeat,
									Period = currentIntervalPeriod,
									FractionPeriod = fractionPeriod
								};
						if (currentIntervalPeriod.EndDateTime > layer.Period.EndDateTime)
						{
							break;
						}
					}
					startTime = currentIntervalPeriod.EndDateTime;
				}
			}
		}

		private static bool fractionOfLayerShouldBeIncluded(DateTimePeriod currentIntervalPeriod, IVisualLayer layer)
		{
			return currentIntervalPeriod.StartDateTime < layer.Period.StartDateTime ||
			       currentIntervalPeriod.EndDateTime > layer.Period.EndDateTime;
		}

		public static void AddScheduleDayToContainer(this IResourceCalculationDataContainerWithSingleOperation resources,
			IScheduleDay scheduleDay, int minutesSplit)
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
		IEnumerable<DateTimePeriod> IntraIntervalResources(ISkill skill, DateTimePeriod period);
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