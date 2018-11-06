using System.Collections.Generic;
using Autofac;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Wfm.Adherence.Test.InfrastructureTesting
{
	public class NoMessageSender : IMessageSender
	{
		public void Send(Message message)
		{
		}

		public void SendMultiple(IEnumerable<Message> messages)
		{
		}
	}

	public class InfrastructureTestStuff
	{
		//internal static IPerson loggedOnPerson;
		//internal static IApplicationData ApplicationData;
		internal static IDataSource DataSource;
		private static int dataHash = 0;

		public static void Before()
		{
			if (dataHash != 0)
			{
				DataSourceHelper.RestoreApplicationDatabase(dataHash);
				DataSourceHelper.RestoreAnalyticsDatabase(dataHash);
				return;
			}

			var builder = new ContainerBuilder();
			var toggles = new FakeToggleManager();
			builder.RegisterModule(new CommonModule(new IocConfiguration(new IocArgs(new ConfigReader()) {FeatureToggle = "http://notinuse"}, toggles)));
			builder.RegisterType<FakeToggleManager>().As<IToggleManager>().SingleInstance();
			builder.RegisterType<NoMessageSender>().As<IMessageSender>().SingleInstance();
			builder.RegisterType<FakeHangfireEventClient>().As<IHangfireEventClient>().SingleInstance();
			var container = builder.Build();

//			IDictionary<string, string> appSettings = new Dictionary<string, string>();
//			ConfigurationManager.AppSettings.AllKeys.ToList().ForEach(
//				name => appSettings.Add(name, ConfigurationManager.AppSettings[name]));

//			Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
			DataSource = DataSourceHelper.CreateDatabasesAndDataSource(DataSourceHelper.MakeFromContainer(container));

			//loggedOnPerson = PersonFactory.CreatePerson("logged on person");

			//ApplicationData = new ApplicationData(appSettings, null);

//			BusinessUnitFactory.BusinessUnitUsedInTest.SetId(null);
//			StateHolderProxyHelper.CreateSessionData(loggedOnPerson, DataSource, BusinessUnitFactory.BusinessUnitUsedInTest);
//
//			StateHolderProxyHelper.ClearAndSetStateHolder(new FakeState {ApplicationScopeData = ApplicationData});

//			persistLoggedOnPerson();
//			persistBusinessUnit();
//			deleteAllAggregates();
			dataHash = 7545;

			DataSourceHelper.BackupApplicationDatabase(dataHash);
			DataSourceHelper.BackupAnalyticsDatabase(dataHash);
		}

		public static void After()
		{
			//DataSourceHelper.RestoreApplicationDatabase(dataHash);
		}

		public static void BeforeWithLogon() => BeforeWithLogon(out _);

		public static void BeforeWithLogon(out IPerson person)
		{
			Before();

			createBusinessUnitAndPerson(out person);
			Login(person);
			using (var unitOfWork = DataSource.Application.CreateAndOpenUnitOfWork())
			{
				saveBusinessUnitAndPerson(person, unitOfWork);
				unitOfWork.PersistAll();
			}

//			return new GenericDisposable(() =>
//			{
//				RestoreCcc7Database();
//				RestoreAnalyticsDatabase();
//				Logout();
//			});
		}

		public static void AfterWithLogon()
		{
			//CurrentAuthorization.DefaultTo(null);
			Logout();
			After();
		}

		private static void createBusinessUnitAndPerson(out IPerson person)
		{
			BusinessUnitFactory.CreateNewBusinessUnitUsedInTest();

			person = PersonFactory.CreatePerson(RandomName.Make());
		}

		private static void saveBusinessUnitAndPerson(IPerson person, IUnitOfWork uow)
		{
			var session = uow.FetchSession();

			((IDeleteTag) person).SetDeleted();
			session.Save(person);

			//force a insert
			var businessUntId = BusinessUnitFactory.BusinessUnitUsedInTest.Id.Value;
			BusinessUnitFactory.BusinessUnitUsedInTest.SetId(null);
			session.Save(BusinessUnitFactory.BusinessUnitUsedInTest, businessUntId);
			session.Flush();
		}

//		private static readonly SelectivePrincipalContext principalContext;
//
//		static DatabaseStuff()
//		{
//			principalContext = SelectivePrincipalContext.Make();
//		}
//
		public static void Login(IPerson person)
		{
//			StateHolderProxyHelper.SetupFakeState(
//				DataSource,
//				person,
//				BusinessUnitFactory.BusinessUnitUsedInTest);

			var principalContext = SelectivePrincipalContext.Make();
			var principal = new TeleoptiPrincipalFactory().MakePrincipal(person, DataSource, BusinessUnitFactory.BusinessUnitUsedInTest, null);
			principalContext.SetCurrentPrincipal(principal);

			//CurrentAuthorization.DefaultTo(new FullPermission());
		}


		public static void Logout()
		{
			var principalContext = SelectivePrincipalContext.Make();
			principalContext.SetCurrentPrincipal(null);
		}
	}
}