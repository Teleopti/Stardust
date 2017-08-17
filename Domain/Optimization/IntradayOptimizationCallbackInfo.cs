using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class IntradayOptimizationCallbackInfo
	{
		public IntradayOptimizationCallbackInfo(IPerson agent, bool wasSuccessful, int numberOfOptimizers)
		{
			Name = agent.Name.ToString(NameOrderOption.FirstNameLastName);
			WasSuccessful = wasSuccessful;
			NumberOfOptimizers = numberOfOptimizers;
		}

		public IntradayOptimizationCallbackInfo(ITeamBlockInfo teamBlockInfo, bool wasSuccessful, int numberOfOptimizers)
		{
			Name = teamBlockInfo.TeamInfo.Name;
			WasSuccessful = wasSuccessful;
			NumberOfOptimizers = numberOfOptimizers;
		}

		public string Name { get; }
		public bool WasSuccessful { get; }
		public int NumberOfOptimizers { get; }
	}
}