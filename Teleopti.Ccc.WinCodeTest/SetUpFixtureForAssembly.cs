using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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
            var appSettings = new Dictionary<string, string>();
            ConfigurationManager.AppSettings.AllKeys.ToList().ForEach(
                name => appSettings.Add(name, ConfigurationManager.AppSettings[name]));

            var dataSource = new DataSource(UnitOfWorkFactoryFactory.CreateUnitOfWorkFactory("for test"), null, null);

            var loggedOnPerson = StateHolderProxyHelper.CreateLoggedOnPerson();
            StateHolderProxyHelper.CreateSessionData(loggedOnPerson, dataSource, BusinessUnitFactory.BusinessUnitUsedInTest);

            var stateMock = new FakeState();
			
            StateHolderProxyHelper.ClearAndSetStateHolder(stateMock);
        }

        public static void ResetStateHolder()
        {
            var setUpFixture = new SetupFixtureForAssembly();
            setUpFixture.RunBeforeAnyTest();
        }
    }
}