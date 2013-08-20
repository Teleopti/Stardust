using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;
using log4net;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Resources
{
	public class ScheduledResourcesPersonSkillRemovedHandler : IHandleEvent<PersonSkillRemovedEvent>
	{
		private readonly IScheduledResourcesReadModelStorage _scheduledResourcesReadModelStorage;
		private readonly IScheduleProjectionReadOnlyRepository _readModelFinder;
		private readonly IPersonRepository _personRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private int configurableIntervalLength = 15;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ScheduledResourcesPersonSkillAddedHandler));

		public ScheduledResourcesPersonSkillRemovedHandler(IScheduledResourcesReadModelStorage scheduledResourcesReadModelStorage, IScheduleProjectionReadOnlyRepository readModelFinder, IPersonRepository personRepository, ISkillRepository skillRepository, IScenarioRepository scenarioRepository)
		{
			_scheduledResourcesReadModelStorage = scheduledResourcesReadModelStorage;
			_readModelFinder = readModelFinder;
			_personRepository = personRepository;
			_skillRepository = skillRepository;
			_scenarioRepository = scenarioRepository;
		}

		public void Handle(PersonSkillRemovedEvent @event)
		{
			if (!@event.SkillActive) return;

			var person = _personRepository.Get(@event.PersonId);
			if (person == null)
			{
				Logger.WarnFormat("No person was found with the id {0}", @event.PersonId);
				return;
			}

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
					removeResourceFromInterval(resourceLayer, combinationBefore);
				}
			}

			var foundskillDetail = skillsBefore.FirstOrDefault(s => s.SkillId == @event.SkillId);
			skillsBefore.Remove(foundskillDetail);

			if (skillsBefore.Count > 0)
			{
				var combinationAfter =
					new SkillCombination(SkillCombination.ToKey(skillsBefore.Select(s => s.SkillId)),
					                     new ISkill[] {}, period,
					                     skillsBefore.Where(s => s.Proficiency != 1d)
					                                 .ToDictionary(k => k.SkillId, v => v.Proficiency));
				foreach (var resourceLayer in oldResources)
				{
					addResourceToInterval(resourceLayer, combinationAfter);
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