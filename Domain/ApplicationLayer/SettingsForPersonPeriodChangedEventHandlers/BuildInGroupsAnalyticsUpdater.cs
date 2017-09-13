using System;
using System.Collections.Generic;
using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.SettingsForPersonPeriodChangedEventHandlers
{
	public class BuildInGroupsAnalyticsUpdater :
		IHandleEvent<SettingsForPersonPeriodChangedEvent>,
		IRunOnHangfire
	{
		private readonly IAnalyticsGroupPageRepository _analyticsGroupPageRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly IPartTimePercentageRepository _partTimePercentageRepository;
		private readonly IRuleSetBagRepository _ruleSetBagRepository;
		private readonly IContractRepository _contractRepository;
		private readonly IContractScheduleRepository _contractScheduleRepository;
		private readonly List<Func<AnalyticsGroup, Guid, bool>> _checks;
		private static readonly ILog logger = LogManager.GetLogger(typeof(BuildInGroupsAnalyticsUpdater));

		public BuildInGroupsAnalyticsUpdater(IAnalyticsGroupPageRepository analyticsGroupPageRepository, ISkillRepository skillRepository,
													 IPartTimePercentageRepository partTimePercentageRepository, IRuleSetBagRepository ruleSetBagRepository, IContractRepository contractRepository,
													 IContractScheduleRepository contractScheduleRepository)
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

		[ImpersonateSystem] 
		[UnitOfWork]
		[AnalyticsUnitOfWork]
		[Attempts(10)]
		public virtual void Handle(SettingsForPersonPeriodChangedEvent @event)
		{
			foreach (var entityId in @event.IdCollection)
			{
				var groupPage = _analyticsGroupPageRepository.GetGroupPageByGroupCode(entityId, @event.LogOnBusinessUnitId);
				if (groupPage == null) continue;
				foreach (var check in _checks)
				{
					if (check(groupPage, entityId))
						break;
				}
			}
		}

		private bool check<T>(AnalyticsGroup group, Func<T> getter, Func<T, string> propertyAccessor)
		{
			var entity = getter();
			if (entity == null)
				return false;

			var groupGroupName = propertyAccessor(entity);
			if (group.GroupName != groupGroupName)
			{
				group.GroupName = groupGroupName;
				logger.Debug($"Updating Group: {group.GroupName}, {group.GroupCode}");
				_analyticsGroupPageRepository.UpdateGroupPage(group);
			}
			return true;
		}
	}
}