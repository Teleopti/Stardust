using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;
using log4net;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel
{
	public class ScheduledResourcesChangedHandler : IHandleEvent<ProjectionChangedEvent>
	{
		private readonly IPersonRepository _personRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly IPersonSkillProvider _personSkillProvider;
		private int configurableIntervalLength = 15;
		private static readonly ILog Logger = LogManager.GetLogger(typeof (ScheduledResourcesChangedHandler));

		public ScheduledResourcesChangedHandler(IPersonRepository personRepository, ISkillRepository skillRepository, IPersonSkillProvider personSkillProvider)
		{
			_personRepository = personRepository;
			_skillRepository = skillRepository;
			_personSkillProvider = personSkillProvider;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods",
			MessageId = "0")]
		public void Handle(ProjectionChangedEvent @event)
		{
			var person = _personRepository.Get(@event.PersonId);
			if (person == null)
			{
				Logger.WarnFormat("No person was found with the id {0}",@event.PersonId);
				return;
			}

			configurableIntervalLength = _skillRepository.MinimumResolution();
			foreach (var scheduleDay in @event.ScheduleDays)
			{
				var combination = _personSkillProvider.SkillsOnPersonDate(person, new DateOnly(scheduleDay.Date));
				var resources = scheduleDay.Layers.ToResourceLayers(configurableIntervalLength);
				foreach (var resourceLayer in resources)
				{
					removeResourceFromInterval(resourceLayer, new ActivitySkillsCombination(resourceLayer.PayloadId,combination));
				}

				resources = scheduleDay.Layers.ToResourceLayers(configurableIntervalLength);
				foreach (var resourceLayer in resources)
				{
					addResourceToInterval(resourceLayer, new ActivitySkillsCombination(resourceLayer.PayloadId, combination));
				}
			}
		}

		private void addResourceToInterval(ResourceLayer resourceLayer, ActivitySkillsCombination activitySkillsCombination)
		{

			//update resources set resource = resource + resource where combination_id = x and startdatetime = period.startdatetime and enddatetime = period.enddatetime
			//or
			//insert into resources (resource, combination_id = x,startdatetime = period.startdatetime, enddatetime = period.enddatetime)
		}

		private void removeResourceFromInterval(ResourceLayer resourceLayer, ActivitySkillsCombination activitySkillsCombination)
		{
			//update resources set resource = resource - resource where combination_id = x and startdatetime = period.startdatetime and enddatetime = period.enddatetime
		}
	}

	//Other cases to handle:
	//- Person terminated
	//- Person deleted
	//- Person period date changes
	//- Person period skill changes
	//- Person team changes
	//- Activity requires seat changed
	//- Team site changes

	public static class ProjectionChangedEventLayerExtensions
	{
		public static IEnumerable<ResourceLayer> ToResourceLayers(this IEnumerable<ProjectionChangedEventLayer> resourceLayerCollection,
		                                                          int minutesSplit)
		{
			if (!resourceLayerCollection.Any()) yield break;

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
					yield return
						new ResourceLayer
							{
								Resource = (minutesSplit - startDiff - endDiff)/minutesSplit,
								PayloadId = layer.PayloadId,
								RequiresSeat = layer.RequiresSeat,
								Period = new DateTimePeriod(startTime, startTime.AddMinutes(minutesSplit))
							};
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