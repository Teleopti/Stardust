using System;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Sdk.ServiceBusTest
{
    [SetUpFixture]
    public class SetupFixtureForAssembly
    {
        [SetUp]
        public void SetupTestAssembly()
        {
            MockRepository mocks = new MockRepository();
            IState state = mocks.StrictMock<IState>();
            IMessageBroker messageBroker = mocks.DynamicMock<IMessageBroker>();
            IApplicationData applicationData = StateHolderProxyHelper.CreateApplicationData(messageBroker);
            IBusinessUnit businessUnit = BusinessUnitFactory.BusinessUnitUsedInTest;

            Expect.Call(messageBroker.IsAlive).Return(false).Repeat.Any();
            mocks.Replay(messageBroker);

            IPerson per = new Person { Name = new Name("Peter", "Westlin Junior") };
            per.SetId(Guid.NewGuid());

            StateHolderProxyHelper.ClearAndSetStateHolder(mocks, per, businessUnit, applicationData, state);

            BasicConfigurator.Configure(new DoNothingAppender());
        }
    }

    public class DoNothingAppender : IAppender
    {
        public void Close()
        {
        }

        public void DoAppend(LoggingEvent loggingEvent)
        {
        }

        public string Name
        {
            get { return "Appender that does nothing, used in tests"; }
            set { }
        }
    }
}
