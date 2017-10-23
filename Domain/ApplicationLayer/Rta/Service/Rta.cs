using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer;
using Teleopti.Ccc.Domain.Collection;
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
			batch.States.EmptyIfNull()
				.ForEach(x => { x.TraceLog = _tracer.StateReceived(x.UserCode, x.StateCode); });
			validateAuthenticationKey(batch);
			validateStateCodes(batch);
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
			batch.States.EmptyIfNull()
				.ForEach(x => { x.TraceLog = _tracer.StateReceived(x.UserCode, x.StateCode); });
			process(batch);
		}

		private void process(BatchInputModel batch)
		{
			_tracer.For(batch.States.EmptyIfNull().Select(x => x.TraceLog), _tracer.StateProcessing);
			validateAuthenticationKey(batch);
			validateStateCodes(batch);
			_contextLoader.ForBatch(batch);
			if (batch.CloseSnapshot)
				_contextLoader.ForClosingSnapshot(batch.SnapshotId.Value, batch.SourceId);
		}

		private void validateAuthenticationKey(BatchInputModel input)
		{
			input.AuthenticationKey = LegacyAuthenticationKey.MakeEncodingSafe(input.AuthenticationKey);
			if (!_tenantLoader.Authenticate(input.AuthenticationKey))
			{
				var exception = new InvalidAuthenticationKeyException("You supplied an invalid authentication key. Please verify the key and try again.");
				_tracer.ProcessException(exception);
				throw exception;
			}
		}

		private void validateStateCodes(BatchInputModel batch)
		{
			var nullStateCode = batch.States.EmptyIfNull().FirstOrDefault(x => x.StateCode == null);
			if (nullStateCode != null)
			{
				_tracer.InvalidStateCode(nullStateCode.TraceLog);
				throw new InvalidStateCodeException("State code is required");
			}

			var hugeStateCode = batch.States.EmptyIfNull().FirstOrDefault(x => x.StateCode.Length > 300);
			if (hugeStateCode != null)
			{
				_tracer.InvalidStateCode(hugeStateCode.TraceLog);
				throw new InvalidStateCodeException("State code can not exceed 300 characters (including platform type id)");
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