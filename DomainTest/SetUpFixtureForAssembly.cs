using log4net.Config;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DomainTest.Common;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.MessageBroker.Client.Composite;

namespace Teleopti.Ccc.DomainTest
{
    [SetUpFixture]
    public class SetupFixtureForAssembly
    {
        [SetUp]
        public void RunBeforeAnyTest()
        {
            var mocks = new MockRepository();
            var stateMock = mocks.StrictMock<IState>();

						var dataSource = new DataSource(UnitOfWorkFactoryFactory.CreateUnitOfWorkFactory("for test"), null, null);
            var applicationData = StateHolderProxyHelper.CreateApplicationData(mocks.StrictMock<IMessageBrokerComposite>());
            var loggedOnPerson = StateHolderProxyHelper.CreateLoggedOnPerson();
            var sessionData = StateHolderProxyHelper.CreateSessionData(loggedOnPerson, dataSource, BusinessUnitFactory.BusinessUnitUsedInTest);

            StateHolderProxyHelper.SetStateReaderExpectations(stateMock, applicationData, sessionData);
            StateHolderProxyHelper.ClearAndSetStateHolder(stateMock);

            mocks.ReplayAll();

            BasicConfigurator.Configure(new DoNothingAppender());
        }
    }
}