using System.Collections.Generic;

namespace Teleopti.Ccc.Intraday.TestApplication
{
	public interface IQueueInfoProvider
	{
		IList<QueueInfo> Provide();
	}
}