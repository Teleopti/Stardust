using System.Collections.Generic;
using Teleopti.Ccc.Domain.Stardust;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeGetAllWorkerNodes : IGetAllWorkerNodes
	{
		private readonly IList<WorkerNode> _nodes;

		public FakeGetAllWorkerNodes()
		{
			_nodes = new List<WorkerNode>();
		}
		
		public void Has(bool isAlive)
		{
			_nodes.Add(new WorkerNode {Alive = isAlive});
		}
		
		public IEnumerable<WorkerNode> GetAllWorkerNodes()
		{
			return _nodes;
		}
	}
}