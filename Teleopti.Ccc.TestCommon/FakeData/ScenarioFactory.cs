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
                                                       bool defaultWorkspace,
                                                       bool auditTrail)
        {
            Scenario ret = new Scenario(name);
            ret.DefaultScenario = defaultWorkspace;
            ret.AuditTrail = auditTrail;
            return ret;
        }

        /// <summary>
        /// Creates a scenario aggregate list.
        /// </summary>
        /// <returns></returns>
        public static IList<IScenario> CreateScenarioAggregateList()
        {
            IList<IScenario> scenarioList = new List<IScenario>();
            scenarioList.Add(CreateScenarioAggregate("Default", true, true));
            scenarioList.Add(CreateScenarioAggregate("Scenario 1", true, true));
            scenarioList.Add(CreateScenarioAggregate("Scenario 2", true, true));
            return scenarioList;
        }
    }
}