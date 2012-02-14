using System;

namespace Teleopti.Ccc.AgentPortalCode.Helper
{
    public class ReplyMessageEventArgs : EventArgs
    {
        private string _textMessage;
        private string _optionChecked;
        private MessagePresenterObject _messagePresenterObject;

        public ReplyMessageEventArgs(string textMessage, string optionChecked, MessagePresenterObject messagePresenterObject)
        {
            _textMessage = textMessage;
            _optionChecked = optionChecked;
            _messagePresenterObject = messagePresenterObject;
        }

        public string TextMessage
        {
            get { return _textMessage; }
        }

        public string OptionChecked
        {
            get { return _optionChecked; }
        }

        public MessagePresenterObject PresenterObject
        {
            get { return _messagePresenterObject; }
            set { _messagePresenterObject = value; }
        }
    }
}