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
		private readonly IList<IPostSetup> _postSetups = new List<IPostSetup>();

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

		public void SetupCulture(IUserSetup setup)
		{
			Me().SetupCulture(setup);
		}

		public void SetupTimeZone(IUserSetup setup)
		{
			Me().SetupTimeZone(setup);
		}

		public void Setup(IUserSetup setup)
		{
			Me().Setup(setup);
		}

		public void Setup(IUserDataSetup setup)
		{
			Me().Setup(setup);
		}

		public void Setup(IDataSetup setup)
		{
			_dataFactory.Apply(setup);
		}

		public void Setup(IPostSetup postSetup)
		{
			_postSetups.Add(postSetup);
		}

		public void Setup(IAnalyticsDataSetup analyticsDataSetup)
		{
			_analyticsDataFactory.Setup(analyticsDataSetup);
		}

		public string MakeUser()
		{
			_persons.ForEach(p => p.Value.Persist());

			_analyticsDataFactory.Persist(Me().Culture);

			using (var uow = GlobalUnitOfWorkState.CurrentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				_postSetups.ForEach(s =>
					{
						s.Apply(Me().Person, uow);
					});
				uow.PersistAll();
			}

			Resources.Culture = Me().Culture;
			return Me().LogOnName;
		}

		private IEnumerable<object> AllSetups
		{
			get
			{
				return Me().Setups
				           .Union(_analyticsDataFactory.Setups)
				           .Union(_postSetups)
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