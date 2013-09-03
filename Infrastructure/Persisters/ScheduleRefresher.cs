using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
    public class ScheduleRefresher : IScheduleRefresher
    {
        private readonly IPersonAssignmentRepository _personAssignmentRepository;
        private readonly IPersonAbsenceRepository _personAbsenceRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IUpdateScheduleDataFromMessages _scheduleDataUpdater;

        public ScheduleRefresher(IPersonAssignmentRepository personAssignmentRepository, IPersonAbsenceRepository personAbsenceRepository, IPersonRepository personRepository, IUpdateScheduleDataFromMessages scheduleDataUpdater)
        {
            _personAssignmentRepository = personAssignmentRepository;
            _personAbsenceRepository = personAbsenceRepository;
            _personRepository = personRepository;
            _scheduleDataUpdater = scheduleDataUpdater;
        }

        public void Refresh(IScheduleDictionary scheduleDictionary, IList<IEventMessage> messageQueue, IEnumerable<IEventMessage> scheduleMessages,
                            ICollection<IPersistableScheduleData> refreshedEntitiesBuffer, ICollection<PersistConflictMessageState> conflictsBuffer)
        {
            var myChanges = scheduleDictionary.DifferenceSinceSnapshot();
            
            foreach (var eventMessage in scheduleMessages)
            {
                var person = _personRepository.Load(eventMessage.DomainObjectId);
                var period = new DateOnlyPeriod(new DateOnly(eventMessage.EventStartDate).AddDays(-1),
                                                new DateOnly(eventMessage.EventEndDate).AddDays(1));

                refreshPersonAssignments(scheduleDictionary, person, period, eventMessage, messageQueue, myChanges, conflictsBuffer, refreshedEntitiesBuffer);
                refreshPersonAbsences(scheduleDictionary, person, period, eventMessage, messageQueue, myChanges, conflictsBuffer, refreshedEntitiesBuffer);

                messageQueue.Remove(eventMessage);
            }

            _scheduleDataUpdater.NotifyMessageQueueSize();
        }

        private void refreshPersonAbsences(IScheduleDictionary scheduleDictionary, IPerson person, DateOnlyPeriod period, IEventMessage eventMessage, IList<IEventMessage> messageQueue, IDifferenceCollection<IPersistableScheduleData> myChanges, ICollection<PersistConflictMessageState> conflictsBuffer, ICollection<IPersistableScheduleData> refreshedEntitiesBuffer)
        {
            var scheduleDays = scheduleDictionary[person].ScheduledDayCollection(period);

            var messagePersonAbsences = _personAbsenceRepository.Find(new[] {person},
                                                                      new DateTimePeriod(DateTime.SpecifyKind(eventMessage.EventStartDate, DateTimeKind.Utc),
                                                                                         DateTime.SpecifyKind(eventMessage.EventEndDate, DateTimeKind.Utc)), scheduleDictionary.Scenario);

            var myVersionOfPersonAbsences = scheduleDays.SelectMany(s => s.PersonAbsenceCollection(true)).Where(a => a.Id.HasValue).Distinct().ToArray();


            var personAbsenceDictionary =
                new Dictionary<Guid, Tuple<IPersistableScheduleData, IPersistableScheduleData>>();
            addItemsToVersionDictionary(personAbsenceDictionary, p => p.Id.GetValueOrDefault(),
                                        myVersionOfPersonAbsences,
                                        (t, i) => new Tuple
                                                      <IPersistableScheduleData, IPersistableScheduleData>(
                                                      i, t.Item2));
            addItemsToVersionDictionary(personAbsenceDictionary, p => p.Id.GetValueOrDefault(),
                                        messagePersonAbsences,
                                        (t, i) => new Tuple
                                                      <IPersistableScheduleData, IPersistableScheduleData>(
                                                      t.Item1, i));

            foreach (var personAbsence in personAbsenceDictionary)
            {
                var myVersionOfEntity = myChanges.FindItemByOriginalId(personAbsence.Key);
                if (myVersionOfEntity.HasValue)
                {
                    _scheduleDataUpdater.FillReloadedScheduleData(personAbsence.Value.Item2);
                    var state = new PersistConflictMessageState(myVersionOfEntity.Value, personAbsence.Value.Item2,
                                                                eventMessage, m => RemoveFromQueue(messageQueue, m));
                    conflictsBuffer.Add(state);
                    continue;
                }

                IPersistableScheduleData messageVersionOfEntity = null;
                if (personAbsence.Value.IsDeletedByMessage())
                {
                    messageVersionOfEntity = scheduleDictionary.DeleteFromBroker(personAbsence.Key);
                }
                if (personAbsence.Value.IsUpdateByMessage() ||
                    personAbsence.Value.IsInsertByMessage())
                {
                    var updatedItem = personAbsence.Value.Item2;
                    messageVersionOfEntity =
                        scheduleDictionary.UpdateFromBroker(
                            new ScheduleEntityLoader(() => updatedItem), personAbsence.Key);
                }

                if (messageVersionOfEntity != null)
                    refreshedEntitiesBuffer.Add(messageVersionOfEntity);
            }
        }

        private void refreshPersonAssignments(IScheduleDictionary scheduleDictionary, IPerson person, DateOnlyPeriod period, IEventMessage eventMessage, IList<IEventMessage> messageQueue, IDifferenceCollection<IPersistableScheduleData> myChanges, ICollection<PersistConflictMessageState> conflictsBuffer, ICollection<IPersistableScheduleData> refreshedEntitiesBuffer)
        {
            var scheduleDays = scheduleDictionary[person].ScheduledDayCollection(period);

            var messagePersonAssignments = _personAssignmentRepository.Find(new[] {person}, period, scheduleDictionary.Scenario);

            var myVersionOfPersonAssignments = scheduleDays.Select(s => s.PersonAssignment()).Where(p => p != null && p.Id.HasValue).ToArray();

            var personAssignmentDictionary = new Dictionary<DateOnly, Tuple<IPersistableScheduleData, IPersistableScheduleData>>();
            addItemsToVersionDictionary(personAssignmentDictionary, k => ((IPersonAssignment) k).Date,
                                        myVersionOfPersonAssignments,
                                        (t, i) => new Tuple<IPersistableScheduleData, IPersistableScheduleData>(i, t.Item2));
            addItemsToVersionDictionary(personAssignmentDictionary, k => ((IPersonAssignment) k).Date,
                                        messagePersonAssignments,
                                        (t, i) => new Tuple<IPersistableScheduleData, IPersistableScheduleData>(t.Item1, i));

            foreach (var personAssignment in personAssignmentDictionary)
            {
                DifferenceCollectionItem<IPersistableScheduleData> myVersionOfEntity =
                    (from d in myChanges
                     let pa = d.CurrentItem as IPersonAssignment
                     where
                         pa != null &&
                         pa.Equals(personAssignment.Value.Item2)
                     select d
                    ).SingleOrDefault();

                if (!myVersionOfEntity.IsEmpty())
                {
                    _scheduleDataUpdater.FillReloadedScheduleData(personAssignment.Value.Item2);
                    var state = new PersistConflictMessageState(myVersionOfEntity, personAssignment.Value.Item2, eventMessage, m => RemoveFromQueue(messageQueue, m));
                    conflictsBuffer.Add(state);
                    continue;
                }

                IPersistableScheduleData messageVersionOfEntity = null;
                if (personAssignment.Value.IsDeletedByMessage())
                {
                    messageVersionOfEntity = scheduleDictionary.DeleteFromBroker(personAssignment.Value.Item1.Id.GetValueOrDefault());
                }
                else if (
                    personAssignment.Value.IsUpdateByMessage() ||
                    personAssignment.Value.IsInsertByMessage())
                {
                    var updatedItem = personAssignment.Value.Item2;
                    messageVersionOfEntity = scheduleDictionary.UpdateFromBroker(new ScheduleEntityLoader(() => updatedItem), personAssignment.Value.Item2.Id.GetValueOrDefault());
                }

                if (messageVersionOfEntity != null)
                    refreshedEntitiesBuffer.Add(messageVersionOfEntity);
            }
        }


        private void addItemsToVersionDictionary<TKey, TVersionedItem>(
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

        private class ScheduleEntityLoader : ILoadAggregateById<IPersistableScheduleData>
        {
            private readonly Func<IPersistableScheduleData> _scehduleEntityFinder;

            public ScheduleEntityLoader(Func<IPersistableScheduleData> scehduleEntityFinder)
            {
                _scehduleEntityFinder = scehduleEntityFinder;
            }

            public IPersistableScheduleData LoadAggregate(Guid id)
            {
                return _scehduleEntityFinder();
            }
        }

        private void RemoveFromQueue(ICollection<IEventMessage> messageQueue, IEventMessage m)
        {
            messageQueue.Remove(m);
            _scheduleDataUpdater.NotifyMessageQueueSize();
        }
    }

    internal static class VersionTupleExtensions
    {
        internal static bool IsDeletedByMessage(this Tuple<IPersistableScheduleData, IPersistableScheduleData> versionTuple)
        {
            return versionTuple.Item2 == null;
        }

        internal static bool IsUpdateByMessage(this Tuple<IPersistableScheduleData, IPersistableScheduleData> versionTuple)
        {
            return (versionTuple.Item1 != null && versionTuple.Item2 != null &&
                    versionTuple.Item1.Version < versionTuple.Item2.Version);
        }

        internal static bool IsInsertByMessage(this Tuple<IPersistableScheduleData, IPersistableScheduleData> versionTuple)
        {
            return versionTuple.Item1 == null;
        }
    }
}