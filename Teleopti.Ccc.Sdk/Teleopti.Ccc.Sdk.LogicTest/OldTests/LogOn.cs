﻿using Teleopti.Ccc.Domain.Common;
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
            var state = new FakeState();
	        var ds = new DataSource(UnitOfWorkFactoryFactory.CreateUnitOfWorkFactory("for test"), null, null);
            var applicationData = StateHolderProxyHelper.CreateApplicationData(null);
            var businessUnit = BusinessUnitFactory.BusinessUnitUsedInTest;
            
            var per = new Person().WithName(new Name("Peter", "Westlin Junior")).WithId();
            
            StateHolderProxyHelper.ClearAndSetStateHolder(per, businessUnit, applicationData, ds, state);
        }
    }
}