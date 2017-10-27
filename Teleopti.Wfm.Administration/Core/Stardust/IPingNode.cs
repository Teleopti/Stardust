using WorkerNode = Teleopti.Ccc.Infrastructure.Repositories.Stardust.WorkerNode;

namespace Teleopti.Wfm.Administration.Core.Stardust
{
	public interface IPingNode
	{
		bool Ping(WorkerNode node);
	}
}