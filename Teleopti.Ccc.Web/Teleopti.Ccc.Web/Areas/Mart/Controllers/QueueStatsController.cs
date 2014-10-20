using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.Mart.Core;
using Teleopti.Ccc.Web.Areas.Mart.Models;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Mart.Controllers
{
	public class QueueStatsController : ApiController
	{
		private readonly IQueueStatHandler _queueStatHandler;

		public QueueStatsController(IQueueStatHandler queueStatHandler)
		{
			_queueStatHandler = queueStatHandler;
		}

		
		public void Post([FromBody]QueueStatsModel value)
		{
			if (!ModelState.IsValid)
			{
				var mess = Request.CreateErrorResponse(HttpStatusCode.BadRequest, "The Queue statistics are not valid");
				throw new HttpResponseException(mess);
			}
			try
			{
				var result = _queueStatHandler.Handle(value);
			}
			catch (QueueStatException ex)
			{
				var mess = Request.CreateErrorResponse(ex.StatusCode, ex.Message);
				throw new HttpResponseException(mess);
			}
			
		}

	}


}