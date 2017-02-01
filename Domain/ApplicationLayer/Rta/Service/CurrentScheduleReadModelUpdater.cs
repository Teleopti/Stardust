﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class CurrentScheduleReadModelUpdater
	{
		private readonly INow _now;
		private readonly ICurrentScheduleReadModelPersister _persister;
		private readonly IPersonRepository _persons;
		private readonly IScenarioRepository _scenarios;
		private readonly IBusinessUnitRepository _businessUnits;
		private readonly IScheduleStorage _schedules;
		private readonly IKeyValueStorePersister _keyValueStore;

		public CurrentScheduleReadModelUpdater(
			INow now,
			ICurrentScheduleReadModelPersister persister,
			IPersonRepository persons,
			IScenarioRepository scenarios,
			IBusinessUnitRepository businessUnits,
			IScheduleStorage schedules,
			IKeyValueStorePersister keyValueStore
		)
		{
			_now = now;
			_persister = persister;
			_persons = persons;
			_scenarios = scenarios;
			_businessUnits = businessUnits;
			_schedules = schedules;
			_keyValueStore = keyValueStore;
		}

		[ReadModelUnitOfWork]
		public virtual void Invalidate(Guid personId, DateTime startDateTime, DateTime endDateTime)
		{
			if (ShouldInvalidate(startDateTime, endDateTime))
				_persister.Invalidate(personId);
		}

		public bool ShouldInvalidate(DateTime startTime, DateTime endTime)
		{
			if (startTime == DateTime.MinValue)
				return true;
			var now = _now.UtcDateTime();
			return new DateTimePeriod(startTime, endTime)
				.Intersect(new DateTimePeriod(now.AddDays(-2), now.AddDays(2)));
		}

		[TestLog]
		[FullPermissions]
		public virtual void UpdateInvalids()
		{
			IEnumerable<Guid> persons = null;

			WithUnitOfWork(() =>
			{
				persons = _keyValueStore.Get("CurrentScheduleReadModelVersion") == null ?
					_persons.LoadAll().Select(x => x.Id.Value).ToArray() :
					_persister.GetInvalid();
			});

			update(persons);
		}

		[FullPermissions]
		[TestLog]
		public virtual  void UpdateAll()
		{
			IEnumerable<Guid> persons = null;

			WithUnitOfWork(() =>
			{
				persons = _persons.LoadAll().Select(x => x.Id.Value).ToArray();
			});

			update(persons);
		}

		[TestLog]
		protected virtual void update(IEnumerable<Guid> personIds)
		{
			personIds
				.Batch(50)
				.ForEach(personsInBatch =>
				{
					WithUnitOfWork(() =>
					{
						PersistSchedules2(personsInBatch);
						_keyValueStore.Update("CurrentScheduleReadModelVersion", Guid.NewGuid().ToString());
					});
				});
		}

		[TestLog]
		protected virtual void PersistSchedules2(IEnumerable<Guid> personIds)
		{
			var time = _now.UtcDateTime();
			var from = new DateOnly(time.AddDays(-1));
			var to = new DateOnly(time.AddDays(1));
			var loadPeriod = new DateOnlyPeriod(@from.AddDays(-1), to.AddDays(1));
			var persistPeriod = new DateOnlyPeriod(@from, to);

			_persons.FindPeople(personIds)
				.Select(x => new
				{
					businessUnitId = x.Period(new DateOnly(_now.UtcDateTime()))?.Team?.Site?.BusinessUnit?.Id,
					person = x
				})
				.Where(x => x.businessUnitId.HasValue)
				.GroupBy(x => x.businessUnitId.Value, x => x.person)
				.ForEach(personsInBusinessUnit =>
				{
					PersistSchedules2Batch(personsInBusinessUnit, loadPeriod, persistPeriod);
				});
		}

		[TestLog]
		protected virtual void PersistSchedules2Batch(IGrouping<Guid, IPerson> personsInBusinessUnit, DateOnlyPeriod loadPeriod, DateOnlyPeriod persistPeriod)
		{
			var scenario = _scenarios.LoadDefaultScenario(_businessUnits.Load(personsInBusinessUnit.Key));
			if (scenario == null)
				return;

			var scheduleDictionary = _schedules.FindSchedulesForPersonsOnlyInGivenPeriod(
				personsInBusinessUnit,
				new ScheduleDictionaryLoadOptions(false, false),
				loadPeriod,
				scenario);

			personsInBusinessUnit.ForEach(x =>
			{
				var scheduledActivities = PersistSchedules2Batch2(loadPeriod, persistPeriod, scheduleDictionary, x);

				_persister.Persist(x.Id.Value, scheduledActivities);
			});
		}

		[TestLog]
		protected virtual ScheduledActivity[] PersistSchedules2Batch2(DateOnlyPeriod loadPeriod, DateOnlyPeriod persistPeriod, IScheduleDictionary scheduleDictionary, IPerson x)
		{
			var projection =
				from scheduleDay in scheduleDictionary[x].ScheduledDayCollection(loadPeriod)
				from layer in scheduleDay.ProjectionService().CreateProjection()
				select new ScheduledActivity
				{
					BelongsToDate = scheduleDay.DateOnlyAsPeriod.DateOnly,
					DisplayColor = layer.DisplayColor().ToArgb(),
					EndDateTime = layer.Period.EndDateTime,
					Name = layer.DisplayDescription().Name,
					PayloadId = layer.Payload.UnderlyingPayload.Id.GetValueOrDefault(),
					PersonId = x.Id.GetValueOrDefault(),
					ShortName = layer.DisplayDescription().ShortName,
					StartDateTime = layer.Period.StartDateTime
				};

			var scheduledActivities = projection.Where(a => persistPeriod.Contains(a.BelongsToDate)).ToArray();
			return scheduledActivities;
		}

		[AllBusinessUnitsUnitOfWork]
		[ReadModelUnitOfWork]
		protected virtual void WithUnitOfWork(Action action)
		{
			action.Invoke();
		}






		public static void PersistSchedules(
			IEnumerable<Guid> personIds,
			INow now,
			IPersonRepository persons,
			IBusinessUnitRepository businessUnits,
			IScenarioRepository scenarios,
			IScheduleStorage schedules,
			Action<Guid, IEnumerable<ScheduledActivity>> persister)
		{
			var time = now.UtcDateTime();
			var from = new DateOnly(time.AddDays(-1));
			var to = new DateOnly(time.AddDays(1));
			var loadPeriod = new DateOnlyPeriod(@from.AddDays(-1), to.AddDays(1));
			var persistPeriod = new DateOnlyPeriod(@from, to);

			persons.FindPeople(personIds)
				.Select(x => new
				{
					businessUnitId = x.Period(new DateOnly(now.UtcDateTime()))?.Team?.Site?.BusinessUnit?.Id,
					person = x
				})
				.Where(x => x.businessUnitId.HasValue)
				.GroupBy(x => x.businessUnitId.Value, x => x.person)
				.ForEach(personsInBusinessUnit =>
				{
					var scenario = scenarios.LoadDefaultScenario(businessUnits.Load(personsInBusinessUnit.Key));
					if (scenario == null)
						return;

					var scheduleDictionary = schedules.FindSchedulesForPersonsOnlyInGivenPeriod(
						personsInBusinessUnit,
						new ScheduleDictionaryLoadOptions(false, false),
						loadPeriod,
						scenario);

					personsInBusinessUnit.ForEach(x =>
					{
						var projection =
							from scheduleDay in scheduleDictionary[x].ScheduledDayCollection(loadPeriod)
							from layer in scheduleDay.ProjectionService().CreateProjection()
							select new ScheduledActivity
							{
								BelongsToDate = scheduleDay.DateOnlyAsPeriod.DateOnly,
								DisplayColor = layer.DisplayColor().ToArgb(),
								EndDateTime = layer.Period.EndDateTime,
								Name = layer.DisplayDescription().Name,
								PayloadId = layer.Payload.UnderlyingPayload.Id.GetValueOrDefault(),
								PersonId = x.Id.GetValueOrDefault(),
								ShortName = layer.DisplayDescription().ShortName,
								StartDateTime = layer.Period.StartDateTime
							};

						var scheduledActivities = projection.Where(a => persistPeriod.Contains(a.BelongsToDate)).ToArray();

						persister.Invoke(x.Id.Value, scheduledActivities);
					});

				});
		}

	}
}