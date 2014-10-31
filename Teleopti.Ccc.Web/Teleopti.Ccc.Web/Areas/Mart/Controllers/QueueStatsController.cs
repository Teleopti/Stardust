using System.Web.Http;
using Teleopti.Ccc.Web.Areas.Mart.Core;
using Teleopti.Ccc.Web.Areas.Mart.Models;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Mart.Controllers
{
	[KeyBasedAuthentication] // Enable authentication via a key
	[Authorize] // Require some form of authentication
	public class QueueStatsController : ApiController
	{
		private readonly IQueueStatHandler _queueStatHandler;

		public QueueStatsController(IQueueStatHandler queueStatHandler)
		{
			_queueStatHandler = queueStatHandler;
		}

		
		public void Post([FromBody]QueueStatsModel value)
		{
				_queueStatHandler.Handle(value);
		}
	}
}