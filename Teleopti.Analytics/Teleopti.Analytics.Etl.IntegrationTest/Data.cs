﻿using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Analytics.Etl.IntegrationTest
{
    public static class Data
    {
        public static void Apply(IDataSetup setup)
        {
            DataFactoryState.DataFactory.Apply(setup);
        }
    }
}