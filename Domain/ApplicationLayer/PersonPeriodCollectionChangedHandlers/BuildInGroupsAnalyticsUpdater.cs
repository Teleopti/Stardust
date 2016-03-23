using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.Analytics;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonPeriodCollectionChangedHandlers
{
	[UseOnToggle(Toggles.ETL_SpeedUpGroupPagePersonIntraday_37623)]
	public class BuildInGroupsAnalyticsUpdater :
		IHandleEvent<SettingsForPersonPeriodChangedEvent>,
		IRunOnServiceBus
	{
		private readonly IAnalyticsGroupPageRepository _analyticsGroupPageRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly IPartTimePercentageRepository _partTimePercentageRepository;
		private readonly IRuleSetBagRepository _ruleSetBagRepository;
		private readonly IContractRepository _contractRepository;
		private readonly IContractScheduleRepository _contractScheduleRepository;
		private readonly IBudgetGroupRepository _budgetGroupRepository;
		private readonly List<Func<AnalyticsGroupPage, Guid, bool>> _checks;
		
		public BuildInGroupsAnalyticsUpdater(IAnalyticsGroupPageRepository analyticsGroupPageRepository, ISkillRepository skillRepository, IPartTimePercentageRepository partTimePercentageRepository, IRuleSetBagRepository ruleSetBagRepository, IContractRepository contractRepository, IContractScheduleRepository contractScheduleRepository, IBudgetGroupRepository budgetGroupRepository)
		{
			_skillRepository = skillRepository;
			_partTimePercentageRepository = partTimePercentageRepository;
			_ruleSetBagRepository = ruleSetBagRepository;
			_contractRepository = contractRepository;
			_contractScheduleRepository = contractScheduleRepository;
			_budgetGroupRepository = budgetGroupRepository;
			_analyticsGroupPageRepository = analyticsGroupPageRepository;
			_checks = new List<Func<AnalyticsGroupPage, Guid, bool>>
			{
				(groupPage, entityId) => check(groupPage, () => _partTimePercentageRepository.Get(entityId), entity => entity.Description.Name),
				(groupPage, entityId) => check(groupPage, () => _ruleSetBagRepository.Get(entityId), entity => entity.Description.Name),
				(groupPage, entityId) => check(groupPage, () => _contractRepository.Get(entityId), entity => entity.Description.Name),
				(groupPage, entityId) => check(groupPage, () => _contractScheduleRepository.Get(entityId), entity => entity.Description.Name),
				(groupPage, entityId) => check(groupPage, () => _budgetGroupRepository.Get(entityId), entity => entity.Name),
				(groupPage, entityId) => check(groupPage, () => _skillRepository.Get(entityId), entity => entity.Name)
			};
		}

		public void Handle(SettingsForPersonPeriodChangedEvent @event)
		{
			foreach (var entityId in @event.IdCollection)
			{
				var groupPage = _analyticsGroupPageRepository.GetGroupPageByGroupCode(entityId);
				if (groupPage != null)
				{
					foreach (var check in _checks)
					{
						if (check(groupPage, entityId))
							break;
					}
				}
			}
		}

		private bool check<T>(AnalyticsGroupPage groupPage, Func<T> getter, Func<T, string> propertyAccessor)
		{
			var entity = getter();
			if (entity == null) return false;
			if (groupPage.GroupName != propertyAccessor(entity))
			{
				groupPage.GroupName = propertyAccessor(entity);
				_analyticsGroupPageRepository.UpdateGroupPage(groupPage);
			}
			return true;
		}
	}
}