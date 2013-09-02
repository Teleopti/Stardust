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
        private readonly IPersonRepository _personRepository;
        private readonly IUpdateScheduleDataFromMessages _scheduleDataUpdater;

        public ScheduleRefresher(IPersonAssignmentRepository personAssignmentRepository, IPersonRepository personRepository, IUpdateScheduleDataFromMessages scheduleDataUpdater)
        {
            _personAssignmentRepository = personAssignmentRepository;
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
                var period = new DateOnlyPeriod(new DateOnly(eventMessage.EventStartDate),
                                                new DateOnly(eventMessage.EventEndDate));
                var result = _personAssignmentRepository.Find(new[]
                    {
                        person
                    }, period, scheduleDictionary.Scenario);

                var days = period.DayCollection();
                var myVersionOfPersonAssignments =
                    scheduleDictionary[person].ScheduledDayCollection(period)
                                              .ToDictionary(k => k.DateOnlyAsPeriod.DateOnly, v => v.PersonAssignment());
                foreach (var dateOnly in days)
                {
                    var myPersonAssignment = myVersionOfPersonAssignments[dateOnly];
                    var messagePersonAssignment = result.FirstOrDefault(d => d.Date == dateOnly);
                    var myVersionOfEntity = myChanges.FindItemByOriginalId(eventMessage.DomainObjectId);

                    if (myVersionOfEntity.HasValue)
                    {
                        _scheduleDataUpdater.FillReloadedScheduleData(messagePersonAssignment);
                        var state = new PersistConflictMessageState(myVersionOfEntity.Value, messagePersonAssignment,
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