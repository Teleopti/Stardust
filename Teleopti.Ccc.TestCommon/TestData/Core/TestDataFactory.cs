using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Core
{
	public class TestDataFactory
	{
		private readonly Action<Action<ICurrentUnitOfWork>> _unitOfWorkAction;
		private readonly Action<Action<ICurrentTenantSession>> _tenantUnitOfWorkAction;

		public TestDataFactory(Action<Action<ICurrentUnitOfWork>> unitOfWorkAction, Action<Action<ICurrentTenantSession>> tenantUnitOfWorkAction)
		{
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
			return AddPerson(name);
		}

		public IEnumerable<PersonDataFactory> AllPersons()
		{
			return _persons.Values;
		}

		public void RemoveLastPerson()
		{
			_persons.Remove(_persons.Keys.Last());
		}

		protected PersonDataFactory AddPerson(string name)
		{
			name = trimName(name);

			PersonDataFactory foundPerson;
			if (!_persons.TryGetValue(name, out foundPerson))
			{
				var person = new PersonConfigurable {Name = name};
				DataFactory.Apply(person);
				foundPerson = new PersonDataFactory(
					person.Person.Id.Value,
					_unitOfWorkAction,
					_tenantUnitOfWorkAction
					);
				_persons.Add(name, foundPerson);
			}
			return foundPerson;
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