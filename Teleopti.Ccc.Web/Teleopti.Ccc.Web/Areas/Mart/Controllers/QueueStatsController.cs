using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Web.Areas.Mart.Core;
using Teleopti.Ccc.Web.Areas.Mart.Models;

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

		public void Post([FromBody]IEnumerable<QueueStatsModel> queueStatsModels)
		{
			IEnumerable<string> headerValues;
			string nhibname = null;
			int sourceId = 0;

			if (Request.Headers.TryGetValues("database", out headerValues))
				nhibname = headerValues.First();
			if (Request.Headers.TryGetValues("sourceId", out headerValues))
				if (!int.TryParse(headerValues.First(), out sourceId))
					throw new ArgumentException();

			_queueStatHandler.Handle(queueStatsModels, nhibname, sourceId);
		}
	}
}