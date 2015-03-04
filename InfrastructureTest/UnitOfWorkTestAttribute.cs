using System;
using NHibernate;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.InfrastructureTest.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest
{
	public class UnitOfWorkTestAttribute : InfrastructureTestAttribute
	{
		private IUnitOfWork unitOfWork;

		protected override void BeforeTest()
		{
			IPerson loggedOnPerson;
			ISession session;
			Before(out loggedOnPerson, out unitOfWork, out session);
		}

		protected override void AfterTest()
		{
			After(unitOfWork, true);
		}

		public static void Before(out IPerson loggedOnPerson, out IUnitOfWork unitOfWork, out ISession session)
		{
			BusinessUnitFactory.CreateNewBusinessUnitUsedInTest();

			loggedOnPerson = PersonFactory.CreatePersonWithBasicPermissionInfo(string.Concat("logOnName", Guid.NewGuid().ToString()), string.Empty);

			StateHolderProxyHelper.SetupFakeState(SetupFixtureForAssembly.DataSource, loggedOnPerson, BusinessUnitFactory.BusinessUnitUsedInTest, new ThreadPrincipalContext(new TeleoptiPrincipalFactory()));

			unitOfWork = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork();
			session = unitOfWork.FetchSession();

			((IDeleteTag)loggedOnPerson).SetDeleted();
			session.Save(loggedOnPerson);

			//force a insert
			var businessUntId = BusinessUnitFactory.BusinessUnitUsedInTest.Id.Value;
			BusinessUnitFactory.BusinessUnitUsedInTest.SetId(null);
			session.Save(BusinessUnitFactory.BusinessUnitUsedInTest, businessUntId);
			session.Flush();
		}

		public static void After(IUnitOfWork unitOfWork, bool cleanUp)
		{
			unitOfWork.Dispose();

			if (cleanUp)
				SetupFixtureForAssembly.RestoreCcc7Database();
			else
				SetupFixtureForAssembly.CheckThatDbIsEmtpy();
		}

	}
}