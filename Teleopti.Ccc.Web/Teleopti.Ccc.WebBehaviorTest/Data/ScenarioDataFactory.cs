using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public class ScenarioDataFactory
	{
		private readonly DataFactory _dataFactory = new DataFactory(ScenarioUnitOfWorkState.UnitOfWorkAction);
		private readonly AnalyticsDataFactory _analyticsDataFactory = new AnalyticsDataFactory();
		private readonly IList<IDelayedSetup> _delayedSetups = new List<IDelayedSetup>();

		private readonly IDictionary<string, PersonDataFactory> _persons = new Dictionary<string, PersonDataFactory>();

		public ScenarioDataFactory()
		{
			var me = new PersonDataFactory(
				new Name("The", "One"),
				new[]
					{
						new UserConfigurable
							{
								UserName = "1",
								Password = TestData.CommonPassword
							}
					},
				ScenarioUnitOfWorkState.UnitOfWorkAction
				);
			_persons.Add("I", me);
		}

		public bool HasPerson(string name)
		{
			return _persons.ContainsKey(trimName(name));
		}

		public PersonDataFactory Person(string name)
		{
			name = trimName(name);

			if (_persons.ContainsKey(name))
				return _persons[name];

			var person = new PersonDataFactory(
				new Name("Person", name),
				new[] {new UserConfigurable {Name = name}},
				ScenarioUnitOfWorkState.UnitOfWorkAction
				);
			_persons.Add(name, person);
			return person;
		}

		private static string trimName(string name)
		{
			return name.Trim('\'');
		}

		public PersonDataFactory Me()
		{
			return _persons.First().Value;
		}

		public CultureInfo MyCulture { get { return Me().Culture; } }
		public IPerson MePerson { get { return Me().Person; } }

		public AnalyticsDataFactory Analytics()
		{
			return _analyticsDataFactory;
		}

		public void Apply(IUserSetup setup)
		{
			Me().Apply(setup);
		}

		public void Apply(IUserDataSetup setup)
		{
			Me().Apply(setup);
		}

		public void Apply(IDataSetup setup)
		{
			_dataFactory.Apply(setup);
		}

		public void ApplyLater(IDelayedSetup delayedSetup)
		{
			_delayedSetups.Add(delayedSetup);
		}

		public string ApplyDelayed()
		{
			_analyticsDataFactory.Persist(Me().Culture);

			ScenarioUnitOfWorkState.UnitOfWorkAction(uow => _delayedSetups.ForEach(s => s.Apply(Me().Person, uow)));

			Resources.Culture = Me().Culture;
			return Me().LogOnName;
		}

		private IEnumerable<object> AllSetups
		{
			get
			{
				return Me().Applied
				           .Union(_analyticsDataFactory.Setups)
				           .Union(_delayedSetups)
				           .Union(_dataFactory.Applied)
					;
			}
		}

		public IEnumerable<T> UserDatasOfType<T>()
		{
			return from s in AllSetups where typeof(T).IsAssignableFrom(s.GetType()) select (T)s;
		}

		public bool HasSetup<T>()
		{
			return UserDatasOfType<T>().Any();
		}

		public T UserData<T>()
		{
			return UserDatasOfType<T>().Last();
		}
	}
}