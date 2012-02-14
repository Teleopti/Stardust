using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms.QuickForecast
{
    public class ScenarioModel
    {
        public IScenario Scenario { get; set; }

        public ScenarioModel(IScenario scenario)
        {
            Scenario = scenario;
        }

        public string Name { get { return Scenario.Description.Name; } }
    }
}