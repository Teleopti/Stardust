using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class OptimizationCallbackInfo
	{
		public OptimizationCallbackInfo(IPerson agent, bool wasSuccessful, int number)
		{
			Name = agent.Name.ToString(NameOrderOption.FirstNameLastName);
			WasSuccessful = wasSuccessful;
			Number = number;
		}

		public OptimizationCallbackInfo(ITeamBlockInfo teamBlockInfo, bool wasSuccessful, int number)
		{
			Name = teamBlockInfo.TeamInfo.Name;
			WasSuccessful = wasSuccessful;
			Number = number;
		}

		public string Name { get; }
		public bool WasSuccessful { get; }
		public int Number { get; }
	}
}