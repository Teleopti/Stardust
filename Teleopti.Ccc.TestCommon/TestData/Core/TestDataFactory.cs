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


		protected readonly ICurrentUnitOfWork _unitOfWork;
		private readonly IResolver _resolver;
		private readonly DataFactory _dataFactory;
		private readonly IDictionary<string, PersonDataFactory> _persons = new Dictionary<string, PersonDataFactory>();

		public TestDataFactory(
			ICurrentUnitOfWork unitOfWork,
			IResolver resolver)
		{
			_unitOfWork = unitOfWork;
			_resolver = resolver;
			_dataFactory = new DataFactory(_unitOfWork);
		}

		public PersonDataFactory Me() => _persons.First().Value;
		public PersonDataFactory Person(string name) => addPerson(name);
		public void Apply(IDataSetup setup) => _dataFactory.Apply(setup);

		private PersonDataFactory addPerson(string name)
		{
			name = trimName(name);

			if (_persons.TryGetValue(name, out var foundPerson)) 
				return foundPerson;
			
			var person = new PersonConfigurable {Name = name};
			_dataFactory.Apply(person);
			foundPerson = _resolver.MakePersonDataFactory();
			foundPerson.Setup(person.Person);
			_persons.Add(name, foundPerson);

			return foundPerson;
		}

		protected IEnumerable<object> Applied => _dataFactory.Applied;

		private static string trimName(string name) => name.Trim('\'');

		public bool HasPerson(string name) => _persons.ContainsKey(trimName(name));
		public void RemoveLastPerson() => _persons.Remove(_persons.Keys.Last());
	}
}