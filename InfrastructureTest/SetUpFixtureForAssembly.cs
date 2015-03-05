using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using Teleopti.Ccc.InfrastructureTest.UnitOfWork;
using Teleopti.Ccc.Domain.Infrastructure;
using NHibernate;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Messaging.Client;


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
			IDictionary<string, string> appSettings = new Dictionary<string, string>();
			ConfigurationManager.AppSettings.AllKeys.ToList().ForEach(
				 name => appSettings.Add(name, ConfigurationManager.AppSettings[name]));

			DataSource = DataSourceHelper.CreateDataSource(null, null);

			loggedOnPerson = PersonFactory.CreatePersonWithBasicPermissionInfo("UserThatClenUpDataSource", string.Empty);

			MessageBrokerContainerDontUse.Configure(null, null, MessageFilterManager.Instance);
			ApplicationData = new ApplicationData(appSettings,
									new ReadOnlyCollection<IDataSource>(new List<IDataSource> { DataSource }),
									MessageBrokerContainerDontUse.CompositeClient(), null, null);

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
		}

		public static void RestoreCcc7Database()
		{
			DataSourceHelper.RestoreCcc7Database(123);
		}

		private static void persistLoggedOnPerson()
		{
			using (var uow = DataSource.Application.CreateAndOpenUnitOfWork())
			{
				new PersonRepository(uow).Add(loggedOnPerson);
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
					if (!(aggregateRoot is IPersonWriteProtectionInfo))
						new Repository(uow).Remove(aggregateRoot);
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






		public static void BeforeTestWithOpenUnitOfWork(out IPerson person, out IUnitOfWork unitOfWork)
		{
			CreateFakeStateEntities(out person);
			SetupFakeState(person);
			unitOfWork = DataSource.Application.CreateAndOpenUnitOfWork();
			SaveFakeState(person, unitOfWork);
		}

		public static void AfterTestWithOpenUnitOfWork(IUnitOfWork unitOfWork, bool cleanUp)
		{
			//roll back if still open
			unitOfWork.Dispose();

			if (cleanUp)
				RestoreCcc7Database();
			else
				CheckThatDbIsEmtpy();
		}

		public static void BeforeTest(out IPerson person)
		{
			CreateFakeStateEntities(out person);
			SetupFakeState(person);
			using (var unitOfWork = DataSource.Application.CreateAndOpenUnitOfWork())
			{
				SaveFakeState(person, unitOfWork);
				unitOfWork.PersistAll();
			}
		}

		public static void AfterTest()
		{
			RestoreCcc7Database();
		}

		private static void CreateFakeStateEntities(out IPerson person)
		{
			BusinessUnitFactory.CreateNewBusinessUnitUsedInTest();

			person = PersonFactory.CreatePersonWithBasicPermissionInfo(
				string.Concat("logOnName", Guid.NewGuid().ToString()),
				string.Empty);
		}

		public static void SetupFakeState(IPerson person)
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

		private static void SaveFakeState(IPerson person, IUnitOfWork uow)
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
