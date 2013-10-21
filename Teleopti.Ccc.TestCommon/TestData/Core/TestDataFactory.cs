using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Core
{
	public class TestDataFactory
	{
		private readonly Action<Action<IUnitOfWork>> _unitOfWorkAction;

		public TestDataFactory(Action<Action<IUnitOfWork>> unitOfWorkAction, string commonPassword)
		{
			_unitOfWorkAction = unitOfWorkAction;
			_dataFactory = new DataFactory(_unitOfWorkAction);

			var me = new PersonDataFactory(
				new Name("The", "One"),
				new[]
					{
						new UserConfigurable
							{
								UserName = "1",
								Password = commonPassword
							}
					},
				_unitOfWorkAction
				);
			_persons.Add("I", me);

		}

		protected readonly DataFactory _dataFactory;
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
			name = trimName(name);

			if (_persons.ContainsKey(name))
				return _persons[name];

			var person = new PersonDataFactory(
				new Name("Person", name),
				new[] { new UserConfigurable { Name = name } },
				_unitOfWorkAction
				);
			_persons.Add(name, person);
			return person;
		}

		public PersonDataFactory Me()
		{
			return _persons.First().Value;
		}

		public void Apply(IDataSetup setup)
		{
			_dataFactory.Apply(setup);
		}

	}
}