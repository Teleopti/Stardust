using Teleopti.Ccc.Domain.Stardust;

namespace Teleopti.Wfm.Administration.Core.Stardust
{
	public interface IPingNode
	{
		bool Ping(WorkerNode node);
	}
}