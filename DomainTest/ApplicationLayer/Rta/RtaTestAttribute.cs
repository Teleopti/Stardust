using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	[LoggedOff]
	public class RtaTestAttribute : DomainTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);

			system.UseTestDouble<AutoFillKeyValueStore>().For<IKeyValueStorePersister>();
			system.UseTestDouble<AutoFillMappingReader>().For<IMappingReader>();
			system.UseTestDouble<AutoFillCurrentScheduleReadModelReader>().For<IScheduleReader>();
			system.UseTestDouble<AutoFillAgentStatePersister>().For<FakeAgentStatePersister, IAgentStatePersister>();
		}
	}

	// if this one always is empty caches will always be refreshed
	public class AutoFillKeyValueStore : IKeyValueStorePersister
	{
		public void Update(string key, string value)
		{
		}

		public string Get(string key)
		{
			return null;
		}

		public void Delete(string key)
		{
		}
	}

	public class AutoFillMappingReader : IMappingReader
	{
		private readonly IBusinessUnitRepository _businessUnits;
		private readonly IActivityRepository _activities;
		private readonly IRtaMapRepository _mapRepository;
		private readonly IRtaStateGroupRepository _stateGroups;

		public AutoFillMappingReader(
			IBusinessUnitRepository businessUnits,
			IActivityRepository activities,
			IRtaMapRepository mapRepository,
			IRtaStateGroupRepository stateGroups
		)
		{
			_businessUnits = businessUnits;
			_activities = activities;
			_mapRepository = mapRepository;
			_stateGroups = stateGroups;
		}

		public IEnumerable<Mapping> Read()
		{
			return MappingReadModelUpdater.MakeMappings(_businessUnits, _activities, _stateGroups, _mapRepository)
				.ToArray();
		}
	}

	public class AutoFillCurrentScheduleReadModelReader : IScheduleReader
	{
		private readonly INow _now;
		private readonly IPersonRepository _persons;
		private readonly IBusinessUnitRepository _businessUnits;
		private readonly IScenarioRepository _scenarios;
		private readonly IScheduleStorage _scheduleStorage;

		public AutoFillCurrentScheduleReadModelReader(
			INow now,
			IPersonRepository persons,
			IBusinessUnitRepository businessUnits,
			IScenarioRepository scenarios,
			IScheduleStorage scheduleStorage)
		{
			_now = now;
			_persons = persons;
			_businessUnits = businessUnits;
			_scenarios = scenarios;
			_scheduleStorage = scheduleStorage;
		}

		public IEnumerable<ScheduledActivity> Read()
		{
			var result = new List<ScheduledActivity>();
			CurrentScheduleReadModelUpdater.PersistSchedules(
				_persons.LoadAll().Select(x => x.Id.Value),
				_now,
				_persons,
				_businessUnits,
				_scenarios,
				_scheduleStorage,
				(a, b) => result.AddRange(b));
			return result;
		}
		
	}

	public class AutoFillAgentStatePersister : FakeAgentStatePersister
	{
		private readonly IPersonRepository _persons;
		private readonly INow _now;

		public AutoFillAgentStatePersister(
			IPersonRepository persons,
			INow now)
		{
			_persons = persons;
			_now = now;
		}

		private void syncFromAggregates()
		{
			_persons.LoadAll()
				.ForEach(x =>
				{
					var period = x.Period(new DateOnly(_now.UtcDateTime()));
					if (period == null)
					{
						Delete(x.Id.Value, DeadLockVictim.Yes);
						return;
					}

					Prepare(new AgentStatePrepare
					{
						PersonId = x.Id.Value,
						BusinessUnitId = period.Team?.Site?.BusinessUnit?.Id ?? Guid.Empty,
						SiteId = period.Team?.Site?.Id,
						TeamId = period.Team?.Id,
						ExternalLogons = period.ExternalLogOnCollection
							.Select(l => new ExternalLogon
							{
								DataSourceId = l.DataSourceId,
								UserCode = l.AcdLogOnOriginalId
							})
					}, DeadLockVictim.Yes);
				});
		}

		public override IEnumerable<ExternalLogonForCheck> FindForCheck()
		{
			syncFromAggregates();
			return base.FindForCheck();
		}

		public override IEnumerable<ExternalLogon> FindForClosingSnapshot(DateTime snapshotId, string sourceId, string loggedOutState)
		{
			syncFromAggregates();
			return base.FindForClosingSnapshot(snapshotId, sourceId, loggedOutState);
		}

		public override LockedData LockNLoad(IEnumerable<ExternalLogon> externalLogons, DeadLockVictim deadLockVictim)
		{
			syncFromAggregates();
			return base.LockNLoad(externalLogons, deadLockVictim);
		}

		public override LockedData LockNLoad(IEnumerable<Guid> personIds, DeadLockVictim deadLockVictim)
		{
			syncFromAggregates();
			return base.LockNLoad(personIds, deadLockVictim);
		}
	}
}
