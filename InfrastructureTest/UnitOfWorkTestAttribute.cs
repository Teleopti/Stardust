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
			SetupFixtureForAssembly.RestoreCcc7Database();

			IPerson loggedOnPerson;
			ISession session;
			Before(out loggedOnPerson, out unitOfWork, out session);
		}

		protected override void AfterTest()
		{
			After(unitOfWork, false);
		}

		public static void Before(out IPerson LoggedOnPerson, out IUnitOfWork UnitOfWork, out ISession Session)
		{
			BusinessUnitFactory.SetBusinessUnitUsedInTestToNull();

			var Mocks = new MockRepository();
			var stateMock = Mocks.StrictMock<IState>();

			Guid buGuid = Guid.NewGuid();
			BusinessUnitFactory.BusinessUnitUsedInTest.SetId(buGuid);
			LoggedOnPerson =
				PersonFactory.CreatePersonWithBasicPermissionInfo(string.Concat("logOnName", Guid.NewGuid().ToString()), string.Empty);

			StateHolderProxyHelper.ClearAndSetStateHolder(Mocks,
				LoggedOnPerson,
				BusinessUnitFactory.BusinessUnitUsedInTest,
				SetupFixtureForAssembly.ApplicationData,
				SetupFixtureForAssembly.DataSource,
				stateMock);

			UnitOfWork = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork();
			Session = UnitOfWork.FetchSession();

			((IDeleteTag)LoggedOnPerson).SetDeleted();
			Session.Save(LoggedOnPerson);

			//force a insert
			BusinessUnitFactory.BusinessUnitUsedInTest.SetId(null);
			Session.Save(BusinessUnitFactory.BusinessUnitUsedInTest, buGuid);
			Session.Flush();
		}

		public static void After(IUnitOfWork UnitOfWork, bool skipRollback)
		{
			UnitOfWork.Dispose();
			if (skipRollback)
			{
				if (BusinessUnitFactory.BusinessUnitUsedInTest.Id.HasValue)
				{
					using (IUnitOfWork uow = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
					{
						IBusinessUnitRepository buRep = new BusinessUnitRepository(uow);
						buRep.Remove(BusinessUnitFactory.BusinessUnitUsedInTest);
						uow.PersistAll();
					}
				}
			}
		}



	}
}