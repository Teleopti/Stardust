using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ContextLoaderWithBatchConnectionOptimization : ContextLoader
	{
		private readonly IConfigReader _config;

		public ContextLoaderWithBatchConnectionOptimization(IDatabaseLoader databaseLoader, INow now, StateMapper stateMapper, IAgentStatePersister agentStatePersister, IMappingReader mappingReader, IDatabaseReader databaseReader, IScheduleProjectionReadOnlyReader scheduleProjectionReadOnlyReader, AppliedAdherence appliedAdherence, ProperAlarm appliedAlarm, IConfigReader config) : base(databaseLoader, now, stateMapper, agentStatePersister, mappingReader, databaseReader, scheduleProjectionReadOnlyReader, appliedAdherence, appliedAlarm)
		{
			_config = config;
		}

		public override void ForBatch(BatchInputModel batch, Action<Context> action)
		{
			var exceptions = new ConcurrentBag<Exception>();

			// 7 transactions seems like a sweet spot when unit testing
			var transactionCount = (int) Math.Ceiling(batch.States.Count() / _config.ReadValue("RtaBatchTransactions", 7d));
			var transactions = batch.States.Batch(transactionCount);
			Parallel.ForEach(transactions, states =>
			{
				ForBatchSingle(batch, action, states)
					.ForEach(exceptions.Add);
			});
			
			if (exceptions.Any())
				throw new AggregateException(exceptions);

		}

		[TenantScope]
		protected virtual IEnumerable<Exception> ForBatchSingle(
			BatchInputModel batch,
			Action<Context> action,
			IEnumerable<BatchStateInputModel> states)
		{
			var exceptions = new List<Exception>();

			WithUnitOfWork(() =>
			{
				var dataSourceId = ValidateSourceId(batch);
				var now = _now.UtcDateTime();

				states.ForEach(input =>
				{
					try
					{
						var found = false;

						_databaseReader.LoadPersonOrganizationData(dataSourceId, input.UserCode)
							.ForEach(x =>
							{
								found = true;

								action.Invoke(new Context(
									now,
									new InputInfo
									{
										PlatformTypeId = batch.PlatformTypeId,
										SourceId = batch.SourceId,
										SnapshotId = batch.SnapshotId,
										StateCode = input.StateCode,
										StateDescription = input.StateDescription
									},
									x.PersonId,
									x.BusinessUnitId,
									x.TeamId,
									x.SiteId,
									() => _agentStatePersister.Get(x.PersonId),
									() => _scheduleProjectionReadOnlyReader.GetCurrentSchedule(now, x.PersonId),
									s =>
									{
										return new MappingsState(() =>
										{
											var stateCodes =
												new[] {s.Stored?.StateCode, s.Input.StateCode}
													.Distinct()
													.ToArray();
											var activities =
												new[] {s.Schedule.CurrentActivityId(), s.Schedule.PreviousActivityId(), s.Schedule.NextActivityId()}
													.Distinct()
													.ToArray();
											return _mappingReader.ReadFor(stateCodes, activities);
										});
									},
									c => _agentStatePersister.Update(c.MakeAgentState()),
									_stateMapper,
									_appliedAdherence,
									_appliedAlarm
									));
							});

						if (!found)
							throw new InvalidUserCodeException(string.Format("No person found for SourceId {0} and UserCode {1}",
								batch.SourceId, input.UserCode));
					}
					catch (Exception e)
					{
						exceptions.Add(e);
					}
				});
			});

			return exceptions;
		}
	}
}