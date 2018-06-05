using System.Collections.Generic;

namespace Teleopti.Ccc.Intraday.TestCommon
{
	public interface IWorkloadQueuesProvider
	{
		IList<WorkloadInfo> Provide();
	}
}