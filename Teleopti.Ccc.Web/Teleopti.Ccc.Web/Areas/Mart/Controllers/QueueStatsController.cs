﻿using System.Linq;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Web.Areas.Mart.Core;
using Teleopti.Ccc.Web.Areas.Mart.Models;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Messages.Rta;

namespace Teleopti.Ccc.Web.Areas.Mart.Controllers
{
	[KeyBasedAuthentication] // Enable authentication via a key
	[Authorize] // Require some form of authentication
	public class QueueStatsController : ApiController
	{
		private readonly IQueueStatHandler _queueStatHandler;
		private IServiceBusEventPublisher _serviceBusSender;

		public QueueStatsController(IQueueStatHandler queueStatHandler, IServiceBusEventPublisher serviceBusSender)
		{
			_queueStatHandler = queueStatHandler;
			_serviceBusSender = serviceBusSender;
		}

		[Route("api/Mart/QueueStats/PostIntervalsCompleted")]
		[HttpPost]
		public void PostIntervalsCompleted([FromBody] QueueDataCompleted queueDataCompleted)
		{
			var headers = getHeader();
			// Call some kind of logic to do comparison of forecasted and actual calls and then send notification
			if (_serviceBusSender.EnsureBus())
			{
				_serviceBusSender.Publish(new FactQueueUpdatedMessage
				{
					Datasource = headers.DataSource,
					SourceId = headers.SourceId,
					DataSentUpUntilInterval = queueDataCompleted.DataSentUpUntilInterval
				});
			}
		}

		[Route("api/Mart/QueueStats/PostIntervals")]
		[HttpPost]
		public void PostIntervals([FromBody]IEnumerable<QueueStatsModel> queueStatsModels)
		{
			var headers = getHeader();
			_queueStatHandler.Handle(queueStatsModels, headers.DataSource, headers.SourceId, headers.DatabaseLatency);
		}

		private postHeader getHeader()
		{
			var principal = RequestContext.Principal as ClaimsPrincipal;
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

			if (string.IsNullOrEmpty(dataSource))
				throw new ArgumentException();

			return new postHeader {DataSource = dataSource, SourceId = sourceId, DatabaseLatency = latency};
		}

		private class postHeader
		{
			public int SourceId { get; set; }
			public string DataSource { get; set; }
			public int DatabaseLatency { get; set; }
		}
	}

	public class QueueDataCompleted
	{
		public string DataSentUpUntilInterval { get; set; }
	}
}