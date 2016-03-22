﻿using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace Teleopti.Ccc.Infrastructure.Web
{
	public class HttpTransientErrorDetectionStrategy : ITransientErrorDetectionStrategy
	{
		private readonly List<HttpStatusCode> statusCodes =
			new List<HttpStatusCode>
			{
			HttpStatusCode.GatewayTimeout,
			HttpStatusCode.RequestTimeout,
			HttpStatusCode.ServiceUnavailable,
			};

		public HttpTransientErrorDetectionStrategy() : this(false)
		{
		}

		public HttpTransientErrorDetectionStrategy(bool isNotFoundAsTransient = false)
		{
			if (isNotFoundAsTransient)
				statusCodes.Add(HttpStatusCode.NotFound);
		}

		public bool IsTransient(Exception ex)
		{
			var we = ex as WebException;
			if (we == null)
				return false;

			var response = we.Response as HttpWebResponse;

			var isTransient = response != null
									   && statusCodes.Contains(response.StatusCode);
			return isTransient;
		}
	}
}
