using System.Collections.Generic;

namespace Teleopti.Ccc.Intraday.TestApplication
{
	public interface IWorkloadQueuesProvider
	{
		IList<WorkloadInfo> Provide();
	}
}