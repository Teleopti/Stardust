﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Infrastructure.Persisters.Refresh
{
	public class ScheduleRefresher : IScheduleRefresher
	{
		private readonly IPersonAssignmentRepository _personAssignmentRepository;
		private readonly IPersonAbsenceRepository _personAbsenceRepository;
		private readonly IMessageQueueRemoval _messageQueueRemoval;
		private readonly IPersonRepository _personRepository;
		private readonly IUpdateScheduleDataFromMessages _scheduleDataUpdater;

		public ScheduleRefresher(IPersonRepository personRepository, IUpdateScheduleDataFromMessages scheduleDataUpdater, IPersonAssignmentRepository personAssignmentRepository, IPersonAbsenceRepository personAbsenceRepository, IMessageQueueRemoval messageQueueRemoval)
		{
			_personAssignmentRepository = personAssignmentRepository;
			_personAbsenceRepository = personAbsenceRepository;
			_messageQueueRemoval = messageQueueRemoval;
			_personRepository = personRepository;
			_scheduleDataUpdater = scheduleDataUpdater;
		}

		public void Refresh(IScheduleDictionary scheduleDictionary, IEnumerable<IEventMessage> scheduleMessages,
							ICollection<INonversionedPersistableScheduleData> refreshedEntitiesBuffer, ICollection<PersistConflict> conflictsBuffer)
		{
			var myChanges = scheduleDictionary.DifferenceSinceSnapshot();
			
			foreach (var eventMessage in scheduleMessages)
			{
				var person = _personRepository.Load(eventMessage.DomainObjectId);
				var period = new DateTimePeriod(DateTime.SpecifyKind(eventMessage.EventStartDate, DateTimeKind.Utc),
												DateTime.SpecifyKind(eventMessage.EventEndDate, DateTimeKind.Utc));

				refreshThingOfType<IPersonAssignment, DateOnly>(scheduleDictionary, myChanges, conflictsBuffer, refreshedEntitiesBuffer, data => ((IPersonAssignment) data).Date,
								   () => myPersonAssignmentCollection(scheduleDictionary, person, period), () => messagePersonAssignmentCollection(scheduleDictionary, person, period));
				refreshThingOfType<IPersonAbsence, Guid>(scheduleDictionary, myChanges, conflictsBuffer, refreshedEntitiesBuffer, data => data.Id.GetValueOrDefault(),
								   () => myPersonAbsenceCollection(scheduleDictionary, person, period), () => messagePersonAbsenceCollection(scheduleDictionary, person, period));

				_messageQueueRemoval.Remove(eventMessage);
			}
		}

		private IEnumerable<INonversionedPersistableScheduleData> myPersonAbsenceCollection(IScheduleDictionary scheduleDictionary, IPerson person, DateTimePeriod period)
		{
			var scheduleDays = scheduleDictionary[person].ScheduledDayCollection(period.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone()));
			return scheduleDays.SelectMany(s => s.PersonAbsenceCollection(false)).Where(a => a.Id.HasValue).Distinct().ToArray();
		}

		private IEnumerable<INonversionedPersistableScheduleData> messagePersonAbsenceCollection(IScheduleDictionary scheduleDictionary, IPerson person, DateTimePeriod period)
		{
			var result =  _personAbsenceRepository.Find(new[] { person }, period, scheduleDictionary.Scenario);
			if (result == null)
				return new INonversionedPersistableScheduleData[] {};
			return result;
		}

		private IEnumerable<INonversionedPersistableScheduleData> myPersonAssignmentCollection(IScheduleDictionary scheduleDictionary, IPerson person, DateTimePeriod period)
		{
			var scheduleDays = scheduleDictionary[person].ScheduledDayCollection(period.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone()));
			return scheduleDays.Select(s => s.PersonAssignment()).Where(p => p != null && p.Id.HasValue).ToArray();
		}

		private IEnumerable<INonversionedPersistableScheduleData> messagePersonAssignmentCollection(IScheduleDictionary scheduleDictionary, IPerson person, DateTimePeriod period)
		{
			var result = _personAssignmentRepository.Find(new[] {person}, period.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone()), scheduleDictionary.Scenario);
			if (result == null)
				return new INonversionedPersistableScheduleData[] { };
			return result;
		}

		private void refreshThingOfType<T, TKey>(
			IScheduleDictionary scheduleDictionary, 
			IDifferenceCollection<INonversionedPersistableScheduleData> myChanges, 
			ICollection<PersistConflict> conflictsBuffer, ICollection<INonversionedPersistableScheduleData> refreshedEntitiesBuffer,
			Func<INonversionedPersistableScheduleData, TKey> key,
			Func<IEnumerable<INonversionedPersistableScheduleData>> myVersionCollection,
			Func<IEnumerable<INonversionedPersistableScheduleData>> messageVersionCollection
			) where T : class
		{
			var messageVersionOfPersistableScheduleData = messageVersionCollection();
			var myVersionOfPersistableScheduleData = myVersionCollection();

			var versionDictionary = new Dictionary<TKey, Tuple<INonversionedPersistableScheduleData, INonversionedPersistableScheduleData>>();
			addItemsToVersionDictionary(versionDictionary, key,
										myVersionOfPersistableScheduleData,
										(t, i) => new Tuple<INonversionedPersistableScheduleData, INonversionedPersistableScheduleData>(i, t.Item2));
			addItemsToVersionDictionary(versionDictionary, key,
										messageVersionOfPersistableScheduleData,
										(t, i) => new Tuple<INonversionedPersistableScheduleData, INonversionedPersistableScheduleData>(t.Item1, i));

			foreach (var versionKeyValuePair in versionDictionary)
			{
				DifferenceCollectionItem<INonversionedPersistableScheduleData>? myVersionOfEntity;
				if (versionKeyValuePair.Value.IsInsertByMessage())
				{
					myVersionOfEntity = (from d in myChanges
										 let pa = d.CurrentItem as T
										 where
											 pa != null &&
											 pa.Equals(versionKeyValuePair.Value.Item2)
										 select (DifferenceCollectionItem<INonversionedPersistableScheduleData>?)d
										).SingleOrDefault();
				}
				else
				{
					myVersionOfEntity = myChanges.FindItemByMatchingOriginal(versionKeyValuePair.Value.Item2);
				}
				if (myVersionOfEntity.HasValue)
				{
					_scheduleDataUpdater.FillReloadedScheduleData(versionKeyValuePair.Value.Item2);
					var state = new PersistConflict(myVersionOfEntity.Value, versionKeyValuePair.Value.Item2);
					conflictsBuffer.Add(state);
					continue;
				}

				INonversionedPersistableScheduleData messageVersionOfEntity = null;
				if (versionKeyValuePair.Value.IsDeletedByMessage())
				{
					messageVersionOfEntity = scheduleDictionary.DeleteFromBroker(versionKeyValuePair.Value.Item1.Id.GetValueOrDefault());
				}
				else if (
					versionKeyValuePair.Value.IsUpdateByMessage() ||
					versionKeyValuePair.Value.IsInsertByMessage())
				{
					var updatedItem = versionKeyValuePair.Value.Item2;
					messageVersionOfEntity = scheduleDictionary.UpdateFromBroker(new ScheduleEntityLoader(() => updatedItem), versionKeyValuePair.Value.Item2.Id.GetValueOrDefault());
				}

				if (messageVersionOfEntity != null && refreshedEntitiesBuffer != null)
					refreshedEntitiesBuffer.Add(messageVersionOfEntity);
			}
		}


		private static void addItemsToVersionDictionary<TKey, TVersionedItem>(
			IDictionary<TKey, Tuple<TVersionedItem, TVersionedItem>> versionDictionary, Func<TVersionedItem,TKey> keyCreator,
			IEnumerable<TVersionedItem> items,
			Func<Tuple<TVersionedItem, TVersionedItem>, TVersionedItem, Tuple<TVersionedItem, TVersionedItem>> addItem)
		{
			foreach (var versionedItem in items)
			{
				var key = keyCreator(versionedItem);
				Tuple<TVersionedItem, TVersionedItem> foundItems;
				if (!versionDictionary.TryGetValue(key, out foundItems))
				{
					versionDictionary.Add(key, addItem(new Tuple<TVersionedItem, TVersionedItem>(default(TVersionedItem),default(TVersionedItem)),versionedItem));
				}
				else
				{
					versionDictionary[key] = addItem(foundItems, versionedItem);
				}
			}
		}
	}
}