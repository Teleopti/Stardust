using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IContextLoader
	{
		void For(StateInputModel input, Action<Context> action);
		void ForBatch(BatchInputModel batch, Action<Context> action);
		void ForAll(Action<Context> action);
		void ForClosingSnapshot(DateTime snapshotId, string sourceId, Action<Context> action);
		void ForSynchronize(Action<Context> action);
	}

	public class ContextLoaderWithPersonOrganizationQueryOptimization : ContextLoader
	{
		private readonly IConfigReader _config;

		public ContextLoaderWithPersonOrganizationQueryOptimization(
			IConfigReader config,
			IDatabaseLoader databaseLoader,
			INow now,
			StateMapper stateMapper,
			IAgentStatePersister agentStatePersister,
			IMappingReader mappingReader,
			IDatabaseReader databaseReader,
			AppliedAdherence appliedAdherence,
			ProperAlarm appliedAlarm) : base(
				databaseLoader,
				now,
				stateMapper,
				agentStatePersister,
				mappingReader,
				databaseReader,
				appliedAdherence,
				appliedAlarm
				)
		{
			_config = config;
		}

		public class batchData
		{
			public IEnumerable<ScheduledActivity> schedules;
			public MappingsState mappings;
			public IEnumerable<AgentStateFound> agentStates;
		}

		public override void ForBatch(BatchInputModel batch, Action<Context> action)
		{
			var exceptions = new ConcurrentBag<Exception>();

			var batchData = new Lazy<batchData>(() =>
			{
				var dataSourceId = ValidateSourceId(batch);

				var userCodes = batch.States.Select(x => x.UserCode);
				var agentStates = _agentStatePersister.Find(dataSourceId, userCodes);

				userCodes
					.Where(x => agentStates.All(s => s.UserCode != x))
					.Select(x => new InvalidUserCodeException($"No person found for UserCode {x}, DataSourceId {dataSourceId}, SourceId {batch.SourceId}"))
					.ForEach(exceptions.Add);

				var personIds = agentStates.Select(x => x.PersonId);
				var schedules = _databaseReader.GetCurrentSchedules(personIds);
				var mappings = new MappingsState(() => _mappingReader.Read());

				return new batchData
				{
					schedules = schedules,
					mappings = mappings,
					agentStates = agentStates
				};
			});

			// 7 transactions seems like a sweet spot when unit testing
			var transactionCount = (int)Math.Ceiling(batch.States.Count() / _config.ReadValue("RtaBatchTransactions", 7d));
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
						data.agentStates
							.Where(x => x.UserCode == input.UserCode)
							.ForEach(x =>
							{
								action.Invoke(new Context(
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
									x.TeamId.GetValueOrDefault(),
									x.SiteId.GetValueOrDefault(),
									() => x,
									() => data.schedules.Where(s => s.PersonId == x.PersonId).ToArray(),
									s => data.mappings,
									c => _agentStatePersister.Update(c.MakeAgentState()),
									_now,
									_stateMapper,
									_appliedAdherence,
									_appliedAlarm
									));
							});
					}
					catch (Exception e)
					{
						exceptions.Add(e);
					}
				});
			});

			return exceptions;
		}

		public override void For(StateInputModel input, Action<Context> action)
		{
			var found = false;

			WithUnitOfWork(() =>
			{
				var mappings = new MappingsState(() => _mappingReader.Read());
				var dataSourceId = ValidateSourceId(input);
				var userCode = input.UserCode;

				_agentStatePersister.Find(dataSourceId, userCode)
					.ForEach(state =>
					{
						found = true;

						action.Invoke(new Context(
							new InputInfo
							{
								PlatformTypeId = input.PlatformTypeId,
								SourceId = input.SourceId,
								SnapshotId = input.SnapshotId,
								StateCode = input.StateCode,
								StateDescription = input.StateDescription
							},
							state.PersonId,
							state.BusinessUnitId,
							state.TeamId.GetValueOrDefault(),
							state.SiteId.GetValueOrDefault(),
							() => state,
							() => _databaseReader.GetCurrentSchedule(state.PersonId),
							s => mappings,
							c => _agentStatePersister.Update(c.MakeAgentState()),
							_now,
							_stateMapper,
							_appliedAdherence,
							_appliedAlarm
							));
					});
			});

			if (!found)
				throw new InvalidUserCodeException($"No person found for SourceId {input.SourceId} and UserCode {input.UserCode}");
		}
	}

	public class ContextLoaderWithBatchQueryOptimization : ContextLoader
	{
		private readonly IConfigReader _config;

		public ContextLoaderWithBatchQueryOptimization(
			IConfigReader config,
			IDatabaseLoader databaseLoader,
			INow now,
			StateMapper stateMapper,
			IAgentStatePersister agentStatePersister,
			IMappingReader mappingReader,
			IDatabaseReader databaseReader,
			AppliedAdherence appliedAdherence,
			ProperAlarm appliedAlarm) : base(
				databaseLoader,
				now,
				stateMapper,
				agentStatePersister,
				mappingReader,
				databaseReader,
				appliedAdherence,
				appliedAlarm
				)
		{
			_config = config;
		}

		public class batchData
		{
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

				var userCodes = batch.States.Select(x => x.UserCode);
				var persons = _databaseReader.LoadPersonOrganizationDatas(dataSourceId, userCodes);
				var personIds = persons.Select(x => x.PersonId).ToArray();
				var schedules = _databaseReader.GetCurrentSchedules(personIds);
				var mappings = new MappingsState(() => _mappingReader.Read());
				var agentStates = _agentStatePersister.Get(personIds);

				return new batchData
				{
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
									() => data.schedules.Where(s => s.PersonId == x.PersonId).ToArray(),
									s => data.mappings,
									c => _agentStatePersister.Update(c.MakeAgentState()),
									_now,
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

	public class ContextLoaderWithBatchConnectionOptimization : ContextLoader
	{
		private readonly IConfigReader _config;

		public ContextLoaderWithBatchConnectionOptimization(
			IConfigReader config,
			IDatabaseLoader databaseLoader,
			INow now,
			StateMapper stateMapper,
			IAgentStatePersister agentStatePersister,
			IMappingReader mappingReader,
			IDatabaseReader databaseReader,
			AppliedAdherence appliedAdherence,
			ProperAlarm appliedAlarm) : base(
				databaseLoader,
				now,
				stateMapper,
				agentStatePersister,
				mappingReader,
				databaseReader,
				appliedAdherence,
				appliedAlarm
				)
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
									() => _databaseReader.GetCurrentSchedule(x.PersonId),
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
									_now,
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

	public class ContextLoaderWithParalellBatch : ContextLoader
	{
		public ContextLoaderWithParalellBatch(
			IDatabaseLoader databaseLoader, 
			INow now, 
			StateMapper stateMapper, 
			IAgentStatePersister agentStatePersister, 
			IMappingReader mappingReader, 
			IDatabaseReader databaseReader, 
			AppliedAdherence appliedAdherence, 
			ProperAlarm appliedAlarm) : base(
				databaseLoader, 
				now, 
				stateMapper, 
				agentStatePersister, 
				mappingReader, 
				databaseReader, 
				appliedAdherence, 
				appliedAlarm
				)
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

	public class ContextLoader : IContextLoader
	{
		private readonly IDatabaseLoader _databaseLoader;
		protected readonly INow _now;
		protected readonly StateMapper _stateMapper;
		protected readonly IAgentStatePersister _agentStatePersister;
		protected readonly IMappingReader _mappingReader;
		protected readonly IDatabaseReader _databaseReader;
		protected readonly AppliedAdherence _appliedAdherence;
		protected readonly ProperAlarm _appliedAlarm;

		public ContextLoader(
			IDatabaseLoader databaseLoader,
			INow now,
			StateMapper stateMapper,
			IAgentStatePersister agentStatePersister,
			IMappingReader mappingReader,
			IDatabaseReader databaseReader,
			AppliedAdherence appliedAdherence,
			ProperAlarm appliedAlarm
			)
		{
			_databaseLoader = databaseLoader;
			_now = now;
			_stateMapper = stateMapper;
			_agentStatePersister = agentStatePersister;
			_mappingReader = mappingReader;
			_databaseReader = databaseReader;
			_appliedAdherence = appliedAdherence;
			_appliedAlarm = appliedAlarm;
		}

		protected int ValidateSourceId(IValidatable input)
		{
			if (string.IsNullOrEmpty(input.SourceId))
				throw new InvalidSourceException("Source id is required");
			int dataSourceId;
			if (!_databaseLoader.Datasources().TryGetValue(input.SourceId, out dataSourceId))
				throw new InvalidSourceException(string.Format("Source id not found {0}", input.SourceId));
			return dataSourceId;
		}

		[AllBusinessUnitsUnitOfWork]
		[ReadModelUnitOfWork]
		[AnalyticsUnitOfWork]
		protected virtual void WithUnitOfWork(Action action)
		{
			action.Invoke();
		}

		[AllBusinessUnitsUnitOfWork]
		[ReadModelUnitOfWork]
		[AnalyticsUnitOfWork]
		protected virtual T WithUnitOfWork<T>(Func<T> func)
		{
			return func.Invoke();
		}

		public virtual void For(StateInputModel input, Action<Context> action)
		{
			var found = false;

			WithUnitOfWork(() =>
			{
				var dataSourceId = ValidateSourceId(input);
				var userCode = input.UserCode;

				_databaseReader.LoadPersonOrganizationData(dataSourceId, userCode)
					.ForEach(x =>
					{
						found = true;

						action.Invoke(new Context(
							new InputInfo
							{
								PlatformTypeId = input.PlatformTypeId,
								SourceId = input.SourceId,
								SnapshotId = input.SnapshotId,
								StateCode = input.StateCode,
								StateDescription = input.StateDescription
							}, 
							x.PersonId,
							x.BusinessUnitId,
							x.TeamId,
							x.SiteId,
							() => _agentStatePersister.Get(x.PersonId),
							() => _databaseReader.GetCurrentSchedule(x.PersonId),
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
							_now,
							_stateMapper,
							_appliedAdherence,
							_appliedAlarm
							));
					});
			});

			if (!found)
				throw new InvalidUserCodeException(string.Format("No person found for SourceId {0} and UserCode {1}", input.SourceId, input.UserCode));

		}

		public virtual void ForBatch(BatchInputModel batch, Action<Context> action)
		{
			var exceptions = new List<Exception>();
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
			inputs.ForEach(input =>
			{
				try
				{
					For(input, action);
				}
				catch (Exception e)
				{
					exceptions.Add(e);
				}
			});
			if (exceptions.Any())
				throw new AggregateException(exceptions);
		}

		public virtual void ForAll(Action<Context> action)
		{
			var mappings = new MappingsState(() => _mappingReader.Read());

			IEnumerable<PersonOrganizationData> persons = null;
			WithUnitOfWork(() =>
			{
				persons = _databaseReader.LoadAllPersonOrganizationData();
			});

			persons.ForEach(x =>
			{
				WithUnitOfWork(() =>
				{
					var state = _agentStatePersister.Get(x.PersonId);
					if (state == null)
						return;
					action.Invoke(new Context(
						null,
						x.PersonId,
						x.BusinessUnitId,
						x.TeamId,
						x.SiteId,
						() => state,
						() => _databaseReader.GetCurrentSchedule(x.PersonId),
						s => mappings,
						c => _agentStatePersister.Update(c.MakeAgentState()),
						_now,
						_stateMapper,
						_appliedAdherence,
						_appliedAlarm
						));
				});
			});
		}

		[AllBusinessUnitsUnitOfWork]
		[ReadModelUnitOfWork]
		public virtual void ForClosingSnapshot(DateTime snapshotId, string sourceId, Action<Context> action)
		{
			var stateCode = Rta.LogOutBySnapshot;

			var missingAgents = _agentStatePersister.GetStatesNotInSnapshot(snapshotId, sourceId);
			var agentsNotAlreadyLoggedOut =
				from a in missingAgents
				where a.StateCode != stateCode
				select a;

			var mappings = new MappingsState(() => _mappingReader.Read());

			agentsNotAlreadyLoggedOut.ForEach(x =>
			{
				action.Invoke(new Context(
					new InputInfo
					{
						StateCode = stateCode,
						PlatformTypeId = Guid.Empty.ToString(),
						SnapshotId = snapshotId
					}, 
					x.PersonId,
					x.BusinessUnitId,
					x.TeamId.GetValueOrDefault(),
					x.SiteId.GetValueOrDefault(),
					() => x,
					() => _databaseReader.GetCurrentSchedule(x.PersonId),
					s => mappings,
					c => _agentStatePersister.Update(c.MakeAgentState()),
					_now,
					_stateMapper,
					_appliedAdherence,
					_appliedAlarm
					));
			});

		}

		[AllBusinessUnitsUnitOfWork]
		[ReadModelUnitOfWork]
		public virtual void ForSynchronize(Action<Context> action)
		{
			var mappings = new MappingsState(() => _mappingReader.Read());

			_agentStatePersister.GetStates()
				.Where(x => x.StateCode != null)
				.ForEach(x =>
				{
					action.Invoke(new Context(
						new InputInfo
						{
							StateCode = x.StateCode,
							PlatformTypeId = x.PlatformTypeId.ToString()
						},
						x.PersonId,
						x.BusinessUnitId,
						x.TeamId.GetValueOrDefault(),
						x.SiteId.GetValueOrDefault(),
						null,
						() => _databaseReader.GetCurrentSchedule(x.PersonId),
						s => mappings,
						null,
						_now,
						_stateMapper,
						_appliedAdherence,
						_appliedAlarm
						));
				});
		}
	}

}