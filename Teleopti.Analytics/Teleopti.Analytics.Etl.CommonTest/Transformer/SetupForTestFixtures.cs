using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer
{
	[SetUpFixture]
	public class SetupForTestFixtures
	{
		[OneTimeSetUp]
		public void SetupForAll()
		{
			var stateMock = new FakeState();

			var dataSource = new DataSource(UnitOfWorkFactoryFactoryForTest.CreateUnitOfWorkFactory("for test"), null, null);
			var loggedOnPerson = StateHolderProxyHelper.CreateLoggedOnPerson();
			StateHolderProxyHelper.CreateSessionData(loggedOnPerson, dataSource, BusinessUnitFactory.BusinessUnitUsedInTest);

			StateHolderProxyHelper.ClearAndSetStateHolder(stateMock);
		}
	}
}