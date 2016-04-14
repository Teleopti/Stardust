using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.WinCodeTest
{
    [SetUpFixture]
    public class SetupFixtureForAssembly
    {
        [SetUp]
        public void RunBeforeAnyTest()
        {
            var dataSource = new DataSource(UnitOfWorkFactoryFactory.CreateUnitOfWorkFactory("for test"), null, null);
            var loggedOnPerson = StateHolderProxyHelper.CreateLoggedOnPerson();

            var stateMock = new FakeState();
            StateHolderProxyHelper.ClearAndSetStateHolder(loggedOnPerson, BusinessUnitFactory.BusinessUnitUsedInTest, StateHolderProxyHelper.CreateApplicationData(new MessageBrokerCompositeDummy()),dataSource, stateMock);
        }

        public static void ResetStateHolder()
        {
            var setUpFixture = new SetupFixtureForAssembly();
            setUpFixture.RunBeforeAnyTest();
        }
    }
}