using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;
using log4net;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Resources
{
	public class ScheduledResourcesChangedHandler : IHandleEvent<ProjectionChangedEvent>, IHandleEvent<ProjectionChangedEventForScheduleProjection>
	{
		private readonly IPersonRepository _personRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly IScheduleProjectionReadOnlyRepository _readModelFinder;
		private readonly IScheduledResourcesReadModelUpdater _scheduledResourcesReadModelStorage;
		private readonly IPersonSkillProvider _personSkillProvider;
		private readonly IPublishEventsFromEventHandlers _bus;
		private int configurableIntervalLength = 15;
		private static readonly ILog Logger = LogManager.GetLogger(typeof (ScheduledResourcesChangedHandler));

		public ScheduledResourcesChangedHandler(IPersonRepository personRepository, ISkillRepository skillRepository, IScheduleProjectionReadOnlyRepository readModelFinder, IScheduledResourcesReadModelUpdater scheduledResourcesReadModelStorage, IPersonSkillProvider personSkillProvider, IPublishEventsFromEventHandlers bus)
		{
			_personRepository = personRepository;
			_skillRepository = skillRepository;
			_readModelFinder = readModelFinder;
			_scheduledResourcesReadModelStorage = scheduledResourcesReadModelStorage;
			_personSkillProvider = personSkillProvider;
			_bus = bus;
		}

		public void Handle(ProjectionChangedEventForScheduleProjection @event)
		{
			createReadModel(@event);
		}

		public void Handle(ProjectionChangedEvent @event)
		{
			createReadModel(@event);
		}

		private void createReadModel(ProjectionChangedEventBase @event)
		{
			var person = _personRepository.Get(@event.PersonId);
			if (person == null)
			{
				Logger.WarnFormat("No person was found with the id {0}", @event.PersonId);
				return;
			}

			_scheduledResourcesReadModelStorage.Update(@event.Datasource, @event.BusinessUnitId, storage =>
				{

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
								storage.RemoveResource(resourceLayer, combination);
							}
						}

						if (scheduleDay.Shift == null) continue;
						var resources = scheduleDay.Shift.Layers.ToResourceLayers(configurableIntervalLength);
						foreach (var resourceLayer in resources)
						{
							storage.AddResource(resourceLayer, combination);
						}
					}

				});

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

	}
}