using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfig.Creators
{
    public class ScenarioCreator
    {
        /// <summary>
		/// Creates the specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="defaultScenario"></param>
        /// <param name="enableReporting"></param>
        /// <param name="restricted"></param>
        /// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public IScenario Create(string name, Description description, bool defaultScenario, bool enableReporting, bool restricted)
        {
            IScenario scenario = new Scenario(name)
            {
                Description = description,
                DefaultScenario = defaultScenario,
                EnableReporting = enableReporting,
         		Restricted = restricted
            };
            return scenario;
        }
   }
}
