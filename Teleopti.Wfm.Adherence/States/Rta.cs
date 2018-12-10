using System;
using System.Linq;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Wfm.Adherence.Tracer;

namespace Teleopti.Wfm.Adherence.States
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
		private readonly IStateQueueHealthChecker _queueHealth;

		public Rta(
			TenantLoader tenantLoader,
			ActivityChangeChecker checker,
			IContextLoader contextLoader,
			IStateQueueWriter queueWriter,
			IStateQueueReader queueReader,
			WithAnalyticsUnitOfWork analytics,
			StateQueueTenants tenants,
			IRtaTracer tracer,
			IStateQueueHealthChecker queueHealth)
		{
			_tenantLoader = tenantLoader;
			_checker = checker;
			_contextLoader = contextLoader;
			_queueWriter = queueWriter;
			_queueReader = queueReader;
			_analytics = analytics;
			_tenants = tenants;
			_tracer = tracer;
			_queueHealth = queueHealth;
		}

		[LogInfo]
		[TenantScope]
		public virtual void Enqueue(BatchInputModel batch, IRtaExceptionHandler exceptionHandler)
		{
			handleExceptions(exceptionHandler, () =>
			{
				validateAuthenticationKey(batch);
				validateStateCodes(batch);
				_tenants.Poke();
				batch.States.EmptyIfNull()
					.ForEach(x => { x.TraceLog = _tracer.StateReceived(x.UserCode, x.StateCode); });
				_tracer.ProcessEnqueuing(batch?.States?.Count());
				validateQueueHealth();
				_analytics.Do(() => { _queueWriter.Enqueue(batch); });
			});
		}

		[LogInfo]
		[TenantScope]
		public virtual bool QueueIteration(string tenant, IRtaExceptionHandler exceptionHandler)
		{
			var iterated = false;
			handleExceptions(exceptionHandler, () =>
			{
				var input = _analytics.Get(() => _queueReader.Dequeue());
				iterated = input != null;
				if (iterated)
				{
					_tracer.ProcessProcessing(input?.States?.Count());
					process(input);
				}
			});
			return iterated;
		}

		[LogInfo]
		[TenantScope]
		public virtual void Process(BatchInputModel batch, IRtaExceptionHandler exceptionHandler)
		{
			handleExceptions(exceptionHandler, () =>
			{
				batch.States.EmptyIfNull()
					.ForEach(x => { x.TraceLog = _tracer.StateReceived(x.UserCode, x.StateCode); });
				process(batch);
			});
		}

		private void handleExceptions(IRtaExceptionHandler handler, Action call)
		{
			handler = handler ?? new ThrowAll();
			try
			{
				call.Invoke();
			}
			catch (InvalidAuthenticationKeyException e)
			{
				if (!traceAndHandleException(e, handler.InvalidAuthenticationKey))
					throw;
			}
			catch (LegacyAuthenticationKeyException e)
			{
				if (!traceAndHandleException(e, handler.LegacyAuthenticationKey))
					throw;
			}
			catch (InvalidSourceException e)
			{
				if (!traceAndHandleException(e, handler.InvalidSource))
					throw;
			}
			catch (InvalidUserCodeException e)
			{
				if (!traceAndHandleException(e, handler.InvalidUserCode))
					throw;
			}
			catch (AggregateException e)
			{
				var exceptions = e
					.AllExceptions()
					.Where(x => x.GetType() != typeof(AggregateException))
					.ToArray();

				var invalidAuthenticationKeyExceptions = exceptions.OfType<InvalidAuthenticationKeyException>().ToArray();
				if (!invalidAuthenticationKeyExceptions.All(x => traceAndHandleException(x, handler.InvalidAuthenticationKey)))
					throw;
				var legacyAuthenticationKeyExceptions = exceptions.OfType<LegacyAuthenticationKeyException>().ToArray();
				if (!legacyAuthenticationKeyExceptions.All(x => traceAndHandleException(x, handler.LegacyAuthenticationKey)))
					throw;
				var invalidSourceExceptions = exceptions.OfType<InvalidSourceException>().ToArray();
				if (!invalidSourceExceptions.All(x => traceAndHandleException(x, handler.InvalidSource)))
					throw;
				var invalidUserCodeExceptions = exceptions.OfType<InvalidUserCodeException>().ToArray();
				if (!invalidUserCodeExceptions.All(x => traceAndHandleException(x, handler.InvalidUserCode)))
					throw;

				if (!exceptions
					.Except(invalidAuthenticationKeyExceptions)
					.Except(legacyAuthenticationKeyExceptions)
					.Except(invalidSourceExceptions)
					.Except(invalidUserCodeExceptions)
					.All(x => traceAndHandleException(x, handler.OtherException)))
					throw;
			}
			catch (Exception e)
			{
				if (!traceAndHandleException(e, handler.OtherException))
					throw;
			}
		}

		private bool traceAndHandleException<T>(T e, Func<T, bool> handleMethod) where T : Exception
		{
			_tracer.ProcessException(e);
			return handleMethod.Invoke(e);
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
				throw new InvalidAuthenticationKeyException("You supplied an invalid authentication key. Please verify the key and try again.");
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

		private void validateQueueHealth()
		{
			var healthState = _queueHealth.Health();
			if (!healthState.Healthy)
				throw new StateQueueHealthException($"State queue is flooded with {healthState.QueueSize} batches");
		}

		[LogInfo]
		[TenantScope]
		public virtual void CheckForActivityChanges(string tenant)
		{
			_checker.CheckForActivityChanges();
		}
	}
}