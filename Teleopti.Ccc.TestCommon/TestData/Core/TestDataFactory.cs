using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;

namespace Teleopti.Ccc.TestCommon.TestData.Core
{
	public class TestDataFactory
	{
		protected readonly ICurrentUnitOfWork _unitOfWork;
		private readonly IResolver _resolver;

		public static TestDataFactory Make(IUnitOfWork uow, TenantUnitOfWorkManager tenantUnitOfWorkManager) =>
			Make(uow, tenantUnitOfWorkManager, tenantUnitOfWorkManager);

		public static TestDataFactory Make(
			IUnitOfWork uow,
			ICurrentTenantSession currentTenantSession,
			ITenantUnitOfWork tenantUnitOfWork)
		{
			var unitOfWork = new ThisUnitOfWork(uow);
			return new TestDataFactory(
				unitOfWork,
				new LegacyResolver(
					unitOfWork,
					currentTenantSession,
					tenantUnitOfWork
				));
		}

		public TestDataFactory(
			ICurrentUnitOfWork unitOfWork,
			IResolver resolver)
		{
			_unitOfWork = unitOfWork;
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
				foundPerson = _resolver.MakePersonDataFactory();
				foundPerson.Setup(person.Person);
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