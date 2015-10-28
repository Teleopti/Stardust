using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.InfrastructureTest.UnitOfWork;
using Teleopti.Ccc.Domain.Infrastructure;
using NHibernate;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Messaging.Client;
using ConfigReader = Teleopti.Ccc.Domain.Config.ConfigReader;


namespace Teleopti.Ccc.InfrastructureTest
{
	[SetUpFixture]
	public class SetupFixtureForAssembly
	{
		internal static IPerson loggedOnPerson;
		internal static IApplicationData ApplicationData;
		private static ISessionData sessionData;
		internal static IDataSource DataSource;

		[SetUp]
		public void BeforeTestSuite()
		{
			var builder = new ContainerBuilder();
			builder.RegisterModule(new CommonModule(new IocConfiguration(new IocArgs(new ConfigReader()) { FeatureToggle = "http://notinuse" }, new FalseToggleManager())));
			builder.RegisterType<FakeToggleManager>().As<IToggleManager>().SingleInstance();
			var container = builder.Build();

			IDictionary<string, string> appSettings = new Dictionary<string, string>();
			ConfigurationManager.AppSettings.AllKeys.ToList().ForEach(
				 name => appSettings.Add(name, ConfigurationManager.AppSettings[name]));

			DataSource = DataSourceHelper.CreateDataSource(container.Resolve<ICurrentPersistCallbacks>(), null);

			loggedOnPerson = PersonFactory.CreatePerson("logged on person");

			MessageBrokerContainerDontUse.Configure(null, null, MessageFilterManager.Instance, new NewtonsoftJsonSerializer(), new NewtonsoftJsonSerializer());

			ApplicationData = new ApplicationData(appSettings, MessageBrokerContainerDontUse.CompositeClient(), null);

			BusinessUnitFactory.BusinessUnitUsedInTest = BusinessUnitFactory.CreateSimpleBusinessUnit("Business unit used in test");
			sessionData = StateHolderProxyHelper.CreateSessionData(loggedOnPerson, DataSource, BusinessUnitFactory.BusinessUnitUsedInTest);

			StateHolderProxyHelper.ClearAndSetStateHolder(
				new FakeState
				{
					IsLoggedIn = true, 
					ApplicationScopeData = ApplicationData, 
					SessionScopeData = sessionData
				});

			persistLoggedOnPerson();
			persistBusinessUnit();
			deleteAllAggregates();

			DataSourceHelper.BackupCcc7Database(123);
			DataSourceHelper.BackupAnalyticsDatabase(123);
		}

		public static void RestoreCcc7Database()
		{
			DataSourceHelper.RestoreCcc7Database(123);
		}

		public static void RestoreAnalyticsDatabase()
		{
			DataSourceHelper.RestoreAnalyticsDatabase(123);
		}

		private static void persistLoggedOnPerson()
		{
			using (var uow = DataSource.Application.CreateAndOpenUnitOfWork())
			{
				new PersonRepository(new ThisUnitOfWork(uow)).Add(loggedOnPerson);
				uow.PersistAll();
			}
		}

		private static void persistBusinessUnit()
		{
			using (var uow = DataSource.Application.CreateAndOpenUnitOfWork())
			{
				new BusinessUnitRepository(uow).Add(BusinessUnitFactory.BusinessUnitUsedInTest);
				uow.PersistAll();
			}
		}

		private static void deleteAllAggregates()
		{
			using (var uow = DataSource.Application.CreateAndOpenUnitOfWork())
			{
				uow.Reassociate(loggedOnPerson);

				var allDbRoots = uow.FetchSession()
					.CreateCriteria(typeof(IAggregateRoot))
					.List<IAggregateRoot>();
				foreach (var aggregateRoot in allDbRoots)
				{
					if (!(aggregateRoot is IPersonWriteProtectionInfo) && !(aggregateRoot is DayOffRules))
					{
						var deleteTag = aggregateRoot as IDeleteTag;
						if(deleteTag==null)
							uow.FetchSession().Delete(aggregateRoot);
						else
							deleteTag.SetDeleted();
					}
				}
				uow.PersistAll();
			}
		}

		[TearDown]
		public void AfterTestSuite()
		{
			DataSource.Application.Dispose();
			if (DataSource.Statistic != null)
				DataSource.Statistic.Dispose();
		}

		public static void CheckThatDbIsEmtpy()
		{
			const string assertMess =
				 @"
After running this test there's still data in db.
If the test executes code that calls PersistAll(),
you have to manually clean up or call CleanUpAfterTest() to restore the database state.
";

			var mocks = new MockRepository();
			var stateMock = mocks.StrictMock<IState>();
			BusinessUnitFactory.BusinessUnitUsedInTest.SetId(Guid.NewGuid());
			StateHolderProxyHelper.ClearAndSetStateHolder(mocks,
																  loggedOnPerson,
																  BusinessUnitFactory.BusinessUnitUsedInTest,
																  ApplicationData,
																	DataSource,
																  stateMock);

			using (IUnitOfWork uowTemp = DataSource.Application.CreateAndOpenUnitOfWork())
			{
				using (uowTemp.DisableFilter(QueryFilter.BusinessUnit))
				{
					ISession s = uowTemp.FetchSession();
					s.CreateSQLQuery(@"delete from PersonWriteProtectionInfo").ExecuteUpdate();
					IList<IAggregateRoot> leftInDb = s.CreateCriteria(typeof(IAggregateRoot))
											  .List<IAggregateRoot>();
					if (leftInDb.Count > 0)
					{
						string mess = string.Concat(assertMess, "\n\nThe problem is with following roots...");
						leftInDb.ForEach(root => mess = string.Concat(mess, "\n", root.GetType(), " : ", root.Id));
						Assert.Fail(mess);
					}
				}
			}
		}

		internal static IDictionary<string, string> Sql2005conf(string connString, int? timeout)
		{
			return DataSourceHelper.CreateDataSourceSettings(connString, timeout, null);
		}






		public static TestScope CreatePersonAndLoginWithOpenUnitOfWork(out IPerson person, out IUnitOfWork unitOfWork)
		{
			createBusinessUnitAndPerson(out person);
			Login(person);
			unitOfWork = DataSource.Application.CreateAndOpenUnitOfWork();
			saveBusinessUnitAndPerson(person, unitOfWork);
			return new TestScope(unitOfWork);
		}

		public class TestScope
		{
			private readonly IUnitOfWork _unitOfWork;

			public TestScope(IUnitOfWork unitOfWork)
			{
				_unitOfWork = unitOfWork;
			}

			public bool CleanUpAfterTest = true;

			public void Teardown()
			{
				//roll back if still open
				_unitOfWork.Dispose();

				if (CleanUpAfterTest)
					RestoreCcc7Database();
				else
					CheckThatDbIsEmtpy();

				// can not logout and clean up state because there are 11 tests at this time that are dependent on that side effect
				//Logout();
			}
		}

		public static IDisposable CreatePersonAndLogin(out IPerson person)
		{
			createBusinessUnitAndPerson(out person);
			Login(person);
			using (var unitOfWork = DataSource.Application.CreateAndOpenUnitOfWork())
			{
				saveBusinessUnitAndPerson(person, unitOfWork);
				unitOfWork.PersistAll();
			}
			return new GenericDisposable(() =>
			{
				RestoreCcc7Database();
				Logout();
			});
		}

		private static void createBusinessUnitAndPerson(out IPerson person)
		{
			BusinessUnitFactory.CreateNewBusinessUnitUsedInTest();

			person = PersonFactory.CreatePerson(RandomName.Make());
		}

		public static void Login(IPerson person)
		{
			StateHolderProxyHelper.SetupFakeState(
				DataSource, 
				person, 
				BusinessUnitFactory.BusinessUnitUsedInTest,
				new ThreadPrincipalContext(new TeleoptiPrincipalFactory()));
		}

		public static void Logout()
		{
			StateHolderProxyHelper.Logout(new ThreadPrincipalContext(null));
		}

		private static void saveBusinessUnitAndPerson(IPerson person, IUnitOfWork uow)
		{
			var session = uow.FetchSession();

			((IDeleteTag)person).SetDeleted();
			session.Save(person);

			//force a insert
			var businessUntId = BusinessUnitFactory.BusinessUnitUsedInTest.Id.Value;
			BusinessUnitFactory.BusinessUnitUsedInTest.SetId(null);
			session.Save(BusinessUnitFactory.BusinessUnitUsedInTest, businessUntId);
			session.Flush();
		}

	}

}
