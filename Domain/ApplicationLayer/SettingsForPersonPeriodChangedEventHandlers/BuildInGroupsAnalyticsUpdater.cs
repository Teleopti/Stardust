using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.Analytics;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.SettingsForPersonPeriodChangedEventHandlers
{
	[UseOnToggle(Toggles.ETL_SpeedUpGroupPagePersonIntraday_37623,
				 Toggles.ETL_SpeedUpPersonPeriodIntraday_37162_37439)]
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
		private readonly List<Func<AnalyticsGroup, Guid, bool>> _checks;
		
		public BuildInGroupsAnalyticsUpdater(IAnalyticsGroupPageRepository analyticsGroupPageRepository, ISkillRepository skillRepository, IPartTimePercentageRepository partTimePercentageRepository, IRuleSetBagRepository ruleSetBagRepository, IContractRepository contractRepository, IContractScheduleRepository contractScheduleRepository)
		{
			_skillRepository = skillRepository;
			_partTimePercentageRepository = partTimePercentageRepository;
			_ruleSetBagRepository = ruleSetBagRepository;
			_contractRepository = contractRepository;
			_contractScheduleRepository = contractScheduleRepository;
			_analyticsGroupPageRepository = analyticsGroupPageRepository;
			_checks = new List<Func<AnalyticsGroup, Guid, bool>>
			{
				(groupPage, entityId) => check(groupPage, () => _partTimePercentageRepository.Get(entityId), entity => entity.Description.Name),
				(groupPage, entityId) => check(groupPage, () => _ruleSetBagRepository.Get(entityId), entity => entity.Description.Name),
				(groupPage, entityId) => check(groupPage, () => _contractRepository.Get(entityId), entity => entity.Description.Name),
				(groupPage, entityId) => check(groupPage, () => _contractScheduleRepository.Get(entityId), entity => entity.Description.Name),
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

		private bool check<T>(AnalyticsGroup @group, Func<T> getter, Func<T, string> propertyAccessor)
		{
			var entity = getter();
			if (entity == null) return false;
			if (@group.GroupName != propertyAccessor(entity))
			{
				@group.GroupName = propertyAccessor(entity);
				_analyticsGroupPageRepository.UpdateGroupPage(@group);
			}
			return true;
		}
	}
}