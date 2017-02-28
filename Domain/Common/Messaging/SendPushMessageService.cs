﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Common.Messaging
{
    /// <summary>
    /// Service for preparing and sending a conversation
    /// </summary>
    public class SendPushMessageService : ISendPushMessageService
    {

        private readonly IList<IPerson> _receivers;

        public virtual ReadOnlyCollection<IPerson> Receivers
        {
            get { return new ReadOnlyCollection<IPerson>(_receivers); }
        }

        #region factory
        public static ISendPushMessageOption<string> CreateConversation(string title, string message, bool allowDialogueReply,
			MessageType type = MessageType.Information)
        {
            var pushMessage = new PushMessage
            {
	            Title = title,
	            Message = message,
	            AllowDialogueReply = allowDialogueReply,
	            MessageType = type
            };
            return new SendPushMessageService(pushMessage);
        }

        #endregion
        public IPushMessage PushMessage { get; private set; }

        
        public SendPushMessageService(IPushMessage pushMessage)
        {
            _receivers = new List<IPerson>();
            PushMessage = pushMessage;

        }

        public ISendPushMessageService To(IPerson receiver)
        {
            AddReceiver(receiver);
            return this;
        }

        public ISendPushMessageService To(IList<IPerson> receivers)
        {
            AddReceivers(receivers);
            return this;
        }

        public ISendPushMessageService From(IPerson sender)
        {
            PushMessage.Sender = sender;
            return this;
        }

        public ISendPushMessageService AddReplyOption(string replyOption)
        {
            PushMessage.ReplyOptions.Add(replyOption);
            return this;
        }

        public ISendPushMessageService AddReplyOption(IEnumerable<string> replyOptions)
        {
            foreach (string option in replyOptions)
            {
                PushMessage.ReplyOptions.Add(option);
            }
            return this;
        }

        public ISendPushMessageService TranslateMessage()
        {
            PushMessage.TranslateMessage = true;
            return this;
        }

		public void SendConversation(IPushMessagePersister repository)
        {
            repository.Add(PushMessage,Receivers);
        }

		public ISendPushMessageReceipt SendConversationWithReceipt(IPushMessagePersister repository)
        {
            return repository.Add(PushMessage, Receivers);
        }

        public void SendConversation(IPushMessageRepository pushMessageRepository, IPushMessageDialogueRepository pushMessageDialogueRepository)
        {
            pushMessageRepository.Add(PushMessage);
            foreach(IPerson receiver in Receivers)
            {
                PushMessageDialogue dialogue = new PushMessageDialogue(PushMessage, receiver);
                pushMessageDialogueRepository.Add(dialogue);
            }
        }

        public void ClearReceivers()
        {
            _receivers.Clear();
        }

        public virtual void AddReceiver(IPerson receiver)
        {
            AddUnique(receiver);
        }

        public virtual void AddReceivers(IList<IPerson> receivers)
        {
            receivers.ForEach(AddUnique);
        }

        #region private
        private void AddUnique(IPerson person)
        {
            if (!_receivers.Contains(person)) _receivers.Add(person);
        }
        #endregion

    }

}