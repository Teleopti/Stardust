using Teleopti.Ccc.Infrastructure.Repositories.Stardust;
using Teleopti.Wfm.Administration.Core.Stardust;

namespace Teleopti.Wfm.AdministrationTest.ControllerActions
{
	public class FakePingNode : IPingNode
	{
		public bool Ping(WorkerNode node)
		{
			return true;
		}
	}
}