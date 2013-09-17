using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;
using Teleopti.Interfaces.Domain;
using log4net;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	/// <summary>
	/// Creates a user or do setups for the current user and persist those settings. Also can create and persists collegaues (persons) for the user.
	/// </summary>
	public class UserFactory
	{
		private readonly DataFactory _dataFactory = new DataFactory(ScenarioUnitOfWorkState.UnitOfWorkAction);
		private readonly PersonDataFactory _personFactory = new PersonDataFactory(new Name("The", "One"), ScenarioUnitOfWorkState.UnitOfWorkAction);
		private readonly AnalyticsDataFactory _analyticsDataFactory = new AnalyticsDataFactory();
		private readonly IList<IPostSetup> _postSetups = new List<IPostSetup>();

		private readonly ICollection<PersonDataFactory> _colleagues = new List<PersonDataFactory>();

		public UserFactory()
		{
			_personFactory.Setup(new UserConfigurable
				{
					UserName = "1", 
					Password = TestData.CommonPassword
				});
		}

		private static void ensureUserFactory()
		{
			if (ScenarioContext.Current.Value<UserFactory>() == null)
			{
				ScenarioContext.Current.Value(new UserFactory());
				var userFactory = ScenarioContext.Current.Value<UserFactory>();
				ScenarioContext.Current.Value("I", userFactory._personFactory);
			}
		}

		public static UserFactory User()
		{
			ensureUserFactory();
			return ScenarioContext.Current.Value<UserFactory>();
		}

		public static PersonDataFactory User(string userName)
		{
			ensureUserFactory();
			var trimmedUserName = userName.Trim('\'');
			if (ScenarioContext.Current.Value<PersonDataFactory>(trimmedUserName) == null)
			{
				var personDataFactory = new PersonDataFactory(new Name("Person", userName), ScenarioUnitOfWorkState.UnitOfWorkAction);
				personDataFactory.Setup(new UserConfigurable { Name = userName });
				ScenarioContext.Current.Value<UserFactory>().AddColleague(personDataFactory);
				ScenarioContext.Current.Value(userName, personDataFactory);
			}
			return ScenarioContext.Current.Value<PersonDataFactory>(trimmedUserName);
		}

		public static bool HasUser(string userName)
		{
			var trimmedUserName = userName.Trim('\'');
			return ScenarioContext.Current.Value<PersonDataFactory>(trimmedUserName) != null;
		}


		private void AddColleague(PersonDataFactory personDataFactory)
		{
			_colleagues.Add(personDataFactory);
		}

		public void Setup(IUserSetup setup)
		{
			_personFactory.Setup(setup);
		}

		public void ReplaceSetupByType<T>(IUserSetup setup)
		{
			_personFactory.ReplaceSetupByType<T>(setup);
		}

		public void Setup(IDataSetup setup)
		{
			_dataFactory.Setup(setup);
		}

		public void Setup(IUserDataSetup setup)
		{
			_personFactory.Setup(setup);
		}

		public void Setup(IPostSetup postSetup)
		{
			_postSetups.Add(postSetup);
		}

		public void Setup(IAnalyticsDataSetup analyticsDataSetup)
		{
			_analyticsDataFactory.Setup(analyticsDataSetup);
		}

		public void SetupCulture(IUserSetup setup)
		{
			_personFactory.SetupCulture(setup);
		}

		public void SetupTimeZone(IUserSetup setup)
		{
			_personFactory.SetupTimeZone(setup);
		}

		public CultureInfo Culture { get { return _personFactory.Person.PermissionInformation.Culture(); } }

		public IPerson Person { get { return _personFactory.Person; } }

		public string MakeUser()
		{
			_dataFactory.Persist();
			_personFactory.Persist();

			_colleagues.ForEach(colleague => colleague.Persist());

			_analyticsDataFactory.Persist(_personFactory.Culture);

			doPostSetups();

			Resources.Culture = Culture;
			return _personFactory.LogOnName;
		}

		private void doPostSetups()
		{
			using (var uow = GlobalUnitOfWorkState.CurrentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				_postSetups.ForEach(s =>
				{
					s.Apply(Person, uow);
				});
				uow.PersistAll();
			}
		}

		private IEnumerable<object> AllSetups
		{
			get
			{
				return _personFactory.Setups.Cast<object>()
					.Union(_analyticsDataFactory.Setups)
					.Union(_postSetups)
					.Union(_dataFactory.Setups)
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
			return UserDatasOfType<T>().SingleOrDefault();
		}

	}
}