using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Setups.Specific;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Core
{
	public class PersonDataFactory : ILogonName
	{
		private readonly Action<Action<ICurrentUnitOfWork>> _unitOfWorkAction;
		private readonly Action<Action<ICurrentTenantSession>> _tenantUnitOfWorkAction;
		private readonly IList<IUserSetup> _userSetups = new List<IUserSetup>();
		private readonly IList<IUserDataSetup> _userDataSetups = new List<IUserDataSetup>();

		public PersonDataFactory(Name name, IEnumerable<IUserSetup> setups, Action<Action<ICurrentUnitOfWork>> unitOfWorkAction, Action<Action<ICurrentTenantSession>> tenantUnitOfWorkAction)
		{
			_unitOfWorkAction = unitOfWorkAction;
			_tenantUnitOfWorkAction = tenantUnitOfWorkAction;

			unitOfWorkAction(uow =>
				{
					Person = PersonFactory.CreatePerson(name);
					Person.Name = name;
					new PersonRepository(uow).Add(Person);
				});
			Apply(new Setups.Specific.SwedishCulture());
			Apply(new UtcTimeZone());
			setups.ForEach(Apply);
		}

		public void Apply(IUserSetup setup)
		{
			//TODO: ändra 
			_unitOfWorkAction(uow => setup.Apply(uow.Current(), Person, Person.PermissionInformation.Culture()));
			var setupTenant = setup as ITenantUserSetup;
			if (setupTenant != null)
			{
				_tenantUnitOfWorkAction(tenantSesssion => setupTenant.Apply(tenantSesssion, Person, this));
			}
			_userSetups.Add(setup);
		}

		public void Apply(IUserDataSetup setup)
		{
			_unitOfWorkAction(uow => setup.Apply(uow, Person, Person.PermissionInformation.Culture()));
			_userDataSetups.Add(setup);
		}


		public IEnumerable<object> Applied { get { return _userSetups.Cast<object>().Union(_userDataSetups); } }
		public string LogOnName { get; private set; }
		public void Set(string logonName)
		{
			LogOnName = logonName;
		}

		public IPerson Person { get; private set; }
		public CultureInfo Culture { get { return Person.PermissionInformation.Culture(); } }
	}
}