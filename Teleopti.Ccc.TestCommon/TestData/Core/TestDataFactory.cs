using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Core
{
	public class TestDataFactory
	{
		private readonly Action<Action<IUnitOfWork>> _unitOfWorkAction;

		public TestDataFactory(Action<Action<IUnitOfWork>> unitOfWorkAction)
		{
			_unitOfWorkAction = unitOfWorkAction;
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
			return AddPerson(name, new Name("Person", name));
		}

		protected PersonDataFactory AddPerson(string referenceName, Name actualName)
		{
			referenceName = trimName(referenceName);

			if (_persons.ContainsKey(referenceName))
				return _persons[referenceName];

			var person = new PersonDataFactory(
				actualName,
				new[] { new UserConfigurable { Name = referenceName } },
				_unitOfWorkAction
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