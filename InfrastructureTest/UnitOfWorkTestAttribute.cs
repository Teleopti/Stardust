using System;
using NHibernate;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
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
			//SetupFixtureForAssembly.RestoreCcc7Database();
		}

		public static void Before(out IPerson loggedOnPerson, out IUnitOfWork unitOfWork, out ISession session)
		{

			BusinessUnitFactory.SetBusinessUnitUsedInTestToNull();

			var Mocks = new MockRepository();
			var stateMock = Mocks.StrictMock<IState>();

			Guid buGuid = Guid.NewGuid();
			BusinessUnitFactory.BusinessUnitUsedInTest.SetId(buGuid);
			loggedOnPerson =
				PersonFactory.CreatePersonWithBasicPermissionInfo(string.Concat("logOnName", Guid.NewGuid().ToString()), string.Empty);

			StateHolderProxyHelper.ClearAndSetStateHolder(Mocks,
				loggedOnPerson,
				BusinessUnitFactory.BusinessUnitUsedInTest,
				SetupFixtureForAssembly.ApplicationData,
				SetupFixtureForAssembly.DataSource,
				stateMock);

			unitOfWork = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork();
			session = unitOfWork.FetchSession();

			((IDeleteTag)loggedOnPerson).SetDeleted();
			session.Save(loggedOnPerson);

			//force a insert
			BusinessUnitFactory.BusinessUnitUsedInTest.SetId(null);
			session.Save(BusinessUnitFactory.BusinessUnitUsedInTest, buGuid);
			session.Flush();
		}

		public static void After(IUnitOfWork unitOfWork, bool cleanUp)
		{
			unitOfWork.Dispose();

			if (cleanUp)
			{
				SetupFixtureForAssembly.RestoreCcc7Database();
				//if (BusinessUnitFactory.BusinessUnitUsedInTest.Id.HasValue)
				//{
				//	using (IUnitOfWork uow = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
				//	{
				//		IBusinessUnitRepository buRep = new BusinessUnitRepository(uow);
				//		buRep.Remove(BusinessUnitFactory.BusinessUnitUsedInTest);
				//		uow.PersistAll();
				//	}
				//}
			}
			else
			{
				SetupFixtureForAssembly.checkThatDbIsEmtpy();
			}
		}

	}
}