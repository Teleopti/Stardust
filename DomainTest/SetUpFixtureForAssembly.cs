using log4net.Config;
using NUnit.Framework;
using Teleopti.Ccc.DomainTest.Common;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest
{
    [SetUpFixture]
    public class SetupFixtureForAssembly
    {
        [SetUp]
        public void RunBeforeAnyTest()
        {
            var stateMock = new FakeState();

	        var dataSource = new DataSource(UnitOfWorkFactoryFactory.CreateUnitOfWorkFactory("for test"), null, null);
            var loggedOnPerson = StateHolderProxyHelper.CreateLoggedOnPerson();
            StateHolderProxyHelper.CreateSessionData(loggedOnPerson, dataSource, BusinessUnitFactory.BusinessUnitUsedInTest);
			
            StateHolderProxyHelper.ClearAndSetStateHolder(stateMock);
			
            BasicConfigurator.Configure(new DoNothingAppender());
        }
    }
}