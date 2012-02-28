﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common.Messaging
{
    public class PushMessage : AggregateRootWithBusinessUnit, IPushMessage
    {
        private string _message = string.Empty;
        private string _title = string.Empty;
        private IPerson _sender;
        private readonly IList<string> _replyOptions;
        private bool _allowDialogueReply = true;
        private bool _translateMessage;

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
    		return formatter.Format(_title);
    	}

    	public virtual string Message
        {
            set { _message = value; }
        }

    	public virtual string GetMessage(ITextFormatter formatter)
    	{
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

        

        public virtual IList<string> ReplyOptions
        {
            get { return _replyOptions; }
        }

        public virtual bool CheckReply(string replyToCheck)
        {
            return (_replyOptions.Contains(replyToCheck));
        }
    }
}
