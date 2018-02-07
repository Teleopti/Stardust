using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.TestCommon.TestData.Setups.Specific;

namespace Teleopti.Ccc.TestCommon.TestData.Core
{
	public class PersonDataFactory : ILogonName
	{
		private readonly IPerson _person;
		private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly ICurrentTenantSession _tenantSession;
		private readonly ITenantUnitOfWork _tenantUnitOfWork;
		private readonly IList<IUserSetup> _userSetups = new List<IUserSetup>();
		private readonly IList<IUserDataSetup> _userDataSetups = new List<IUserDataSetup>();

		public PersonDataFactory(
			IPerson person, 
			ICurrentUnitOfWork unitOfWork, 
			ICurrentTenantSession tenantSession,
			ITenantUnitOfWork tenantUnitOfWork)
		{
			_person = person;
			_unitOfWork = unitOfWork;
			_tenantSession = tenantSession;
			_tenantUnitOfWork = tenantUnitOfWork;
			Apply(new Setups.Specific.SwedishCulture());
			Apply(new UtcTimeZone());
		}

		public void Apply(IUserSetup setup)
		{
			setup.Apply(_unitOfWork.Current(), _person, _person.PermissionInformation.Culture());
			_unitOfWork.Current().PersistAll();

			var setupTenant = setup as ITenantUserSetup;
			if (setupTenant != null)
				using (_tenantUnitOfWork.EnsureUnitOfWorkIsStarted())
					setupTenant.Apply(_tenantSession, Person, this);

			_userSetups.Add(setup);
		}

		public void Apply(IUserDataSetup setup)
		{
			setup.Apply(_unitOfWork, _person, _person.PermissionInformation.Culture());
			_unitOfWork.Current().PersistAll();

			_userDataSetups.Add(setup);
		}

		public IEnumerable<object> Applied => _userSetups.Cast<object>().Union(_userDataSetups);

		public string LogOnName { get; private set; }
		public void Set(string logonName)
		{
			LogOnName = logonName;
		}
		public IPerson Person => _person;
		public CultureInfo Culture => Person.PermissionInformation.Culture();
	}
}