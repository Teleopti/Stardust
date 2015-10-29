#region Imports

using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client.Composite;

#endregion

namespace Teleopti.Ccc.WinCodeTest
{
    /// <summary>
    /// Setup fixture for assembly
    /// </summary>
    [SetUpFixture]
    public class SetupFixtureForAssembly
    {
        private MockRepository mocks;
        private IPerson loggedOnPerson;
        private ApplicationData applicationData;

        /// <summary>
        /// Runs before any test.
        /// </summary>
        [SetUp]
        public void RunBeforeAnyTest()
        {
            mocks = new MockRepository();

            IDictionary<string, string> appSettings = new Dictionary<string, string>();
            ConfigurationManager.AppSettings.AllKeys.ToList().ForEach(
                name => appSettings.Add(name, ConfigurationManager.AppSettings[name]));

            var dataSource = new DataSource(UnitOfWorkFactoryFactory.CreateUnitOfWorkFactory("for test"), null, null);

            loggedOnPerson = StateHolderProxyHelper.CreateLoggedOnPerson();
						applicationData = new ApplicationData(appSettings, mocks.StrictMock<IMessageBrokerComposite>(), null);
            StateHolderProxyHelper.CreateSessionData(loggedOnPerson, dataSource, BusinessUnitFactory.BusinessUnitUsedInTest);

            IState stateMock = mocks.StrictMock<IState>();

            StateHolderProxyHelper.SetStateReaderExpectations(stateMock, applicationData);
            StateHolderProxyHelper.ClearAndSetStateHolder(stateMock);
            mocks.ReplayAll();
        }

        public static void ResetStateHolder()
        {
            var setUpFixture = new SetupFixtureForAssembly();
            setUpFixture.RunBeforeAnyTest();
        }
    }
}