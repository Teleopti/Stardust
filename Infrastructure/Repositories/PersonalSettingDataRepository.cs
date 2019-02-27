﻿using NHibernate.Criterion;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class PersonalSettingDataRepository : SettingDataRepository, IPersonalSettingDataRepository
	{
		public static PersonalSettingDataRepository DONT_USE_CTOR(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new PersonalSettingDataRepository(currentUnitOfWork);
		}

		public static PersonalSettingDataRepository DONT_USE_CTOR(IUnitOfWork unitOfWork)
		{
			return new PersonalSettingDataRepository(new ThisUnitOfWork(unitOfWork));
		}

		public PersonalSettingDataRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
		{
		}

		public override ISettingData FindByKey(string key)
		{
			var person = new PersonRepository(CurrentUnitOfWork, null, null).Get(TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.PersonId);
			return CurrentUnitOfWork.Current().Session().CreateCriteria(typeof(PersonalSettingData))
				.Add(Restrictions.Eq("Key", key))
				.Add(Restrictions.Eq("OwnerPerson", person))
//					TeleoptiPrincipal.CurrentPrincipal.GetPerson(new PersonRepository(CurrentUnitOfWork))))
				.SetCacheable(true)
				.UniqueResult<ISettingData>();
		}

		public T FindValueByKeyAndOwnerPerson<T>(string key, IPerson ownerPerson, T defaultValue)
			where T : class, ISettingValue
		{
			var data = CurrentUnitOfWork.Current().Session().CreateCriteria(typeof(PersonalSettingData))
				.Add(Restrictions.Eq("Key", key))
				.Add(Restrictions.Eq("OwnerPerson", ownerPerson))
				.SetCacheable(true)
				.UniqueResult<ISettingData>();

			return data == null ? defaultValue : data.GetValue(defaultValue);
		}

		public T FindValueByKey<T>(string key, T defaultValue) where T : class, ISettingValue
		{
			var person = new PersonRepository(CurrentUnitOfWork, null, null).Get(TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.PersonId);
			ISettingData data = FindByKey(key)
								?? GlobalSettingDataRepository.DONT_USE_CTOR(CurrentUnitOfWork).FindByKey(key)
								??
								new PersonalSettingData(key, person);
//									TeleoptiPrincipal.CurrentPrincipal.GetPerson(new PersonRepository(CurrentUnitOfWork)));
			return data.GetValue(defaultValue);
		}
	}
}
