using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData
{
    public sealed class ScenarioFactory
    {
        #region Constructors (1)

        private ScenarioFactory()
        {
        }

        #endregion Constructors

        #region Methods (1)

        // Public Methods (1)

        public static IList<IScenario> CreateScenarioCollection()
        {
            IList<IScenario> retList = new List<IScenario>();

            IScenario scenario = Ccc.TestCommon.FakeData.ScenarioFactory.CreateScenarioAggregate("Default", true);
            scenario.SetId(Guid.NewGuid());
            RaptorTransformerHelper.SetUpdatedOn(scenario, DateTime.Now);
            retList.Add(scenario);

            scenario = Ccc.TestCommon.FakeData.ScenarioFactory.CreateScenarioAggregate("Worst case", false);
            scenario.SetId(Guid.NewGuid());
            RaptorTransformerHelper.SetUpdatedOn(scenario, DateTime.Now);
            retList.Add(scenario);

            scenario = Ccc.TestCommon.FakeData.ScenarioFactory.CreateScenarioAggregate("Best case", false);
            scenario.SetId(Guid.NewGuid());
            RaptorTransformerHelper.SetUpdatedOn(scenario, DateTime.Now);
            retList.Add(scenario);

            scenario = Ccc.TestCommon.FakeData.ScenarioFactory.CreateScenarioAggregate("Deleted Scenario", false);
            scenario.SetId(Guid.NewGuid());
            RaptorTransformerHelper.SetUpdatedOn(scenario, DateTime.Now);
            ((Scenario)scenario).SetDeleted();
            retList.Add(scenario);

            return retList;
        }

        #endregion Methods
    }
}