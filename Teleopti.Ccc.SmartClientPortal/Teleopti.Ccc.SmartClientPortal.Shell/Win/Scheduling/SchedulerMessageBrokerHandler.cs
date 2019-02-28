using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using log4net;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MessageBroker.Scheduling;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Infrastructure.Persisters.Refresh;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
	public class SchedulerMessageBrokerHandler : IInitiatorIdentifier, IDisposable, IReassociateDataForSchedules, IUpdateScheduleDataFromMessages, IUpdateMeetingsFromMessages, IUpdatePersonRequestsFromMessages, IMessageQueueRemoval
	{
		private SchedulingScreen _owner;
		private readonly ILifetimeScope _container;
		private readonly IScheduleScreenRefresher _scheduleScreenRefresher;
		private readonly Guid _instanceId = Guid.NewGuid();
		private static readonly ILog Log = LogManager.GetLogger(typeof(SchedulerMessageBrokerHandler));
		private readonly IList<IEventMessage> _messageQueue = new List<IEventMessage>();
		private readonly IScheduleMessageSubscriber _scheduleMessageSubscriber;
		private readonly IJsonDeserializer _deserializer;
		private readonly IScheduleStorage _scheduleStorage;

		public Guid InitiatorId => _instanceId;

		public SchedulerMessageBrokerHandler(SchedulingScreen owner, ILifetimeScope container)
		{
			if (owner == null) throw new ArgumentNullException(nameof(owner));
			_owner = owner;
			_container = container;
			_scheduleScreenRefresher = container.Resolve<IScheduleScreenRefresher>(
				TypedParameter.From<IReassociateDataForSchedules>(this),
				TypedParameter.From(container.Resolve<IScheduleRefresher>(
					TypedParameter.From<IUpdateScheduleDataFromMessages>(this),
					TypedParameter.From<IMessageQueueRemoval>(this)
					)),
				TypedParameter.From(container.Resolve<IScheduleDataRefresher>(
					TypedParameter.From<IUpdateScheduleDataFromMessages>(this),
					TypedParameter.From<IMessageQueueRemoval>(this)
					)),
				TypedParameter.From(container.Resolve<IMeetingRefresher>(
					TypedParameter.From<IUpdateMeetingsFromMessages>(this),
					TypedParameter.From<IMessageQueueRemoval>(this)
					)),
				TypedParameter.From(container.Resolve<IPersonRequestRefresher>(
					TypedParameter.From<IUpdatePersonRequestsFromMessages>(this),
					TypedParameter.From<IMessageQueueRemoval>(this)
					))
			);
			_scheduleMessageSubscriber = container.Resolve<IScheduleMessageSubscriber>();
			_deserializer = container.Resolve<IJsonDeserializer>();
			_scheduleStorage = container.Resolve<IScheduleStorage>();
		}

		public void Listen(DateTimePeriod period)
		{
			_scheduleMessageSubscriber.Subscribe(_owner.SchedulerState.SchedulerStateHolder.RequestedScenario.Id.GetValueOrDefault(), period, onEventMessage);

		}

		private void onEventMessage(object sender, EventMessageArgs e)
		{
			if (_owner == null)
				return;
			if (_owner.IsDisposed) return;
			if (e.Message == null)
				return;
			if (e.Message.ModuleId == InitiatorId) return;
			if (_owner.InvokeRequired)
			{
				_owner.BeginInvoke(new EventHandler<EventMessageArgs>(onEventMessage), sender, e);
				return;
			}
			if (e.Message.InterfaceType.IsAssignableFrom(typeof (IScheduleChangedMessage)))
			{
				dispatchAggregatedScheduleChangeEvent(sender, e);
			}

			if (!messageIsRelevant(e.Message)) return;
			_messageQueue.Add(e.Message);
			_owner.SizeOfMessageBrokerQueue(_messageQueue.Count);
		}

		private void dispatchAggregatedScheduleChangeEvent(object sender, EventMessageArgs e)
		{
			var message = e.InternalMessage;
			foreach (var personId in _deserializer.DeserializeObject<Guid[]>(message.BinaryData))
			{
				_owner.BeginInvoke(new EventHandler<EventMessageArgs>(onEventMessage), sender,
					new EventMessageArgs(new EventMessage
					{
						InterfaceType = typeof (IScheduleChangedEvent),
						DomainObjectType = typeof (IScheduleChangedEvent).Name,
						DomainObjectId = personId,
						ModuleId = message.ModuleIdAsGuid(),
						ReferenceObjectId = message.DomainReferenceIdAsGuid(),
						EventStartDate = message.StartDateAsDateTime(),
						EventEndDate = message.EndDateAsDateTime(),
						DomainUpdateType = message.DomainUpdateTypeAsDomainUpdateType(),
					}));
			}
		}

		private bool messageIsRelevant(IEventMessage message)
		{
			return isRelevantPerson(message.DomainObjectId) || isRelevantPerson(message.ReferenceObjectId) ||
				   message.InterfaceType.IsAssignableFrom(typeof (IMeeting)) ||
				   message.InterfaceType.IsAssignableFrom(typeof (IPersonRequest));
		}

		public void Refresh(ICollection<IPersistableScheduleData> refreshedEntitiesBuffer, ICollection<PersistConflict> conflictsBuffer, bool loadRequests)
		{
			_scheduleScreenRefresher.Refresh(_owner.SchedulerState.SchedulerStateHolder.Schedules, _messageQueue, refreshedEntitiesBuffer, conflictsBuffer, isRelevantPerson, loadRequests);
		}

		private bool isRelevantPerson(Guid personId)
		{
			return _owner.SchedulerState.SchedulerStateHolder.SchedulingResultState.LoadedAgents.Any(p => p.Id == personId);
		}

		public void FillReloadedScheduleData(IPersistableScheduleData databaseVersionOfEntity)
		{
			var changeInfo = databaseVersionOfEntity as IChangeInfo;
			if (changeInfo != null)
				LazyLoadingManager.Initialize(changeInfo.UpdatedBy);
		}

	  public void ReassociateDataForAllPeople()
		{
			var uow = UnitOfWorkFactory.Current.CurrentUnitOfWork();
			uow.Reassociate(_owner.SchedulerState.SchedulerStateHolder.SchedulingResultState.LoadedAgents);
			reassociateScheduleStuff(uow);
		}

		public void ReassociateDataFor(IPerson person)
		{
			var uow = UnitOfWorkFactory.Current.CurrentUnitOfWork();
			uow.Reassociate(person);
			reassociateScheduleStuff(uow);
		}

		private void reassociateScheduleStuff(IUnitOfWork unitOfWork)
		{
			unitOfWork.Reassociate(_owner.SchedulerState.SchedulerStateHolder.RequestedScenario);
			unitOfWork.Reassociate(_owner.MultiplicatorDefinitionSet);
			unitOfWork.Reassociate(_owner.SchedulerState.SchedulerStateHolder.CommonStateHolder.Absences);
			unitOfWork.Reassociate(_owner.SchedulerState.SchedulerStateHolder.CommonStateHolder.DayOffs);
			unitOfWork.Reassociate(_owner.SchedulerState.SchedulerStateHolder.CommonStateHolder.Activities);
			unitOfWork.Reassociate(_owner.SchedulerState.SchedulerStateHolder.CommonStateHolder.ShiftCategories);
			unitOfWork.Reassociate(_owner.SchedulerState.ScheduleTags.Where(scheduleTag => scheduleTag.Id != null).ToList());
		}

		public IEnumerable<IAggregateRoot>[] DataToReassociate(IPerson personToReassociate)
		{
			IEnumerable<IAggregateRoot> personsToReassociate = 
				personToReassociate != null ? 
				new[] {personToReassociate} : 
				new IAggregateRoot[] {};
			return new[]
					{
						new IAggregateRoot[] {_owner.SchedulerState.SchedulerStateHolder.RequestedScenario},
						personsToReassociate,
						_owner.MultiplicatorDefinitionSet,
						_owner.SchedulerState.SchedulerStateHolder.CommonStateHolder.Absences,
						_owner.SchedulerState.SchedulerStateHolder.CommonStateHolder.DayOffs,
						_owner.SchedulerState.SchedulerStateHolder.CommonStateHolder.Activities,
						_owner.SchedulerState.SchedulerStateHolder.CommonStateHolder.ShiftCategories,
						_owner.SchedulerState.ScheduleTags.Where(scheduleTag => scheduleTag.Id != null).ToList()
					};
		}

		public event EventHandler<EventArgs> SchedulesUpdatedFromBroker;

		public void UpdateMeeting(IEventMessage eventMessage)
		{
			if (_owner.InvokeRequired)
			{
				_owner.BeginInvoke(new Action<IEventMessage>(UpdateMeeting), eventMessage);
			}
			else
			{
				if (_owner.IsDisposed) return;
				Log.Debug("Message broker - catched event");

				deleteMeetingOnEvent(eventMessage);
				if (eventMessage.DomainUpdateType != DomainUpdateType.Delete)
				{
					updateInsertOnEventMeetings(eventMessage);
				}
				NotifySchedulesUpdated();
			}
		}

		public event EventHandler<CustomEventArgs<IPersonRequest>> RequestDeletedFromBroker;
		public event EventHandler<CustomEventArgs<IPersonRequest>> RequestInsertedFromBroker;

		public void UpdatePersonRequest(IEventMessage eventMessage)
		{
			if (_owner.InvokeRequired)
			{
				_owner.BeginInvoke(new Action<IEventMessage>(UpdatePersonRequest), eventMessage);
			}
			else
			{
				if (_owner.IsDisposed) return;
				Log.Debug("Message broker - catched event");
				if (eventMessage.ModuleId != InitiatorId)
				{
					var deletedRequest = deleteOnEventRequest(eventMessage);
					RequestDeletedFromBroker?.Invoke(this, new CustomEventArgs<IPersonRequest>(deletedRequest));

					if (eventMessage.DomainUpdateType != DomainUpdateType.Delete)
					{
						IPersonRequest insertedRequest = updateInsertOnEventRequests(eventMessage);
						RequestInsertedFromBroker?.Invoke(this, new CustomEventArgs<IPersonRequest>(insertedRequest));
					}
				}
			}
		}

		public IPersistableScheduleData DeleteScheduleData(IEventMessage eventMessage)
		{
			if (Log.IsInfoEnabled)
				Log.Info("Message broker - Removing " + eventMessage.DomainObjectType + " [" + eventMessage.DomainObjectId + "]");

			return _owner.SchedulerState.SchedulerStateHolder.Schedules.DeleteFromBroker(eventMessage.DomainObjectId);
		}

		private void deleteMeetingOnEvent(IEventMessage message)
		{
			if (Log.IsInfoEnabled)
				Log.Info("Message broker - Removing personal meeting belonging to " + message.DomainObjectType + " [" +
						 message.DomainObjectId + "]");

			_owner.SchedulerState.SchedulerStateHolder.Schedules.DeleteMeetingFromBroker(message.DomainObjectId);
		}

		private IPersonRequest deleteOnEventRequest(IEventMessage message)
		{
			if (Log.IsInfoEnabled)
				Log.Info("Message broker - Removing person request belonging to " + message.DomainObjectType + " [" + message.DomainObjectId + "]");
			
			return _owner.SchedulerState.RequestDeleteFromBroker(message.DomainObjectId);
		}

		public IPersistableScheduleData UpdateInsertScheduleData(IEventMessage eventMessage)
			{

			var currentUnitOfWork = new FromFactory(() => UnitOfWorkFactory.Current);

				if (Log.IsInfoEnabled)
				Log.Info("Message broker - Updating " + eventMessage.DomainObjectType + " [" + eventMessage.DomainObjectId + "]");

			if (eventMessage.InterfaceType.IsAssignableFrom(typeof(IAgentDayScheduleTag)))
			{
				return _owner.SchedulerState.SchedulerStateHolder.Schedules.UpdateFromBroker(AgentDayScheduleTagRepository.DONT_USE_CTOR(currentUnitOfWork), eventMessage.DomainObjectId);
			}

			if (eventMessage.InterfaceType.IsAssignableFrom(typeof(IPersonAssignment)))
				{
				return _owner.SchedulerState.SchedulerStateHolder.Schedules.UpdateFromBroker(_container.Resolve<IPersonAssignmentRepository>(), eventMessage.DomainObjectId);
				}
			if (eventMessage.InterfaceType.IsAssignableFrom(typeof(IPersonAbsence)))
				{
				return _owner.SchedulerState.SchedulerStateHolder.Schedules.UpdateFromBroker(new PersonAbsenceRepository(currentUnitOfWork), eventMessage.DomainObjectId);
				}
			if (eventMessage.InterfaceType.IsAssignableFrom(typeof(IPreferenceDay)))
				{
				return _owner.SchedulerState.SchedulerStateHolder.Schedules.UpdateFromBroker(new PreferenceDayRepository(currentUnitOfWork), eventMessage.DomainObjectId);
				}
			if (eventMessage.InterfaceType.IsAssignableFrom(typeof(INote)))
				{
				return _owner.SchedulerState.SchedulerStateHolder.Schedules.UpdateFromBroker(new NoteRepository(currentUnitOfWork), eventMessage.DomainObjectId);
				}
			if (eventMessage.InterfaceType.IsAssignableFrom(typeof(IPublicNote)))
				{
				return _owner.SchedulerState.SchedulerStateHolder.Schedules.UpdateFromBroker(PublicNoteRepository.DONT_USE_CTOR(currentUnitOfWork), eventMessage.DomainObjectId);
				}
			if (eventMessage.InterfaceType.IsAssignableFrom(typeof(IStudentAvailabilityDay)))
			{
				return _owner.SchedulerState.SchedulerStateHolder.Schedules.UpdateFromBroker(StudentAvailabilityDayRepository.DONT_USE_CTOR(currentUnitOfWork), eventMessage.DomainObjectId);
			}
			if (eventMessage.InterfaceType.IsAssignableFrom(typeof(IOvertimeAvailability)))
			{
				return _owner.SchedulerState.SchedulerStateHolder.Schedules.UpdateFromBroker(new OvertimeAvailabilityRepository (currentUnitOfWork), eventMessage.DomainObjectId);
			}

			Log.Warn("Message broker - Got a message of an unknown IScheduleData type: " + eventMessage.DomainObjectType);

			return null;
		}

		private void updateInsertOnEventMeetings(IEventMessage message)
		{
			_owner.SchedulerState.SchedulerStateHolder.Schedules.MeetingUpdateFromBroker(new MeetingRepository(new FromFactory(() => UnitOfWorkFactory.Current)), message.DomainObjectId);
		}

		private IPersonRequest updateInsertOnEventRequests(IEventMessage message)
		{
			return _owner.SchedulerState.RequestUpdateFromBroker(PersonRequestRepository.DONT_USE_CTOR(new FromFactory(() => UnitOfWorkFactory.Current)), message.DomainObjectId, _scheduleStorage);
		}

		private void NotifySchedulesUpdated()
		{
			var handler = SchedulesUpdatedFromBroker;
			if (handler!= null)
			{
				handler.Invoke(this, EventArgs.Empty);
			}
		}

		internal void HandleMeetingChange(IMeeting meeting, bool deleted)
		{
			var meetingEventMessage = new EventMessage();
			meetingEventMessage.ModuleId = Guid.Empty;
			meetingEventMessage.DomainObjectId = meeting.Id.GetValueOrDefault(Guid.Empty);
			meetingEventMessage.InterfaceType = typeof (IMeeting);
			meetingEventMessage.DomainUpdateType = deleted ? DomainUpdateType.Delete : DomainUpdateType.Update;
			var eventMessageArgs = new EventMessageArgs(meetingEventMessage);
			UpdateMeeting(eventMessageArgs.Message);
		}

		#region IDisposable Members

		public void Dispose()
		{
			Disposing(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Disposing(bool disposing)
		{
			if (disposing)
			{
				ReleaseManagedResources();
			}
			ReleaseUnmanagedResources();

		}

		protected virtual void ReleaseUnmanagedResources()
		{
		}

		protected virtual void ReleaseManagedResources()
		{
			stopListen();
			_owner = null;
		}

		private void stopListen()
		{
			_scheduleMessageSubscriber.Unsubscribe(onEventMessage);
		}

		#endregion

		public void NotifyMessageQueueSizeChange()
		{
			_owner.SizeOfMessageBrokerQueue(_messageQueue.Count);
		}

		public void Remove(IEventMessage eventMessage)
		{
			_messageQueue.Remove(eventMessage);
			NotifyMessageQueueSizeChange();
		}

		public void Remove(PersistConflict persistConflict)
		{
			var id = persistConflict.InvolvedId();
			for (var i = _messageQueue.Count - 1; i >= 0; i--)
			{
				var theEvent = _messageQueue[i];
				if (theEvent.DomainObjectId == id)
				{
					_messageQueue.RemoveAt(i);
					NotifyMessageQueueSizeChange();
				}
			}
		}
	}
}
