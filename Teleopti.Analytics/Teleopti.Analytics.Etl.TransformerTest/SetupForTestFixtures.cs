using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Events;
using BusinessUnitFactory=Teleopti.Analytics.Etl.TransformerTest.FakeData.BusinessUnitFactory;

namespace Teleopti.Analytics.Etl.TransformerTest
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

            applicationData = StateHolderProxyHelper.CreateApplicationData(mocks.StrictMock<IMessageBroker>());
            loggedOnPerson = StateHolderProxyHelper.CreateLoggedOnPerson();
            sessionData = StateHolderProxyHelper.CreateSessionData(loggedOnPerson, applicationData, _bu);

            StateHolderProxyHelper.SetStateReaderExpectations(stateMock, applicationData, sessionData);
            StateHolderProxyHelper.ClearAndSetStateHolder(stateMock);

            mocks.ReplayAll();                      
        }
    }
}