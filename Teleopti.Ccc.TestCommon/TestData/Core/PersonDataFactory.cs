﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.TestCommon.TestData.Setups.Specific;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Core
{
	public class PersonDataFactory : ILogonName
	{
		private readonly IPerson _person;
		private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly Action<Action<ICurrentTenantSession>> _tenantUnitOfWorkAction;
		private readonly IList<IUserSetup> _userSetups = new List<IUserSetup>();
		private readonly IList<IUserDataSetup> _userDataSetups = new List<IUserDataSetup>();

		public PersonDataFactory(IPerson person, ICurrentUnitOfWork unitOfWork, Action<Action<ICurrentTenantSession>> tenantUnitOfWorkAction)
		{
			_person = person;
			_unitOfWork = unitOfWork;
			_tenantUnitOfWorkAction = tenantUnitOfWorkAction;
			Apply(new Setups.Specific.SwedishCulture());
			Apply(new UtcTimeZone());
		}

		public void Apply(IUserSetup setup)
		{
			setup.Apply(_unitOfWork.Current(), _person, _person.PermissionInformation.Culture());
			_unitOfWork.Current().PersistAll();

			var setupTenant = setup as ITenantUserSetup;
			if (setupTenant != null)
				_tenantUnitOfWorkAction(ses =>
				{
					setupTenant.Apply(ses, Person, this);
				});
			_userSetups.Add(setup);
		}

		public void Apply(IUserDataSetup setup)
		{
			setup.Apply(_unitOfWork, _person, _person.PermissionInformation.Culture());
			_unitOfWork.Current().PersistAll();

			_userDataSetups.Add(setup);
		}

		public IEnumerable<object> Applied { get { return _userSetups.Cast<object>().Union(_userDataSetups); } }
		
		public string LogOnName { get; private set; }
		public void Set(string logonName)
		{
			LogOnName = logonName;
		}
		public IPerson Person { get { return _person; } }
		public CultureInfo Culture { get { return Person.PermissionInformation.Culture(); } }
	}
}