using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common.Messaging
{
    public class PushMessage : AggregateRoot_Events_ChangeInfo_Versioned_BusinessUnit, IPushMessage
    {
        private string _message = string.Empty;
        private string _title = string.Empty;
        private IPerson _sender;
        private readonly IList<string> _replyOptions;
        private bool _allowDialogueReply = true;
        private bool _translateMessage;
        private MessageType _messageType = MessageType.Information;

        public PushMessage()
            : this(new List<string>())
        {
        }

        public PushMessage(IEnumerable<string> replyOptions)
        {
            _replyOptions = new List<string>(replyOptions);
        }

        public virtual string Title
        {
            set { _title = value; }
        }

    	public virtual string GetTitle(ITextFormatter formatter)
    	{
			if (formatter == null)
				throw new ArgumentNullException(nameof(formatter));
			
			return formatter.Format(_title);
    	}

    	public virtual string Message
        {
            set { _message = value; }
        }

    	public virtual string GetMessage(ITextFormatter formatter)
    	{
			if (formatter == null)
				throw new ArgumentNullException(nameof(formatter));
			
			return formatter.Format(_message);
    	}

    	public virtual bool AllowDialogueReply
        {
            get { return _allowDialogueReply; }
            set { _allowDialogueReply = value; }
        }

        public virtual bool TranslateMessage
        {
            get { return _translateMessage; }
            set { _translateMessage = value; }
        }

        public virtual MessageType MessageType
        {
            get { return _messageType; }
            set { _messageType = value; }
        }

        public virtual IPerson Sender
        {
            get
            {

                return _sender ?? CreatedBy;
            }
            set 
            {
                _sender = value;
            }
        }
		
        public virtual IList<string> ReplyOptions => _replyOptions;

	    public virtual bool CheckReply(string replyToCheck)
        {
            return (_replyOptions.Contains(replyToCheck));
        }

				public virtual IPerson CreatedBy { get; protected set; }
				public virtual DateTime? CreatedOn { get; protected set; }

    }
}
