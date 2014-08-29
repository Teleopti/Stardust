#region Imports

using System.Collections.Generic;
using log4net.Config;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.DomainTest.Common;
using Teleopti.Ccc.DomainTest.Helper;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Events;
using Configuration=NHibernate.Cfg.Configuration;

#endregion

namespace Teleopti.Ccc.DomainTest
{
    /// <summary>
    /// Setup fixture for assembly
    /// </summary>
    [SetUpFixture]
    public class SetupFixtureForAssembly
    {
        internal static IPerson loggedOnPerson;
        internal  MockRepository mocks;
        private IState stateMock;
        internal static IApplicationData applicationData;
        private static ISessionData sessionData;
        
        /// <summary>
        /// Runs before any test.
        /// </summary>
        [SetUp]
        public void RunBeforeAnyTest()
        {
            mocks = new MockRepository();
            stateMock = mocks.StrictMock<IState>();

            //entityinfo
            IDictionary<string, string> ret = new Dictionary<string, string>();
            ret.Add("dialect", "NHibernate.Dialect.MsSql2005Dialect");
            Configuration cfg = new Configuration();
            cfg.SetNamingStrategy(TeleoptiDatabaseNamingStrategy.Instance);
            cfg.SetProperties(ret);
            cfg.AddAssembly("Teleopti.Ccc.Domain");

            applicationData = StateHolderProxyHelper.CreateApplicationData(mocks.StrictMock<IMessageBrokerComposite>());
            loggedOnPerson = StateHolderProxyHelper.CreateLoggedOnPerson();
            sessionData = StateHolderProxyHelper.CreateSessionData(loggedOnPerson, applicationData, BusinessUnitFactory.BusinessUnitUsedInTest);

            StateHolderProxyHelper.SetStateReaderExpectations(stateMock, applicationData, sessionData);
            StateHolderProxyHelper.ClearAndSetStateHolder(stateMock);

            mocks.ReplayAll();

            BasicConfigurator.Configure(new DoNothingAppender());
        }
    }
}