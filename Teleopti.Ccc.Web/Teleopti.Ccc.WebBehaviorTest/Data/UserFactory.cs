using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data.User;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;
using log4net;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public class UserFactory
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(UserFactory));

		private IUserSetup _cultureSetup = new SwedishCulture();

		private readonly DataFactory _dataFactory = new DataFactory();
		private readonly IList<IUserSetup> _userSetups = new List<IUserSetup>();
		private readonly IList<IUserDataSetup> _userDataSetups = new List<IUserDataSetup>();
		private readonly IList<IPostSetup> _postSetups = new List<IPostSetup>();
		private readonly AnalyticsDataFactory _analyticsDataFactory = new AnalyticsDataFactory();

		private readonly ICollection<UserFactory> _colleagues = new List<UserFactory>();
		private UserFactory _teamColleague;

		public static UserFactory User()
		{
			if (ScenarioContext.Current.Value<UserFactory>("user") == null)
				ScenarioContext.Current.Value("user", new UserFactory());
			return ScenarioContext.Current.Value<UserFactory>("user");
		}

		public void AddColleague() { _colleagues.Add(new UserFactory()); }
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

		public CultureInfo Culture { get { return Person.PermissionInformation.Culture(); } }
		public IPerson Person { get; private set; }

		public string MakeUser()
		{
			TestDataSetup.EnsureThreadPrincipal();

			var userName = NextUserName();
			Person = PersonFactory.CreatePersonWithBasicPermissionInfo(userName.LogOnName, TestData.CommonPassword);
			Person.Name = new Name("Agent", userName.LastName);

			Log.Write("Making user " + Person.Name);

			MakePerson(Person);

			_colleagues.ForEach(colleague =>
			                    	{
			                    		var colleagueName = NextColleagueName();
			                    		colleague.Person = PersonFactory.CreatePerson();
										colleague.Person.Name = new Name("Colleague", colleagueName.LastName);
			                    		colleague.MakePerson(colleague.Person);
			                    	});

			Resources.Culture = Culture;

			return userName.LogOnName;
		}

		private void MakePerson(IPerson person)
		{
			_dataFactory.Persist();

			_cultureSetup.Apply(person, null);
			var culture = person.PermissionInformation.Culture();
			_userSetups.ForEach(s => s.Apply(person, culture));

			TestDataSetup.UnitOfWorkAction(uow => new PersonRepository(uow).Add(person));

			TestDataSetup.UnitOfWorkAction(uow => _userDataSetups.ForEach(s => s.Apply(uow, person, culture)));

			_postSetups.ForEach(s => s.Apply(person, culture));

			_analyticsDataFactory.Persist(culture);
		}

		private IEnumerable<object> AllSpecs
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

		private IEnumerable<T> QueryData<T>()
		{
			return from s in AllSpecs where typeof (T).IsAssignableFrom(s.GetType()) select (T) s;
		}

		public bool HasSetup<T>()
		{
			return QueryData<T>().Any();
		}

		public T UserData<T>()
		{
			return QueryData<T>().SingleOrDefault();
		}

	}
}