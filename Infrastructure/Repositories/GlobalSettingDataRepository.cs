﻿using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class GlobalSettingDataRepository : SettingDataRepository, IGlobalSettingDataRepository
	{
		public GlobalSettingDataRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
		{
		}

		public GlobalSettingDataRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
		}

		public GlobalSettingDataRepository(IUnitOfWorkFactory unitOfWorkFactory) : base(unitOfWorkFactory)
		{
		}

		public override ISettingData FindByKey(string key)
		{
			return CurrentUnitOfWork.Current().Session().CreateCriteria(typeof(GlobalSettingData))
						.Add(Restrictions.Eq("Key", key))
						.UniqueResult<ISettingData>();
		}

		public T FindValueByKey<T>(string key, T defaultValue) where T : class, ISettingValue
		{
			var data = FindByKey(key) ?? new GlobalSettingData(key);
			return data.GetValue(defaultValue);
		}
	}
}
