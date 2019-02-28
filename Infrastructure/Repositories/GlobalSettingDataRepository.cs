using System;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class GlobalSettingDataRepository : SettingDataRepository, IGlobalSettingDataRepository
	{
		public static GlobalSettingDataRepository DONT_USE_CTOR(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new GlobalSettingDataRepository(currentUnitOfWork);
		}

		public static GlobalSettingDataRepository DONT_USE_CTOR(IUnitOfWork unitOfWork)
		{
			return new GlobalSettingDataRepository(new ThisUnitOfWork(unitOfWork));
		}

		public GlobalSettingDataRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
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
			checkBusinessUnitIsSet();
			var data = FindByKey(key) ?? new GlobalSettingData(key);
			return data.GetValue(defaultValue);
		}

		private void checkBusinessUnitIsSet()
		{
			// Want to make sure that if we are about to use a default value it should not be because some handler has not set an correct business unit.
			var filter = CurrentUnitOfWork.Current().Session().GetEnabledFilter("businessUnitFilter") as NHibernate.Impl.FilterImpl;
			var buParamter = filter?.Parameters["businessUnitParameter"] as Guid?;
			if (buParamter != null && buParamter.GetValueOrDefault() == Guid.Empty)
				throw new ApplicationException(
					$"Cannot get {nameof(GlobalSettingData)} for not defined business unit. Application error, check attributes on event handlers.");
		}
	}
}
