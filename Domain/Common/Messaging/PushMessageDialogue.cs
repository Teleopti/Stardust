using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common.Messaging
{
    public class PushMessageDialogue : AggregateRootWithBusinessUnit, IPushMessageDialogue
    {
        private static readonly string _defaultReply = string.Empty;
        private readonly IPushMessage _pushMessage;
        private readonly IPerson _receiver;
        private readonly IList<IDialogueMessage> _dialogueMessages;
        private readonly NormalizeText _normalizeText = new NormalizeText();
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

        public virtual IPushMessage PushMessage
        {
            get { return _pushMessage; }
        }

        public virtual IPerson Receiver
        {
            get { return _receiver; }
        }

        public virtual IList<IDialogueMessage> DialogueMessages
        {
            get { return _dialogueMessages; }
        }

        public virtual bool IsReplied
        {
            get { return _isReplied; }
            protected set { _isReplied = value; }
        }

        public virtual string Message
        {
            get
            {
                if (_pushMessage.TranslateMessage)
                {
                   string ret = UserTexts.Resources.ResourceManager.GetString(_pushMessage.Message,
                                                                  Receiver.PermissionInformation.UICulture());
                    if (!string.IsNullOrEmpty(ret)) return ret;
                }
                return _pushMessage.Message;
            }
        }

        public virtual void DialogueReply(string message, IPerson sender)
        {
            InParameter.NotNull("sender", sender);
            if (!_pushMessage.AllowDialogueReply)
                throw new ArgumentException("No text replies are allowed for this message.", "message");

            message = _normalizeText.Normalize(message);
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

        public virtual IAggregateRoot MainRoot
        {
            get { return Receiver; }
        }

        public virtual string Reply
        {
            get { return _reply; }
        }

        public virtual void SetReply(string reply)
        {
            if (PushMessage.CheckReply(reply) && !IsReplied)
            {
                _reply = _normalizeText.Normalize(reply);
                _isReplied = true;
            }
        }
    }
}
