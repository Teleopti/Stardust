﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Core
{
	public class TestDataFactory
	{
		private readonly Tenant _defaultTenant;
		private readonly Action<Action<IUnitOfWork>> _unitOfWorkAction;
		private readonly Action<Action<ICurrentTenantSession>> _tenantUnitOfWorkAction;

		public TestDataFactory(Tenant defaultTenant, Action<Action<IUnitOfWork>> unitOfWorkAction, Action<Action<ICurrentTenantSession>> tenantUnitOfWorkAction)
		{
			_defaultTenant = defaultTenant;
			_unitOfWorkAction = unitOfWorkAction;
			_tenantUnitOfWorkAction = tenantUnitOfWorkAction;
			DataFactory = new DataFactory(_unitOfWorkAction);
		}

		protected readonly DataFactory DataFactory;
		private readonly IDictionary<string, PersonDataFactory> _persons = new Dictionary<string, PersonDataFactory>();

	    public bool HasPerson(string name)
		{
			return _persons.ContainsKey(trimName(name));
		}

		private static string trimName(string name)
		{
			return name.Trim('\'');
		}

		public PersonDataFactory Person(string name)
		{
			return AddPerson(_defaultTenant, name, new Name("Person", name));
		}

		public IEnumerable<PersonDataFactory> AllPersons()
		{
			return _persons.Values;
		}

		protected void RemoveLastPerson()
		{
			_persons.Remove(_persons.Keys.Last());
		}

		protected PersonDataFactory AddPerson(Tenant tenant, string referenceName, Name actualName)
		{
			referenceName = trimName(referenceName);

			if (_persons.ContainsKey(referenceName))
				return _persons[referenceName];

			var person = new PersonDataFactory(
				tenant,
				actualName,
				new[] { new UserConfigurable { Name = referenceName } },
				_unitOfWorkAction,
				_tenantUnitOfWorkAction
				);
			_persons.Add(referenceName, person);
			return person;
		}

		public PersonDataFactory Me()
		{
			return _persons.First().Value;
		}

		public void Apply(IDataSetup setup)
		{
			DataFactory.Apply(setup);
		}

		public DataFactory Data()
		{
			return DataFactory;
		}
	}
}