using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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
		private readonly INow _now;
		private readonly IStateQueueWriter _queueWriter;
		private readonly IStateQueueReader _queueReader;
		private readonly WithAnalyticsUnitOfWork _witUnitOfWork;

		public Rta(
			TenantLoader tenantLoader,
			ActivityChangeChecker checker,
			IContextLoader contextLoader,
			INow now,
			IStateQueueWriter queueWriter,
			IStateQueueReader queueReader,
			WithAnalyticsUnitOfWork witUnitOfWork)
		{
			_tenantLoader = tenantLoader;
			_checker = checker;
			_contextLoader = contextLoader;
			_now = now;
			_queueWriter = queueWriter;
			_queueReader = queueReader;
			_witUnitOfWork = witUnitOfWork;
		}

		[LogInfo]
		[TenantScope]
		public virtual void Enqueue(BatchInputModel batch)
		{
			validateAuthenticationKey(batch);
			_witUnitOfWork.Do(() => _queueWriter.Enqueue(batch));
		}

		[LogInfo]
		[TenantScope]
		public virtual void QueueIteration(string tenant)
		{
			var input = _witUnitOfWork.Get(() => _queueReader.Dequeue());
			if (input != null)
				process(input);
		}

		[LogInfo]
		[TenantScope]
		public virtual void Process(BatchInputModel batch)
		{
			process(batch);
		}

		private void process(BatchInputModel batch)
		{
			validateAuthenticationKey(batch);
			_contextLoader.ForBatch(batch);
			if (batch.CloseSnapshot)
				_contextLoader.ForClosingSnapshot(batch.SnapshotId.Value, batch.SourceId);
		}

		private void validateAuthenticationKey(IValidatable input)
		{
			input.AuthenticationKey = LegacyAuthenticationKey.MakeEncodingSafe(input.AuthenticationKey);
			if (!_tenantLoader.Authenticate(input.AuthenticationKey))
				throw new InvalidAuthenticationKeyException("You supplied an invalid authentication key. Please verify the key and try again.");
		}

		[LogInfo]
		[TenantScope]
		public virtual void CheckForActivityChanges(string tenant)
		{
			_checker.CheckForActivityChanges();
		}
	}
}
