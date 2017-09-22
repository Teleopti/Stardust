﻿using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Analytics.Etl.IntegrationTest
{
    public static class TestState
    {
		public static TestDataFactory TestDataFactory;
	    public static IUnitOfWork UnitOfWork;
	    public static IBusinessUnit BusinessUnit;
    }
}