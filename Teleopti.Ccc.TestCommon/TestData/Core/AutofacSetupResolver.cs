using System;
using Autofac;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.TestData.Setups.Specific;

namespace Teleopti.Ccc.TestCommon.TestData.Core
{
	public interface IResolver
	{
		IUserDataSetup<T> ResolveUserDataSetupFor<T>();
		IUserSetup<T> ResolveUserSetupFor<T>();
		PersonDataFactory MakePersonDataFactory();
		IDataSetup<T> ResolveDataSetupFor<T>();
	}

	public class LegacyResolver : IResolver
	{
		private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly ICurrentTenantSession _currentTenantSession;
		private readonly ITenantUnitOfWork _tenantUnitOfWork;

		public LegacyResolver(ICurrentUnitOfWork unitOfWork, ICurrentTenantSession currentTenantSession,
			ITenantUnitOfWork tenantUnitOfWork)
		{
			_unitOfWork = unitOfWork;
			_currentTenantSession = currentTenantSession;
			_tenantUnitOfWork = tenantUnitOfWork;
		}

		public IUserDataSetup<T> ResolveUserDataSetupFor<T>()
		{
			return null;
		}

		public IUserSetup<T> ResolveUserSetupFor<T>()
		{
			if (typeof(T) == typeof(SwedishCultureSpec))
				return new SwedishCultureSetup() as IUserSetup<T>;
			if (typeof(T) == typeof(UtcTimeZoneSpec))
				return new UtcTimeZoneSetup() as IUserSetup<T>;
			return null;
		}

		public PersonDataFactory MakePersonDataFactory()
		{
			return new PersonDataFactory(
				_unitOfWork,
				_currentTenantSession,
				_tenantUnitOfWork,
				this
			);
		}

		public IDataSetup<T> ResolveDataSetupFor<T>()
		{
			if (typeof(T) == typeof(ActivitySpec))
				return new ActivitySetup(
					new BusinessUnitRepository(_unitOfWork, null, null),
					new ActivityRepository(_unitOfWork)
				) as IDataSetup<T>;
			return null;
		}
	}

	public class AutofacResolver : IResolver
	{
		private readonly IComponentContext _container;

		public AutofacResolver(IComponentContext container)
		{
			_container = container;
		}

		public PersonDataFactory MakePersonDataFactory() => _container.Resolve<PersonDataFactory>();

		public IDataSetup<T> ResolveDataSetupFor<T>()
		{
			var specType = typeof(T);
			var setupType = typeof(IDataSetup<>).MakeGenericType(specType);
			return _container.ResolveOptional(setupType) as IDataSetup<T>;
		}

		public IUserDataSetup<T> ResolveUserDataSetupFor<T>()
		{
			var specType = typeof(T);
			var setupType = typeof(IUserDataSetup<>).MakeGenericType(specType);
			return _container.ResolveOptional(setupType) as IUserDataSetup<T>;
		}

		public IUserSetup<T> ResolveUserSetupFor<T>()
		{
			var specType = typeof(T);
			var setupType = typeof(IUserSetup<>).MakeGenericType(specType);
			return _container.ResolveOptional(setupType) as IUserSetup<T>;
		}
	}
}