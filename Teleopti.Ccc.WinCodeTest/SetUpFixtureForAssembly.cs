using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Scheduling;

namespace Teleopti.Ccc.WinCodeTest
{
    [SetUpFixture]
    public class SetupFixtureForAssembly
    {
		public static IPerson loggedOnPerson;

		[OneTimeSetUp]
        public void OneTimeSetUp()
        {
			TimeZoneGuardForDesktop.Set(new TimeZoneGuard());
            var dataSource = new DataSource(UnitOfWorkFactoryFactoryForTest.CreateUnitOfWorkFactory("for test"), null, null);
            loggedOnPerson = StateHolderProxyHelper.CreateLoggedOnPerson();

            var stateMock = new FakeState();
			StateHolderProxyHelper.PrincipalFactory = new TeleoptiPrincipalForLegacyFactory();
            StateHolderProxyHelper.ClearAndSetStateHolder(loggedOnPerson, BusinessUnitUsedInTests.BusinessUnit, StateHolderProxyHelper.CreateApplicationData(new MessageBrokerCompositeDummy()),dataSource, stateMock);
        }

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			StateHolderProxyHelper.ClearStateHolder();
		}
        
    }
}