using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Setups.Specific;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Core
{
	public class PersonDataFactory : ILogonName
	{
		private readonly Guid _personId;
		private readonly Action<Action<ICurrentUnitOfWork>> _unitOfWorkAction;
		private readonly Action<Action<ICurrentTenantSession>> _tenantUnitOfWorkAction;
		private readonly IList<IUserSetup> _userSetups = new List<IUserSetup>();
		private readonly IList<IUserDataSetup> _userDataSetups = new List<IUserDataSetup>();

		public PersonDataFactory(Guid personId, Action<Action<ICurrentUnitOfWork>> unitOfWorkAction, Action<Action<ICurrentTenantSession>> tenantUnitOfWorkAction)
		{
			_personId = personId;
			_unitOfWorkAction = unitOfWorkAction;
			_tenantUnitOfWorkAction = tenantUnitOfWorkAction;
			Apply(new Setups.Specific.SwedishCulture());
			Apply(new UtcTimeZone());
		}

		public void Apply(IUserSetup setup)
		{
			//TODO: ändra 
			_unitOfWorkAction(uow =>
			{
				var person = new PersonRepository(uow).Load(_personId);
				setup.Apply(uow.Current(), person, person.PermissionInformation.Culture());
			});
			var setupTenant = setup as ITenantUserSetup;
			if (setupTenant != null)
				_tenantUnitOfWorkAction(ses => setupTenant.Apply(ses, Person, this));
			_userSetups.Add(setup);
		}

		public void Apply(IUserDataSetup setup)
		{
			_unitOfWorkAction(uow =>
			{
				var person = new PersonRepository(uow).Load(_personId);
				setup.Apply(uow, person, person.PermissionInformation.Culture());
			});
			_userDataSetups.Add(setup);
		}

		public IEnumerable<object> Applied { get { return _userSetups.Cast<object>().Union(_userDataSetups); } }
		
		public string LogOnName { get; private set; }
		public void Set(string logonName)
		{
			LogOnName = logonName;
		}

		public IPerson Person
		{
			get
			{
				IPerson person = null;
				_unitOfWorkAction.Invoke(uow =>
				{
					person = new PersonRepository(uow).Load(_personId);
				});
				return person;
			}
		}

		public CultureInfo Culture { get { return Person.PermissionInformation.Culture(); } }
	}
}