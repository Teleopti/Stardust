using log4net.Appender;
using log4net.Config;
using log4net.Core;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.Sdk.ServiceBusTest
{
    [SetUpFixture]
    public class SetupFixtureForAssembly
    {
        [OneTimeSetUp]
        public void SetupTestAssembly()
        {
            var state = new FakeState();
	        var ds = new DataSource(UnitOfWorkFactoryFactoryForTest.CreateUnitOfWorkFactory("for test"), null, null);
            var applicationData = StateHolderProxyHelper.CreateApplicationData(null);
            var businessUnit = BusinessUnitUsedInTests.BusinessUnit;

            var per = new Person().WithName(new Name("Peter", "Westlin Junior")).WithId();

            StateHolderProxyHelper.ClearAndSetStateHolder(per, businessUnit, applicationData, ds, state);

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
