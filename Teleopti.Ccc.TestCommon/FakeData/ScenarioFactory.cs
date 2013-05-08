using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// Creating test data for scenario domain object
    /// </summary>
    public static class ScenarioFactory
    {
        /// <summary>
        /// Creates an scenario aggregate.
        /// </summary>
        /// <returns></returns>
        public static IScenario CreateScenarioAggregate()
        {
            IScenario ret = new Scenario("Default");

            return ret;
        }

        /// <summary>
        /// Creates an scenario aggregate.
        /// </summary>
        /// <returns></returns>
        public static Scenario CreateScenarioAggregate(string name,
                                                       bool defaultWorkspace)
        {
            Scenario ret = new Scenario(name);
            ret.DefaultScenario = defaultWorkspace;
            return ret;
        }

		public static Scenario CreateScenarioWithId(string name, bool defaultWorkspace)
		{
			Scenario ret = new Scenario(name);
			ret.SetId(Guid.NewGuid());
			ret.DefaultScenario = defaultWorkspace;
			return ret;
		}

        /// <summary>
        /// Creates a scenario aggregate list.
        /// </summary>
        /// <returns></returns>
        public static IList<IScenario> CreateScenarioAggregateList()
        {
            IList<IScenario> scenarioList = new List<IScenario>();
            scenarioList.Add(CreateScenarioAggregate("Default", true));
            scenarioList.Add(CreateScenarioAggregate("Scenario 1", true));
            scenarioList.Add(CreateScenarioAggregate("Scenario 2", true));
            return scenarioList;
        }

				public static IScenario ScenarioWithId()
				{
					var scenario = new Scenario("sdf");
					scenario.SetId(Guid.NewGuid());
					return scenario;
				}
    }
}