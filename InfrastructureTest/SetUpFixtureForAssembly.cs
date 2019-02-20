using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using Autofac;
using log4net.Config;
using Teleopti.Ccc.Domain;
using NHibernate;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;

using Teleopti.Messaging.Client;
using ConfigReader = Teleopti.Ccc.Domain.Config.ConfigReader;


namespace Teleopti.Ccc.InfrastructureTest
{
	[SetUpFixture]
	public class SetupFixtureForAssembly
	{
		internal static IPerson loggedOnPerson;
		internal static IApplicationData ApplicationData;
		internal static IDataSource DataSource;
		private static int dataHash = 0;

		[OneTimeSetUp]
		public void BeforeTestSuite()
		{
			XmlConfigurator.Configure();
			var builder = new ContainerBuilder();
			var toggles = new FakeToggleManager();
			builder.RegisterModule(new CommonModule(new IocConfiguration(new IocArgs(new ConfigReader()) { FeatureToggle = "http://notinuse" }, toggles)));
			builder.RegisterType<FakeToggleManager>().As<IToggleManager>().SingleInstance();
			builder.RegisterType<NoMessageSender>().As<IMessageSender>().SingleInstance();
			builder.RegisterType<FakeHangfireEventClient>().As<IHangfireEventClient>().SingleInstance();
			var container = builder.Build();

			IDictionary<string, string> appSettings = new Dictionary<string, string>();
			ConfigurationManager.AppSettings.AllKeys.ToList().ForEach(
				 name => appSettings.Add(name, ConfigurationManager.AppSettings[name]));

			Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
			DataSource = DataSourceHelper.CreateDatabasesAndDataSource(DataSourceFactoryFactory.MakeFromContainer(container));

			loggedOnPerson = PersonFactory.CreatePerson("logged on person");

			ApplicationData = new ApplicationData(appSettings, null);

			BusinessUnitUsedInTests.BusinessUnit.SetId(null);
			StateHolderProxyHelper.CreateSessionData(loggedOnPerson, DataSource, BusinessUnitUsedInTests.BusinessUnit);

			StateHolderProxyHelper.ClearAndSetStateHolder(new FakeState {ApplicationScopeData_DONTUSE = ApplicationData});

			persistLoggedOnPerson();
			persistBusinessUnit();
			deleteAllAggregates();
			dataHash = 4575;

			DataSourceHelper.BackupApplicationDatabase(dataHash);
			DataSourceHelper.BackupAnalyticsDatabase(dataHash);
		}

		public static void RestoreCcc7Database()
		{
			DataSourceHelper.RestoreApplicationDatabase(dataHash);
		}

		public static void RestoreAnalyticsDatabase()
		{
			DataSourceHelper.RestoreAnalyticsDatabase(dataHash);
		}

		private static void persistLoggedOnPerson()
		{
			using (var uow = DataSource.Application.CreateAndOpenUnitOfWork())
			{
				new PersonRepository(new ThisUnitOfWork(uow), null, null).Add(loggedOnPerson);
				uow.PersistAll();
			}
		}

		private static void persistBusinessUnit()
		{
			using (var uow = DataSource.Application.CreateAndOpenUnitOfWork())
			{
				new BusinessUnitRepository(uow).Add(BusinessUnitUsedInTests.BusinessUnit);
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
					if (!(aggregateRoot is IPersonWriteProtectionInfo) && !(aggregateRoot is PlanningGroupSettings))
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

		[OneTimeTearDown]
		public void AfterTestSuite()
		{
			DataSource.Application.Dispose();
			if (DataSource.Analytics != null)
				DataSource.Analytics.Dispose();
		}

		public static void CheckThatDbIsEmtpy()
		{
			const string assertMess =
				 @"
After running this test there's still data in db.
If the test executes code that calls PersistAll(),
you have to manually clean up or call CleanUpAfterTest() to restore the database state.
";
			
			var stateMock = new FakeState();
			BusinessUnitUsedInTests.BusinessUnit.SetId(Guid.NewGuid());
			StateHolderProxyHelper.ClearAndSetStateHolder(
				loggedOnPerson,
				BusinessUnitUsedInTests.BusinessUnit,
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
						StringBuilder builder = new StringBuilder(assertMess);
						builder.Append("\n\nThe problem is with following roots...");

						leftInDb.ForEach(root => builder.AppendFormat("\n{0} : {1}", root.GetType(), root.Id));
						Assert.Fail(builder.ToString());
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
				RestoreAnalyticsDatabase();
				Logout();
			});
		}

		private static void createBusinessUnitAndPerson(out IPerson person)
		{
			BusinessUnitUsedInTests.Reset();

			person = PersonFactory.CreatePerson(RandomName.Make());
		}

		public static void Login(IPerson person)
		{
			StateHolderProxyHelper.SetupFakeState(
				DataSource, 
				person, 
				BusinessUnitUsedInTests.BusinessUnit);
		}

		public static void Logout()
		{
			StateHolderProxyHelper.Logout();
		}

		private static void saveBusinessUnitAndPerson(IPerson person, IUnitOfWork uow)
		{
			var session = uow.FetchSession();

			((IDeleteTag)person).SetDeleted();
			session.Save(person);

			//force a insert
			var businessUntId = BusinessUnitUsedInTests.BusinessUnit.Id.Value;
			BusinessUnitUsedInTests.BusinessUnit.SetId(null);
			session.Save(BusinessUnitUsedInTests.BusinessUnit, businessUntId);
			session.Flush();
		}

	}

}
