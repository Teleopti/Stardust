using System;
using System.Collections.Concurrent;
using System.Linq;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ContextLoaderWithParalellBatch : ContextLoader
	{
		public ContextLoaderWithParalellBatch(IDatabaseLoader databaseLoader, INow now, StateMapper stateMapper, IAgentStatePersister agentStatePersister, IMappingReader mappingReader, IDatabaseReader databaseReader, IScheduleReader scheduleReader, AppliedAdherence appliedAdherence, ProperAlarm appliedAlarm) : base(databaseLoader, now, stateMapper, agentStatePersister, mappingReader, databaseReader, scheduleReader, appliedAdherence, appliedAlarm)
		{
		}

		public override void ForBatch(BatchInputModel batch, Action<Context> action)
		{
			var exceptions = new ConcurrentBag<Exception>();
			var inputs = from s in batch.States
				select new StateInputModel
				{
					AuthenticationKey = batch.AuthenticationKey,
					PlatformTypeId = batch.PlatformTypeId,
					SourceId = batch.SourceId,
					SnapshotId = batch.SnapshotId,
					UserCode = s.UserCode,
					StateCode = s.StateCode,
					StateDescription = s.StateDescription
				};
			inputs
				.AsParallel()
				.ForAll(input =>
				{
					try
					{
						ForBatchSingle(input, action);
					}
					catch (Exception e)
					{
						exceptions.Add(e);
					}
				});
			if (exceptions.Any())
				throw new AggregateException(exceptions);
		}

		[TenantScope]
		protected virtual void ForBatchSingle(StateInputModel input, Action<Context> action)
		{
			For(input, action);
		}

	}
}