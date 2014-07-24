using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
    public class PersonRequest : VersionedAggregateRootWithBusinessUnit,
                                 IPersonRequest, IDeleteTag, IPushMessageWhenRootAltered
    {
        private const int messageLength = 2000;
        private readonly IPerson _person;
        private string _message;
        private int requestStatus = 3;
        private personRequestState _requestState;
        private personRequestState _persistedState;
        private bool _deserialized;
        private string _subject;
        private bool _changed;
        private ISet<IRequest> requests = new HashSet<IRequest>();
        private bool _isDeleted;
        private string _denyReason = string.Empty;
        private DateTime _updatedOnServerUtc;

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
            InParameter.NotNull("person", person);
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

        public virtual IPerson Person
        {
            get { return _person; }
        }

        public virtual bool TrySetMessage(string message)
        {
            checkIfEditable();
            message = message ?? string.Empty;
            if (message.Length > messageLength)
                return false;

            var request = Request;
            if (request != null && message != _message)
            {
                request.TextForNotification = message.Length > 255 ? message.Substring(0, 255) : message;
                //Need to set this to make the message to be sent
            }

            _message = message;
            notifyPropertyChanged("Message");
            return true;
        }

        public virtual string GetMessage(ITextFormatter formatter)
        {
            if (formatter == null)
                throw new ArgumentNullException("formatter");

            return formatter.Format(_message);
        }

        private string Message
        {
            get { return _message; }
        }

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
                notifyPropertyChanged("Subject");
            }
        }


        public virtual string GetSubject(ITextFormatter formatter)
        {
            if (formatter == null)
                throw new ArgumentNullException("formatter");

            return formatter.Format(_subject);
        }

	    public virtual IList<IBusinessRuleResponse> Approve(IRequestApprovalService approvalService,
	                                                        IPersonRequestCheckAuthorization authorization,
	                                                        bool isAutoGrant = false)
        {
            authorization.VerifyEditRequestPermission(this);
            var brokenRules = new List<IBusinessRuleResponse>();
            brokenRules.AddRange(((Request) getRequest()).Approve(approvalService));
            if (brokenRules.Count == 0)
            {
                RequestState.Approve(isAutoGrant);
                notifyOnStatusChange();
            }
            return brokenRules;
        }

        private void notifyOnStatusChange()
        {
            notifyPropertyChanged("StatusText");
            notifyPropertyChanged("IsNew");
            notifyPropertyChanged("IsPending");
            notifyPropertyChanged("IsDenied");
            notifyPropertyChanged("IsApproved");
        }

	    public virtual void Deny(IPerson denyPerson, string denyReasonTextResourceKey,
                                 IPersonRequestCheckAuthorization authorization)
        {
            authorization.VerifyEditRequestPermission(this);
            IRequest request = getRequest();
            if (request != null) request.Deny(denyPerson);
            RequestState.Deny();
            _denyReason = denyReasonTextResourceKey ?? string.Empty;
            notifyOnStatusChange();
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

        public virtual bool IsDeleted
        {
            get { return _isDeleted; }
        }

        public virtual DateTime UpdatedOnServerUtc
        {
            get { return _updatedOnServerUtc; }
        }


        private void checkIfEditable()
        {
            if (!IsEditable)
                throw new InvalidOperationException("Requests cannot be changed once they have been handled.");
        }

        #region ICloneableEntity<PersonRequest> Members

        public virtual IPersonRequest NoneEntityClone()
        {
            var clone = (PersonRequest) MemberwiseClone();
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
            var clone = (PersonRequest) MemberwiseClone();
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
            checkIfEditable();
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
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(info));
            }
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
            var previousStateTyped = (PersonRequest) previousState;
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
            if (IsNew) return null;
            string message = Request.TextForNotification;
            string title = String.IsNullOrEmpty(Subject) ? message : Subject;

            return SendPushMessageService.CreateConversation(title, message, false).
                                          To(Request.ReceiversForNotification).TranslateMessage().AddReplyOption("OK");
        }

        public virtual string DenyReason
        {
            get { return _denyReason ?? string.Empty; }
            protected set { _denyReason = value; }
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

        private void moveToDenied(bool autoDenied = false)
        {
            if (autoDenied)
                RequestState = new autoDeniedPersonRequest(this);
            else
                RequestState = new deniedPersonRequest(this);
        }

        private void movetoNew()
        {
            RequestState = new newPersonRequest(this);
        }

        public virtual bool IsNew
        {
            get { return RequestState.IsNew; }
        }

        public virtual bool IsPending
        {
            get { return RequestState.IsPending; }
        }

        public virtual bool IsDenied
        {
            get { return RequestState.IsDenied; }
        }

        public virtual bool IsAutoDenied
        {
            get { return RequestState.IsAutoDenied; }
        }

        public virtual bool IsApproved
        {
            get { return RequestState.IsApproved; }
        }

	    public virtual bool IsAutoAproved
	    {
			get { return RequestState.IsAutoApproved; }
	    }

	    private personRequestState PersistedRequestState
        {
            get { return _persistedState ?? (_persistedState = RequestState); }
        }

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

        public virtual string StatusText
        {
            get { return RequestState.StatusText; }
        }

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

            protected internal int RequestStatusId
            {
                get { return _requestStatusId; }
            }

            protected internal virtual bool IsDenied
            {
                get { return false; }
            }

            protected internal virtual bool IsAutoDenied
            {
                get { return false; }
            }

            protected internal virtual bool IsApproved
            {
                get { return false; }
            }

	        protected internal virtual bool IsAutoApproved
	        {
		        get { return false; }
	        }

            protected internal virtual bool IsPending
            {
                get { return false; }
            }

            protected internal virtual bool IsNew
            {
                get { return false; }
            }

            protected internal virtual void Deny()
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
                switch (requestStatusId)
                {
                    case 0:
                        return new pendingPersonRequest(personRequest);
                    case 1:
                        return new deniedPersonRequest(personRequest);
                    case 2:
                        return new approvedPersonRequest(personRequest);
                    case 3:
                        return new newPersonRequest(personRequest);
                    case 4:
                        return new autoDeniedPersonRequest(personRequest);
                }
                throw new ArgumentOutOfRangeException("requestStatusId", "The request status id is invalid");
            }
        }

        private class approvedPersonRequest : personRequestState
        {
            public approvedPersonRequest(PersonRequest personRequest)
                : base(personRequest, 2)
            {
            }

            protected internal override bool IsApproved
            {
                get { return true; }
            }

            protected internal override string StatusText
            {
                get { return Resources.Approved; }
            }
        }

		private class autoApprovedPersonRequest : personRequestState
		{
			public autoApprovedPersonRequest(PersonRequest personRequest) 
				: base(personRequest, 2)
			{
			}

			protected internal override bool IsApproved
			{
				get { return true; }
			}

			protected internal override bool IsAutoApproved
			{
				get { return true; }
			}

			protected internal override string StatusText
			{
				get { return Resources.Approved; }
			}
		}

        private class deniedPersonRequest : personRequestState
        {
            public deniedPersonRequest(PersonRequest personRequest)
                : base(personRequest, 1)
            {
            }

            protected internal override bool IsDenied
            {
                get { return true; }
            }

            protected internal override string StatusText
            {
                get { return Resources.Denied; }
            }
        }

        private class autoDeniedPersonRequest : personRequestState
        {
            public autoDeniedPersonRequest(PersonRequest personRequest)
                : base(personRequest, 4)
            {
            }

            protected internal override bool IsDenied
            {
                get { return true; }
            }

            protected internal override bool IsAutoDenied
            {
                get { return true; }
            }

            protected internal override string StatusText
            {
                get { return Resources.Denied; }
            }
        }

        private class pendingPersonRequest : personRequestState
        {
            public pendingPersonRequest(PersonRequest personRequest)
                : base(personRequest, 0)
            {
            }

            protected internal override void Deny()
            {
                PersonRequest.moveToDenied();
            }

            protected internal override void Approve(bool isAutoGrant)
            {
                PersonRequest.moveToApproved(isAutoGrant);
            }

            protected internal override void SetNew()
            {
                PersonRequest.movetoNew();
            }

            protected internal override string StatusText
            {
                get { return Resources.Pending; }
            }

            protected internal override bool IsPending
            {
                get { return true; }
            }
        }

        private class newPersonRequest : personRequestState
        {
            public newPersonRequest(PersonRequest personRequest)
                : base(personRequest, 3)
            {
            }

            protected internal override void Deny()
            {
                PersonRequest.moveToDenied(true);
            }

            protected internal override void MakePending()
            {
                PersonRequest.moveToPending();
            }

            protected internal override string StatusText
            {
                get { return Resources.New; }
            }

            protected internal override bool IsNew
            {
                get { return true; }
            }
        }

        #endregion

        public static int GetUnderlyingStateId(IPersonRequest request)
        {
            var typedRequest = request as PersonRequest;
            return typedRequest == null
                       ? 3
                       : typedRequest.requestStatus;
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
            var shiftTradeRequest = Request as ShiftTradeRequest;
            if (shiftTradeRequest != null)
            {
                var shiftTradeStatus = shiftTradeRequest.GetShiftTradeStatus(new EmptyShiftTradeRequestChecker());
                if (_persistedState.IsNew && _requestState.IsNew)
                    return false;
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
    }
}