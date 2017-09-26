﻿using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class Rta
	{
		public static string LogOutBySnapshot = "CCC Logged out";

		private readonly TenantLoader _tenantLoader;
		private readonly ActivityChangeChecker _checker;
		private readonly IContextLoader _contextLoader;
		private readonly IStateQueueWriter _queueWriter;
		private readonly IStateQueueReader _queueReader;
		private readonly WithAnalyticsUnitOfWork _analytics;
		private readonly StateQueueTenants _tenants;
		private readonly IRtaTracer _tracer;

		public Rta(
			TenantLoader tenantLoader,
			ActivityChangeChecker checker,
			IContextLoader contextLoader,
			IStateQueueWriter queueWriter,
			IStateQueueReader queueReader,
			WithAnalyticsUnitOfWork analytics,
			StateQueueTenants tenants,
			IRtaTracer tracer)
		{
			_tenantLoader = tenantLoader;
			_checker = checker;
			_contextLoader = contextLoader;
			_queueWriter = queueWriter;
			_queueReader = queueReader;
			_analytics = analytics;
			_tenants = tenants;
			_tracer = tracer;
		}

		[LogInfo]
		[TenantScope]
		public virtual void Enqueue(BatchInputModel batch)
		{
			validateAuthenticationKey(batch);
			_tenants.Poke();
			_analytics.Do(() => _queueWriter.Enqueue(batch));
		}

		[LogInfo]
		[TenantScope]
		public virtual bool QueueIteration(string tenant)
		{
			var input = _analytics.Get(() => _queueReader.Dequeue());
			if (input == null) return false;
			process(input);
			return true;
		}

		[LogInfo]
		[TenantScope]
		public virtual void Process(BatchInputModel batch)
		{
			process(batch);
		}

		private void process(BatchInputModel batch)
		{
			_tracer.StateProcessing(batch.States.Select(x => x.TraceInfo));
			validateAuthenticationKey(batch);
			_contextLoader.ForBatch(batch);
			if (batch.CloseSnapshot)
				_contextLoader.ForClosingSnapshot(batch.SnapshotId.Value, batch.SourceId);
		}

		private void validateAuthenticationKey(BatchInputModel input)
		{
			input.AuthenticationKey = LegacyAuthenticationKey.MakeEncodingSafe(input.AuthenticationKey);
			if (!_tenantLoader.Authenticate(input.AuthenticationKey))
			{
				_tracer.InvalidAuthenticationKey(input.States.Select(x => x.TraceInfo));
				throw new InvalidAuthenticationKeyException("You supplied an invalid authentication key. Please verify the key and try again.");
			}
		}

		[LogInfo]
		[TenantScope]
		public virtual void CheckForActivityChanges(string tenant)
		{
			_checker.CheckForActivityChanges();
		}
	}
}
