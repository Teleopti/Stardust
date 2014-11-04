using Teleopti.Ccc.Web.Areas.Mart.Models;

namespace Teleopti.Ccc.Web.Areas.Mart.Core
{
	public interface IQueueStatHandler
	{
		void Handle(QueueStatsModel queueData, string dataSource);
	}
}