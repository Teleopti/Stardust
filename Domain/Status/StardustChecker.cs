using System.Linq;
using Teleopti.Ccc.Domain.Stardust;

namespace Teleopti.Ccc.Domain.Status
{
	public class StardustChecker : IStatusStep
	{
		private readonly IGetAllWorkerNodes _getAllWorkerNodes;
		public readonly string Output = "{0} nodes found. {1} of them is alive.";

		public StardustChecker(IGetAllWorkerNodes getAllWorkerNodes)
		{
			_getAllWorkerNodes = getAllWorkerNodes;
		}
		
		public StatusStepResult Execute()
		{
			var nodes = _getAllWorkerNodes.GetAllWorkerNodes();
			var aliveNodesCount = nodes.Count(x => x.Alive);
			return new StatusStepResult(aliveNodesCount > 0, string.Format(Output, nodes.Count(), aliveNodesCount));
		}

		public string Name { get; } = "Stardust";
		public string Description { get; } = "Verifies that long running background executions can run.";
	}
}