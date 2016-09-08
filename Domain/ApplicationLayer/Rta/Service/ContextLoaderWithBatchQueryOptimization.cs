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
	public class ContextLoaderWithBatchQueryOptimization : ContextLoader
	{
		private readonly IConfigReader _config;

		public ContextLoaderWithBatchQueryOptimization(IDatabaseLoader databaseLoader, INow now, StateMapper stateMapper, IAgentStatePersister agentStatePersister, IMappingReader mappingReader, IDatabaseReader databaseReader, IScheduleProjectionReadOnlyReader scheduleProjectionReadOnlyReader, AppliedAdherence appliedAdherence, ProperAlarm appliedAlarm, IConfigReader config) : base(databaseLoader, now, stateMapper, agentStatePersister, mappingReader, databaseReader, scheduleProjectionReadOnlyReader, appliedAdherence, appliedAlarm)
		{
			_config = config;
		}

		public class batchData
		{
			public DateTime now;
			public IEnumerable<PersonOrganizationData> persons;
			public IEnumerable<ScheduledActivity> schedules;
			public MappingsState mappings;
			public IEnumerable<AgentState> agentStates;
		}

		public override void ForBatch(BatchInputModel batch, Action<Context> action)
		{
			var exceptions = new ConcurrentBag<Exception>();

			var batchData = new Lazy<batchData>(() =>
			{
				var dataSourceId = ValidateSourceId(batch);

				var now = _now.UtcDateTime();
				var userCodes = batch.States.Select(x => x.UserCode);
				var persons = _databaseReader.LoadPersonOrganizationDatas(dataSourceId, userCodes);
				var personIds = persons.Select(x => x.PersonId).ToArray();
				var schedules = _scheduleProjectionReadOnlyReader.GetCurrentSchedules(now, personIds);
				var mappings = new MappingsState(() => _mappingReader.Read());
				var agentStates = _agentStatePersister.Get(personIds);

				return new batchData
				{
					now = now,
					persons = persons,
					schedules = schedules,
					mappings = mappings,
					agentStates = agentStates
				};
			});

			// 7 transactions seems like a sweet spot when unit testing
			var transactionCount = (int) Math.Ceiling(batch.States.Count() / _config.ReadValue("RtaBatchTransactions", 7d));
			var transactions = batch.States.Batch(transactionCount);
			Parallel.ForEach(transactions, states =>
			{
				ForBatchSingle(batch, action, batchData, states)
					.ForEach(exceptions.Add);
			});

			if (exceptions.Any())
				throw new AggregateException(exceptions);

		}

		[TenantScope]
		protected virtual IEnumerable<Exception> ForBatchSingle(
			BatchInputModel batch, 
			Action<Context> action, 
			Lazy<batchData> batchData, 
			IEnumerable<BatchStateInputModel> states)
		{
			var exceptions = new List<Exception>();

			WithUnitOfWork(() =>
			{
				var data = batchData.Value;

				states.ForEach(input =>
				{
					try
					{
						var found = false;

						data.persons.Where(x => x.UserCode == input.UserCode)
							.ForEach(x =>
							{
								found = true;

								action.Invoke(new Context(
									data.now,
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
									() => data.agentStates.FirstOrDefault(s => s.PersonId == x.PersonId),
									() => new ScheduleState(data.schedules.Where(s => s.PersonId == x.PersonId).ToArray(), false),
									s => data.mappings,
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