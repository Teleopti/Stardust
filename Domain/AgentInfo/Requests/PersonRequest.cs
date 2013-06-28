using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
    /// <summary>
    /// Contains information about a request from an employee
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-06-05
    /// </remarks>
    public class PersonRequest : AggregateRootWithBusinessUnit, 
                                IPersonRequest, IDeleteTag,IPushMessageWhenRootAltered
    {
        private const int messageLength = 2000;
        private readonly IPerson _person;
        private string _message;
        private int requestStatus = 3;
        private PersonRequestState _requestState;
        private PersonRequestState _persistedState;
        private bool _deserialized;
        private string _subject;
        private bool _changed;
        private IList<IRequest> requests = new List<IRequest>(1);
        private bool _isDeleted;
        private string _denyReason = string.Empty;
		private DateTime _updatedOnServerUtc;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonRequest"/> class.
        /// Empty constructor for NHibernate to use.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-05
        /// </remarks>
        protected PersonRequest()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonRequest"/> class.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-05
        /// </remarks>
        public PersonRequest(IPerson person)
            : this(person, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonRequest"/> class.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="request">The request.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-05
        /// </remarks>
        public PersonRequest(IPerson person, IRequest request) : this()
        {
            InParameter.NotNull("person", person);
            _person = person;
            SetRequest(request);
        }

        private void SetRequest(IRequest request)
        {
            //We have to do it this way as cascading actions for one-to-one maappings aren't implemented in NHibernate
            requests.Clear();
            if (request!=null)
            {
                request.SetParent(this);
                requests.Add(request);
            }
        }

        /// <summary>
        /// Tells this PersonRequest that it has been persisted.
        /// </summary>
        public virtual void Persisted()
        {
            _deserialized = true;
            _persistedState = RequestState;
            _changed = false;
        }

        /// <summary>
        /// Gets the date.
        /// </summary>
        /// <value>The date.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-05
        /// </remarks>
        public virtual DateTime RequestedDate
        {
            get
            {
                IRequest request = GetRequest();
                if (request!=null) return request.Period.StartDateTime;
                return CreatedOn.GetValueOrDefault(DateTime.Today);
            }
        }

        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <value>The person.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-05
        /// </remarks>
        public virtual IPerson Person
        {
            get { return _person; }
        }

        public virtual bool TrySetMessage(string message)
        {
            CheckIfEditable();
	        message = message ?? string.Empty;
            if (message.Length > messageLength)
                return false;

            var request = Request;
            if (request != null && message!=_message)
            {
                request.TextForNotification = message.Length>255 ? message.Substring(0,255) : message; //Need to set this to make the message to be sent
            }

            _message = message;
            NotifyPropertyChanged("Message");
            return true;
        }

    	/// <summary>
    	/// Gets the formatted message
    	/// </summary>
    	/// <value>The message.</value>
    	public virtual string GetMessage(ITextFormatter formatter)
    	{
    		if(formatter == null)
				throw new ArgumentNullException("formatter");

			return formatter.Format(_message);
    	}

    	/// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-05
        /// </remarks>
        private string Message
        {
            get { return _message; }
        }

        public virtual IRequest Request
        {
            get { return GetRequest(); }
            set
            {
                SetRequest(value);
            }
        }

        /// <summary>
        /// Gets or sets the subject of the message.
        /// </summary>
        /// <value>The subject.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-10-06
        /// </remarks>
		public virtual string Subject
		{
			protected get
			{
				return _subject;
			}

			set
			{
				_subject = value;
				NotifyPropertyChanged("Subject");
			}
		}


		public virtual string GetSubject(ITextFormatter formatter)
		{
			if (formatter == null)
				throw new ArgumentNullException("formatter");
			
			return formatter.Format(_subject);
		}

        public virtual IList<IBusinessRuleResponse> Approve(IRequestApprovalService approvalService, IPersonRequestCheckAuthorization authorization)
        {
            authorization.VerifyEditRequestPermission(this);
            List<IBusinessRuleResponse> brokenRules = new List<IBusinessRuleResponse>();
            brokenRules.AddRange(((Request)GetRequest()).Approve(approvalService));
            if (brokenRules.Count == 0)
            {
                RequestState.Approve();
                NotifyOnStatusChange();
            }
            return brokenRules;
        }

        private void NotifyOnStatusChange()
        {
            NotifyPropertyChanged("StatusText");
            NotifyPropertyChanged("IsNew");
            NotifyPropertyChanged("IsPending");
            NotifyPropertyChanged("IsDenied");
            NotifyPropertyChanged("IsApproved");
        }

        /// <summary>
        /// Denies this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-09
        /// </remarks>
        public virtual void Deny(IPerson denyPerson,string denyReasonTextResourceKey, IPersonRequestCheckAuthorization authorization)
        {
            authorization.VerifyEditRequestPermission(this);
            IRequest request = GetRequest();
            if (request!=null) request.Deny(denyPerson);
			RequestState.Deny();
            _denyReason = denyReasonTextResourceKey ?? string.Empty;
            NotifyOnStatusChange();
        }

    	/// <summary>
        /// Gets a value indicating whether this instance is editable.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is editable; otherwise, <c>false</c>.
        /// </value>
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
            set{ _changed = value;}
        }

        public virtual bool IsDeleted
        {
            get { return _isDeleted; }
        }

		public virtual DateTime UpdatedOnServerUtc { get { return _updatedOnServerUtc; } }


        private void CheckIfEditable()
        {
            if (!IsEditable) throw new InvalidOperationException("Requests cannot be changed once they have been handled.");
        }

        #region ICloneableEntity<PersonRequest> Members

        /// <summary>
        /// NoneEntityClone
        /// </summary>
        /// <returns></returns>
        public virtual IPersonRequest NoneEntityClone()
        {
            PersonRequest clone = (PersonRequest)MemberwiseClone();
            IRequest request = GetRequest();
            if (request != null)
            {
                IRequest requestClone = request.NoneEntityClone();
                clone.requests = new List<IRequest>(1);
                clone.SetRequest(requestClone);
            }
            clone.SetId(null);

            return clone;
        }

        /// <summary>
        /// EntityClone
        /// </summary>
        /// <returns></returns>
        public virtual IPersonRequest EntityClone()
        {
            PersonRequest clone = (PersonRequest)MemberwiseClone();
            IRequest request = GetRequest();
            if (request != null)
            {
                IRequest requestClone = request.EntityClone();
                clone.requests = new List<IRequest>(1);
                clone.SetRequest(requestClone);
            }
            clone.SetId(Id);

            return clone;
        }

        #endregion

        #region ICloneable Members

        /// <summary>
        /// Clone
        /// </summary>
        /// <returns></returns>
        public virtual object Clone()
        {
            return NoneEntityClone();
        }

        #endregion

        /// <summary>
        /// Answers the specified message.
        /// </summary>
        /// <param name="answerMessage">The answer message.</param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-10-06
        /// </remarks>
        public virtual bool Reply(string answerMessage)
        {
            if(string.IsNullOrEmpty(answerMessage))
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
            CheckIfEditable();
            return builder.Length <= messageLength;
        }
        /// <summary>
        /// Sets the status to pending IF it was pending originally. Or if the current state is new.
        /// </summary>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-10-08
        /// </remarks>
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
            NotifyOnStatusChange();
        }

        public virtual void ForcePending()
        {
            MoveToPending();
            NotifyOnStatusChange();
        }

        #region INotifyPropertyChanged Members

        public virtual event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void NotifyPropertyChanged(string info)
        {
        	var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(info));
            }
            _changed = true;
        }

        private IRequest GetRequest()
        {
            //We have to do it this way as cascading actions for one-to-one maappings aren't implemented in NHibernate
            if (requests.Count==0)
            return null;

            return requests[0];
        }

        public virtual void Restore(IPersonRequest previousState)
        {
        	_message = previousState.GetMessage(new NoFormatting());
            var previousStateTyped = (PersonRequest) previousState;
            RequestState = previousStateTyped.RequestState;
            Subject = previousState.GetSubject(new NoFormatting());
            _denyReason = previousState.DenyReason;
            _isDeleted = previousStateTyped.IsDeleted;
            SetRequest(previousState.Request);
        }

        public virtual IMemento CreateMemento()
        {
            return new Memento<IPersonRequest>(this, 
                                                EntityClone(), 
                                                string.Format(CultureInfo.CurrentUICulture, Resources.UndoRedoModifyPersonRequest, Person.Name));
        }

        public virtual void SetDeleted()
        {
            _isDeleted = true;
        }

        public virtual bool ShouldSendPushMessageWhenAltered()
        {
           return  Request.ShouldNotifyWithMessage;
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
            NotifyOnStatusChange();
        }

        private void MoveToPending()
        {
            RequestState = new PendingPersonRequest(this);
        }

        private void MoveToApproved()
        {
            RequestState = new ApprovedPersonRequest(this);
        }

	    private void MoveToDenied(bool autoDenied = false)
	    {
		    if (autoDenied)
			    RequestState = new AutoDeniedPersonRequest(this);
		    else
			    RequestState = new DeniedPersonRequest(this);
	    }

	    private void MovetoNew()
        {
            RequestState = new NewPersonRequest(this);
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

        private PersonRequestState PersistedRequestState
        {
            get
            {
                if (_persistedState == null)
                    _persistedState = RequestState;
                return _persistedState;
            }
        }

        private PersonRequestState RequestState
        {
            get
            {
                if (_requestState == null)
                {
                    _requestState = PersonRequestState.CreateFromId(this, requestStatus);
                }
                return _requestState;
            }
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

        public virtual string StatusText { get { return RequestState.StatusText; } }

        #region Classes handling status transitions

        private abstract class PersonRequestState
        {
            protected readonly PersonRequest _personRequest;
            private readonly int _requestStatusId;

            protected PersonRequestState(PersonRequest personRequest, int requestStatusId)
            {
                _personRequest = personRequest;
                _requestStatusId = requestStatusId;
            }

            protected internal int RequestStatusId
            {
                get { return _requestStatusId; }
            }

            protected internal virtual bool IsDenied { get { return false; } }
			protected internal virtual bool IsAutoDenied { get { return false; } }
            protected internal virtual bool IsApproved { get { return false; } }
            protected internal virtual bool IsPending { get { return false; } }
            protected internal virtual bool IsNew { get { return false; } }

            protected internal virtual void Deny()
            {
                throw new InvalidRequestStateTransitionException(string.Format(CultureInfo.InvariantCulture,
                                                                  "This transition is not allowed (from {0} to denied).",
                                                                  GetType().Name));
            }
            protected internal virtual void Approve()
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

            public static PersonRequestState CreateFromId(PersonRequest personRequest, int requestStatusId)
            {
                //This should be refactored into a more dynamic way...
                switch (requestStatusId)
                {
                    case 0:
                        return new PendingPersonRequest(personRequest);
                    case 1:
                        return new DeniedPersonRequest(personRequest);
                    case 2:
                        return new ApprovedPersonRequest(personRequest);
                    case 3:
                        return new NewPersonRequest(personRequest);
					case 4:
						return new AutoDeniedPersonRequest(personRequest);
                }
                throw new ArgumentOutOfRangeException("requestStatusId", "The request status id is invalid");
            }
        }

        private class ApprovedPersonRequest : PersonRequestState
        {
            public ApprovedPersonRequest(PersonRequest personRequest)
                : base(personRequest, 2)
            {
            }

            protected internal override bool IsApproved { get { return true; } }
            protected internal override string StatusText
            {
                get { return Resources.Approved; }
            }
        }

        private class DeniedPersonRequest : PersonRequestState
        {
            public DeniedPersonRequest(PersonRequest personRequest)
                : base(personRequest, 1)
            {
            }

            protected internal override bool IsDenied { get { return true; } }
            protected internal override string StatusText
            {
                get { return Resources.Denied; }
            }
        }

		private class AutoDeniedPersonRequest : PersonRequestState
		{
			public AutoDeniedPersonRequest(PersonRequest personRequest)
				: base(personRequest, 4)
			{
			}

			protected internal override bool IsDenied { get { return true; } }
			protected internal override bool IsAutoDenied { get { return true; } }
			protected internal override string StatusText
			{
				get { return Resources.Denied; }
			}
		}

        private class PendingPersonRequest : PersonRequestState
        {
            public PendingPersonRequest(PersonRequest personRequest)
                : base(personRequest, 0)
            {
            }

            protected internal override void Deny()
            {
                _personRequest.MoveToDenied();
            }

            protected internal override void Approve()
            {
                _personRequest.MoveToApproved();
            }

            protected internal override void SetNew()
            {
                _personRequest.MovetoNew();
            }

            protected internal override string StatusText
            {
                get { return Resources.Pending; }
            }

            protected internal override bool IsPending { get { return true; } }
        }

        private class NewPersonRequest : PersonRequestState
        {
            public NewPersonRequest(PersonRequest personRequest)
                : base(personRequest, 3)
            {
            }

            protected internal override void Deny()
            {
				_personRequest.MoveToDenied(true);
            }

            protected internal override void MakePending()
            {
                _personRequest.MoveToPending();
            }

            protected internal override string StatusText
            {
                get { return Resources.New; }
            }

            protected internal override bool IsNew { get { return true; } }
        }

        #endregion

        public static int GetUnderlyingStateId(IPersonRequest request)
        {
            var typedRequest = request as PersonRequest;
            if (typedRequest == null) return 3;
            return typedRequest.requestStatus;
        }

		public virtual void DummyMethodToRemoveCompileErrorsWithUnusedVariable()
		{
			_updatedOnServerUtc = new DateTime();
		}
    }
}
