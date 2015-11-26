﻿using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class PersonalSettingDataRepository : SettingDataRepository, IPersonalSettingDataRepository
    {
        public PersonalSettingDataRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

				public PersonalSettingDataRepository(IUnitOfWorkFactory unitOfWorkFactory)
					: base(unitOfWorkFactory)
				{
				}

				public PersonalSettingDataRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork)
	    {
	    }

        public override ISettingData FindByKey(string key)
        {
            return CurrentUnitOfWork.Current().Session().CreateCriteria(typeof(PersonalSettingData))
                        .Add(Restrictions.Eq("Key", key))
                        .Add(Restrictions.Eq("OwnerPerson", TeleoptiPrincipal.CurrentPrincipal.GetPerson(new PersonRepository(CurrentUnitOfWork))))
                        .SetCacheable(true)
                        .UniqueResult<ISettingData>();
        }

		public T FindValueByKeyAndOwnerPerson<T>(string key, IPerson ownerPerson, T defaultValue) where T : class, ISettingValue
		{

			var data = CurrentUnitOfWork.Current().Session().CreateCriteria(typeof (PersonalSettingData))
				.Add(Restrictions.Eq("Key", key))
				.Add(Restrictions.Eq("OwnerPerson", ownerPerson))
				.SetCacheable(true)
				.UniqueResult<ISettingData>();

			return data == null ? defaultValue : data.GetValue(defaultValue);
		}

        public T FindValueByKey<T>(string key, T defaultValue) where T : class, ISettingValue
        {
            ISettingData data = FindByKey(key)
                   ?? new GlobalSettingDataRepository(CurrentUnitOfWork).FindByKey(key)
                   ?? new PersonalSettingData(key, TeleoptiPrincipal.CurrentPrincipal.GetPerson(new PersonRepository(CurrentUnitOfWork)));
            return data.GetValue(defaultValue);
        }
    }
}
