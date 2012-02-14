using System;
using System.Collections.Generic;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortalCode.Helper
{
    public class MessagePresenterObject
    {
        private readonly ICollection<string> _replyOptions;
        private readonly string _title;
        private readonly string _originalMessage;
        private readonly string _sender;
        private readonly string _originalDate;
        private readonly Boolean _allowDialogueReply;
        private PushMessageDialogueDto _dto;
        private string  _dialogMessages;

        public MessagePresenterObject(PushMessageDialogueDto pushMessageDialogueDto)
        {
            _replyOptions = pushMessageDialogueDto.PushMessage.ReplyOptions;
            _title = pushMessageDialogueDto.PushMessage.Title;
            _originalMessage = pushMessageDialogueDto.Message;
            _sender = pushMessageDialogueDto.PushMessage.Sender.Name;
            _originalDate = pushMessageDialogueDto.OriginalDate;
            _allowDialogueReply = pushMessageDialogueDto.PushMessage.AllowDialogueReply;
            _dto = pushMessageDialogueDto;
            _dialogMessages = "";

            
            foreach (DialogueMessageDto message in pushMessageDialogueDto.Messages)
            {
                _dialogMessages +="\n" +  message.CreatedOn + " " + message.Sender.Name + "\n" + message.Text;
            }

           
        }

        public string DialogMessages
        {
            get { return _dialogMessages; }
        }

        public ICollection<string> ReplyOptions
        {
            get { return _replyOptions; }
        }

        public string Title
        {
            get { return _title; }
        }

        public string OriginalMessage
        {
            get { return _originalMessage; }
        }

        public string Sender
        {
            get { return _sender; }
        }

        public string OriginalDate
        {
            get { return _originalDate; }
        }

        public bool AllowDialogueReply
        {
            get { return _allowDialogueReply; }
        }

        public PushMessageDialogueDto Dto
        {
            get {
                return _dto;
            }
            set { _dto = value; }
        }

        public bool HasReplied
        {
            get{ return _dto.IsReplied;} 
        }

        public string LatestReply { get; set; }
    }
}