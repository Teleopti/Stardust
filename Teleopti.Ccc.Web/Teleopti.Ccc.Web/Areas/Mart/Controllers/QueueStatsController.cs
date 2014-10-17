using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Teleopti.Ccc.Web.Areas.Mart.Controllers
{
	public class QueueStatsController : ApiController
	{
		//private readonly IQueueStatHandler _queueStatHandler;

		//public QueueStatsController(IQueueStatHandler queueStatHandler)
		//{
		//	_queueStatHandler = queueStatHandler;
		//}

		// POST api/<controller>
		public void Post([FromBody]Stat value)
		{
		}

	}

	public interface IQueueStatHandler
	{
	}

	public class QueueStatHandler : IQueueStatHandler
	{
		
	}
	public class Stat

{
	public string Queue { get; set; }
}
}