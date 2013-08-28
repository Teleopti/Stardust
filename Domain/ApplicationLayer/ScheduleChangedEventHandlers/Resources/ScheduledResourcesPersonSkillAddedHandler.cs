using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Resources
{
	public class ScheduledResourcesPersonSkillAddedHandler : IHandleEvent<PersonSkillAddedEvent>
	{
		private readonly IScheduledResourcesReadModelUpdater _scheduledResourcesReadModelStorage;
		private readonly IScheduleProjectionReadOnlyRepository _readModelFinder;
		private readonly ISkillRepository _skillRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private int configurableIntervalLength = 15;

		public ScheduledResourcesPersonSkillAddedHandler(IScheduledResourcesReadModelUpdater scheduledResourcesReadModelStorage, IScheduleProjectionReadOnlyRepository readModelFinder, ISkillRepository skillRepository, IScenarioRepository scenarioRepository)
		{
			_scheduledResourcesReadModelStorage = scheduledResourcesReadModelStorage;
			_readModelFinder = readModelFinder;
			_skillRepository = skillRepository;
			_scenarioRepository = scenarioRepository;
		}

		public void Handle(PersonSkillAddedEvent @event)
		{
			configurableIntervalLength = _skillRepository.MinimumResolution();

			var period = new DateOnlyPeriod(new DateOnly(@event.StartDate), new DateOnly(@event.EndDate));
			var oldSchedule = _readModelFinder.ForPerson(period, @event.PersonId, _scenarioRepository.LoadDefaultScenario().Id.GetValueOrDefault());
			var oldResources = oldSchedule.ToResourceLayers(configurableIntervalLength);
			var skillsBefore = @event.SkillsBefore.Where(s => s.Active).ToList();

			if (skillsBefore.Count > 0)
			{
				var combinationBefore =
					new SkillCombination(SkillCombination.ToKey(skillsBefore.Select(s => s.SkillId)),
					                     new ISkill[] {}, period,
					                     skillsBefore.Where(s => s.Proficiency != 1d)
					                                 .ToDictionary(k => k.SkillId, v => v.Proficiency));
				foreach (var resourceLayer in oldResources)
				{
					_scheduledResourcesReadModelStorage.RemoveResource(resourceLayer, combinationBefore);
				}
			}

			skillsBefore.Add(new PersonSkillDetail
				{
					Active = @event.SkillActive,
					SkillId = @event.SkillId,
					Proficiency = @event.Proficiency
				});
			var combinationAfter =
				new SkillCombination(SkillCombination.ToKey(skillsBefore.Select(s => s.SkillId)),
									 new ISkill[] { }, period,
									 skillsBefore.Where(s => s.Proficiency != 1d)
										   .ToDictionary(k => k.SkillId, v => v.Proficiency));
			foreach (var resourceLayer in oldResources)
			{
				_scheduledResourcesReadModelStorage.AddResource(resourceLayer, combinationAfter);
			}
		}

	}
}