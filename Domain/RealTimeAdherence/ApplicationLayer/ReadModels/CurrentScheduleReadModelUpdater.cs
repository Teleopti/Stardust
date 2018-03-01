﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ReadModels
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
		
		public void InvalidateAll()
		{
			WithUnitOfWork(() =>
			{
				_persons
					.LoadAll()
					.Select(x => x.Id.Value)
					.ForEach(x => _persister.Invalidate(x));
			});
		}
		
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

		private void update(IEnumerable<Guid> personIds)
		{
			personIds
				.Batch(50)
				.ForEach(personsInBatch =>
				{
					WithUnitOfWork(() =>
					{
						var version = _keyValueStore.Get("CurrentScheduleReadModelVersion", CurrentScheduleReadModelVersion.Generate)
							.NextRevision();
						_keyValueStore.Update("CurrentScheduleReadModelVersion", version);
						persistSchedules(personsInBatch, version.Revision());
					});
				});
		}

		private void persistSchedules(IEnumerable<Guid> personIds, int revision)
		{
			var time = _now.UtcDateTime();
			var from = new DateOnly(time.AddDays(-1));
			var to = new DateOnly(time.AddDays(1));
			var loadPeriod = new DateOnlyPeriod(from.AddDays(-1), to.AddDays(1));
			var persistPeriod = new DateOnlyPeriod(from, to);
			var persons = _persons.FindPeople(personIds)
				.Select(x => new
				{
					businessUnitId = x.Period(new DateOnly(time))?.Team?.Site?.BusinessUnit?.Id,
					person = x
				})
				.ToArray();

			persons
				.Where(x => !x.businessUnitId.HasValue)
				.ForEach(person =>
				{
					_persister.Persist(person.person.Id.Value, revision, Enumerable.Empty<ScheduledActivity>());
				});

			persons
				.Where(x => x.businessUnitId.HasValue)
				.GroupBy(x => x.businessUnitId.Value, x => x.person)
				.ForEach(personsInBusinessUnit =>
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
						var scheduledActivities = (
							from scheduleDay in scheduleDictionary[x].ScheduledDayCollection(loadPeriod)
							let belongsToDate = scheduleDay.DateOnlyAsPeriod.DateOnly
							where persistPeriod.Contains(belongsToDate)
							from layer in scheduleDay.ProjectionService().CreateProjection()
							select new ScheduledActivity
							{
								BelongsToDate = belongsToDate,
								DisplayColor = layer.DisplayColor().ToArgb(),
								EndDateTime = layer.Period.EndDateTime,
								Name = layer.DisplayDescription().Name,
								PayloadId = layer.Payload.UnderlyingPayload.Id.GetValueOrDefault(),
								PersonId = x.Id.GetValueOrDefault(),
								ShortName = layer.DisplayDescription().ShortName,
								StartDateTime = layer.Period.StartDateTime
							})
							.ToArray();

						_persister.Persist(x.Id.Value, revision, scheduledActivities);
					});

				});
		}

		[AllBusinessUnitsUnitOfWork]
		[ReadModelUnitOfWork]
		protected virtual void WithUnitOfWork(Action action)
		{
			action.Invoke();
		}

	}
}