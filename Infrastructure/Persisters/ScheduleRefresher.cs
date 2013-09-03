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
                var messagePersonAssignments = _personAssignmentRepository.Find(new[]
                    {
                        person
                    }, period, scheduleDictionary.Scenario);
                var messagePersonAbsences = _personAbsenceRepository.Find(new[] {person},
                                                                          new DateTimePeriod(
                                                                              DateTime.SpecifyKind(
                                                                                  eventMessage.EventStartDate,
                                                                                  DateTimeKind.Utc),
                                                                              DateTime.SpecifyKind(
                                                                                  eventMessage.EventEndDate,
                                                                                  DateTimeKind.Utc)),
                                                                          scheduleDictionary.Scenario);

                var days = period.DayCollection();
                var scheduleDays = scheduleDictionary[person].ScheduledDayCollection(period);
                var myVersionOfPersonAssignments = scheduleDays.ToDictionary(k => k.DateOnlyAsPeriod.DateOnly, v => v.PersonAssignment());
                var myVersionOfPersonAbsences = scheduleDays.SelectMany(s => s.PersonAbsenceCollection(true)).Where(a => a.Id.HasValue).Distinct();
                
                var personAbsenceDictionary =
                    new Dictionary<Guid, Tuple<IPersistableScheduleData, IPersistableScheduleData>>();

                foreach (var myVersionOfPersonAbsence in myVersionOfPersonAbsences)
                {
                    Tuple<IPersistableScheduleData, IPersistableScheduleData> foundItems;
                    if (
                        !personAbsenceDictionary.TryGetValue(myVersionOfPersonAbsence.Id.GetValueOrDefault(),
                                                             out foundItems))
                    {
                        foundItems = new Tuple<IPersistableScheduleData, IPersistableScheduleData>(myVersionOfPersonAbsence, null);
                        personAbsenceDictionary.Add(myVersionOfPersonAbsence.Id.GetValueOrDefault(),
                                                    foundItems);
                    }
                    else
                    {
                        personAbsenceDictionary[myVersionOfPersonAbsence.Id.GetValueOrDefault()] =
                            new Tuple<IPersistableScheduleData, IPersistableScheduleData>(myVersionOfPersonAbsence,
                                                                                          foundItems.Item2);
                    }
                }

                foreach (var messageVersionOfPersonAbsence in messagePersonAbsences)
                {
                    Tuple<IPersistableScheduleData, IPersistableScheduleData> foundItems;
                    if (
                        !personAbsenceDictionary.TryGetValue(messageVersionOfPersonAbsence.Id.GetValueOrDefault(),
                                                             out foundItems))
                    {
                        foundItems = new Tuple<IPersistableScheduleData, IPersistableScheduleData>(null, messageVersionOfPersonAbsence);
                        personAbsenceDictionary.Add(messageVersionOfPersonAbsence.Id.GetValueOrDefault(),
                                                    foundItems);
                    }
                    else
                    {
                        personAbsenceDictionary[messageVersionOfPersonAbsence.Id.GetValueOrDefault()] =
                            new Tuple<IPersistableScheduleData, IPersistableScheduleData>(foundItems.Item1,
                                                                                          messageVersionOfPersonAbsence);
                    }
                }

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
                    if (personAbsence.Value.Item2 == null)
                    {
                        messageVersionOfEntity = scheduleDictionary.DeleteFromBroker(personAbsence.Key);
                    }
                    if (personAbsence.Value.Item1 == null ||
                        (personAbsence.Value.Item1 != null && personAbsence.Value.Item2 != null &&
                         personAbsence.Value.Item1.Version < personAbsence.Value.Item2.Version))
                    {
                        var updatedItem = personAbsence.Value.Item2;
                        messageVersionOfEntity =
                            scheduleDictionary.UpdateFromBroker(
                                new ScheduleEntityLoader(() => updatedItem), personAbsence.Key);
                    }

                    if (messageVersionOfEntity != null)
                        refreshedEntitiesBuffer.Add(messageVersionOfEntity);
                }

                foreach (var dateOnly in days)
                {
                    var myPersonAssignment = myVersionOfPersonAssignments[dateOnly];
                    var messagePersonAssignment = messagePersonAssignments.FirstOrDefault(d => d.Date == dateOnly);

                    
	                DifferenceCollectionItem<IPersistableScheduleData> myVersionOfEntity = (from d in myChanges
	                                     let pa = d.CurrentItem as IPersonAssignment
	                                     where
	                                         pa != null &&
	                                         pa.Equals(messagePersonAssignment)
	                                     select d).SingleOrDefault();

                    if (!myVersionOfEntity.IsEmpty())
                    {
                        _scheduleDataUpdater.FillReloadedScheduleData(messagePersonAssignment);
                        var state = new PersistConflictMessageState(myVersionOfEntity, messagePersonAssignment,
                                                                    eventMessage, m => RemoveFromQueue(messageQueue, m));
                        conflictsBuffer.Add(state);
                        continue;
                    }

                    IPersistableScheduleData messageVersionOfEntity = null;
                    if (isDeletedByMessage(messagePersonAssignment, myPersonAssignment))
                    {
                        messageVersionOfEntity = scheduleDictionary.DeleteFromBroker(eventMessage.DomainObjectId);
                    }
                    if (isInsertedByMessage(messagePersonAssignment, myPersonAssignment) ||
                        isUpdatedByMessage(messagePersonAssignment, myPersonAssignment))
                    {
                        messageVersionOfEntity = scheduleDictionary.UpdateFromBroker(new ScheduleEntityLoader(() => messagePersonAssignment), eventMessage.DomainObjectId);
                    }
                    
                    if (messageVersionOfEntity != null)
                        refreshedEntitiesBuffer.Add(messageVersionOfEntity);
                }
                messageQueue.Remove(eventMessage);
            }

            _scheduleDataUpdater.NotifyMessageQueueSize();
        }

        private static bool isUpdatedByMessage(IPersonAssignment messagePersonAssignment, IPersonAssignment myPersonAssignment)
        {
            return messagePersonAssignment != null && myPersonAssignment != null &&
                   messagePersonAssignment.Id == myPersonAssignment.Id &&
                   messagePersonAssignment.Version > myPersonAssignment.Version;
        }

        private static bool isInsertedByMessage(IPersonAssignment messagePersonAssignment, IPersonAssignment myPersonAssignment)
        {
            return messagePersonAssignment != null &&
                   ((myPersonAssignment != null && messagePersonAssignment.Id != myPersonAssignment.Id) ||
                    (myPersonAssignment == null));
        }

        private static bool isDeletedByMessage(IPersonAssignment databaseAssignment, IPersonAssignment myPersonAssignment)
        {
            return databaseAssignment == null && myPersonAssignment != null && myPersonAssignment.Id.HasValue;
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
}