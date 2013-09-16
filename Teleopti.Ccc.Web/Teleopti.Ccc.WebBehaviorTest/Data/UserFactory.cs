using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific;
using Teleopti.Interfaces.Domain;
using log4net;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	/// <summary>
	/// Creates a user or do setups for the current user and persist those settings. Also can create and persists collegaues (persons) for the user.
	/// </summary>
	public class UserFactory
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(UserFactory));

		private IUserSetup _cultureSetup = new SwedishCulture();
		private IUserSetup _timeZoneSetup = new UtcTimeZone();

		private readonly DataFactory _dataFactory = new DataFactory(ScenarioUnitOfWorkState.UnitOfWorkAction);
		private readonly IList<IUserSetup> _userSetups = new List<IUserSetup>();
		private readonly IList<IUserDataSetup> _userDataSetups = new List<IUserDataSetup>();
		private readonly IList<IPostSetup> _postSetups = new List<IPostSetup>();
		private readonly AnalyticsDataFactory _analyticsDataFactory = new AnalyticsDataFactory();

		private readonly ICollection<UserFactory> _colleagues = new List<UserFactory>();
		private UserFactory _teamColleague;

		public static UserFactory User()
		{
			const string userNameForMe = "I";
			if (ScenarioContext.Current.Value<UserFactory>(userNameForMe) == null)
			{
				ScenarioContext.Current.Value(userNameForMe, new UserFactory());
			}
			return ScenarioContext.Current.Value<UserFactory>(userNameForMe);
		}

		public static UserFactory User(string userName)
		{
			var trimmedUserName = userName.Trim('\'');
			if (ScenarioContext.Current.Value<UserFactory>(trimmedUserName) == null)
			{
				var rootUser = User();
				rootUser.AddColleague(trimmedUserName);
			}
			return ScenarioContext.Current.Value<UserFactory>(trimmedUserName);
		}

		public void AddColleague() { _colleagues.Add(new UserFactory()); }

		private void AddColleague(string userName)
		{
			var userFactory = new UserFactory();
			userFactory.Setup(new UserConfigurable { Name = userName });
			_colleagues.Add(userFactory);
			ScenarioContext.Current.Value(userName, userFactory);
		}

		public void AddTeamColleague()
		{
			_teamColleague = new UserFactory();
			_colleagues.Add(_teamColleague);
		}

		public UserFactory LastColleague() { return _colleagues.Last(); }
		public UserFactory TeamColleague() { return _teamColleague; }
		public IEnumerable<UserFactory> AllColleagues() { return _colleagues; }

		private class NameInfo
		{
			public string LogOnName;
			public string LastName;
		}

		private static int _userNumber;
		private static NameInfo NextUserName()
		{
			_userNumber++;
			return new NameInfo
							{
								LogOnName = _userNumber.ToString(),
								LastName = _userNumber.ToString().PadLeft(3, '0')
							};
		}

		private static int _colleagueNumber;
		private static NameInfo NextColleagueName()
		{
			_colleagueNumber++;
			return new NameInfo
							{
								LogOnName = _colleagueNumber.ToString(),
								LastName = _colleagueNumber.ToString().PadLeft(3, '0')
							};
		}

		public void Setup(IUserSetup setup)
		{
			_userSetups.Add(setup);
		}

		public void ReplaceSetupByType<T>(IUserSetup setup)
		{
			ReplaceSetupByType<T, IUserSetup>(_userSetups, setup);
		}

		public void Setup(IDataSetup setup)
		{
			_dataFactory.Setup(setup);
		}

		public void Setup(IUserDataSetup setup)
		{
			_userDataSetups.Add(setup);
		}

		public void Setup(IPostSetup postSetup)
		{
			_postSetups.Add(postSetup);
		}

		public void Setup(IAnalyticsDataSetup analyticsDataSetup)
		{
			_analyticsDataFactory.Setup(analyticsDataSetup);
		}

		private void ReplaceSetupByType<TReplace, TSetup>(IList<TSetup> setups, TSetup setup)
		{
			var existing = setups
				.Where(s => typeof(TReplace).IsAssignableFrom(s.GetType()))
				.Select((s, i) => new { s, i }).Single();
			setups.Remove(existing.s);
			setups.Insert(existing.i, setup);
		}

		public void SetupCulture(IUserSetup setup)
		{
			_cultureSetup = setup;
		}

		public void SetupTimeZone(IUserSetup setup)
		{
			_timeZoneSetup = setup;
		}

		public CultureInfo Culture { get { return Person.PermissionInformation.Culture(); } }

		public IPerson Person { get; private set; }

		/// <summary>
		/// Creates ans persists a person with an automatic number as name, plus creates and persists the list of persons who are in the inner Colleague list.
		/// </summary>
		/// <returns></returns>
		public string MakeUser()
		{
			var userName = NextUserName();
			return MakeUser(userName.LogOnName, userName.LastName, TestData.CommonPassword);
		}

		/// <summary>
		/// Creates and persists a person with the given name, plus creates and persists the list of persons who are in the inner Colleague list.
		/// </summary>
		/// <param name="logonName">Name of the logon.</param>
		/// <param name="lastName">The last name.</param>
		/// <param name="password">The password.</param>
		/// <param name="updateReadModel"></param>
		/// <returns>Returns the given logonName</returns>
		public string MakeUser(string logonName, string lastName, string password)
		{
			Person = PersonFactory.CreatePersonWithBasicPermissionInfo(logonName, password);
			Person.Name = new Name("Agent", lastName);

			Log.Info("Making user " + Person.Name);

			MakeMePerson(Person);

			_colleagues.ForEach(colleague =>
			{
				var colleagueName = NextColleagueName();
				colleague.Person = PersonFactory.CreatePerson();
				colleague.Person.Name = new Name("Colleague", colleagueName.LastName);
				colleague.MakeOtherPerson(colleague.Person);
			});

			doPostSetups();
			_colleagues.ForEach(c => c.doPostSetups());
			Resources.Culture = Culture;

			return logonName;
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

		private void MakeMePerson(IPerson person)
		{
			CultureInfo culture = null;

			_dataFactory.Apply();

			ApplyPersonData(person, out culture);

			_analyticsDataFactory.Persist(culture);
		}

		public void MakeOtherPerson(IPerson person)
		{
			CultureInfo culture;
			ApplyPersonData(person, out culture);
		}

		private void ApplyPersonData(IPerson person, out CultureInfo culture)
		{
			CultureInfo cultureInfo = null;

			ScenarioUnitOfWorkState.UnitOfWorkAction(uow =>
				{
					_cultureSetup.Apply(uow, person, null);
					cultureInfo = person.PermissionInformation.Culture();
					_timeZoneSetup.Apply(uow, person, cultureInfo);
					_userSetups.ForEach(s => s.Apply(uow, person, cultureInfo));
					new PersonRepository(uow).Add(person);
				});

			ScenarioUnitOfWorkState.UnitOfWorkAction(uow => _userDataSetups.ForEach(s => s.Apply(uow, person, cultureInfo)));

			culture = cultureInfo;
		}



		private IEnumerable<object> AllSetups
		{
			get
			{
				return _userSetups.Cast<object>()
					.Union(_analyticsDataFactory.Setups)
					.Union(_userDataSetups)
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