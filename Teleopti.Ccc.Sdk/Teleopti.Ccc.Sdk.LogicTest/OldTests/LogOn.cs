using System;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    public static class LogOn
    {
        public static void RunAsPeterWestlinJunior()
        {
            MockRepository mocks = new MockRepository();
            IState state = mocks.StrictMock<IState>();
	        var ds = new DataSource(UnitOfWorkFactoryFactory.CreateUnitOfWorkFactory("for test"), null, null);
            IApplicationData applicationData = StateHolderProxyHelper.CreateApplicationData(null);
            IBusinessUnit businessUnit = BusinessUnitFactory.BusinessUnitUsedInTest;
            
            IPerson per = new Person {Name = new Name("Peter", "Westlin Junior")};
            per.SetId(Guid.NewGuid());
            
            StateHolderProxyHelper.ClearAndSetStateHolder(mocks, per, businessUnit, applicationData, ds, state);
        }
    }
}