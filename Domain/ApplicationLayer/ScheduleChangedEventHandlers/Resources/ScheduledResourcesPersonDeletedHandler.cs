using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Resources
{
	public class ScheduledResourcesPersonDeletedHandler : IHandleEvent<PersonDeletedEvent>
	{
		private readonly IScheduledResourcesReadModelUpdater _scheduledResourcesReadModelStorage;
		private readonly IScheduleProjectionReadOnlyRepository _readModelFinder;
		private readonly ISkillRepository _skillRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private int configurableIntervalLength = 15;

		public ScheduledResourcesPersonDeletedHandler(IScheduledResourcesReadModelUpdater scheduledResourcesReadModelStorage, IScheduleProjectionReadOnlyRepository readModelFinder, ISkillRepository skillRepository, IScenarioRepository scenarioRepository)
		{
			_scheduledResourcesReadModelStorage = scheduledResourcesReadModelStorage;
			_readModelFinder = readModelFinder;
			_skillRepository = skillRepository;
			_scenarioRepository = scenarioRepository;
		}

		public void Handle(PersonDeletedEvent @event)
		{
			configurableIntervalLength = _skillRepository.MinimumResolution();

			_scheduledResourcesReadModelStorage.Update(@event.Datasource, @event.BusinessUnitId, storage =>
				{

					var defaultScenarioId = _scenarioRepository.LoadDefaultScenario().Id.GetValueOrDefault();
					foreach (var personPeriodDetail in @event.PersonPeriodsBefore)
					{
						var period = new DateOnlyPeriod(new DateOnly(personPeriodDetail.StartDate), new DateOnly(personPeriodDetail.EndDate));
						var oldSchedule = _readModelFinder.ForPerson(period, @event.PersonId, defaultScenarioId);
						var oldResources = oldSchedule.ToResourceLayers(configurableIntervalLength);
						var skillsBefore = personPeriodDetail.PersonSkillDetails.Where(s => s.Active).ToList();

						if (skillsBefore.Count > 0)
						{
							var combinationBefore =
								new SkillCombination(SkillCombination.ToKey(skillsBefore.Select(s => s.SkillId)),
								                     new ISkill[] {}, period,
								                     skillsBefore.Where(s => s.Proficiency != 1d)
																 .Select(k => new SkillEffiencyResource(k.SkillId, k.Proficiency)).ToArray());
							foreach (var resourceLayer in oldResources)
							{
								storage.RemoveResource(resourceLayer, combinationBefore);
							}
						}
					}

				});

		}

	}
}