#region Imports

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Events;

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
        private IApplicationData applicationData;
        internal static ISessionData SessionData;

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

            IList<IDataSource> dataSources = new List<IDataSource>();
            dataSources.Add(new DataSource(UnitOfWorkFactoryFactory.CreateUnitOfWorkFactory("for test"), null, null));

            loggedOnPerson = StateHolderProxyHelper.CreateLoggedOnPerson();
            applicationData = new ApplicationData(appSettings, new ReadOnlyCollection<IDataSource>(dataSources), mocks.StrictMock<IMessageBrokerComposite>(), null);
            SessionData = StateHolderProxyHelper.CreateSessionData(loggedOnPerson, applicationData, BusinessUnitFactory.BusinessUnitUsedInTest);

            IState stateMock = mocks.StrictMock<IState>();

            StateHolderProxyHelper.SetStateReaderExpectations(stateMock, applicationData, SessionData);
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