using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common.Messaging
{
    public class PushMessageDialogue : AggregateRoot_Events_ChangeInfo_Versioned_BusinessUnit, IPushMessageDialogue
    {
        private static readonly string _defaultReply = string.Empty;
        private readonly IPushMessage _pushMessage;
        private readonly IPerson _receiver;
        private readonly IList<IDialogueMessage> _dialogueMessages;
        private bool _isReplied;
        private string _reply = _defaultReply;

        protected PushMessageDialogue()
        {
        }

        public PushMessageDialogue(IPushMessage pushMessage, IPerson receiver)
            : this()
        {
            _dialogueMessages = new List<IDialogueMessage>();
            _receiver = receiver;
            _pushMessage = pushMessage;
        }

        public virtual IPushMessage PushMessage => _pushMessage;

	    public virtual IPerson Receiver => _receiver;

	    public virtual IList<IDialogueMessage> DialogueMessages => _dialogueMessages;

	    public virtual bool IsReplied
        {
            get { return _isReplied; }
            protected set { _isReplied = value; }
        }

	    public virtual string Message(ITextFormatter textFormatter)
	    {
		    if (_pushMessage.TranslateMessage)
		    {
			    string ret = UserTexts.Resources.ResourceManager.GetString(_pushMessage.GetMessage(new NoFormatting()),
			                                                               Receiver.PermissionInformation.UICulture());
			    if (!string.IsNullOrEmpty(ret)) return ret;
		    }
		    return _pushMessage.GetMessage(textFormatter);
	    }

	    public virtual void DialogueReply(string message, IPerson sender)
        {
            InParameter.NotNull(nameof(sender), sender);
            if (!_pushMessage.AllowDialogueReply)
                throw new ArgumentException("No text replies are allowed for this message.", nameof(message));

            var messageToAdd = new DialogueMessage(message, sender);
            messageToAdd.SetParent(this);
            _dialogueMessages.Add(messageToAdd);
            if (!sender.Equals(Receiver)) ClearReply();
        }

        public virtual void ClearReply()
        {
            _reply = _defaultReply;
            IsReplied = false;
        }

        public virtual IAggregateRoot MainRoot => Receiver;

	    protected virtual string Reply => _reply;

	    public virtual string GetReply(ITextFormatter formatter)
        {
			if (formatter == null)
				throw new ArgumentNullException(nameof(formatter));
			
			return formatter.Format(_reply);
        }

        public virtual void SetReply(string reply)
        {
	        if (reply == null && !PushMessage.ReplyOptions.Any())
	        {
				_reply = "OK";
				_isReplied = true;
	        }
			if (PushMessage.CheckReply(reply) && !IsReplied)
			{
				_reply = reply;
				_isReplied = true;
			}
        }

				public virtual IPerson CreatedBy { get; protected set; }
				public virtual DateTime? CreatedOn { get; protected set; }
    }
}
