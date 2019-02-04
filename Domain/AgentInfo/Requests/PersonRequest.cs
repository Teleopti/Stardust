using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class PersonRequest : AggregateRoot_Events_ChangeInfo_Versioned_BusinessUnit,
								 IPersonRequest, IDeleteTag, IPushMessageWhenRootAltered, IAggregateRoot_Events
	{
		private const int messageLength = 2000;
		private const int messageTipLength = 255;
		private readonly IPerson _person;
		private string _message;
		private BusinessRuleFlags? _brokenBusinessRules;
		private int requestStatus = (int)PersonRequestStatus.New;
		private personRequestState _requestState;
		private personRequestState _persistedState;
		private bool _deserialized;
		private string _subject;
		private bool _changed;
		private ISet<IRequest> requests = new HashSet<IRequest>();
		private bool _isDeleted;
		private string _denyReason = string.Empty;
		private DateTime _updatedOnServerUtc;	
		private PersonRequestDenyOption _personRequestDenyOption = PersonRequestDenyOption.None;

		private static readonly PersonRequestDenyOption[] _nonWaitlistedDenyOptions = new []
		{
			PersonRequestDenyOption.AlreadyAbsence,
			PersonRequestDenyOption.RequestExpired,
			PersonRequestDenyOption.InsufficientPersonAccount,
			PersonRequestDenyOption.AllPersonSkillsClosed,
			PersonRequestDenyOption.TechnicalIssues,
			PersonRequestDenyOption.PeriodToLong
		};

		protected PersonRequest()
		{
		}

		public PersonRequest(IPerson person)
			: this(person, null)
		{
		}

		public PersonRequest(IPerson person, IRequest request)
			: this()
		{
			InParameter.NotNull(nameof(person), person);
			_person = person;
			setRequest(request);
		}

		private void setRequest(IRequest request)
		{
			//We have to do it this way as cascading actions for one-to-one maappings aren't implemented in NHibernate
			requests.Clear();
			if (request != null)
			{
				request.SetParent(this);
				requests.Add(request);
			}
		}

		public virtual void Persisted()
		{
			_deserialized = true;
			_persistedState = RequestState;
			_changed = false;
		}

		public virtual DateTime RequestedDate
		{
			get
			{
				IRequest request = getRequest();
				if (request != null) return request.Period.StartDateTime;
				return CreatedOn.GetValueOrDefault(DateTime.Today);
			}
		}

		public virtual IPerson Person => _person;

		public virtual bool IsAlreadyAbsent => _personRequestDenyOption.HasFlag(PersonRequestDenyOption.AlreadyAbsence);

		public virtual bool IsExpired => _personRequestDenyOption.HasFlag(PersonRequestDenyOption.RequestExpired);

		public virtual bool InsufficientPersonAccount => _personRequestDenyOption.HasFlag(PersonRequestDenyOption.InsufficientPersonAccount);

		public virtual bool TrySetMessage(string message)
		{
			if(!checkIfCanSetMessage()) return false;
			message = message ?? string.Empty;
			if (message.Length > messageLength)
				return false;

			var request = Request;
			if (request != null && message != _message)
			{
				request.TextForNotification = message.Length > messageTipLength ? message.Substring(0, messageTipLength - 3) + "..." : message;
				//Need to set this to make the message to be sent
			}

			_message = message;
			notifyPropertyChanged(nameof(Message));
			return true;
		}

		public virtual string GetMessage(ITextFormatter formatter)
		{
			if (formatter == null)
				throw new ArgumentNullException(nameof(formatter));

			return formatter.Format(_message);
		}

		private string Message => _message;

		public virtual bool TrySetBrokenBusinessRule(BusinessRuleFlags brokenRules)
		{
			checkIfEditable();
			_brokenBusinessRules = brokenRules;
			return true;
		}

		public virtual BusinessRuleFlags? BrokenBusinessRules => _brokenBusinessRules;
		
		public virtual IRequest Request
		{
			get { return getRequest(); }
			set { setRequest(value); }
		}

		public virtual string Subject
		{
			protected get { return _subject; }

			set
			{
				_subject = value;
				notifyPropertyChanged(nameof(Subject));
			}
		}


		public virtual string GetSubject(ITextFormatter formatter)
		{
			if (formatter == null)
				throw new ArgumentNullException(nameof(formatter));

			return formatter.Format(_subject);
		}

		public virtual IList<IBusinessRuleResponse> Approve(IRequestApprovalService approvalService,
															IPersonRequestCheckAuthorization authorization,
															bool isAutoGrant = false)
		{
			authorization.VerifyEditRequestPermission(this);
			var brokenRules = new List<IBusinessRuleResponse>();
			brokenRules.AddRange(((Request)getRequest()).Approve(approvalService));
			if (brokenRules.Count == 0)
			{
				RequestState.Approve(isAutoGrant);
				DenyReason = string.Empty;
				notifyOnStatusChange();
			}
			return brokenRules;
		}

		private void notifyOnStatusChange()
		{
			notifyPropertyChanged(nameof(StatusText));
			notifyPropertyChanged(nameof(IsNew));
			notifyPropertyChanged(nameof(IsPending));
			notifyPropertyChanged(nameof(IsDenied));
			notifyPropertyChanged(nameof(IsApproved));
		}

		public virtual void Deny(string denyReasonTextResourceKey,
			IPersonRequestCheckAuthorization authorization, IPerson denyPerson = null, PersonRequestDenyOption personRequestDenyOption = PersonRequestDenyOption.None)
		{
			var wasWaitlisted = IsWaitlisted;
			_personRequestDenyOption = personRequestDenyOption;
			authorization.VerifyEditRequestPermission(this);
			if (canDeny(personRequestDenyOption))
			{
				RequestState.Deny(personRequestDenyOption);
			}
			
			var request = getRequest();
			request?.Deny(denyPerson);

			_denyReason = denyReasonTextResourceKey ?? string.Empty;
			if (wasWaitlisted && IsWaitlisted)
				return;
			notifyOnStatusChange();
		}

		public virtual void Cancel(IPersonRequestCheckAuthorization authorization)
		{
			if (!authorization.HasCancelRequestPermission (this))
			{
				throw new PermissionException (Resources.InsufficientPermission);
			}

			var request = getRequest();
			if (!(request is AbsenceRequest absenceRequest))
			{
				return;
			}

			RequestState.Cancel();
			absenceRequest.Cancel();

			notifyOnStatusChange();
		}

		private bool canDeny(PersonRequestDenyOption personRequestDenyOption)
		{
			var isAutoDeny = personRequestDenyOption.HasFlag(PersonRequestDenyOption.AutoDeny);
			var nonWaitlistedDeny = _nonWaitlistedDenyOptions.Any(o=> personRequestDenyOption.HasFlag(o));
			var isDeniedButNotWaitlisted = IsDenied && !IsWaitlisted;
			return (!isAutoDeny && IsWaitlisted) || !IsDenied || nonWaitlistedDeny || isDeniedButNotWaitlisted;
		}

		public virtual bool IsEditable
		{
			get
			{
				if (PersistedRequestState.IsPending)
				{
					return true;
				}
				if (PersistedRequestState.IsNew)
				{
					return true;
				}
				return false;
			}
		}

		public virtual bool Changed
		{
			get { return _changed; }
			set { _changed = value; }
		}

		public virtual bool IsDeleted => _isDeleted;

		public virtual DateTime UpdatedOnServerUtc => _updatedOnServerUtc;

		private bool checkIfCanSetMessage()
		{
			if (PersistedRequestState.IsPending)
			{
				return true;
			}
			if (PersistedRequestState.IsNew)
			{
				return true;
			}
			if (PersistedRequestState.IsWaitlisted)
			{
				return true;
			}
			if (PersistedRequestState.IsApproved)
			{
				return true;
			}
			throw new InvalidOperationException();
		}

		private void checkIfEditable()
		{
			if (!IsEditable)
				throw new InvalidOperationException("Requests cannot be changed once they have been handled.");
		}

		#region ICloneableEntity<PersonRequest> Members

		public virtual IPersonRequest NoneEntityClone()
		{
			var clone = (PersonRequest)MemberwiseClone();
			CloneEvents(clone);
			var request = getRequest();
			if (request != null)
			{
				IRequest requestClone = request.NoneEntityClone();
				clone.requests = new HashSet<IRequest>();
				clone.setRequest(requestClone);
			}
			clone.SetId(null);

			return clone;
		}

		public virtual IPersonRequest EntityClone()
		{
			var clone = (PersonRequest)MemberwiseClone();
			CloneEvents(clone);
			var request = getRequest();
			if (request != null)
			{
				IRequest requestClone = request.EntityClone();
				clone.requests = new HashSet<IRequest>();
				clone.setRequest(requestClone);
			}
			clone.SetId(Id);

			return clone;
		}

		#endregion

		#region ICloneable Members

		public virtual object Clone()
		{
			return NoneEntityClone();
		}

		#endregion

		public virtual bool Reply(string answerMessage)
		{
			if (string.IsNullOrEmpty(answerMessage))
				return true;
			var builder = new StringBuilder();
			builder.AppendLine(Message);
			builder.Append(answerMessage);
			return TrySetMessage(builder.ToString());
		}

		//Need to add this one to make the check without setting the reply test (so we can undo/redo)
		public virtual bool CheckReplyTextLength(string answerMessage)
		{
			var builder = new StringBuilder();
			builder.AppendLine(Message);
			builder.Append(answerMessage);
			checkIfCanSetMessage();
			return builder.Length <= messageLength;
		}

		public virtual void Pending()
		{
			if (PersistedRequestState.IsPending)
			{
				RequestState = PersistedRequestState;
			}
			if (RequestState.IsNew)
			{
				RequestState.MakePending();
			}
			notifyOnStatusChange();
		}

		public virtual void ForcePending()
		{
			moveToPending();
			notifyOnStatusChange();
		}

		#region INotifyPropertyChanged Members

		public virtual event PropertyChangedEventHandler PropertyChanged;

		#endregion

		private void notifyPropertyChanged(string info)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
			_changed = true;
		}

		private IRequest getRequest()
		{
			//We have to do it this way as cascading actions for one-to-one maappings aren't implemented in NHibernate
			if (requests.Count == 0)
				return null;

			return requests.Single();
		}

		public virtual void Restore(IPersonRequest previousState)
		{
			_message = previousState.GetMessage(new NoFormatting());
			var previousStateTyped = (PersonRequest)previousState;
			RequestState = previousStateTyped.RequestState;
			Subject = previousState.GetSubject(new NoFormatting());
			_denyReason = previousState.DenyReason;
			_isDeleted = previousStateTyped.IsDeleted;
			setRequest(previousState.Request);
		}

		public virtual IMemento CreateMemento()
		{
			return new Memento<IPersonRequest>(this, EntityClone());
		}

		public virtual void SetDeleted()
		{
			_isDeleted = true;
		}

		public virtual bool ShouldSendPushMessageWhenAltered()
		{
			return Request.ShouldNotifyWithMessage;
		}

		public virtual ISendPushMessageService PushMessageWhenAlteredInformation()
		{
			MessageType type = MessageType.Information;
			if (IsNew) return null;
			if (IsWaitlisted && !_changed) return null;

			string message = Request.TextForNotification;
			if (Request is IShiftTradeRequest shiftTradeRequest && shiftTradeRequest.Offer != null)
			{
				type = MessageType.ShiftTradeFromOffer;
			}
			var title = string.IsNullOrEmpty(Subject) ? message : Subject;

			return SendPushMessageService.CreateConversation(title, message, false, type).
										  To(Request.ReceiversForNotification).TranslateMessage().AddReplyOption("OK");
		}

		public virtual string DenyReason
		{
			get { return _denyReason ?? string.Empty; }
			protected set
			{
				//bug 40906 max 150 in db
				if (value.Length > 150)
					value = value.Substring(150);
				_denyReason = value;
			}
		}

		public virtual void SetNew()
		{
			RequestState.SetNew();
			notifyOnStatusChange();
		}
		
		private void moveToPending()
		{
			RequestState = new pendingPersonRequest(this);
		}

		private void moveToApproved(bool isAutoGrant)
		{
			if (isAutoGrant)
				RequestState = new autoApprovedPersonRequest(this);
			else
				RequestState = new approvedPersonRequest(this);
		}


		private void moveToCancelled()
		{
		
			RequestState = new cancelledPersonRequest (this);
		}


		private void moveToDenied(PersonRequestDenyOption personRequestDenyOption)
		{
			var autoDenied = personRequestDenyOption.HasFlag(PersonRequestDenyOption.AutoDeny);
			var nonWaitlistedDeny = _nonWaitlistedDenyOptions.Any(o => personRequestDenyOption.HasFlag(o));
			if (autoDenied)
			{
				if (waitlistingIsEnabled() && !nonWaitlistedDeny)
				{
					RequestState = new waitListedPersonRequest(this);
				}
				else
				{
					RequestState = new autoDeniedPersonRequest(this);
				}
			}
			else
			{
				RequestState = new deniedPersonRequest(this);
			}
		}


		private bool waitlistingIsEnabled()
		{
			if (getRequest() is IAbsenceRequest absenceRequest)
			{
				var workflowControlSet = Person.WorkflowControlSet;
				if (workflowControlSet != null && workflowControlSet.WaitlistingIsEnabled(absenceRequest))
				{
					return true;
				}
			}

			return false;
		}

		private void movetoNew()
		{
			RequestState = new newPersonRequest(this);
		}

		public virtual bool IsNew => RequestState.IsNew;

		public virtual bool IsPending => RequestState.IsPending;

		public virtual bool IsDenied => RequestState.IsDenied;

		public virtual bool IsAutoDenied => RequestState.IsAutoDenied;

		public virtual bool IsWaitlisted => RequestState.IsWaitlisted;

		public virtual bool IsCancelled => RequestState.IsCancelled;
		
		public virtual bool IsApproved => RequestState.IsApproved;

		public virtual bool IsAutoAproved => RequestState.IsAutoApproved;

		private personRequestState PersistedRequestState => _persistedState ?? (_persistedState = RequestState);

		private personRequestState RequestState
		{
			get { return _requestState ?? (_requestState = personRequestState.CreateFromId(this, requestStatus)); }
			set
			{
				if (!_deserialized)
				{
					_persistedState = RequestState;
					_deserialized = true;
				}
				_requestState = value;
				requestStatus = _requestState.RequestStatusId;
			}
		}

		public virtual string StatusText => RequestState.StatusText;

		#region Classes handling status transitions

		private abstract class personRequestState
		{
			protected readonly PersonRequest PersonRequest;
			private readonly int _requestStatusId;

			protected personRequestState(PersonRequest personRequest, int requestStatusId)
			{
				PersonRequest = personRequest;
				_requestStatusId = requestStatusId;
			}

			protected internal int RequestStatusId => _requestStatusId;

			protected internal virtual bool IsDenied => false;

			protected internal virtual bool IsAutoDenied => false;

			protected internal virtual bool IsWaitlisted => false;

			protected internal virtual bool IsCancelled => false;

			protected internal virtual bool IsApproved => false;

			protected internal virtual bool IsAutoApproved => false;

			protected internal virtual bool IsPending => false;

			protected internal virtual bool IsNew => false;

			protected internal virtual void Deny(PersonRequestDenyOption denyOption)
			{
				throw new InvalidRequestStateTransitionException(string.Format(CultureInfo.InvariantCulture,
																			   "This transition is not allowed (from {0} to denied).",
																			   GetType().Name));
			}

			protected internal virtual void Approve(bool isAutoGrant)
			{
				throw new InvalidRequestStateTransitionException(string.Format(CultureInfo.InvariantCulture,
																			   "This transition is not allowed (from {0} to approved).",
																			   GetType().Name));
			}
			protected internal virtual void Cancel()
			{
				throw new InvalidRequestStateTransitionException(string.Format(CultureInfo.InvariantCulture,
																			   "This transition is not allowed (from {0} to cancelled).",
																			   GetType().Name));
			}

			protected internal virtual void MakePending()
			{
				throw new InvalidRequestStateTransitionException(string.Format(CultureInfo.InvariantCulture,
																			   "This transition is not allowed (from {0} to pending).",
																			   GetType().Name));
			}

			protected internal virtual void SetNew()
			{
				throw new InvalidRequestStateTransitionException(string.Format(CultureInfo.InvariantCulture,
																			   "This transition is not allowed (from {0} to new).",
																			   GetType().Name));
			}

			protected internal abstract string StatusText { get; }

			public static personRequestState CreateFromId(PersonRequest personRequest, int requestStatusId)
			{
				//This should be refactored into a more dynamic way...

				var status = (PersonRequestStatus)requestStatusId;

				switch (status)
				{
					case PersonRequestStatus.Pending:
						return new pendingPersonRequest(personRequest);
					case PersonRequestStatus.Denied:
						return new deniedPersonRequest(personRequest);
					case PersonRequestStatus.Approved:
						return new approvedPersonRequest(personRequest);
					case PersonRequestStatus.New:
						return new newPersonRequest(personRequest);
					case PersonRequestStatus.AutoDenied:
						return new autoDeniedPersonRequest(personRequest);
					case PersonRequestStatus.Waitlisted:
						return new waitListedPersonRequest(personRequest);
					case PersonRequestStatus.Cancelled:
						return new cancelledPersonRequest(personRequest);
				}
				throw new ArgumentOutOfRangeException(nameof(requestStatusId), @"The request status id is invalid");
			}
		}

		private class approvedPersonRequest : personRequestState
		{
			public approvedPersonRequest(PersonRequest personRequest)
				: base(personRequest, (int)PersonRequestStatus.Approved)
			{
			}

			protected internal override bool IsApproved => true;

			protected internal override void Cancel()
			{
				PersonRequest.moveToCancelled();
			}
			
			protected internal override string StatusText => Resources.Approved;
		}
		

		private class autoApprovedPersonRequest : personRequestState
		{
			public autoApprovedPersonRequest(PersonRequest personRequest)
				: base(personRequest, (int)PersonRequestStatus.Approved)
			{
			}

			protected internal override bool IsApproved => true;

			protected internal override bool IsAutoApproved => true;

			protected internal override string StatusText => Resources.Approved;
		}

		private class deniedPersonRequest : personRequestState
		{
			public deniedPersonRequest(PersonRequest personRequest)
				: base(personRequest, (int) PersonRequestStatus.Denied)
			{
			}

			protected internal override bool IsDenied => true;

			protected internal override string StatusText => Resources.Denied;
		}

		private class autoDeniedPersonRequest : personRequestState
		{
			public autoDeniedPersonRequest(PersonRequest personRequest)
				: base(personRequest, (int)PersonRequestStatus.AutoDenied)
			{
			}

			protected internal override bool IsDenied => true;

			protected internal override bool IsAutoDenied => true;

			protected internal override string StatusText => Resources.Denied;
		}

		private class waitListedPersonRequest : personRequestState
		{
			public waitListedPersonRequest(PersonRequest personRequest)
				: base(personRequest, (int)PersonRequestStatus.Waitlisted)
			{
			}
			
			protected internal override bool IsDenied => true;

			protected internal override bool IsWaitlisted => true;
			
			protected internal override void Approve(bool isAutoGrant)
			{
				PersonRequest.moveToApproved(isAutoGrant);
			}

			protected internal override void Deny(PersonRequestDenyOption denyOption)
			{
				PersonRequest.moveToDenied(denyOption);
			}

			protected internal override string StatusText => Resources.Waitlisted;
		}
		
		private class pendingPersonRequest : personRequestState
		{
			public pendingPersonRequest(PersonRequest personRequest)
				: base(personRequest, (int) PersonRequestStatus.Pending)
			{
			}
			
			protected internal override void Deny(PersonRequestDenyOption denyOption)
			{
				PersonRequest.moveToDenied(denyOption);
			}

			protected internal override void Approve(bool isAutoGrant)
			{
				PersonRequest.moveToApproved(isAutoGrant);
			}

			protected internal override void SetNew()
			{
				PersonRequest.movetoNew();
			}

			protected internal override string StatusText => Resources.Pending;

			protected internal override bool IsPending => true;
		}

		private class cancelledPersonRequest : personRequestState
		{
			public cancelledPersonRequest(PersonRequest personRequest)
				: base(personRequest, (int)PersonRequestStatus.Cancelled)
			{
			}

			protected internal override bool IsCancelled => true;
			
			protected internal override string StatusText => Resources.Cancelled;
		}

		private class newPersonRequest : personRequestState
		{
			public newPersonRequest(PersonRequest personRequest)
				: base(personRequest, (int)PersonRequestStatus.New)
			{
			}

			protected internal override void Deny(PersonRequestDenyOption denyOption)
			{
				if (denyOption == PersonRequestDenyOption.None)
					denyOption = PersonRequestDenyOption.AutoDeny;
				PersonRequest.moveToDenied(denyOption);
			}

			protected internal override void MakePending()
			{
				PersonRequest.moveToPending();
			}

			protected internal override string StatusText => Resources.New;

			protected internal override bool IsNew => true;
		}

		#endregion

		public static int GetUnderlyingStateId(IPersonRequest request)
		{
			var typedRequest = request as PersonRequest;
			return typedRequest?.requestStatus ?? (int)PersonRequestStatus.New;
		}

		public virtual void DummyMethodToRemoveCompileErrorsWithUnusedVariable()
		{
			_updatedOnServerUtc = new DateTime();
		}

		public virtual IPerson CreatedBy { get; protected set; }
		public virtual DateTime? CreatedOn { get; protected set; }
		
		public virtual bool SendChangeOverMessageBroker()
		{
			if (_persistedState == null)
				return false;
			if (Request is ShiftTradeRequest shiftTradeRequest)
			{
				var shiftTradeStatus = shiftTradeRequest.GetShiftTradeStatus(new EmptyShiftTradeRequestChecker());
				if (_persistedState.IsNew && _requestState.IsNew)
					return false;
				if (_persistedState.IsNew && _requestState.IsPending && shiftTradeStatus == ShiftTradeStatus.OkByBothParts)
					return true;
				if (_persistedState.IsNew && _requestState.IsPending)
					return false;
				if (_persistedState.IsPending && _requestState.IsDenied && shiftTradeStatus == ShiftTradeStatus.OkByMe)
					return false;
				if (_persistedState.IsPending && _requestState.IsDenied)
					return true;
				if (_persistedState.IsDenied && _requestState.IsDenied)
					return true;
				if (_persistedState.IsPending && _requestState.IsAutoApproved)
					return false;
				return true;
			}
			return !(_persistedState.IsNew && _requestState.IsNew);
		}

		public override void NotifyTransactionComplete(DomainUpdateType operation)
		{
			base.NotifyTransactionComplete(operation);
			switch (operation)
			{
				case DomainUpdateType.Insert:
					AddEvent(new PersonRequestCreatedEvent
					{
						PersonRequestId = Id.GetValueOrDefault()
					});
					break;
				case DomainUpdateType.Update:
					AddEvent(new PersonRequestChangedEvent
					{
						PersonRequestId = Id.GetValueOrDefault()
					});
					break;
				case DomainUpdateType.Delete:
					AddEvent(new PersonRequestDeletedEvent
					{
						PersonRequestId = Id.GetValueOrDefault()
					});
					break;
			}
		}
	}
}