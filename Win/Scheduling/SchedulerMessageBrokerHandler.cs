using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using log4net;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Events;

namespace Teleopti.Ccc.Win.Scheduling
{
	public class SchedulerMessageBrokerHandler : IMessageBrokerIdentifier, IDisposable, IOwnMessageQueue, IReassociateData, IUpdateScheduleDataFromMessages, IUpdateMeetingsFromMessages, IUpdatePersonRequestsFromMessages
	{
		private SchedulingScreen _owner;
		private readonly IScheduleScreenRefresher _scheduleScreenRefresher;
		private readonly Guid _instanceId = Guid.NewGuid();
		private static readonly ILog Log = LogManager.GetLogger(typeof(SchedulerMessageBrokerHandler));
		private readonly IList<IEventMessage> _messageQueue = new List<IEventMessage>();

		public Guid InstanceId
		{
			get { return _instanceId; }
		}

		public SchedulerMessageBrokerHandler(SchedulingScreen owner, ILifetimeScope container)
		{
		    if (owner == null) throw new ArgumentNullException("owner");
		    _owner = owner;

			_scheduleScreenRefresher = container.Resolve<IScheduleScreenRefresher>(
				TypedParameter.From<IOwnMessageQueue>(this),
				TypedParameter.From(container.Resolve<IScheduleDataRefresher>(
					TypedParameter.From<IUpdateScheduleDataFromMessages>(this)
					)),
				TypedParameter.From(container.Resolve<IMeetingRefresher>(
					TypedParameter.From<IUpdateMeetingsFromMessages>(this)
					)),
				TypedParameter.From(container.Resolve<IPersonRequestRefresher>(
					TypedParameter.From<IUpdatePersonRequestsFromMessages>(this)
					))
			);
		}

		public void Listen(DateTimePeriod period)
		{
			// add new message listener here
			StateHolder.Instance.StateReader.ApplicationScopeData.Messaging.RegisterEventSubscription(OnEventMessage,
			                                                                                          _owner.SchedulerState.RequestedScenario.Id.GetValueOrDefault(), typeof (Scenario), typeof (IScheduleChangedEvent),
			                                                                                          period.StartDateTime,
			                                                                                          period.EndDateTime);
			StateHolder.Instance.StateReader.ApplicationScopeData.Messaging.RegisterEventSubscription(OnEventMessage,
																					   typeof(IPersistableScheduleData),
																					   period.StartDateTime,
																					   period.EndDateTime);
			StateHolder.Instance.StateReader.ApplicationScopeData.Messaging.RegisterEventSubscription(OnEventMessage,
																					   typeof(IMeeting));
			StateHolder.Instance.StateReader.ApplicationScopeData.Messaging.RegisterEventSubscription(OnEventMessage,
																					   typeof(IPersonRequest));
		}

		public void StopListen()
		{
			StateHolder.Instance.StateReader.ApplicationScopeData.Messaging.UnregisterEventSubscription(OnEventMessage);
		}

		private void OnEventMessage(object sender, EventMessageArgs e)
		{
			if (_owner.IsDisposed)
				return;
			if (e.Message.ModuleId == InstanceId)
				return;
			if (_owner.InvokeRequired)
			{
				_owner.BeginInvoke(new EventHandler<EventMessageArgs>(OnEventMessage), sender, e);
			}
			else
			{
				_messageQueue.Add(e.Message);
				_owner.SizeOfMessageBrokerQueue(_messageQueue.Count);
			}
		}

		public void Refresh(ICollection<IPersistableScheduleData> refreshedEntitiesBuffer, ICollection<PersistConflictMessageState> conflictsBuffer)
		{
			_scheduleScreenRefresher.Refresh(_owner.SchedulerState.Schedules, _messageQueue, refreshedEntitiesBuffer, conflictsBuffer);
		}

		public void FillReloadedScheduleData(IPersistableScheduleData databaseVersionOfEntity)
		{
			var changeInfo = databaseVersionOfEntity as IChangeInfo;
			if (changeInfo != null)
				LazyLoadingManager.Initialize(changeInfo.UpdatedBy);
		}

	    public void ReassociateDataWithAllPeople()
		{
			var uow = UnitOfWorkFactory.Current.CurrentUnitOfWork();
			uow.Reassociate(_owner.SchedulerState.SchedulingResultState.PersonsInOrganization);
			uow.Reassociate(DataToReassociate(null));
		}

		public IEnumerable<IAggregateRoot>[] DataToReassociate(IPerson personToReassociate)
		{
			IEnumerable<IAggregateRoot> personsToReassociate = 
				personToReassociate != null ? 
				new[] {personToReassociate} : 
				new IAggregateRoot[] {};
			return new[]
			       	{
			       		new IAggregateRoot[] {_owner.SchedulerState.RequestedScenario},
			       		personsToReassociate,
			       		_owner.MultiplicatorDefinitionSet.Cast<IAggregateRoot>(),
			       		_owner.SchedulerState.CommonStateHolder.Absences.Cast<IAggregateRoot>(),
			       		_owner.SchedulerState.CommonStateHolder.DayOffs.Cast<IAggregateRoot>(),
			       		_owner.SchedulerState.CommonStateHolder.Activities.Cast<IAggregateRoot>(),
			       		_owner.SchedulerState.CommonStateHolder.ShiftCategories.Cast<IAggregateRoot>()
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
				if (eventMessage.ModuleId != InstanceId)
				{
					deleteMeetingOnEvent(eventMessage);
					if (eventMessage.DomainUpdateType != DomainUpdateType.Delete)
					{
						updateInsertOnEventMeetings(eventMessage);
					}
					NotifySchedulesUpdated();
				}
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
				if (eventMessage.ModuleId != InstanceId)
				{
					var deletedRequest = deleteOnEventRequest(eventMessage);
					var onRequestDeletedFromBroker = RequestDeletedFromBroker;
					if (onRequestDeletedFromBroker != null)
						onRequestDeletedFromBroker(this, new CustomEventArgs<IPersonRequest>(deletedRequest));

					if (eventMessage.DomainUpdateType != DomainUpdateType.Delete)
					{
						IPersonRequest insertedRequest = updateInsertOnEventRequests(eventMessage);
						var onRequestInsertedFromBroker = RequestInsertedFromBroker;
						if (onRequestInsertedFromBroker != null)
							onRequestInsertedFromBroker(this, new CustomEventArgs<IPersonRequest>(insertedRequest));
					}
				}
			}
		}

		public IPersistableScheduleData DeleteScheduleData(IEventMessage eventMessage)
		{
			if (Log.IsInfoEnabled)
				Log.Info("Message broker - Removing " + eventMessage.DomainObjectType + " [" + eventMessage.DomainObjectId + "]");

			return _owner.SchedulerState.Schedules.DeleteFromBroker(eventMessage.DomainObjectId);
		}

		private void deleteMeetingOnEvent(IEventMessage message)
		{
			if (Log.IsInfoEnabled)
				Log.Info("Message broker - Removing personal meeting belonging to " + message.DomainObjectType + " [" +
						 message.DomainObjectId + "]");

			_owner.SchedulerState.Schedules.DeleteMeetingFromBroker(message.DomainObjectId);
		}

		private IPersonRequest deleteOnEventRequest(IEventMessage message)
		{
			if (Log.IsInfoEnabled)
				Log.Info("Message broker - Removing person request belonging to " + message.DomainObjectType + " [" + message.DomainObjectId + "]");
			
			return _owner.SchedulerState.RequestDeleteFromBroker(message.DomainObjectId);
		}

		public IPersistableScheduleData UpdateInsertScheduleData(IEventMessage eventMessage)
			{
			var unitOfWorkFactory = UnitOfWorkFactory.Current;

				if (Log.IsInfoEnabled)
				Log.Info("Message broker - Updating " + eventMessage.DomainObjectType + " [" + eventMessage.DomainObjectId + "]");

			if (eventMessage.InterfaceType.IsAssignableFrom(typeof(IPersonAssignment)))
				{
				return _owner.SchedulerState.Schedules.UpdateFromBroker(new PersonAssignmentRepository(unitOfWorkFactory), eventMessage.DomainObjectId);
				}
			if (eventMessage.InterfaceType.IsAssignableFrom(typeof(IPersonAbsence)))
				{
				return _owner.SchedulerState.Schedules.UpdateFromBroker(new PersonAbsenceRepository(unitOfWorkFactory), eventMessage.DomainObjectId);
				}
			if (eventMessage.InterfaceType.IsAssignableFrom(typeof(IPreferenceDay)))
				{
				return _owner.SchedulerState.Schedules.UpdateFromBroker(new PreferenceDayRepository(unitOfWorkFactory), eventMessage.DomainObjectId);
				}
			if (eventMessage.InterfaceType.IsAssignableFrom(typeof(INote)))
				{
				return _owner.SchedulerState.Schedules.UpdateFromBroker(new NoteRepository(unitOfWorkFactory), eventMessage.DomainObjectId);
				}
            if (eventMessage.InterfaceType.IsAssignableFrom(typeof(IStudentAvailabilityDay)))
            {
                return _owner.SchedulerState.Schedules.UpdateFromBroker(new StudentAvailabilityDayRepository(unitOfWorkFactory), eventMessage.DomainObjectId);
            }
            if (eventMessage.InterfaceType.IsAssignableFrom(typeof(IOvertimeAvailability)))
            {
                return _owner.SchedulerState.Schedules.UpdateFromBroker(new OvertimeAvailabilityRepository (unitOfWorkFactory), eventMessage.DomainObjectId);
            }

			Log.Warn("Message broker - Got a message of an unknown IScheduleData type: " + eventMessage.DomainObjectType);

			return null;
		}

		private void updateInsertOnEventMeetings(IEventMessage message)
		{
			_owner.SchedulerState.Schedules.MeetingUpdateFromBroker(new MeetingRepository(UnitOfWorkFactory.Current), message.DomainObjectId);
		}

		private IPersonRequest updateInsertOnEventRequests(IEventMessage message)
		{
			return _owner.SchedulerState.RequestUpdateFromBroker(new PersonRequestRepository(UnitOfWorkFactory.Current), message.DomainObjectId);
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
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Virtual dispose method
		/// </summary>
		/// <param name="disposing">
		/// If set to <c>true</c>, explicitly called.
		/// If set to <c>false</c>, implicitly called from finalizer.
		/// </param>
		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				ReleaseManagedResources();
			}
			ReleaseUnmanagedResources();
		}

		/// <summary>
		/// Releases the unmanaged resources.
		/// </summary>
		protected virtual void ReleaseUnmanagedResources()
		{
		}

		/// <summary>
		/// Releases the managed resources.
		/// </summary>
		protected virtual void ReleaseManagedResources()
		{
			_owner = null;
		}
		#endregion

		public void NotifyMessageQueueSize()
		{
			_owner.SizeOfMessageBrokerQueue(_messageQueue.Count);
		}
	}
}
