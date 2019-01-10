using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.WinCodeTest
{
    [SetUpFixture]
    public class SetupFixtureForAssembly
    {
        [OneTimeSetUp]
        public void RunBeforeAnyTest()
        {
            var dataSource = new DataSource(UnitOfWorkFactoryFactoryForTest.CreateUnitOfWorkFactory("for test"), null, null);
            var loggedOnPerson = StateHolderProxyHelper.CreateLoggedOnPerson();

            var stateMock = new FakeState();
            StateHolderProxyHelper.ClearAndSetStateHolder(loggedOnPerson, BusinessUnitFactory.BusinessUnitUsedInTest, StateHolderProxyHelper.CreateApplicationData(new MessageBrokerCompositeDummy()),dataSource, stateMock);
        }
    }
}