using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Resources
{
	public class ScheduledResourcesPersonTerminatedHandler : IHandleEvent<PersonTerminatedEvent>
	{
		private readonly IScheduledResourcesReadModelStorage _scheduledResourcesReadModelStorage;
		private readonly IScheduleProjectionReadOnlyRepository _readModelFinder;
		private readonly ISkillRepository _skillRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private int configurableIntervalLength = 15;

		public ScheduledResourcesPersonTerminatedHandler(IScheduledResourcesReadModelStorage scheduledResourcesReadModelStorage, IScheduleProjectionReadOnlyRepository readModelFinder, ISkillRepository skillRepository, IScenarioRepository scenarioRepository)
		{
			_scheduledResourcesReadModelStorage = scheduledResourcesReadModelStorage;
			_readModelFinder = readModelFinder;
			_skillRepository = skillRepository;
			_scenarioRepository = scenarioRepository;
		}

		public void Handle(PersonTerminatedEvent @event)
		{
			configurableIntervalLength = _skillRepository.MinimumResolution();

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
											 new ISkill[] { }, period,
											 skillsBefore.Where(s => s.Proficiency != 1d)
														 .ToDictionary(k => k.SkillId, v => v.Proficiency));
					foreach (var resourceLayer in oldResources)
					{
						removeResourceFromInterval(resourceLayer, combinationBefore);
					}
				}
			}

			foreach (var personPeriodDetail in @event.PersonPeriodsAfter)
			{
				var period = new DateOnlyPeriod(new DateOnly(personPeriodDetail.StartDate), new DateOnly(personPeriodDetail.EndDate));
				var oldSchedule = _readModelFinder.ForPerson(period, @event.PersonId, defaultScenarioId);
				var oldResources = oldSchedule.ToResourceLayers(configurableIntervalLength);
				var skillsBefore = personPeriodDetail.PersonSkillDetails.Where(s => s.Active).ToList();

				if (skillsBefore.Count > 0)
				{
					var combinationBefore =
						new SkillCombination(SkillCombination.ToKey(skillsBefore.Select(s => s.SkillId)),
											 new ISkill[] { }, period,
											 skillsBefore.Where(s => s.Proficiency != 1d)
														 .ToDictionary(k => k.SkillId, v => v.Proficiency));
					foreach (var resourceLayer in oldResources)
					{
						addResourceToInterval(resourceLayer, combinationBefore);
					}
				}
			}
		}

		private void addResourceToInterval(ResourceLayer resourceLayer, SkillCombination combination)
		{
			var resourceId = _scheduledResourcesReadModelStorage.AddResources(resourceLayer.PayloadId, resourceLayer.RequiresSeat,
															 combination.Key, resourceLayer.Period,
															 resourceLayer.Resource, 1);
			foreach (var skillEfficiency in combination.SkillEfficiencies)
			{
				_scheduledResourcesReadModelStorage.AddSkillEfficiency(resourceId, skillEfficiency.Key, skillEfficiency.Value);
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
}