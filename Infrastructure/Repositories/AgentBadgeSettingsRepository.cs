using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class AgentBadgeSettingsRepository : SettingDataRepository, IAgentBadgeSettingsRepository
	{
		public const string Key = "AgentBadgeThresholdSettings";

		public AgentBadgeSettingsRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
		{
		}

		public AgentBadgeSettingsRepository(IUnitOfWorkFactory unitOfWorkFactory) : base(unitOfWorkFactory)
		{
		}

		public AgentBadgeSettingsRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
		}

		public override ISettingData FindByKey(string key)
		{
			return Session.CreateCriteria(typeof(AgentBadgeThresholdSettings))
				.Add(Restrictions.Eq("Key", key))
				.SetCacheable(true)
				.UniqueResult<ISettingData>();
		}

		public T FindValueByKey<T>(string key, T defaultValue) where T : class, ISettingValue
		{
			ISettingData data = FindByKey(key)
			                    ?? new GlobalSettingDataRepository(UnitOfWork).FindByKey(key)
			                    ?? (ISettingData) new AgentBadgeThresholdSettings();
			return data.GetValue(defaultValue);
		}

		public IAgentBadgeThresholdSettings GetSettings()
		{
				IRepositoryFactory repositoryFactory = new RepositoryFactory();
				var settings = repositoryFactory.CreateGlobalSettingDataRepository(UnitOfWork)
					.FindValueByKey(Key, new AgentBadgeThresholdSettings());

			return settings;
		}
	}
}
