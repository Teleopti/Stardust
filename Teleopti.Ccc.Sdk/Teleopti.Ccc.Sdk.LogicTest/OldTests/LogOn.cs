using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    public static class LogOn
    {
		public static Person loggedOnPerson;

		public static void RunAsPeterWestlinJunior()
        {
            var state = new FakeState();
	        var ds = new DataSource(UnitOfWorkFactoryFactoryForTest.CreateUnitOfWorkFactory("for test"), null, null);
            var applicationData = StateHolderProxyHelper.CreateApplicationData(null);
            var businessUnit = BusinessUnitUsedInTests.BusinessUnit;
            
            loggedOnPerson = new Person().WithName(new Name("Peter", "Westlin Junior")).WithId();
            
            StateHolderProxyHelper.ClearAndSetStateHolder(loggedOnPerson, businessUnit, applicationData, ds, state);
        }
    }
}