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

        public override ISettingData FindByKey(string key)
        {

            return Session.CreateCriteria(typeof(PersonalSettingData))
                        .Add(Restrictions.Eq("Key", key))
                        .Add(Restrictions.Eq("OwnerPerson", TeleoptiPrincipal.Current.GetPerson(new PersonRepository(UnitOfWork))))
                        .SetCacheable(true)
                        .UniqueResult<ISettingData>();
        }

        public T FindValueByKey<T>(string key, T defaultValue) where T : class, ISettingValue
        {
            ISettingData data = FindByKey(key)
                   ?? new GlobalSettingDataRepository(UnitOfWork).FindByKey(key)
                   ?? new PersonalSettingData(key, TeleoptiPrincipal.Current.GetPerson(new PersonRepository(UnitOfWork)));
            return data.GetValue(defaultValue);
        }
    }
}
