using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;
using log4net;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Resources
{
	public class ScheduledResourcesChangedHandler : IHandleEvent<ProjectionChangedEvent>
	{
		private readonly IPersonRepository _personRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly IScheduleProjectionReadOnlyRepository _readModelFinder;
		private readonly IScheduledResourcesReadModelPersister _scheduledResourcesReadModelStorage;
		private readonly IPersonSkillProvider _personSkillProvider;
		private readonly IPublishEventsFromEventHandlers _bus;
		private int configurableIntervalLength = 15;
		private static readonly ILog Logger = LogManager.GetLogger(typeof (ScheduledResourcesChangedHandler));

		public ScheduledResourcesChangedHandler(IPersonRepository personRepository, ISkillRepository skillRepository, IScheduleProjectionReadOnlyRepository readModelFinder, IScheduledResourcesReadModelPersister scheduledResourcesReadModelStorage, IPersonSkillProvider personSkillProvider, IPublishEventsFromEventHandlers bus)
		{
			_personRepository = personRepository;
			_skillRepository = skillRepository;
			_readModelFinder = readModelFinder;
			_scheduledResourcesReadModelStorage = scheduledResourcesReadModelStorage;
			_personSkillProvider = personSkillProvider;
			_bus = bus;
		}

		public void Handle(ProjectionChangedEvent @event)
		{
			var person = _personRepository.Get(@event.PersonId);
			if (person == null)
			{
				Logger.WarnFormat("No person was found with the id {0}", @event.PersonId);
				return;
			}

			configurableIntervalLength = _skillRepository.MinimumResolution();
			foreach (var scheduleDay in @event.ScheduleDays)
			{
				var date = new DateOnly(scheduleDay.Date);
				var combination = _personSkillProvider.SkillsOnPersonDate(person, date);

				if (!@event.IsInitialLoad)
				{
					var oldSchedule = _readModelFinder.ForPerson(date, @event.PersonId, @event.ScenarioId);
					var oldResources = oldSchedule.ToResourceLayers(configurableIntervalLength);
					foreach (var resourceLayer in oldResources)
					{
						removeResourceFromInterval(resourceLayer, combination);
					}
				}

				var resources = scheduleDay.Layers.ToResourceLayers(configurableIntervalLength);
				foreach (var resourceLayer in resources)
				{
					addResourceToInterval(resourceLayer, combination);
				}
			}

			_bus.Publish(new ScheduledResourcesChangedEvent
				{
					BusinessUnitId = @event.BusinessUnitId,
					Datasource = @event.Datasource,
					IsDefaultScenario = @event.IsDefaultScenario,
					IsInitialLoad = @event.IsInitialLoad,
					PersonId = @event.PersonId,
					ScenarioId = @event.ScenarioId,
					ScheduleDays = @event.ScheduleDays,
					Timestamp = @event.Timestamp
				});
		}

		private void addResourceToInterval(ResourceLayer resourceLayer, SkillCombination combination)
		{
			var resourceId = _scheduledResourcesReadModelStorage.AddResources(resourceLayer.PayloadId, resourceLayer.RequiresSeat,
			                                                 combination.Key, resourceLayer.Period,
			                                                 resourceLayer.Resource, 1);
			foreach (var skillEfficiency in combination.SkillEfficiencies)
			{
				_scheduledResourcesReadModelStorage.AddSkillEfficiency(resourceId,skillEfficiency.Key,skillEfficiency.Value);
			}
		}

		private void removeResourceFromInterval(ResourceLayer resourceLayer, SkillCombination combination)
		{
			var resourceId = _scheduledResourcesReadModelStorage.RemoveResources(resourceLayer.PayloadId, combination.Key,
			                                                    resourceLayer.Period, resourceLayer.Resource, 1);
			if (!resourceId.HasValue) return;

			foreach (var skillEfficiency in combination.SkillEfficiencies)
			{
				_scheduledResourcesReadModelStorage.RemoveSkillEfficiency(resourceId.Value, skillEfficiency.Key, skillEfficiency.Value);
			}
		}
	}

	//Other cases to handle:
	//- Person terminated -
	//- Person reactivated -
	//- Person deleted -
	//- Person period date changes -
	//- Person period added -
	//- Person period removed -
	//- Person period skill changes
	//- Person team changes
	//- Team site changes -
}