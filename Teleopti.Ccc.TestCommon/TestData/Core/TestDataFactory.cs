using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;

namespace Teleopti.Ccc.TestCommon.TestData.Core
{
	public class TestDataFactory
	{
		private readonly ICurrentUnitOfWork _unitOfWorkAction;
		private readonly ICurrentTenantSession _tenantSession;
		private readonly ITenantUnitOfWork _tenantUnitOfWork;

		public TestDataFactory(
			ICurrentUnitOfWork unitOfWorkAction, 
			ICurrentTenantSession tenantSession,
			ITenantUnitOfWork tenantUnitOfWork)
		{
			_unitOfWorkAction = unitOfWorkAction;
			_tenantSession = tenantSession;
			_tenantUnitOfWork = tenantUnitOfWork;
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
					person.Person,
					_unitOfWorkAction,
					_tenantSession,
					_tenantUnitOfWork
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