using System.Linq;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Web.Areas.Mart.Core;
using Teleopti.Ccc.Web.Areas.Mart.Models;
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

		public void Post([FromBody]IEnumerable<QueueStatsModel> queueStatsModels)
		{
			// would be possible to send the datasource (the name in the nhib) as a claim
			var principal =  RequestContext.Principal as ClaimsPrincipal;
			Claim claim = null;
			IEnumerable<string> headerValues;
			string dataSource = null;
			int sourceId = 0;
			int latency = 0;

			if (principal != null)
				claim = principal.Claims.FirstOrDefault(c => c.Type.Equals(System.IdentityModel.Claims.ClaimTypes.Locality));
			if (claim != null)
				dataSource = claim.Value;
				
			if (Request.Headers.TryGetValues("sourceId", out headerValues))
				if (!int.TryParse(headerValues.First(), out sourceId))
					throw new ArgumentException();

			if (Request.Headers.TryGetValues("dbLatency", out headerValues))
				latency = int.Parse(headerValues.First());

			if (!string.IsNullOrEmpty(dataSource))
				_queueStatHandler.Handle(queueStatsModels, dataSource, sourceId, latency);
		}
	}
}