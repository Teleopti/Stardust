using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.TestCommon.TestData.Setups.Specific;

namespace Teleopti.Ccc.TestCommon.TestData.Core
{
	public class PersonDataFactory : ILogonName
	{
		private IPerson _person;
		private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly ICurrentTenantSession _tenantSession;
		private readonly ITenantUnitOfWork _tenantUnitOfWork;
		private readonly IResolver _resolver;
		private readonly IList<object> _applied = new List<object>();

		public PersonDataFactory(
			ICurrentUnitOfWork unitOfWork,
			ICurrentTenantSession tenantSession,
			ITenantUnitOfWork tenantUnitOfWork,
			IResolver resolver)
		{
			_unitOfWork = unitOfWork;
			_tenantSession = tenantSession;
			_tenantUnitOfWork = tenantUnitOfWork;
			_resolver = resolver;
		}

		public void Setup(IPerson person)
		{
			_person = person;
			apply(new SwedishCultureSpec());
			apply(new UtcTimeZoneSpec());
		}
		
		public void Apply<T>(T specOrSetup)
		{
			switch (specOrSetup)
			{
				case IUserSetup setup:
					apply(setup);
					break;
				case IUserDataSetup setup:
					apply(setup);
					break;
				default:
					apply(specOrSetup);
					break;
			}
		}

		private void apply<T>(T spec)
		{
			var userDataSetup = _resolver.ResolveUserDataSetupFor<T>();
			if (userDataSetup != null)
			{
				userDataSetup.Apply(spec, _person, _person.PermissionInformation.Culture());
				_unitOfWork.Current().PersistAll();
			}

			var userSetup = _resolver.ResolveUserSetupFor<T>();
			if (userSetup != null)
			{
				userSetup.Apply(spec, _person, _person.PermissionInformation.Culture());
				_unitOfWork.Current().PersistAll();
			}

			_applied.Add(spec);
		}

		private void apply(IUserSetup setup)
		{
			setup.Apply(_unitOfWork.Current(), _person, _person.PermissionInformation.Culture());
			_unitOfWork.Current().PersistAll();

			var setupTenant = setup as ITenantUserSetup;
			if (setupTenant != null)
				using (_tenantUnitOfWork.EnsureUnitOfWorkIsStarted())
					setupTenant.Apply(_tenantSession, Person, this);

			_applied.Add(setup);
		}

		private void apply(IUserDataSetup setup)
		{
			setup.Apply(_unitOfWork, _person, _person.PermissionInformation.Culture());
			_unitOfWork.Current().PersistAll();
			_applied.Add(setup);
		}

		public IEnumerable<object> Applied => _applied;

		public string LogOnName { get; private set; }

		public void Set(string logonName)
		{
			LogOnName = logonName;
		}

		public IPerson Person => _person;
		public CultureInfo Culture => Person.PermissionInformation.Culture();
	}
}