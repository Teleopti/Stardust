using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	public interface IFakeDataBuilder
	{
		IFakeDataBuilder WithDefaultsFromState(ExternalUserStateForTest state);
		IFakeDataBuilder WithDataFromState(ExternalUserStateForTest state);
		IFakeDataBuilder WithSource(string sourceId);
		IFakeDataBuilder WithBusinessUnit(Guid businessUnitId);
		IFakeDataBuilder WithUser(string userCode, Guid personId, Guid? businessUnitId, Guid? teamId, Guid? siteId);
		IFakeDataBuilder WithSchedule(Guid personId, Guid activityId, string name, DateOnly date, string start, string end);
		IFakeDataBuilder WithAlarm(string stateCode, Guid? activityId, Guid? alarmId, int staffingEffect, string name, bool isLoggedOutState, TimeSpan threshold, Adherence? adherence, Guid? platformTypeId);
		IFakeDataBuilder WithDefaultStateGroup();
		FakeRtaDatabase Make();
	}

	public class FakeRtaDatabase : IDatabaseReader, IDatabaseWriter, IPersonOrganizationReader, IFakeDataBuilder
	{
		private readonly List<KeyValuePair<string, int>> _datasources = new List<KeyValuePair<string, int>>();
		private readonly List<KeyValuePair<string, IEnumerable<ResolvedPerson>>> _externalLogOns = new List<KeyValuePair<string, IEnumerable<ResolvedPerson>>>();
		private readonly List<scheduleLayer2> _schedules = new List<scheduleLayer2>();
		private readonly List<PersonOrganizationData> _personOrganizationData = new List<PersonOrganizationData>();

		public FakeRtaStateGroupRepository RtaStateGroupRepository = new FakeRtaStateGroupRepository();
		public FakeStateGroupActivityAlarmRepository StateGroupActivityAlarmRepository = new FakeStateGroupActivityAlarmRepository();
		public FakeAgentStateReadModelReader AgentStateReadModelReader = new FakeAgentStateReadModelReader();

		private class scheduleLayer2
		{
			public Guid PersonId { get; set; }
			public ScheduleLayer ScheduleLayer { get; set; }
		}

		public AgentStateReadModel PersistedAgentStateReadModel { get; set; }

		public IRtaState AddedStateCode
		{
			get { return RtaStateGroupRepository.LoadAll().Single(x => x.DefaultStateGroup).StateCollection.SingleOrDefault(); }
		}

		private BusinessUnit _businessUnit;
		private Guid _businessUnitId;
		private string _platformTypeId;

		public FakeRtaDatabase()
		{
			WithBusinessUnit(Guid.NewGuid());
			WithDefaultsFromState(new ExternalUserStateForTest());
		}

		public IFakeDataBuilder WithDefaultsFromState(ExternalUserStateForTest state)
		{
			WithSource(state.SourceId);
			_platformTypeId = state.PlatformTypeId;
			return this;
		}

		public IFakeDataBuilder WithDataFromState(ExternalUserStateForTest state)
		{
			WithDefaultsFromState(state);
			return this.WithUser(state.UserCode, Guid.NewGuid());
		}

		public IFakeDataBuilder WithSource(string sourceId)
		{
			if (_datasources.Any(x => x.Key == sourceId))
				return this;
			_datasources.Add(new KeyValuePair<string, int>(sourceId, 0));
			return this;
		}

		public IFakeDataBuilder WithBusinessUnit(Guid businessUnitId)
		{
			_businessUnitId = businessUnitId;
			_businessUnit = new BusinessUnit(".");
			_businessUnit.SetId(_businessUnitId);
			return this;
		}

		public IFakeDataBuilder WithUser(string userCode, Guid personId, Guid? businessUnitId, Guid? teamId, Guid? siteId)
		{
			if (!businessUnitId.HasValue) businessUnitId = _businessUnitId;
			if (!teamId.HasValue) teamId = Guid.NewGuid();
			if (!siteId.HasValue) siteId = Guid.NewGuid();

			var lookupKey = string.Format("{0}|{1}", _datasources.Last().Value, userCode).ToUpper(); //putting this logic here is just WRONG
			_externalLogOns.Add(
				new KeyValuePair<string, IEnumerable<ResolvedPerson>>(
					lookupKey, new[]
					{
						new ResolvedPerson
						{
							PersonId = personId,
							BusinessUnitId = businessUnitId.Value
						}
					}));

			_personOrganizationData.Add(new PersonOrganizationData
			{
				PersonId = personId,
				TeamId = teamId.Value,
				SiteId = siteId.Value,
			});

			return this;
		}

		public IFakeDataBuilder WithSchedule(Guid personId, Guid activityId, string name, DateOnly belongsToDate, string start, string end)
		{
			_schedules.Add(new scheduleLayer2
			{
				PersonId = personId,
				ScheduleLayer = new ScheduleLayer
				{
					PayloadId = activityId,
					Name = name,
					StartDateTime = start.Utc(),
					EndDateTime = end.Utc(),
					BelongsToDate = belongsToDate
				}
			});
			return this;
		}

		public IFakeDataBuilder ClearSchedule(Guid personId)
		{
			_schedules.RemoveAll(x => x.PersonId == personId);
			return this;
		}

		public IFakeDataBuilder WithAlarm(string stateCode, Guid? activityId, Guid? alarmId, int staffingEffect, string name, bool isLoggedOutState, TimeSpan threshold, Adherence? adherence, Guid? platformTypeId)
		{
			var platformTypeIdGuid = platformTypeId ?? new Guid(_platformTypeId);

			IAlarmType alarmType = null;
			if (alarmId != null)
			{
				alarmType = new AlarmType();
				alarmType.SetId(alarmId);
				alarmType.SetBusinessUnit(_businessUnit);
				alarmType.StaffingEffect = staffingEffect;
				alarmType.ThresholdTime = threshold;
				alarmType.Adherence = adherence;
			}

			IRtaStateGroup stateGroup = null;
			if (stateCode != null)
			{
				stateGroup = (
					from g in RtaStateGroupRepository.LoadAll()
					from s in g.StateCollection
					where s.StateCode == stateCode &&
					      s.PlatformTypeId == platformTypeIdGuid
					select g
					).FirstOrDefault();
				if (stateGroup == null)
				{
					stateGroup = new RtaStateGroup(name, false, true);
					stateGroup.SetId(Guid.NewGuid());
					stateGroup.SetBusinessUnit(_businessUnit);
					stateGroup.IsLogOutState = isLoggedOutState;
					stateGroup.AddState(null, stateCode, platformTypeIdGuid);
					RtaStateGroupRepository.Add(stateGroup);
				}
			}

			IActivity activity = null;
			if (activityId != null)
			{
				activity = new Activity(stateCode ?? "activity");
				activity.SetId(activityId);
				activity.SetBusinessUnit(_businessUnit);
			}

			var mapping = new StateGroupActivityAlarm(stateGroup, activity);
			mapping.AlarmType = alarmType;
			mapping.SetId(Guid.NewGuid());
			mapping.SetBusinessUnit(_businessUnit);
			StateGroupActivityAlarmRepository.Add(mapping);

			return this;
		}

		public IFakeDataBuilder WithDefaultStateGroup()
		{
			var defaultStateGroup = RtaStateGroupRepository.LoadAll().SingleOrDefault(x => x.DefaultStateGroup);
			if (defaultStateGroup == null)
			{
				defaultStateGroup = new RtaStateGroup(".", true, true);
				defaultStateGroup.SetId(Guid.NewGuid());
				defaultStateGroup.SetBusinessUnit(_businessUnit);
				RtaStateGroupRepository.Add(defaultStateGroup);
			}
			return this;
		}

		public FakeRtaDatabase Make()
		{
			return this;
		}





		public IList<ScheduleLayer> GetCurrentSchedule(Guid personId)
		{
			var layers = from l in _schedules
						 where l.PersonId == personId
						 select l.ScheduleLayer;
			return new List<ScheduleLayer>(layers);
		}

		public TimeZoneInfo GetTimeZone(Guid personId)
		{
			return new UtcTimeZone().TimeZone();
		}

		public ConcurrentDictionary<string, IEnumerable<ResolvedPerson>> ExternalLogOns()
		{
			return new ConcurrentDictionary<string, IEnumerable<ResolvedPerson>>(_externalLogOns);
		}

		public ConcurrentDictionary<string, int> Datasources()
		{
			return new ConcurrentDictionary<string, int>(_datasources);
		}

		public void PersistActualAgentReadModel(AgentStateReadModel model)
		{
			AgentStateReadModelReader.Has(model);
			PersistedAgentStateReadModel = model;
		}

		public IEnumerable<PersonOrganizationData> PersonOrganizationData()
		{
			return _personOrganizationData;
		}
	}

	public class FakeStateGroupActivityAlarmRepository : IStateGroupActivityAlarmRepository
	{
		private readonly IList<IStateGroupActivityAlarm> _data = new List<IStateGroupActivityAlarm>();

		public void Add(IStateGroupActivityAlarm entity)
		{
			_data.Add(entity);
		}

		public void Remove(IStateGroupActivityAlarm entity)
		{
			throw new NotImplementedException();
		}

		public IStateGroupActivityAlarm Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IStateGroupActivityAlarm> LoadAll()
		{
			return _data;
		}

		public IStateGroupActivityAlarm Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IStateGroupActivityAlarm> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork
		{
			get { throw new NotImplementedException(); }
		}

		public IList<IStateGroupActivityAlarm> LoadAllCompleteGraph()
		{
			throw new NotImplementedException();
		}
	}

	public class FakeRtaStateGroupRepository : IRtaStateGroupRepository
	{
		private readonly IList<IRtaStateGroup> _data = new List<IRtaStateGroup>();
		
		public void Add(IRtaStateGroup entity)
		{
			_data.Add(entity);
		}

		public void Remove(IRtaStateGroup entity)
		{
			throw new NotImplementedException();
		}

		public IRtaStateGroup Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IRtaStateGroup> LoadAll()
		{
			return _data;
		}

		public IRtaStateGroup Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IRtaStateGroup> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork
		{
			get { throw new NotImplementedException(); }
		}

		public IList<IRtaStateGroup> LoadAllCompleteGraph()
		{
			throw new NotImplementedException();
		}
	}

	public class FakeAlarmTypeRepository : IAlarmTypeRepository
	{
		private readonly IList<IAlarmType> _data = new List<IAlarmType>();

		public void Add(IAlarmType entity)
		{
			_data.Add(entity);
		}

		public void Remove(IAlarmType entity)
		{
			throw new NotImplementedException();
		}

		public IAlarmType Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IAlarmType> LoadAll()
		{
			return _data;
		}

		public IAlarmType Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IAlarmType> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork
		{
			get
			{
				throw new NotImplementedException();
			}
		}
	}

	public static class FakeDatabaseBuilderExtensions
	{
		public static IFakeDataBuilder WithUser(this IFakeDataBuilder fakeDataBuilder, string userCode)
		{
			return fakeDataBuilder.WithUser(userCode, Guid.NewGuid(), null, null, null);
		}

		public static IFakeDataBuilder WithUser(this IFakeDataBuilder fakeDataBuilder, string userCode, Guid personId)
		{
			return fakeDataBuilder.WithUser(userCode, personId, null, null, null);
		}

		public static IFakeDataBuilder WithUser(this IFakeDataBuilder fakeDataBuilder, string userCode, Guid personId, Guid businessUnitId)
		{
			return fakeDataBuilder.WithUser(userCode, personId, businessUnitId, null, null);
		}

		public static IFakeDataBuilder WithSchedule(this IFakeDataBuilder fakeDataBuilder, Guid personId, Guid activityId, string start, string end)
		{
			return fakeDataBuilder.WithSchedule(personId, activityId, null, new DateOnly(start.Utc()), start, end);
		}

		public static IFakeDataBuilder WithSchedule(this IFakeDataBuilder fakeDataBuilder, Guid personId, Guid activityId, string name, string start, string end)
		{
			return fakeDataBuilder.WithSchedule(personId, activityId, name, new DateOnly(start.Utc()), start, end);
		}

		public static IFakeDataBuilder WithSchedule(this IFakeDataBuilder fakeDataBuilder, Guid personId, Guid activityId, DateOnly belongsToDate, string start, string end)
		{
			return fakeDataBuilder.WithSchedule(personId, activityId, null, belongsToDate, start, end);
		}

		public static IFakeDataBuilder WithAlarm(this IFakeDataBuilder fakeDataBuilder, string stateCode, Guid? activityId)
		{
			return fakeDataBuilder.WithAlarm(stateCode, activityId, Guid.NewGuid(), 0, null, false, TimeSpan.Zero, null, null);
		}

		public static IFakeDataBuilder WithAlarm(this IFakeDataBuilder fakeDataBuilder, string stateCode, Guid? activityId, int staffingEffect)
		{
			return fakeDataBuilder.WithAlarm(stateCode, activityId, Guid.NewGuid(), staffingEffect, null, false, TimeSpan.Zero, null, null);
		}

		public static IFakeDataBuilder WithAlarm(this IFakeDataBuilder fakeDataBuilder, string stateCode, Guid? activityId, Guid? alarmId)
		{
			return fakeDataBuilder.WithAlarm(stateCode, activityId, alarmId, 0, null, false, TimeSpan.Zero, null, null);
		}

		public static IFakeDataBuilder WithAlarm(this IFakeDataBuilder fakeDataBuilder, string stateCode, Guid? activityId, string name)
		{
			return fakeDataBuilder.WithAlarm(stateCode, activityId, Guid.NewGuid(), 0, name, false, TimeSpan.Zero, null, null);
		}

		public static IFakeDataBuilder WithAlarm(this IFakeDataBuilder fakeDataBuilder, string stateCode, Guid? activityId, bool isLoggedOutState)
		{
			return fakeDataBuilder.WithAlarm(stateCode, activityId, Guid.NewGuid(), 0, null, isLoggedOutState, TimeSpan.Zero, null, null);
		}

		public static IFakeDataBuilder WithAlarm(this IFakeDataBuilder fakeDataBuilder, string stateCode, Guid? activityId, TimeSpan threshold)
		{
			return fakeDataBuilder.WithAlarm(stateCode, activityId, Guid.NewGuid(), 0, null, false, threshold, null, null);
		}

		public static IFakeDataBuilder WithAlarm(this IFakeDataBuilder fakeDataBuilder, string stateCode, Guid platformTypeId, Guid activityId, int staffingEffect, Adherence adherence)
		{
			return fakeDataBuilder.WithAlarm(stateCode, activityId, Guid.NewGuid(), staffingEffect, null, false, TimeSpan.Zero, adherence, platformTypeId);
		}

		public static IFakeDataBuilder WithAlarm(this IFakeDataBuilder fakeDataBuilder, string stateCode, Guid? activityId, int staffingEffect, Adherence adherence)
		{
			return fakeDataBuilder.WithAlarm(stateCode, activityId, Guid.NewGuid(), staffingEffect, null, false, TimeSpan.Zero, adherence, null);
		}
	}

}