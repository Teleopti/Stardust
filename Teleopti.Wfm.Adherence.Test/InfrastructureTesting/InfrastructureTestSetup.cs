using System.Collections.Generic;
using Autofac;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Wfm.Adherence.Test.InfrastructureTesting
{
	public class InfrastructureTestSetup
	{
		internal static IDataSource DataSource;
		private static int createdDatabaseHash = 0;

		public static IPerson Before()
		{
			IPerson person;

			ensureDatabase();

			createBusinessUnitAndPerson(out person);
			Login(person);
			using (var unitOfWork = DataSource.Application.CreateAndOpenUnitOfWork())
			{
				saveBusinessUnitAndPerson(person, unitOfWork);
				unitOfWork.PersistAll();
			}

			return person;
		}

		public static void After()
		{
			Logout();
		}

		private static void ensureDatabase()
		{
			if (createdDatabaseHash != 0)
			{
				DataSourceHelper.RestoreApplicationDatabase(createdDatabaseHash);
				DataSourceHelper.RestoreAnalyticsDatabase(createdDatabaseHash);
				return;
			}

			createdDatabaseHash = createDatabase();
		}

		private static int createDatabase()
		{
			DataSource = DataSourceHelper.CreateDatabasesAndDataSource();

			const int someHash = 7545;
			DataSourceHelper.BackupApplicationDatabase(someHash);
			DataSourceHelper.BackupAnalyticsDatabase(someHash);
			return someHash;
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

		public static void Login(IPerson person)
		{
			var principalContext = SelectivePrincipalContext.Make();
			var principal = TeleoptiPrincipalFactory.Make().MakePrincipal(new PersonAndBusinessUnit(person, BusinessUnitFactory.BusinessUnitUsedInTest), DataSource, null);
			principalContext.SetCurrentPrincipal(principal);
		}

		public static void Logout()
		{
			var principalContext = SelectivePrincipalContext.Make();
			principalContext.SetCurrentPrincipal(null);
		}
	}
}