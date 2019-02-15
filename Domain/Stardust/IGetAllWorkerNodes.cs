using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Stardust
{
	public interface IGetAllWorkerNodes
	{
		IEnumerable<WorkerNode> GetAllWorkerNodes();
	}
}