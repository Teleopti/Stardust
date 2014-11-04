using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.Mart.Models;

namespace Teleopti.Ccc.Web.Areas.Mart.Core
{
	public interface IQueueStatHandler
	{
		void Handle(IEnumerable<QueueStatsModel> queueData, string nhibName, int logObjectId);
	}
}