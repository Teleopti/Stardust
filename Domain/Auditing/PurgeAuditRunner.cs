﻿using System;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Audit;

namespace Teleopti.Ccc.Domain.Auditing
{
	public class PurgeAuditRunner
	{
		private readonly IResolve _resolve;
		private static readonly ILog logger = LogManager.GetLogger(typeof(PurgeAuditRunner));

		public PurgeAuditRunner(IResolve resolve)
		{
			_resolve = resolve;
		}

		public void Run()
		{
			var types = _resolve.ConcreteTypesFor(typeof(IPurgeAudit));

			foreach (var type in types)
			{
				try
				{
					var purge = _resolve.Resolve(type) as IPurgeAudit;
					purge.PurgeAudits();
				}
				catch (Exception e)
				{
					logger.Error($"PurgeAudits for type {type} failed.",e);
				}
			}
		}
	}
}
