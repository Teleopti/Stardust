using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using BusinessUnitFactory = Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData.BusinessUnitFactory;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer
{
    [SetUpFixture]
    public class SetupForTestFixtures
    {
        private IBusinessUnit _bu;
        internal static IPerson loggedOnPerson;
        internal MockRepository mocks;
        private IState stateMock;
        private static IApplicationData applicationData;
        private static ISessionData sessionData;

        [SetUp]
        public void SetupForAll()
        {
            mocks = new MockRepository();
            stateMock = mocks.StrictMock<IState>();

            _bu = BusinessUnitFactory.CreateSimpleBusinessUnit("bu1");
            _bu.SetId(Guid.NewGuid());

	        var dataSource = new DataSource(UnitOfWorkFactoryFactory.CreateUnitOfWorkFactory("for test"), null, null);
					applicationData = StateHolderProxyHelper.CreateApplicationData(mocks.StrictMock<IMessageBrokerComposite>());
            loggedOnPerson = StateHolderProxyHelper.CreateLoggedOnPerson();
						sessionData = StateHolderProxyHelper.CreateSessionData(loggedOnPerson, dataSource, _bu);

            StateHolderProxyHelper.SetStateReaderExpectations(stateMock, applicationData, sessionData);
            StateHolderProxyHelper.ClearAndSetStateHolder(stateMock);

            mocks.ReplayAll();                      
        }
    }
}