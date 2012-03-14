using System.Collections.Generic;
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
        private readonly NormalizeText _normalizeText = new NormalizeText();

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
            get { return _title; }
            set { _title = _normalizeText.Normalize(value); }
        }

        public virtual string Message
        {
            get { return _message; }
            set { _message = _normalizeText.Normalize(value); }
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
