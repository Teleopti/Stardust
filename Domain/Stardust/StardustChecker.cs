using System.Linq;
using Teleopti.Ccc.Domain.MonitorSystem;

namespace Teleopti.Ccc.Domain.Stardust
{
	public class StardustChecker : IMonitorStep
	{
		private readonly IGetAllWorkerNodes _getAllWorkerNodes;
		public readonly string Output = "{0} nodes found. {1} of them is alive.";

		public StardustChecker(IGetAllWorkerNodes getAllWorkerNodes)
		{
			_getAllWorkerNodes = getAllWorkerNodes;
		}
		
		public MonitorStepResult Execute()
		{
			var nodes = _getAllWorkerNodes.GetAllWorkerNodes();
			var aliveNodesCount = nodes.Count(x => x.Alive);
			return new MonitorStepResult(aliveNodesCount > 0, string.Format(Output, nodes.Count(), aliveNodesCount));
		}

		public string Name { get; } = "Stardust";
	}
}