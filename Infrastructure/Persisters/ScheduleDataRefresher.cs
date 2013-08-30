using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Events;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	public class ScheduleDataRefresher : IScheduleDataRefresher
    {
        private readonly IScheduleRepository _scheduleRepository;
		private readonly IPersonAssignmentRepository _personAssignmentRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IUpdateScheduleDataFromMessages _scheduleDataUpdater;

        public ScheduleDataRefresher(IScheduleRepository scheduleRepository, IPersonAssignmentRepository personAssignmentRepository, IPersonRepository personRepository, IUpdateScheduleDataFromMessages scheduleDataUpdater)
        {
            _scheduleRepository = scheduleRepository;
	        _personAssignmentRepository = personAssignmentRepository;
	        _personRepository = personRepository;
	        _scheduleDataUpdater = scheduleDataUpdater;
        }

	    public void Refresh(IScheduleDictionary scheduleDictionary, IList<IEventMessage> messageQueue,
	                        IEnumerable<IEventMessage> scheduleDataMessages,
	                        ICollection<IPersistableScheduleData> refreshedEntitiesBuffer,
	                        ICollection<PersistConflictMessageState> conflictsBuffer)
	    {
	        var myChanges = scheduleDictionary.DifferenceSinceSnapshot();
	        IList<IEventMessage> newList = new List<IEventMessage>();

	        foreach (var eventMessage in scheduleDataMessages)
	        {
	            if (eventMessage.InterfaceType == typeof (IScheduleChangedEvent))
	            {
	                var person = _personRepository.Load(eventMessage.DomainObjectId);
	                var period = new DateOnlyPeriod(new DateOnly(eventMessage.EventStartDate),
	                                                new DateOnly(eventMessage.EventEndDate));
	                var result = _personAssignmentRepository.Find(new[]
	                    {
	                        person
	                    }, period, scheduleDictionary.Scenario);

	                var days = period.DayCollection();
	                foreach (var dateOnly in days)
	                {
	                    var myPersonAssignment = scheduleDictionary[person].ScheduledDay(dateOnly).PersonAssignment();
	                    var databaseAssignment = result.FirstOrDefault(d => d.Date == dateOnly);
	                    if (databaseAssignment == null && myPersonAssignment != null && myPersonAssignment.Id.HasValue)
	                    {
	                        var deleteMessage = new EventMessage
	                            {
	                                InterfaceType = typeof (IPersonAssignment),
	                                DomainUpdateType = DomainUpdateType.Delete,
	                                DomainObjectId = myPersonAssignment.Id.GetValueOrDefault(),
	                                ReferenceObjectId = eventMessage.DomainObjectId,
                                    EventStartDate = myPersonAssignment.Date,
                                    EventEndDate = myPersonAssignment.Date
	                            };
	                        newList.Add(deleteMessage);
	                    }
	                    if (databaseAssignment != null &&
	                        ((myPersonAssignment != null && databaseAssignment.Id != myPersonAssignment.Id) ||
	                         (myPersonAssignment == null)))
	                    {
	                        var insertMessage = new EventMessage
	                            {
	                                InterfaceType = typeof (IPersonAssignment),
	                                DomainUpdateType = DomainUpdateType.Insert,
	                                DomainObjectId = databaseAssignment.Id.GetValueOrDefault(),
	                                ReferenceObjectId = eventMessage.DomainObjectId,
                                    EventStartDate = databaseAssignment.Date,
                                    EventEndDate = databaseAssignment.Date
	                            };
	                        newList.Add(insertMessage);
	                    }
	                    if (databaseAssignment != null && myPersonAssignment != null &&
	                        databaseAssignment.Id == myPersonAssignment.Id &&
	                        databaseAssignment.Version > myPersonAssignment.Version)
	                    {
	                        var updateMessage = new EventMessage
	                            {
	                                InterfaceType = typeof (IPersonAssignment),
	                                DomainUpdateType = DomainUpdateType.Update,
	                                DomainObjectId = databaseAssignment.Id.GetValueOrDefault(),
	                                ReferenceObjectId = eventMessage.DomainObjectId,
                                    EventStartDate = databaseAssignment.Date,
                                    EventEndDate = databaseAssignment.Date
	                            };
	                        newList.Add(updateMessage);
	                    }
                    }
                    messageQueue.Remove(eventMessage);
	            }
	            else
	            {
	                newList.Add(eventMessage);
	            }
	        }
	        
	        foreach (var eventMessage in newList)
	        {
	            DifferenceCollectionItem<IPersistableScheduleData>? myVersionOfEntity;

	            if (eventMessage.InterfaceType == typeof (IPersonAssignment) &&
	                eventMessage.DomainUpdateType == DomainUpdateType.Insert)
	            {
	                var person = _personRepository.Load(eventMessage.ReferenceObjectId);
	                var period = new DateOnlyPeriod(new DateOnly(eventMessage.EventStartDate),
	                                                new DateOnly(eventMessage.EventEndDate));
	                var result = _personAssignmentRepository.Find(new[] {person}, period, scheduleDictionary.Scenario);
	                var databaseVerionOfEntity = result.Single();

	                myVersionOfEntity = (from d in myChanges
	                                     let pa = d.CurrentItem as IPersonAssignment
	                                     where
	                                         pa != null &&
	                                         pa.Equals(databaseVerionOfEntity)
	                                     select d).SingleOrDefault();
	            }
	            else
	            {
	                myVersionOfEntity = myChanges.FindItemByOriginalId(eventMessage.DomainObjectId);
	            }

	            if (myVersionOfEntity.HasValue)
	            {
	                var databaseVerionOfEntity =
	                    _scheduleRepository.LoadScheduleDataAggregate(eventMessage.InterfaceType,
	                                                                  eventMessage.DomainObjectId);
	                _scheduleDataUpdater.FillReloadedScheduleData(databaseVerionOfEntity);
	                var state = new PersistConflictMessageState(myVersionOfEntity.Value, databaseVerionOfEntity,
	                                                            eventMessage, m => RemoveFromQueue(messageQueue, m));
	                conflictsBuffer.Add(state);
	                continue;
	            }

	            if (!myVersionOfEntity.HasValue)
	            {
	                IPersistableScheduleData messageVersionOfEntity;
	                if (eventMessage.DomainUpdateType == DomainUpdateType.Delete)
	                {
	                    messageVersionOfEntity = _scheduleDataUpdater.DeleteScheduleData(eventMessage);
	                }
	                else
	                {
	                    messageVersionOfEntity = _scheduleDataUpdater.UpdateInsertScheduleData(eventMessage);
	                }
	                if (messageVersionOfEntity != null)
	                    refreshedEntitiesBuffer.Add(messageVersionOfEntity);
	                //denna gör lite fel idag, ska ta hänsyn till både gammalt och nytt
	                messageQueue.Remove(eventMessage);
	            }
	        }
	    }

	    private void RemoveFromQueue(ICollection<IEventMessage> messageQueue, IEventMessage m) 
		{
            messageQueue.Remove(m);
            _scheduleDataUpdater.NotifyMessageQueueSize();
        }
    }
}