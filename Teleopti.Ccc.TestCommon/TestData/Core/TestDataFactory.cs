using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;

namespace Teleopti.Ccc.TestCommon.TestData.Core
{
	public class TestDataFactory
	{
		protected readonly ICurrentUnitOfWork _unitOfWork;
		private readonly ICurrentTenantSession _tenantSession;
		private readonly ITenantUnitOfWork _tenantUnitOfWork;
		private readonly ISetupResolver _resolver;

		public TestDataFactory(
			ICurrentUnitOfWork unitOfWork,
			ICurrentTenantSession tenantSession,
			ITenantUnitOfWork tenantUnitOfWork,
			ISetupResolver resolver)
		{
			_unitOfWork = unitOfWork;
			_tenantSession = tenantSession;
			_tenantUnitOfWork = tenantUnitOfWork;
			_resolver = resolver;
			DataFactory = new DataFactory(_unitOfWork);
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
					_unitOfWork,
					_tenantSession,
					_tenantUnitOfWork,
					_resolver
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