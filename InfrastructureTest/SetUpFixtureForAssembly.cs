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
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				CleanupHistory(uow);
				uow.PersistAll();
			}
			checkThatDbIsEmtpy();

			DataSource.Application.Close();
			if (DataSource.Statistic != null)
				DataSource.Statistic.Close();	
		}

		public static void CleanupHistory(IUnitOfWork uow)
		{
			var s = uow.FetchSession();
			foreach (var classMetaData in s.SessionFactory.GetAllClassMetadata().Values)
			{
				var entityName = classMetaData.EntityName;
				if (entityName.Contains("_AUD"))
					s.CreateQuery("delete from " + entityName).ExecuteUpdate();
			}
			s.CreateQuery("delete from Revision").ExecuteUpdate();
		}

		public static void checkThatDbIsEmtpy()
		{
			const string assertMess =
				 @"After running last test in suite, there's still data in db.
Every test that's marked as 'SkipRollBack()' have to clean up db rows.
This hasn't been done somewhere. Unfortunatly you will have to search yourself 
in what infrastructuretest this has happened - it is unknown for me.";

			createTemporaryStateHolder();

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

		/// <summary>
		/// Creates a local temporary state holder.
		/// </summary>
		/// <remarks>
		/// we need to rectreated a local temporary mocked stateholder to make the ApplicationData object available in the stateholder when creting
		/// Unit of work in DB empt check. It is because we are in the verified state of the old mock object, so its properies are not available
		/// but we want to read its ApplicationScopeData property
		/// </remarks>
		private static void createTemporaryStateHolder()
		{
			var mocks = new MockRepository();
			var stateMock = mocks.StrictMock<IState>();
			BusinessUnitFactory.BusinessUnitUsedInTest.SetId(Guid.NewGuid());
			StateHolderProxyHelper.ClearAndSetStateHolder(mocks,
																  loggedOnPerson,
																  BusinessUnitFactory.BusinessUnitUsedInTest,
																  ApplicationData,
																	DataSource,
																  stateMock);
		}

		internal static IDictionary<string, string> Sql2005conf(string connString, int? timeout)
		{
			return DataSourceHelper.CreateDataSourceSettings(connString, timeout, null);
		}
	}
}
