using Teleopti.Ccc.Infrastructure.Repositories.Stardust;
using Teleopti.Wfm.Administration.Models.Stardust;

namespace Teleopti.Wfm.Administration.Core.Stardust
{
	public interface IPingNode
	{
		bool Ping(WorkerNode node);
	}
}